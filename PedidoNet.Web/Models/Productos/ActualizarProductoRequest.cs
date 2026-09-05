using System.ComponentModel.DataAnnotations;

namespace PedidoNet.Web.Models.Productos
{
    public class ActualizarProductoRequest
    {
        public string? Codigo { get; set; }

        [Required(ErrorMessage = "Nombre es requerido")]
        [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999999",
            ErrorMessage = "Precio debe ser mayor a 0")]
        public decimal PrecioVenta { get; set; }

        [Range(0, int.MaxValue,
            ErrorMessage = "Existencias no pueden ser negativas")]
        public int Existencias { get; set; }

        public bool? TieneIVA { get; set; }

        public bool? TieneISC { get; set; }
    }
}
