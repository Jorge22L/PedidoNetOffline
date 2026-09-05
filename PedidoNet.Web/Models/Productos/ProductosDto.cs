namespace PedidoNet.Web.Models.Productos
{
    public class ProductosDto
    {
        public int ProductoId { get; set; }

        public string? Codigo { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public decimal PrecioVenta { get; set; }

        public int Existencias { get; set; }

        public bool? TieneIVA { get; set; }

        public bool? TieneISC { get; set; }
    }
}
