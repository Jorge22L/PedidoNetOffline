using PedidoNet.UI.Shared.Api;
using PedidoNet.UI.Shared.Models.Pedidos;
using System.Net;
using System.Net.Http.Json;

namespace PedidoNet.UI.Shared.Pedidos;

/*
 * Debe recibir el HttpClient AUTENTICADO.
 *
 * Todas las escrituras de la API pasan por procedimientos almacenados
 * (GuardarPedido, ActualizarPedido, CompletarPedido, CancelarPedido,
 * EliminarPedido). Sus validaciones llegan como 404/422 con { message },
 * que ApiHttp convierte en ApiException con ese mensaje.
 */
public sealed class PedidosApiClient
{
    private const string BaseUrl = "api/v1/Pedidos";

    private readonly HttpClient _httpClient;

    public PedidosApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PedidoDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(BaseUrl, cancellationToken);

        return await ApiHttp.ReadDataAsync<List<PedidoDto>>(response, cancellationToken) ?? [];
    }

    public async Task<List<PedidoDto>> GetByClienteAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{BaseUrl}/cliente/{clienteId}", cancellationToken);

        return await ApiHttp.ReadDataAsync<List<PedidoDto>>(response, cancellationToken) ?? [];
    }

    public async Task<PedidoDto?> GetByIdAsync(int pedidoId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{BaseUrl}/{pedidoId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        return await ApiHttp.ReadDataAsync<PedidoDto>(response, cancellationToken);
    }

    public async Task<int> CreateAsync(CrearPedidoRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(BaseUrl, request, cancellationToken);

        var creado = await ApiHttp.ReadDataAsync<PedidoCreadoDto>(response, cancellationToken);

        return creado?.Id ?? throw new InvalidOperationException(
            "La API creó el pedido, pero no devolvió su identificador.");
    }

    public async Task UpdateAsync(int pedidoId, ActualizarPedidoRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{pedidoId}", request, cancellationToken);

        await ApiHttp.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(int pedidoId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{BaseUrl}/{pedidoId}", cancellationToken);

        await ApiHttp.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task CompletarAsync(int pedidoId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PatchAsync($"{BaseUrl}/{pedidoId}/completar", content: null, cancellationToken);

        await ApiHttp.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task CancelarAsync(int pedidoId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PatchAsync($"{BaseUrl}/{pedidoId}/cancelar", content: null, cancellationToken);

        await ApiHttp.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task<PedidoEstadisticasDto?> GetEstadisticasAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{BaseUrl}/estadisticas", cancellationToken);

        return await ApiHttp.ReadDataAsync<PedidoEstadisticasDto>(response, cancellationToken);
    }
}
