using PedidoNet.UI.Shared.Api;
using PedidoNet.UI.Shared.Models.Clientes;
using System.Net;
using System.Net.Http.Json;

namespace PedidoNet.UI.Shared.Clientes;

/*
 * Debe recibir el HttpClient AUTENTICADO
 * (el que pasa por AuthenticatedHttpHandler).
 */
public sealed class ClientesApiClient
{
    private const string BaseUrl = "api/v1/Clientes";

    private readonly HttpClient _httpClient;

    public ClientesApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ClienteDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(BaseUrl, cancellationToken);

        await ApiHttp.EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<List<ClienteDto>>(
            cancellationToken: cancellationToken) ?? [];
    }

    public async Task<ClienteDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{BaseUrl}/{id}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await ApiHttp.EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<ClienteDto>(
            cancellationToken: cancellationToken);
    }

    public async Task CreateAsync(GuardarClienteRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(BaseUrl, request, cancellationToken);

        await ApiHttp.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task UpdateAsync(int id, GuardarClienteRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request, cancellationToken);

        await ApiHttp.EnsureSuccessAsync(response, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}", cancellationToken);

        await ApiHttp.EnsureSuccessAsync(response, cancellationToken);
    }
}
