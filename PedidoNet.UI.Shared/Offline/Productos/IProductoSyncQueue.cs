using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Offline.Productos
{
    public interface IProductoSyncQueue
    {
        Task<List<ProductoSyncOperation>> GetPendingAsync(CancellationToken cancellationToken = default);
        Task EnqueueAsync(ProductoSyncOperation operation, CancellationToken cancellationToken = default);
        Task RemoveAsync(Guid operationId, CancellationToken cancellationToken = default);
        Task UpdateAsync(ProductoSyncOperation operation, CancellationToken cancellationToken = default);
    }
}
