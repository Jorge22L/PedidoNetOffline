namespace PedidoNet.UI.Shared.Models.Clientes;

public class ClienteDto
{
    public int ClienteId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? Cedula { get; set; }

    public string? Telefono { get; set; }

    public string? Direccion { get; set; }

    public bool EsConsumidorFinal { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public bool TieneUbicacion => Latitud.HasValue && Longitud.HasValue;
}
