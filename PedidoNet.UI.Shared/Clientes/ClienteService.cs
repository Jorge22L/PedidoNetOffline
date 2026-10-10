using PedidoNet.UI.Shared.Models.Clientes;

namespace PedidoNet.UI.Shared.Clientes;

public sealed class ClienteService : IClienteService
{
    private readonly ClientesApiClient _apiClient;

    public ClienteService(ClientesApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<ClienteDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var clientes = await _apiClient.GetAllAsync(cancellationToken);

        return clientes
            .OrderBy(x => x.Nombre)
            .ToList();
    }

    public Task<ClienteDto?> ObtenerPorIdAsync(int clienteId, CancellationToken cancellationToken = default)
        => _apiClient.GetByIdAsync(clienteId, cancellationToken);

    public Task CrearAsync(GuardarClienteRequest request, CancellationToken cancellationToken = default)
        => _apiClient.CreateAsync(request, cancellationToken);

    public Task ActualizarAsync(int clienteId, GuardarClienteRequest request, CancellationToken cancellationToken = default)
        => _apiClient.UpdateAsync(clienteId, request, cancellationToken);

    public Task EliminarAsync(int clienteId, CancellationToken cancellationToken = default)
        => _apiClient.DeleteAsync(clienteId, cancellationToken);
}
