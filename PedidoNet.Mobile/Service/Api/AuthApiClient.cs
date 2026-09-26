using PedidoNet.Mobile.Models.Auth;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PedidoNet.Mobile.Service.Api
{
    public sealed class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("PedidoNetApi");
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/Auth/login", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            using var jsonDocument = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);

            var root = jsonDocument.RootElement;

            if(!root.TryGetProperty("data", out var dataElement))
            {
                return null;
            }

            return dataElement.Deserialize<LoginResponse>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
