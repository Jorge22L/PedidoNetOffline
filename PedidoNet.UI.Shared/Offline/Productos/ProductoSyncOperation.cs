using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Offline.Productos
{
    public sealed class ProductoSyncOperation
    {
        public Guid OperationId { get; set; }
        public Guid ProductoLocalId { get; set; }
        public ProductoSyncOperationType Type { get; set; }
        public DateTime CreatedUtc { get; set; }
        public int RetryCount { get; set; }
        public string? LastError { get; set; }
    }
}
