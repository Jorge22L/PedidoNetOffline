using System.ComponentModel.DataAnnotations;

namespace PedidoNet.Web.Models.Productos
{
    public class ProductoFormModel
    {
        [Required(ErrorMessage = "El código es requerido.")]
        [StringLength(
        20,
        ErrorMessage = "El código no puede exceder 20 caracteres.")]
        public string? Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(
            100,
            ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Range(
            typeof(decimal),
            "0.01",
            "999999999",
            ErrorMessage = "El precio debe ser mayor a 0.",
            ParseLimitsInInvariantCulture = true)]
        public decimal PrecioVenta { get; set; }

        [Range(
            0,
            int.MaxValue,
            ErrorMessage = "Las existencias no pueden ser negativas.")]
        public int Existencias { get; set; }

        public bool TieneIVA { get; set; }

        public bool TieneISC { get; set; }
    }
}
