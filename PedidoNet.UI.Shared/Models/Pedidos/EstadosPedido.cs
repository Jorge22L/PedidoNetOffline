namespace PedidoNet.UI.Shared.Models.Pedidos;

/// <summary>Debe coincidir con Domain.Constantes.EstadosPedido de la API.</summary>
public static class EstadosPedido
{
    public const string Pendiente = "Pendiente";
    public const string Completado = "Completado";
    public const string Cancelado = "Cancelado";

    public static readonly string[] Todos = [Pendiente, Completado, Cancelado];

    public static string BadgeCss(string? estado) => estado switch
    {
        Pendiente => "text-bg-warning",
        Completado => "text-bg-success",
        Cancelado => "text-bg-secondary",
        _ => "text-bg-light"
    };
}

/// <summary>Valores válidos según la entidad Pedido de la API.</summary>
public static class FormasPago
{
    public const string Contado = "Contado";

    public static readonly string[] Todas = [Contado, "Crédito", "Transferencia", "Tarjeta"];
}

/// <summary>
/// Solo para la vista previa de totales en el formulario.
/// El cálculo definitivo lo hace el procedimiento almacenado.
/// Debe coincidir con Domain.Constantes.Impuestos.TasaIva.
/// </summary>
public static class ImpuestosPedido
{
    public const decimal TasaIva = 0.15m;
}
