using PedidoNet.UI.Shared.Models.Productos;
using PedidoNet.UI.Shared.Offline.Productos;
using PedidoNet.Web.Models.Productos;
using PedidoNet.Web.Services.Api;
using PedidoNet.Web.Services.Offline;

namespace PedidoNet.Web.Services.Productos
{
    public class ProductoService : IProductoService
    {
        private readonly ProductosApiClient _apiClient;
        private readonly IProductoOfflineStore _localStore;
        private readonly IProductoSyncQueue _syncQueue;
        private readonly ProductoSyncService _syncService;
        private readonly ConnectivityService _connectivity;
        public ProductoService(ProductosApiClient apiClient, IProductoOfflineStore localStore,
            IProductoSyncQueue syncQueue, ProductoSyncService syncService, ConnectivityService connectivity)
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

                }
            }

            return await GetLocalDtosAsync(cancellationToken);
        }

        public Task<ProductosDto?> ObtenerPorIdAsync(int id)
            => _apiClient.GetByIdAsync(id);

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

                SyncStatus = UI.Shared.Offline.SyncStatus.PendingCreate,
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

            if (_connectivity.IsOnline)
            {
                try
                {
                    await _syncService.SynchronizeAsync(cancellationToken);
                }
                catch (HttpRequestException)
                {

                }
            }

            return producto.LocalId;
        }

        public async Task ActualizarAsync(Guid localId, ActualizarProductoRequest model, CancellationToken cancellationToken)
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
                producto.SyncStatus = UI.Shared.Offline.SyncStatus.PendingUpdate;
            }
            else
            {
                producto.SyncStatus = UI.Shared.Offline.SyncStatus.PendingCreate;
            }

            await _localStore.UpsertAsync(producto, cancellationToken);

            if (producto.ProductoId.HasValue)
            {
                await EnqueueUpdateIfNeededAsync(producto, cancellationToken);
            }

            if (_connectivity.IsOnline)
            {
                try
                {
                    await _syncService.SynchronizeAsync(cancellationToken);
                }
                catch (HttpRequestException)
                {

                }
            }


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

            producto.isDeleted = true;
            producto.SyncStatus = UI.Shared.Offline.SyncStatus.PendingDelete;

            await _localStore.UpsertAsync(producto, cancellationToken);

            var alreadyHasDelete = operations.Any(x => x.Type == ProductoSyncOperationType.Delete);

            if (!alreadyHasDelete)
            {
                await _syncQueue.EnqueueAsync(new ProductoSyncOperation
                {
                    OperationId = Guid.NewGuid(),
                    ProductoLocalId = localId,
                    Type = ProductoSyncOperationType.Delete,
                    CreatedUtc = DateTime.UtcNow
                });

            }

            try
            {
                await _syncService.SynchronizeAsync(cancellationToken);
            }
            catch (Exception)
            {
                // Queda pendiente
            }
        }

        public Task<ProductoImagenDTO> SubirImagenAsync(int productoId, ProductoImageUpload image, CancellationToken cancellationToken = default)
        {
            return _apiClient.UploadImageAsync(productoId, image, cancellationToken);
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
            foreach (var remote in remoteProducts)
            {
                if (!remote.ProductoId.HasValue)
                {
                    continue;
                }

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
                 * que todavía están pendientes de enviarse.
                 */
                if (local is not null &&
                    (local.SyncStatus ==
                        UI.Shared.Offline.SyncStatus.PendingUpdate ||
                     local.SyncStatus ==
                        UI.Shared.Offline.SyncStatus.PendingDelete))
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

                local.SyncStatus = UI.Shared.Offline.SyncStatus.Synced;

                local.isDeleted = false;

                local.LastModifiedUtc = DateTime.UtcNow;

                await _localStore.UpsertAsync(local,cancellationToken);
            }
        }

        private async Task EnqueueUpdateIfNeededAsync(ProductoLocal producto, CancellationToken cancellationToken)
        {
            var pending = await _syncQueue.GetPendingAsync(cancellationToken);

            var hasCreate = pending.Any(x => x.ProductoLocalId == producto.LocalId && x.Type == ProductoSyncOperationType.Create);

            if (hasCreate)
            {
                return;
            }

            var hasUpdate = pending.Any(x => x.ProductoLocalId == producto.LocalId && x.Type == ProductoSyncOperationType.Update);

            if (hasUpdate) return;

            await _syncQueue.EnqueueAsync(new ProductoSyncOperation
            {
                OperationId = Guid.NewGuid(),
                ProductoLocalId = producto.LocalId,
                Type = ProductoSyncOperationType.Update,
                CreatedUtc = DateTime.UtcNow
            }, cancellationToken);
        }

        public async Task<ProductosDto?> ObtenerPorLocalIdAsync(Guid localId, CancellationToken cancellationToken = default)
        {
            var producto = await _localStore.GetByLocalIdAsync(localId, cancellationToken);
            if (producto is null || producto.isDeleted) { return null; }

            return MapToDto(producto);
        }

        public string ObtenerUrlImagen(string ruta)
        {
            throw new NotImplementedException();
        }
    }
}
