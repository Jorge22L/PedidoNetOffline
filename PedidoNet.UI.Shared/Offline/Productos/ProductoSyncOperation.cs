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

        /// <summary>
        /// La operación falló con un error permanente (400, 403, 409, validación...).
        /// No se reintenta automáticamente hasta que el usuario edite o elimine el producto.
        /// </summary>
        public bool RequiresAttention { get; set; }
    }
}
