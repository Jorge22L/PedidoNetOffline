using PedidoNet.UI.Shared.Models.Pedidos;

namespace PedidoNet.UI.Shared.Pedidos;

/// <summary>
/// Pedidos: operaciones en línea. Requieren conexión porque la API
/// valida y descuenta existencias en el procedimiento almacenado.
/// </summary>
public interface IPedidoService
{
    Task<List<PedidoDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);

    Task<List<PedidoDto>> ObtenerPorClienteAsync(int clienteId, CancellationToken cancellationToken = default);

    Task<PedidoDto?> ObtenerPorIdAsync(int pedidoId, CancellationToken cancellationToken = default);

    Task<PedidoEstadisticasDto?> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default);

    /// <returns>Id del pedido creado.</returns>
    Task<int> CrearAsync(CrearPedidoRequest request, CancellationToken cancellationToken = default);

    Task ActualizarAsync(int pedidoId, ActualizarPedidoRequest request, CancellationToken cancellationToken = default);

    Task EliminarAsync(int pedidoId, CancellationToken cancellationToken = default);

    Task CompletarAsync(int pedidoId, CancellationToken cancellationToken = default);

    Task CancelarAsync(int pedidoId, CancellationToken cancellationToken = default);
}
