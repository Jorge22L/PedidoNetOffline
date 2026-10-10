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

    public static ClienteFormModel Desde(ClienteDto cliente) => new()
    {
        Nombre = cliente.Nombre,
        Cedula = cliente.Cedula,
        Telefono = cliente.Telefono,
        Direccion = cliente.Direccion,
        EsConsumidorFinal = cliente.EsConsumidorFinal
    };

    public GuardarClienteRequest ToRequest() => new()
    {
        Nombre = Nombre.Trim(),
        Cedula = string.IsNullOrWhiteSpace(Cedula) ? null : Cedula.Trim(),
        Telefono = string.IsNullOrWhiteSpace(Telefono) ? null : Telefono.Trim(),
        Direccion = string.IsNullOrWhiteSpace(Direccion) ? null : Direccion.Trim(),
        EsConsumidorFinal = EsConsumidorFinal
    };
}
