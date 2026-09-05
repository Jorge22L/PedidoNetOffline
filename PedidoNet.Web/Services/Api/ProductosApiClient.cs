using PedidoNet.Web.Models.Auth;
using PedidoNet.Web.Models.Productos;
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
    }
}
