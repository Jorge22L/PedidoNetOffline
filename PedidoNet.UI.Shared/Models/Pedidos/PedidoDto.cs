namespace PedidoNet.UI.Shared.Models.Pedidos;

public class PedidoDto
{
    public int PedidoId { get; set; }

    public int ClienteId { get; set; }

    public string ClienteNombre { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    public decimal SubTotal { get; set; }

    public decimal IVA { get; set; }

    public decimal Descuento { get; set; }

    public decimal Total { get; set; }

    public string FormaPago { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public List<DetallePedidoDto> Detalles { get; set; } = [];

    public bool EsPendiente => Estado == EstadosPedido.Pendiente;
}
