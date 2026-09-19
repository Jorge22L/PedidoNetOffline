using PedidoNet.UI.Shared.Models.Productos;
using PedidoNet.Web.Models.Auth;
using PedidoNet.Web.Models.Productos;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PedidoNet.Web.Services.Api
{
    public class ProductosApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ITokenStorage _tokenStorage;

        public ProductosApiClient(HttpClient httpClient, ITokenStorage tokenStorage)
        {
            _httpClient = httpClient;
            _tokenStorage = tokenStorage;
        }

        private async Task<HttpRequestMessage> CreateRequestAsync(
       HttpMethod method,
       string url)
        {
            var token = await _tokenStorage.GetAccessTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException(
                    "No existe un token de acceso.");
            }

            var request = new HttpRequestMessage(method, url);

            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            return request;
        }

        // LISTAR
        public async Task<List<ProductosDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            using var request = await CreateRequestAsync(HttpMethod.Get,"api/Producto");

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ProductosDto>>(cancellationToken: cancellationToken) ?? [];
        }

        // OBTENER POR ID
        public async Task<ProductosDto?> GetByIdAsync(int id)
        {
            using var request =
                await CreateRequestAsync(
                    HttpMethod.Get,
                    $"api/Producto/{id}");

            using var response =
                await _httpClient.SendAsync(request);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content
                .ReadFromJsonAsync<ProductosDto>();
        }

        // CREAR
        public async Task<ProductosDto> CreateAsync(CrearProductoRequest model, CancellationToken cancellationToken = default)
        {
            using var request = await CreateRequestAsync(HttpMethod.Post,"api/Producto");

            request.Content = JsonContent.Create(model);

            using var response =
                await _httpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessAsync(response);

            var producto = await response.Content.ReadFromJsonAsync<ProductosDto>(cancellationToken: cancellationToken);

            return producto ?? throw new InvalidOperationException("La API creó el producto, pero no devolvió el recurso");
        }

        // ACTUALIZAR
        public async Task UpdateAsync(int id, ActualizarProductoRequest model, CancellationToken cancellationToken = default)
        {
            using var request =await CreateRequestAsync(HttpMethod.Put,$"api/Producto/{id}");

            request.Content = JsonContent.Create(model);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessAsync(response);
        }

        // ELIMINAR
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            using var request = await CreateRequestAsync(HttpMethod.Delete,$"api/Producto/{id}");

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessAsync(response);
        }

        // SUBIR IMAGEN
        public async Task<ProductoImagenDTO> UploadImageAsync(int productoId, ProductoImageUpload image, CancellationToken cancellationToken)
        {
            using var request = await CreateRequestAsync(HttpMethod.Post, $"api/Producto/{productoId}/imagenes");

            using var content = new MultipartFormDataContent();

            using var streamContent = new StreamContent(image.Stream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);

            content.Add(streamContent, "imagen", image.FileName);

            content.Add(new StringContent(image.EsPrincipal.ToString()), "esPrincipal");

            request.Content = content;

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            var result = await response.Content.ReadFromJsonAsync<ProductoImagenDTO>(cancellationToken: cancellationToken);

            return result ?? throw new InvalidOperationException("La API no devolvió información de la imagen");
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var content = await response.Content.ReadAsStringAsync();

            throw new HttpRequestException(
                $"API respondió {(int)response.StatusCode} " +
                $"{response.StatusCode}. Respuesta: {content}",
                inner: null,
                response.StatusCode);
        }
    }
}
