using PedidoNet.UI.Shared.Models.Productos;
using PedidoNet.UI.Shared.Offline;
using PedidoNet.UI.Shared.Offline.Productos;

namespace PedidoNet.UI.Shared.Productos
{
    /*
     * Servicio offline-first de Productos compartido.
     *
     * Lectura:  almacenamiento local -> respuesta inmediata;
     *           si hay red: sincroniza cola, consulta API, merge.
     * Escritura: siempre local primero + operación en cola;
     *           si hay red intenta sincronizar.
     */
    public class ProductoService : IProductoService
    {
        private readonly ProductosApiClient _apiClient;
        private readonly IProductoOfflineStore _localStore;
        private readonly IProductoSyncQueue _syncQueue;
        private readonly ProductoSyncService _syncService;
        private readonly IConnectivityService _connectivity;

        public ProductoService(ProductosApiClient apiClient, IProductoOfflineStore localStore,
            IProductoSyncQueue syncQueue, ProductoSyncService syncService, IConnectivityService connectivity)
        {
            _apiClient = apiClient;
            _localStore = localStore;
            _syncQueue = syncQueue;
            _syncService = syncService;
            _connectivity = connectivity;
        }

        public async Task<List<ProductosDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        {
            if (_connectivity.IsOnline)
            {
                try
                {
                    await _syncService.SynchronizeAsync(cancellationToken);

                    var remote = await _apiClient.GetAllAsync(cancellationToken);

                    await MergeRemoteAsync(remote, cancellationToken);
                }
                catch (HttpRequestException)
                {
                    // API no disponible: se devuelven los datos locales.
                }
                catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // Timeout: se devuelven los datos locales.
                }
            }

            return await GetLocalDtosAsync(cancellationToken);
        }

        public async Task<Guid> CrearAsync(CrearProductoRequest model, CancellationToken cancellationToken = default)
        {
            var localId = Guid.NewGuid();

            var producto = new ProductoLocal
            {
                LocalId = localId,
                ProductoId = null,

                Codigo = model.Codigo,
                Nombre = model.Nombre,
                PrecioVenta = model.PrecioVenta,
                Existencias = model.Existencias,
                TieneIVA = model.TieneIVA,
                TieneISC = model.TieneISC,

                SyncStatus = SyncStatus.PendingCreate,
                isDeleted = false,
                LastModifiedUtc = DateTime.UtcNow
            };

            await _localStore.UpsertAsync(producto, cancellationToken);

            await _syncQueue.EnqueueAsync(new ProductoSyncOperation
            {
                OperationId = Guid.NewGuid(),
                ProductoLocalId = producto.LocalId,
                Type = ProductoSyncOperationType.Create,
                CreatedUtc = DateTime.UtcNow
            }, cancellationToken);

            await TrySynchronizeAsync(cancellationToken);

            return producto.LocalId;
        }

        public async Task ActualizarAsync(Guid localId, ActualizarProductoRequest model, CancellationToken cancellationToken = default)
        {
            var producto = await _localStore.GetByLocalIdAsync(localId, cancellationToken) ?? throw new InvalidOperationException("Producto local no encontrado");

            producto.Codigo = model.Codigo;
            producto.Nombre = model.Nombre;
            producto.PrecioVenta = model.PrecioVenta;
            producto.Existencias = model.Existencias;
            producto.TieneIVA = model.TieneIVA;
            producto.TieneISC = model.TieneISC;
            producto.LastModifiedUtc = DateTime.UtcNow;

            if (producto.ProductoId.HasValue)
            {
                producto.SyncStatus = SyncStatus.PendingUpdate;
            }
            else
            {
                producto.SyncStatus = SyncStatus.PendingCreate;
            }

            await _localStore.UpsertAsync(producto, cancellationToken);

            var operationType = producto.ProductoId.HasValue
                ? ProductoSyncOperationType.Update
                : ProductoSyncOperationType.Create;

            await EnsureOperationAsync(producto, operationType, cancellationToken);

            await TrySynchronizeAsync(cancellationToken);
        }

