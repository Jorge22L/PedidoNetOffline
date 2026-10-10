using PedidoNet.UI.Shared.Models.Clientes;

namespace PedidoNet.UI.Shared.Clientes;

/// <summary>
/// Clientes: operaciones en línea contra la API (sin almacenamiento offline por ahora).
/// </summary>
public interface IClienteService
{
    Task<List<ClienteDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);

    Task<ClienteDto?> ObtenerPorIdAsync(int clienteId, CancellationToken cancellationToken = default);

    Task CrearAsync(GuardarClienteRequest request, CancellationToken cancellationToken = default);

    Task ActualizarAsync(int clienteId, GuardarClienteRequest request, CancellationToken cancellationToken = default);

    Task EliminarAsync(int clienteId, CancellationToken cancellationToken = default);
}
