using PedidoNet.UI.Shared.Offline.Productos;
using PedidoNet.Web.Models.Productos;
using PedidoNet.Web.Services.Api;
using System.Net;

namespace PedidoNet.Web.Services.Offline
{
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

                foreach(var operation in operations .OrderBy(x => x.CreatedUtc))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        await ProcessOperationAsync(operation, cancellationToken);

                        await _queue.RemoveAsync(operation.OperationId, cancellationToken);
                    }
                    catch(HttpRequestException ex) when(IsTransient(ex))
                    {
                        await RegisterFailureAsync(operation, ex, cancellationToken);
                        // Si la api no está disponible
                        // no se necesita intentar las demás

                        break;
                    }
                    catch(Exception ex)
                    {
                        await RegisterFailureAsync(operation, ex, cancellationToken);

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

            producto.SyncStatus = UI.Shared.Offline.SyncStatus.Synced;

            await _store.UpsertAsync(producto, cancellationToken);
        }

        private async Task ProcessCreateAsync(ProductoLocal producto, CancellationToken cancellationToken)
        {
            // Protección
            if (producto.ProductoId.HasValue)
            {
                producto.SyncStatus = UI.Shared.Offline.SyncStatus.Synced;

                await _store.UpsertAsync(producto, cancellationToken);

                return;
            }

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

            producto.SyncStatus = UI.Shared.Offline.SyncStatus.Synced;
            producto.isDeleted = false;
            producto.LastModifiedUtc = DateTime.UtcNow;

            await _store.UpsertAsync(producto, cancellationToken);
        }

        private async Task RegisterFailureAsync(ProductoSyncOperation operation, Exception ex, CancellationToken cancellationToken)
        {
            operation.RetryCount++;
            operation.LastError = ex.Message;

            await _queue.UpdateAsync(operation, cancellationToken);
        }

        private static bool IsTransient(HttpRequestException exception)
        {
            if(exception.StatusCode is null)
            {
                // DNS, API apagada, pérdida de conexión
                // timeout de red, etc.
                return true;
            }

            return exception.StatusCode switch
            {
                HttpStatusCode.RequestTimeout => true,
                HttpStatusCode.TooManyRequests => true,
                HttpStatusCode.InternalServerError => true,
                HttpStatusCode.BadGateway => true,
                HttpStatusCode.ServiceUnavailable => true,
                HttpStatusCode.GatewayTimeout => true,

                _ => false
            };
        }
    }
}