        public async Task EliminarAsync(Guid localId, CancellationToken cancellationToken = default)
        {
            var producto = await _localStore.GetByLocalIdAsync(localId, cancellationToken);

            if (producto is null) return;

            var pending = await _syncQueue.GetPendingAsync(cancellationToken);

            var operations = pending
                .Where(x => x.ProductoLocalId == localId).ToList();

            if (!producto.ProductoId.HasValue)
            {
                // Nunca llegó al servidor
                foreach (var operation in operations)
                {
                    await _syncQueue.RemoveAsync(operation.OperationId, cancellationToken);
                }

                await _localStore.DeleteAsync(localId, cancellationToken);

                return;
            }

            /*
             * Eliminar updates anteriores
             */

            foreach (var operation in operations.Where(x => x.Type == ProductoSyncOperationType.Update))
            {
                await _syncQueue.RemoveAsync(operation.OperationId, cancellationToken);
            }

            // Tombstone local: se oculta en la UI hasta confirmar en el servidor.
            producto.isDeleted = true;
            producto.SyncStatus = SyncStatus.PendingDelete;
            producto.LastModifiedUtc = DateTime.UtcNow;

            await _localStore.UpsertAsync(producto, cancellationToken);

            await EnsureOperationAsync(producto, ProductoSyncOperationType.Delete, cancellationToken);

            await TrySynchronizeAsync(cancellationToken);
        }

        public async Task<ProductosDto?> ObtenerPorLocalIdAsync(Guid localId, CancellationToken cancellationToken = default)
        {
            var producto = await _localStore.GetByLocalIdAsync(localId, cancellationToken);
            if (producto is null || producto.isDeleted) { return null; }

            return MapToDto(producto);
        }

        /*
         * Imágenes: solo online y para productos ya sincronizados
         * (requieren ProductoId). Las imágenes offline son una fase posterior.
         */
        public Task<ProductoImagenDTO> SubirImagenAsync(int productoId, ProductoImageUpload image, CancellationToken cancellationToken = default)
        {
            return _apiClient.UploadImageAsync(productoId, image, cancellationToken);
        }

        public string ObtenerUrlImagen(string ruta)
        {
            return _apiClient.BuildImageUrl(ruta);
        }

        private async Task TrySynchronizeAsync(CancellationToken cancellationToken)
        {
            if (!_connectivity.IsOnline)
            {
                return;
            }

            try
            {
                await _syncService.SynchronizeAsync(cancellationToken);
            }
            catch (HttpRequestException)
            {
                // Queda pendiente
            }
            catch (UnauthorizedAccessException)
            {
                // Queda pendiente hasta que el usuario vuelva a iniciar sesión
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Timeout: queda pendiente
            }
        }

        private async Task<List<ProductosDto>> GetLocalDtosAsync(CancellationToken cancellationToken)
        {
            var locales = await _localStore.GetAllAsync(cancellationToken);
            return locales
                .Where(x => !x.isDeleted)
                .OrderBy(x => x.Nombre)
                .Select(MapToDto)
                .ToList();

        }

        private static ProductosDto MapToDto(ProductoLocal local)
        {
            return new ProductosDto
            {
                LocalId = local.LocalId,
                ClientId = local.LocalId,
                ProductoId = local.ProductoId,

                Codigo = local.Codigo,
                Nombre = local.Nombre,
                PrecioVenta = local.PrecioVenta,
                Existencias = local.Existencias,
                TieneIVA = local.TieneIVA,
                TieneISC = local.TieneISC,

                SyncStatus = local.SyncStatus,
            };
        }

