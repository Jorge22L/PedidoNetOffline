using System.ComponentModel.DataAnnotations;

namespace PedidoNet.UI.Shared.Models.Clientes;

public class ClienteFormModel
{
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "La cédula no puede exceder 50 caracteres.")]
    public string? Cedula { get; set; }

    [StringLength(30, ErrorMessage = "El teléfono no puede exceder 30 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(250, ErrorMessage = "La dirección no puede exceder 250 caracteres.")]
    public string? Direccion { get; set; }

    public bool EsConsumidorFinal { get; set; } = true;

    [Range(-90d, 90d, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
    public decimal? Latitud { get; set; }

    [Range(-180d, 180d, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
    public decimal? Longitud { get; set; }

    public double? PrecisionMetros { get; set; }

    public bool TieneUbicacion => Latitud.HasValue && Longitud.HasValue;

    public static ClienteFormModel Desde(ClienteDto cliente) => new()
    {
        Nombre = cliente.Nombre,
        Cedula = cliente.Cedula,
        Telefono = cliente.Telefono,
        Direccion = cliente.Direccion,
        EsConsumidorFinal = cliente.EsConsumidorFinal,
        Latitud = cliente.Latitud,
        Longitud = cliente.Longitud,
    };

    public GuardarClienteRequest ToRequest() => new()
    {
        Nombre = Nombre.Trim(),
        Cedula = string.IsNullOrWhiteSpace(Cedula) ? null : Cedula.Trim(),
        Telefono = string.IsNullOrWhiteSpace(Telefono) ? null : Telefono.Trim(),
        Direccion = string.IsNullOrWhiteSpace(Direccion) ? null : Direccion.Trim(),
        EsConsumidorFinal = EsConsumidorFinal,
        Latitud = Latitud,
        Longitud = Longitud
    };
}
