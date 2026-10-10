using System.ComponentModel.DataAnnotations;

namespace PedidoNet.UI.Shared.Models.Pedidos;

public class PedidoFormModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un cliente.")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "Selecciona la forma de pago.")]
    [StringLength(20)]
    public string FormaPago { get; set; } = FormasPago.Contado;

    public List<DetallePedidoFormModel> Detalles { get; set; } = [];

    public decimal SubTotal => Detalles.Sum(x => x.SubtotalLinea);

    public decimal IVA => Detalles.Sum(x => x.IVA);

    public decimal Total => SubTotal + IVA;

    /// <summary>
    /// Validaciones que DataAnnotations no cubre (colección de líneas).
    /// Replican las reglas del dominio; la API vuelve a validarlas.
    /// </summary>
    public List<string> ValidarDetalles()
    {
        var errores = new List<string>();

        if (Detalles.Count == 0)
        {
            errores.Add("Agrega al menos un producto al pedido.");
            return errores;
        }

        foreach (var linea in Detalles)
        {
            if (linea.Cantidad <= 0)
                errores.Add($"{linea.ProductoNombre}: la cantidad debe ser mayor que cero.");

            if (linea.Descuento < 0)
                errores.Add($"{linea.ProductoNombre}: el descuento no puede ser negativo.");

            if (linea.Descuento > linea.ImporteBruto)
                errores.Add($"{linea.ProductoNombre}: el descuento no puede ser mayor que el importe de la línea.");
        }

        if (Detalles.GroupBy(x => x.ProductoId).Any(g => g.Count() > 1))
            errores.Add("No se permite repetir un producto.");

        return errores;
    }

    public CrearPedidoRequest ToCrearRequest() => new()
    {
        ClienteId = ClienteId,
        FormaPago = FormaPago,
        Detalles = Detalles.Select(x => x.ToRequest()).ToList()
    };

    public ActualizarPedidoRequest ToActualizarRequest() => new()
    {
        ClienteId = ClienteId,
        FormaPago = FormaPago,
        Detalles = Detalles.Select(x => x.ToRequest()).ToList()
    };
}

public class DetallePedidoFormModel
{
    public int ProductoId { get; set; }

    public string? ProductoCodigo { get; set; }

    public string ProductoNombre { get; set; } = string.Empty;

    /// <summary>Precio para la vista previa (la API usa el precio vigente del producto).</summary>
    public decimal PrecioUnitario { get; set; }

    public bool TieneIVA { get; set; }

    /// <summary>Existencias conocidas al armar el pedido (solo informativo).</summary>
    public int? Existencias { get; set; }

    public int Cantidad { get; set; } = 1;

    public decimal Descuento { get; set; }

    public decimal ImporteBruto => Cantidad * PrecioUnitario;

    public decimal SubtotalLinea => ImporteBruto - Descuento;

    public decimal IVA => TieneIVA ? Math.Round(SubtotalLinea * ImpuestosPedido.TasaIva, 2) : 0;

    public decimal TotalLinea => SubtotalLinea + IVA;

    public DetallePedidoRequest ToRequest() => new()
    {
        ProductoId = ProductoId,
        Cantidad = Cantidad,
        Descuento = Descuento
    };
}
