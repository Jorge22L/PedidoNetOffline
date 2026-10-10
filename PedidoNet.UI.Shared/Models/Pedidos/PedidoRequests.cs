namespace PedidoNet.UI.Shared.Models.Pedidos;

/// <summary>Línea enviada al procedimiento (se serializa como @DetallesJson).</summary>
public class DetallePedidoRequest
{
    public int ProductoId { get; set; }

    public int Cantidad { get; set; }

    public decimal Descuento { get; set; }
}

/// <summary>POST /api/v1/Pedidos → dbo.GuardarPedido</summary>
public class CrearPedidoRequest
{
    public int ClienteId { get; set; }

    public string FormaPago { get; set; } = FormasPago.Contado;

    public List<DetallePedidoRequest> Detalles { get; set; } = [];
}

/// <summary>PUT /api/v1/Pedidos/{id} → dbo.ActualizarPedido</summary>
public class ActualizarPedidoRequest
{
    public int? ClienteId { get; set; }

    public string? FormaPago { get; set; }

    public List<DetallePedidoRequest>? Detalles { get; set; }
}

/// <summary>data de la respuesta de POST /api/v1/Pedidos.</summary>
public class PedidoCreadoDto
{
    public int Id { get; set; }
}

/// <summary>data de GET /api/v1/Pedidos/estadisticas.</summary>
public class PedidoEstadisticasDto
{
    public int TotalPedidos { get; set; }

    public int PedidosPendientes { get; set; }

    public int PedidosCompletados { get; set; }

    public int PedidosCancelados { get; set; }

    public decimal MontoTotalVentas { get; set; }

    public decimal PromedioVentaPorPedido { get; set; }
}
