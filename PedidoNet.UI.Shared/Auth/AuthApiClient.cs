using PedidoNet.UI.Shared.Api;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace PedidoNet.UI.Shared.Auth
{
    public sealed class AuthApiClient
    {
        private readonly HttpClient _httpClient;

        public AuthApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<LoginResponse>?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            using var httpResponse = await _httpClient.PostAsJsonAsync(
                "api/v1/Auth/login",
                request,
                cancellationToken);

            var response = await httpResponse.Content
                .ReadFromJsonAsync<ApiResponse<LoginResponse>>(
                    cancellationToken: cancellationToken);

            return response;
        }

        public async Task<ApiResponse<LoginResponse>?> RefreshAsync(RefreshTokenRequest request,CancellationToken cancellationToken = default)
        {
            using var httpResponse = await _httpClient.PostAsJsonAsync(
                "api/v1/Auth/refresh",
                request,
                cancellationToken);

            return await httpResponse.Content
                .ReadFromJsonAsync<ApiResponse<LoginResponse>>(
                    cancellationToken: cancellationToken);
        }
    }
}
