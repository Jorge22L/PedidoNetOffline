using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Offline.Productos
{
    public sealed class ProductoLocal
    {
        public Guid LocalId { get; set; }
        public int? ProductoId { get; set; }
        public string? Codigo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal PrecioVenta { get; set; }
        public int Existencias { get; set; }
        public bool? TieneIVA { get; set; }
        public bool? TieneISC { get; set; }
        public SyncStatus SyncStatus { get; set; }
        public bool isDeleted { get; set; }
        public DateTime LastModifiedUtc { get; set; }

    }
}
