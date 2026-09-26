using PedidoNet.UI.Shared.Models.Productos;
using PedidoNet.UI.Shared.Offline;

namespace PedidoNet.Web.Models.Productos
{
    public class ProductosDto
    {
        public Guid LocalId { get; set; }
        public Guid? ClientId { get; set; }
        public int? ProductoId { get; set; }

        public string? Codigo { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal PrecioVenta { get; set; }

        public int Existencias { get; set; }

        public bool? TieneIVA { get; set; }

        public bool? TieneISC { get; set; }
        public List<ProductoImagenDTO> Imagenes { get; set; } = [];

        public SyncStatus SyncStatus { get; set; }
        public bool PendingSync => SyncStatus != SyncStatus.Synced;
    }
}
