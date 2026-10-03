using PedidoNet.UI.Shared.Api;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace PedidoNet.UI.Shared.Auth
{
    /*
     * IMPORTANTE:
     * Este cliente debe recibir un HttpClient SIN
     * AuthenticatedHttpHandler. El handler depende de
     * AuthService -> AuthApiClient, por lo que usar el
     * cliente autenticado aquí provocaría un refresh recursivo.
     */
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

            return await ReadResponseAsync(
                httpResponse,
                cancellationToken);
        }

        public async Task<ApiResponse<LoginResponse>?> RefreshAsync(RefreshTokenRequest request,CancellationToken cancellationToken = default)
        {
            using var httpResponse = await _httpClient.PostAsJsonAsync(
                "api/v1/Auth/refresh",
                request,
                cancellationToken);

            return await ReadResponseAsync(
                httpResponse,
                cancellationToken);
        }

        public async Task RevokeAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            using var httpResponse = await _httpClient.PostAsJsonAsync(
                "api/v1/Auth/revoke",
                request,
                cancellationToken);
        }

        private static async Task<ApiResponse<LoginResponse>?> ReadResponseAsync(
            HttpResponseMessage httpResponse,
            CancellationToken cancellationToken)
        {
            /*
             * Errores transitorios (rate limit, API caída, gateway):
             * se lanzan como HttpRequestException para que quien llama
             * NO los interprete como "credenciales inválidas" y no
             * borre la sesión almacenada.
             */
            if (IsTransient(httpResponse.StatusCode))
            {
                throw new HttpRequestException(
                    $"API respondió {(int)httpResponse.StatusCode} " +
                    $"{httpResponse.StatusCode} en autenticación.",
                    inner: null,
                    httpResponse.StatusCode);
            }

            try
            {
                return await httpResponse.Content
                    .ReadFromJsonAsync<ApiResponse<LoginResponse>>(
                        cancellationToken: cancellationToken);
            }
            catch (JsonException)
            {
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = $"Respuesta inválida del servidor ({(int)httpResponse.StatusCode})."
                };
            }
        }

        private static bool IsTransient(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.RequestTimeout ||
                   statusCode == HttpStatusCode.TooManyRequests ||
                   (int)statusCode >= 500;
        }
    }
}