        private async Task MergeRemoteAsync(IEnumerable<ProductosDto> remoteProducts, CancellationToken cancellationToken)
        {
            var remoteIds = new HashSet<int>();

            foreach (var remote in remoteProducts)
            {
                if (!remote.ProductoId.HasValue)
                {
                    continue;
                }

                remoteIds.Add(remote.ProductoId.Value);

                ProductoLocal? local = null;

                /*
                 * Primero se intenta reconciliar usando ClientId.
                 *
                 * ClientId del servidor representa exactamente
                 * el LocalId con el que el producto fue creado
                 * originalmente en el cliente.
                 */
                if (remote.ClientId.HasValue && remote.ClientId.Value != Guid.Empty)
                {
                    local = await _localStore.GetByLocalIdAsync(remote.ClientId.Value,cancellationToken);
                }

                /*
                 * Productos antiguos creados antes de agregar ClientId
                 * se siguen relacionando mediante ProductoId.
                 */
                if (local is null)
                {
                    local = await _localStore.GetByserverIdAsync(remote.ProductoId.Value,cancellationToken);
                }

                /*
                 * Nunca sobrescribir cambios locales
                 * que todavía están pendientes de enviarse
                 * o que requieren intervención del usuario.
                 */
                if (local is not null &&
                    (local.SyncStatus == SyncStatus.PendingUpdate ||
                     local.SyncStatus == SyncStatus.PendingDelete ||
                     local.SyncStatus == SyncStatus.Failed))
                {
                    continue;
                }

                if (local is null)
                {
                    local = new ProductoLocal
                    {
                        LocalId = remote.ClientId.HasValue &&
                                  remote.ClientId.Value != Guid.Empty
                                  ? remote.ClientId.Value
                                  : Guid.NewGuid()
                    };
                }

                local.ProductoId = remote.ProductoId.Value;

                local.Codigo = remote.Codigo;

                local.Nombre = remote.Nombre;

                local.PrecioVenta = remote.PrecioVenta;

                local.Existencias = remote.Existencias;

                local.TieneIVA = remote.TieneIVA;

                local.TieneISC = remote.TieneISC;

                local.SyncStatus = SyncStatus.Synced;

                local.isDeleted = false;

                local.LastModifiedUtc = DateTime.UtcNow;

                await _localStore.UpsertAsync(local,cancellationToken);
            }

            /*
             * Productos sincronizados que ya no existen en el servidor
             * (eliminados desde otro dispositivo) se quitan localmente.
             * Nunca se tocan registros con cambios pendientes.
             */
            var locales = await _localStore.GetAllAsync(cancellationToken);

            foreach (var local in locales.Where(x =>
                x.SyncStatus == SyncStatus.Synced &&
                x.ProductoId.HasValue &&
                !remoteIds.Contains(x.ProductoId.Value)))
            {
                await _localStore.DeleteAsync(local.LocalId, cancellationToken);
            }
        }

        /*
         * Garantiza una única operación pendiente por tipo y producto.
         * Si existía una operación marcada con error permanente, el
         * cambio del usuario la reactiva para volver a intentarla.
         */
        private async Task EnsureOperationAsync(ProductoLocal producto, ProductoSyncOperationType type, CancellationToken cancellationToken)
        {
            var pending = await _syncQueue.GetPendingAsync(cancellationToken);

            var operations = pending
                .Where(x => x.ProductoLocalId == producto.LocalId)
                .ToList();

            foreach (var operation in operations.Where(x => x.RequiresAttention))
            {
                operation.RequiresAttention = false;
                operation.LastError = null;

                await _syncQueue.UpdateAsync(operation, cancellationToken);
            }

            // Un Create pendiente ya envía los datos más recientes del registro local.
            if (type == ProductoSyncOperationType.Update &&
                operations.Any(x => x.Type == ProductoSyncOperationType.Create))
            {
                return;
            }

            if (operations.Any(x => x.Type == type))
            {
                return;
            }

            await _syncQueue.EnqueueAsync(new ProductoSyncOperation
            {
                OperationId = Guid.NewGuid(),
                ProductoLocalId = producto.LocalId,
                Type = type,
                CreatedUtc = DateTime.UtcNow
            }, cancellationToken);
        }
    }
}
