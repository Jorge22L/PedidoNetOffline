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

        public async Task<List<ProductosDto>> GetAllSync()
        {
            var token = await _tokenStorage.GetAccessTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new UnauthorizedAccessException();
            }

            using var request = new HttpRequestMessage(HttpMethod.Get, "api/Producto");

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            using var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<ProductosDto>>() ?? [];
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
        public async Task<List<ProductosDto>> GetAllAsync()
        {
            using var request =
                await CreateRequestAsync(
                    HttpMethod.Get,
                    "api/Producto");

            using var response =
                await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            return await response.Content
                       .ReadFromJsonAsync<List<ProductosDto>>()
                   ?? [];
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
        public async Task CreateAsync(
            CrearProductoRequest model)
        {
            using var request =
                await CreateRequestAsync(
                    HttpMethod.Post,
                    "api/Producto");

            request.Content = JsonContent.Create(model);

            using var response =
                await _httpClient.SendAsync(request);

            await EnsureSuccessAsync(response);
        }

        // ACTUALIZAR
        public async Task UpdateAsync(
            int id,
            ActualizarProductoRequest model)
        {
            using var request =
                await CreateRequestAsync(
                    HttpMethod.Put,
                    $"api/Producto/{id}");

            request.Content = JsonContent.Create(model);

            using var response =
                await _httpClient.SendAsync(request);

            await EnsureSuccessAsync(response);
        }

        // ELIMINAR
        public async Task DeleteAsync(int id)
        {
            using var request =await CreateRequestAsync(
                    HttpMethod.Delete,
                    $"api/Producto/{id}");

            using var response =
                await _httpClient.SendAsync(request);

            await EnsureSuccessAsync(response);
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
                $"{response.StatusCode}. Respuesta: {content}");
        }
    }
}
