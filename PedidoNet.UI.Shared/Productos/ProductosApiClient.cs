using PedidoNet.UI.Shared.Models.Productos;
using System.Net;
using System.Net.Http.Json;

namespace PedidoNet.UI.Shared.Productos;

public sealed class ProductosApiClient
{
    private readonly HttpClient _httpClient;

    public ProductosApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ProductosDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            "api/v1/Producto",
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<List<ProductosDto>>(
                cancellationToken: cancellationToken)
            ?? [];
    }

    public async Task<ProductosDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"api/v1/Producto/{id}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ProductosDto>(
                cancellationToken: cancellationToken);
    }

    public async Task<ProductosDto> CreateAsync(
        CrearProductoRequest model,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/v1/Producto",
            model,
            cancellationToken);

        await EnsureSuccessAsync(response);

        var producto = await response.Content
            .ReadFromJsonAsync<ProductosDto>(
                cancellationToken: cancellationToken);

        return producto ??
            throw new InvalidOperationException(
                "La API creó el producto, pero no devolvió el recurso.");
    }

    public async Task UpdateAsync(
        int id,
        ActualizarProductoRequest model,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync(
            $"api/v1/Producto/{id}",
            model,
            cancellationToken);

        await EnsureSuccessAsync(response);
    }

    public async Task DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(
            $"api/v1/Producto/{id}",
            cancellationToken);

        await EnsureSuccessAsync(response);
    }

    public async Task<ProductoImagenDTO> UploadImageAsync(
        int productoId,
        ProductoImageUpload image,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();

        using var streamContent =
            new StreamContent(image.Stream);

        streamContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(
                image.ContentType);

        content.Add(
            streamContent,
            "imagen",
            image.FileName);

        content.Add(
            new StringContent(
                image.EsPrincipal.ToString()),
            "esPrincipal");

        using var response = await _httpClient.PostAsync(
            $"api/v1/Producto/{productoId}/imagenes",
            content,
            cancellationToken);

        await EnsureSuccessAsync(response);

        var result = await response.Content
            .ReadFromJsonAsync<ProductoImagenDTO>(
                cancellationToken: cancellationToken);

        return result ??
            throw new InvalidOperationException(
                "La API no devolvió información de la imagen.");
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content =
            await response.Content.ReadAsStringAsync();

        throw new HttpRequestException(
            $"API respondió {(int)response.StatusCode} " +
            $"{response.StatusCode}. Respuesta: {content}",
            inner: null,
            response.StatusCode);
    }
}