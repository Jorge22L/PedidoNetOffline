using PedidoNet.UI.Shared.Models.Pedidos;

namespace PedidoNet.UI.Shared.Pedidos;

public sealed class PedidoService : IPedidoService
{
    private readonly PedidosApiClient _apiClient;

    public PedidoService(PedidosApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<PedidoDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
        => Ordenar(await _apiClient.GetAllAsync(cancellationToken));

    public async Task<List<PedidoDto>> ObtenerPorClienteAsync(int clienteId, CancellationToken cancellationToken = default)
        => Ordenar(await _apiClient.GetByClienteAsync(clienteId, cancellationToken));

    public Task<PedidoDto?> ObtenerPorIdAsync(int pedidoId, CancellationToken cancellationToken = default)
        => _apiClient.GetByIdAsync(pedidoId, cancellationToken);

    public Task<PedidoEstadisticasDto?> ObtenerEstadisticasAsync(CancellationToken cancellationToken = default)
        => _apiClient.GetEstadisticasAsync(cancellationToken);

    public Task<int> CrearAsync(CrearPedidoRequest request, CancellationToken cancellationToken = default)
        => _apiClient.CreateAsync(request, cancellationToken);

    public Task ActualizarAsync(int pedidoId, ActualizarPedidoRequest request, CancellationToken cancellationToken = default)
        => _apiClient.UpdateAsync(pedidoId, request, cancellationToken);

    public Task EliminarAsync(int pedidoId, CancellationToken cancellationToken = default)
        => _apiClient.DeleteAsync(pedidoId, cancellationToken);

    public Task CompletarAsync(int pedidoId, CancellationToken cancellationToken = default)
        => _apiClient.CompletarAsync(pedidoId, cancellationToken);

    public Task CancelarAsync(int pedidoId, CancellationToken cancellationToken = default)
        => _apiClient.CancelarAsync(pedidoId, cancellationToken);

    // Más recientes primero.
    private static List<PedidoDto> Ordenar(List<PedidoDto> pedidos)
        => pedidos
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.PedidoId)
            .ToList();
}
