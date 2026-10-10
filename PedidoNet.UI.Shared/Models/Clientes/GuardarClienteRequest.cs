namespace PedidoNet.UI.Shared.Models.Clientes;

/// <summary>
/// Cuerpo de POST y PUT /api/v1/Clientes
/// (CrearClienteCommand / ActualizarClienteCommand tienen los mismos campos).
/// </summary>
public class GuardarClienteRequest
{
    public string Nombre { get; set; } = string.Empty;

    public string? Cedula { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public bool EsConsumidorFinal { get; set; }
}
