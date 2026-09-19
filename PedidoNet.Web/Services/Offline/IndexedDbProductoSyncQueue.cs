using Microsoft.JSInterop;
using PedidoNet.UI.Shared.Offline.Productos;

namespace PedidoNet.Web.Services.Offline
{
    public sealed class IndexedDbProductoSyncQueue : IProductoSyncQueue
    {
        private readonly IJSRuntime _js;

        public IndexedDbProductoSyncQueue(IJSRuntime js)
        {
            _js = js;
        }

        public Task EnqueueAsync(ProductoSyncOperation operation, CancellationToken cancellationToken = default)
        {
            return SaveAsync(operation, cancellationToken);
        }

        public async Task<List<ProductoSyncOperation>> GetPendingAsync(CancellationToken cancellationToken = default)
        {
            var result = await _js.InvokeAsync<List<ProductoSyncOperation>>("productoDb.getSyncOperations", cancellationToken);

            return result
                .OrderBy(x => x.CreatedUtc)
                .ToList();
        }

        public Task RemoveAsync(Guid operationId, CancellationToken cancellationToken = default)
        {
            return _js.InvokeVoidAsync("productoDb.deleteSyncOperation", cancellationToken, operationId.ToString()).AsTask();
        }

        public Task UpdateAsync(ProductoSyncOperation operation, CancellationToken cancellationToken = default)
        {
            return SaveAsync(operation, cancellationToken);
        }

        private Task SaveAsync(ProductoSyncOperation operation, CancellationToken cancellationToken)
        {
            return _js.InvokeVoidAsync("productoDb.putSyncOperation", cancellationToken, operation).AsTask();
        }
    }
}
