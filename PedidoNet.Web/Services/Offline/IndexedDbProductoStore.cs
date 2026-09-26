using Microsoft.JSInterop;
using PedidoNet.UI.Shared.Offline.Productos;

namespace PedidoNet.Web.Services.Offline
{
    public sealed class IndexedDbProductoStore : IProductoOfflineStore
    {
        private readonly IJSRuntime _js;

        public IndexedDbProductoStore(IJSRuntime js)
        {
            _js = js;
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            return _js.InvokeVoidAsync("productoDb.clearProductos", cancellationToken).AsTask();
        }

        public Task DeleteAsync(Guid localId, CancellationToken cancellationToken = default)
        {
            return _js.InvokeVoidAsync("productoDb.deleteProducto", cancellationToken, localId.ToString()).AsTask();
        }

        public async Task<List<ProductoLocal>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _js.InvokeAsync<List<ProductoLocal>>("productoDb.getProductos", cancellationToken) ?? [];
        }

        public async Task<ProductoLocal?> GetByLocalIdAsync(Guid localId, CancellationToken cancellationToken = default)
        {
            return await _js.InvokeAsync<ProductoLocal?>("productoDb.getProducto", cancellationToken, localId.ToString());
        }

        public async Task<ProductoLocal?> GetByserverIdAsync(int productoId, CancellationToken cancellationToken = default)
        {
            var productos = await GetAllAsync(cancellationToken);

            return productos.FirstOrDefault(x => x.ProductoId == productoId);
        }

        public Task UpsertAsync(ProductoLocal producto, CancellationToken cancellationToken = default)
        {
            return _js.InvokeVoidAsync("productoDb.putProducto", cancellationToken, producto).AsTask();
        }

        public async Task UpsertRangeAsync(IEnumerable<ProductoLocal> productos, CancellationToken cancellationToken = default)
        {
            foreach(var producto in productos)
            {
                await UpsertAsync(producto, cancellationToken);
            }
        }
    }
}
