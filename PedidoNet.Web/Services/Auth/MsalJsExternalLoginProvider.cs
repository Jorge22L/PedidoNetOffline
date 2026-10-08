using Microsoft.JSInterop;
using PedidoNet.UI.Shared.Auth;

namespace PedidoNet.Web.Services.Auth
{
    public sealed class EntraWebOptions
    {
        public required string TenantId { get; init; }
        public required string ClientId { get; init; }
        public required string ApiScope { get; init; }
    }

    /// <summary>
    /// IExternalLoginProvider para el navegador: MSAL.js con popup.
    /// </summary>
    public sealed class MsalJsExternalLoginProvider : IExternalLoginProvider, IAsyncDisposable
    {
        private readonly IJSRuntime _js;
        private readonly EntraWebOptions _options;

        private IJSObjectReference? _module;

        public MsalJsExternalLoginProvider(IJSRuntime js, EntraWebOptions options)
        {
            _js = js;
            _options = options;
        }

        public bool IsSupported => true;

        public async Task<string?> AcquireTokenAsync(CancellationToken cancellationToken = default)
        {
            var module = await GetModuleAsync();

            return await module.InvokeAsync<string?>(
                "acquireToken",
                cancellationToken,
                _options.ClientId,
                _options.TenantId,
                _options.ApiScope);
        }

        public async Task SignOutAsync()
        {
            var module = await GetModuleAsync();

            await module.InvokeVoidAsync("signOut");
        }

        private async Task<IJSObjectReference> GetModuleAsync()
        {
            return _module ??= await _js.InvokeAsync<IJSObjectReference>(
                "import", "./js/entraAuth.js");
        }

        public async ValueTask DisposeAsync()
        {
            if (_module is not null)
            {
                try
                {
                    await _module.DisposeAsync();
                }
                catch (JSDisconnectedException)
                {
                }
            }
        }
    }
}
