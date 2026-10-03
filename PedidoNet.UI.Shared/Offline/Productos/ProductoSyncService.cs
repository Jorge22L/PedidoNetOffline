using PedidoNet.UI.Shared.Models.Productos;
using PedidoNet.UI.Shared.Productos;
using System.Net;
using System.Text.Json;

namespace PedidoNet.UI.Shared.Offline.Productos
{
    /*
     * Lógica de sincronización compartida entre Web y Mobile.
     *
     * No depende de la plataforma: el almacenamiento local
     * (IndexedDB / SQLite) llega por IProductoOfflineStore e
     * IProductoSyncQueue, implementados en cada host.
     */
    public sealed class ProductoSyncService
    {
        private readonly ProductosApiClient _api;
        private readonly IProductoOfflineStore _store;
        private readonly IProductoSyncQueue _queue;

        private readonly SemaphoreSlim _syncLock = new(1, 1);

        public ProductoSyncService(
            ProductosApiClient api,
            IProductoOfflineStore store,
            IProductoSyncQueue queue)
        {
            _api = api;
            _store = store;
            _queue = queue;
        }

        public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
        {
            if(!await _syncLock.WaitAsync(0, cancellationToken))
            {
                return;
            }

            try
            {
                var operations = await _queue.GetPendingAsync(cancellationToken);

                foreach(var operation in operations
                    .Where(x => !x.RequiresAttention)
                    .OrderBy(x => x.CreatedUtc))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        await ProcessOperationAsync(operation, cancellationToken);

                        await _queue.RemoveAsync(operation.OperationId, cancellationToken);
                    }
                    catch(UnauthorizedAccessException)
                    {
                        /*
                         * 401: AuthenticatedHttpHandler ya intentó
                         * refresh + un reintento. La operación queda
                         * intacta en la cola y la UI debe pedir login.
                         */
                        throw;
                    }
                    catch(HttpRequestException ex) when(IsTransient(ex))
                    {
                        await RegisterTransientFailureAsync(operation, ex, cancellationToken);
                        // Si la api no está disponible
                        // no se necesita intentar las demás

                        break;
                    }
                    catch(TaskCanceledException ex) when(!cancellationToken.IsCancellationRequested)
                    {
                        // Timeout de HttpClient: también es transitorio.
                        await RegisterTransientFailureAsync(operation, ex, cancellationToken);

                        break;
                    }
                    catch(Exception ex) when(ex is HttpRequestException
                                              or InvalidOperationException
                                              or JsonException)
                    {
                        // 400, 403, 404, 409, 422, validación, respuesta inválida...
                        await RegisterPermanentFailureAsync(operation, ex, cancellationToken);
                    }
                }
            }
            finally
            {
                _syncLock.Release();
            }
        }

        private async Task ProcessOperationAsync(ProductoSyncOperation operation, CancellationToken cancellationToken)
        {
            var producto = await _store.GetByLocalIdAsync(operation.ProductoLocalId, cancellationToken);

            if(producto is null)
            {
                /*
                 * El registro ya no existe localmente
                 * Quitamos la operacion de la cola
                 */
                return;
            }

            switch (operation.Type)
            {
                case ProductoSyncOperationType.Create:
                    await ProcessCreateAsync(producto, cancellationToken);
                    break;

                case ProductoSyncOperationType.Update:
                    await ProcessUpdateAsync(producto, cancellationToken);
                    break;

                case ProductoSyncOperationType.Delete:
                    await ProcessDeleteAsync(producto, cancellationToken);
                    break;

                default:
                    throw new InvalidOperationException($"Operación de Sincronización no soportada: {operation.Type}");
            }
        }

        private async Task ProcessDeleteAsync(ProductoLocal producto, CancellationToken cancellationToken)
        {
            if (!producto.ProductoId.HasValue)
            {
                // Nunca llegó al servidor
                await _store.DeleteAsync(producto.LocalId, cancellationToken);
                return;
            }

            try
            {
                await _api.DeleteAsync(producto.ProductoId.Value, cancellationToken);
            }
            catch(HttpRequestException ex) when(ex.StatusCode == HttpStatusCode.NotFound)
            {
                // El producto ya no existe en el servidor
                // El objetivo del DELETE ya fue alcanzado
            }
            await _store.DeleteAsync(producto.LocalId, cancellationToken);
        }

        private async Task ProcessUpdateAsync(ProductoLocal producto, CancellationToken cancellationToken)
        {
            if (!producto.ProductoId.HasValue)
            {
                throw new InvalidOperationException("No se puede actualizar en el servidor un producto que no tiene ProductoId");
            }

            var request = new ActualizarProductoRequest
            {
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                PrecioVenta = producto.PrecioVenta,
                Existencias = producto.Existencias,
                TieneIVA = producto.TieneIVA,
                TieneISC = producto.TieneISC,
            };

            await _api.UpdateAsync(producto.ProductoId.Value, request, cancellationToken);

            producto.SyncStatus = SyncStatus.Synced;

            await _store.UpsertAsync(producto, cancellationToken);
        }

        private async Task ProcessCreateAsync(ProductoLocal producto, CancellationToken cancellationToken)
        {
            // Protección
            if (producto.ProductoId.HasValue)
            {
                producto.SyncStatus = SyncStatus.Synced;

                await _store.UpsertAsync(producto, cancellationToken);

                return;
            }

            /*
             * ClientId = LocalId.
             *
             * Si el POST llegó al servidor pero se perdió la
             * respuesta, el reintento envía el mismo ClientId y
             * la API devuelve el producto existente (índice único
             * filtrado + consulta previa por ClientId).
             */
            var request = new CrearProductoRequest
            {
                ClientId = producto.LocalId,

                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                PrecioVenta = producto.PrecioVenta,
                Existencias = producto.Existencias,
                TieneIVA = producto.TieneIVA,
                TieneISC = producto.TieneISC,
            };

            var created = await _api.CreateAsync(request, cancellationToken);

            if(!created.ProductoId.HasValue || created.ProductoId.Value <= 0)
            {
                throw new InvalidOperationException("La API no devolvió un ProductoId válido");
            }

            if(!created.ClientId.HasValue || created.ClientId.Value != producto.LocalId)
            {
                throw new InvalidOperationException("El ClientId devuelto por la API no coincide con el LocalId del producto.");
            }

            // Este es el dato más importante
            producto.ProductoId = created.ProductoId;

            producto.Codigo = created.Codigo;
            producto.Nombre = created.Nombre;
            producto.PrecioVenta = created.PrecioVenta;
            producto.Existencias = created.Existencias;
            producto.TieneIVA = created.TieneIVA;
            producto.TieneISC = created.TieneISC;

            producto.SyncStatus = SyncStatus.Synced;
            producto.isDeleted = false;
            producto.LastModifiedUtc = DateTime.UtcNow;

            await _store.UpsertAsync(producto, cancellationToken);
        }

        private async Task RegisterTransientFailureAsync(ProductoSyncOperation operation, Exception ex, CancellationToken cancellationToken)
        {
            operation.RetryCount++;
            operation.LastError = ex.Message;

            await _queue.UpdateAsync(operation, cancellationToken);
        }

        private async Task RegisterPermanentFailureAsync(ProductoSyncOperation operation, Exception ex, CancellationToken cancellationToken)
        {
            operation.RetryCount++;
            operation.LastError = ex.Message;
            operation.RequiresAttention = true;

            await _queue.UpdateAsync(operation, cancellationToken);

            var producto = await _store.GetByLocalIdAsync(operation.ProductoLocalId, cancellationToken);

            if (producto is null)
            {
                return;
            }

            producto.SyncStatus = SyncStatus.Failed;

            await _store.UpsertAsync(producto, cancellationToken);
        }

        private static bool IsTransient(HttpRequestException exception)
        {
            if(exception.StatusCode is null)
            {
                // DNS, API apagada, pérdida de conexión
                // timeout de red, etc.
                return true;
            }

            var statusCode = (int)exception.StatusCode.Value;

            return exception.StatusCode == HttpStatusCode.RequestTimeout ||
                   exception.StatusCode == HttpStatusCode.TooManyRequests ||
                   statusCode >= 500;
        }
    }
}
