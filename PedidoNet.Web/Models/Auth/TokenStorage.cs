using Microsoft.JSInterop;
using System.Text.Json;

namespace PedidoNet.Web.Models.Auth
{
    public class TokenStorage : ITokenStorage
    {
        private const string SessionKey = "pedidonet_session";
        private readonly IJSRuntime _js;

        public TokenStorage(IJSRuntime js)
        {
            _js = js;
        }

        public async Task ClearAsync()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", SessionKey);
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            var session = await GetAsync();

            return session?.AccessToken;
        }

        public async Task<LoginResponse?> GetAsync()
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", SessionKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<LoginResponse>(json);
        }

        public async Task SaveAsync(LoginResponse session)
        {
            var json = JsonSerializer.Serialize(session);

            await _js.InvokeVoidAsync("localStorage.setItem", SessionKey, json);
        }
    }
}
