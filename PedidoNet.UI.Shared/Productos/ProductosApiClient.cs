using PedidoNet.UI.Shared.Models.Productos;
using System.Net;
using System.Net.Http.Json;

namespace PedidoNet.UI.Shared.Productos;

/*
 * Debe recibir el HttpClient AUTENTICADO
 * (el que pasa por AuthenticatedHttpHandler).
 */
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

        await EnsureSuccessAsync(response);

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

        await EnsureSuccessAsync(response);

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

    public async Task DeleteImageAsync(
        int productoId,
        int productoImagenId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync(
            $"api/v1/Producto/{productoId}/imagenes/{productoImagenId}",
            cancellationToken);

        await EnsureSuccessAsync(response);
    }

    /// <summary>
    /// La API devuelve rutas relativas (/uploads/productos/...).
    /// Se resuelven contra la BaseAddress configurada en el host.
    /// </summary>
    public string BuildImageUrl(string ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(ruta, UriKind.Absolute, out var absolute) &&
            (absolute.Scheme == Uri.UriSchemeHttp ||
             absolute.Scheme == Uri.UriSchemeHttps))
        {
            return absolute.ToString();
        }

        if (_httpClient.BaseAddress is null)
        {
            return ruta;
        }

        return new Uri(
            _httpClient.BaseAddress,
            ruta.TrimStart('/')).ToString();
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        /*
         * AuthenticatedHttpHandler ya intentó un refresh y
         * reintentó una vez. Si aun así llega 401, la sesión
         * no es válida: se usa la misma excepción que lanza
         * el handler para que la UI redirija a login.
         */
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "La sesión ya no es válida.");
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
