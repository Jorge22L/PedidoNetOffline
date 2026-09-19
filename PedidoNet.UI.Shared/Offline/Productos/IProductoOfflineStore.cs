using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Offline.Productos
{
    public interface IProductoOfflineStore
    {
        Task<List<ProductoLocal>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<ProductoLocal?> GetByLocalIdAsync(Guid localId, CancellationToken cancellationToken = default);
        Task<ProductoLocal?> GetByserverIdAsync(int productoId, CancellationToken cancellationToken = default);
        Task UpsertAsync(ProductoLocal producto, CancellationToken cancellationToken = default);
        Task UpsertRangeAsync(IEnumerable<ProductoLocal> productos, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid localId, CancellationToken cancellationToken= default);
        Task ClearAsync(CancellationToken cancellationToken= default);
    }
}
