using Microsoft.Identity.Client;
using PedidoNet.Mobile.Configuration;
using PedidoNet.Mobile.Device;
using PedidoNet.UI.Shared.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Service.Auth
{
    /*
     * IExternalLoginProvider para MAUI con MSAL.NET (cliente público, sin secreto).
     *
     * Solo obtiene el token de Entra ID que se intercambia en
     * POST /api/v1/Auth/entra. Después la app usa su propia sesión
     * (JWT + refresh token en SecureStorage), igual que el login normal.
     */
    public sealed class MsalExternalLoginProvider : IExternalLoginProvider
    {
        private readonly IPublicClientApplication _pca;
        private readonly string[] _scopes;
        private readonly ExternalActivityTracker _externalActivity;

        public MsalExternalLoginProvider(EntraOptions options, ExternalActivityTracker externalActivity)
        {
            _externalActivity = externalActivity;

            _scopes = [options.ApiScope];

            var builder = PublicClientApplicationBuilder
                .Create(options.ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, options.TenantId);

#if ANDROID
            builder = builder
                .WithRedirectUri(options.AndroidRedirectUri)
                .WithParentActivityOrWindow(() => Platform.CurrentActivity);
#elif WINDOWS
            builder = builder
                .WithRedirectUri(EntraOptions.WindowsRedirectUri);
#endif

            _pca = builder.Build();
        }

        public bool IsSupported => true;

        public async Task<string?> AcquireTokenAsync(CancellationToken cancellationToken = default)
        {
            var account = (await _pca.GetAccountsAsync()).FirstOrDefault();

            // 1. Si ya hubo login antes, MSAL renueva el token sin mostrar interfaz.
            if (account is not null)
            {
                try
                {
                    var silent = await _pca
                        .AcquireTokenSilent(_scopes, account)
                        .ExecuteAsync(cancellationToken);

                    return silent.AccessToken;
                }
                catch (MsalUiRequiredException)
                {
                    // Requiere interacción (expiró, cambió la contraseña, MFA...)
                }
            }

            // 2. Login interactivo con el navegador del sistema.
            //    Si Android cierra la app mientras el navegador está abierto,
            //    al reiniciar se avisa en /login (Home.razor).
            _externalActivity.Begin(ExternalActivityKind.MicrosoftLogin, "/login");

            try
            {
                var result = await MainThread.InvokeOnMainThreadAsync(() =>
                    _pca.AcquireTokenInteractive(_scopes)
                        .WithPrompt(Prompt.SelectAccount)
                        .WithUseEmbeddedWebView(false)
                        .ExecuteAsync(cancellationToken));

                return result.AccessToken;
            }
            catch (MsalClientException ex) when (ex.ErrorCode == MsalError.AuthenticationCanceledError)
            {
                // El usuario cerró el navegador
                return null;
            }
            finally
            {
                _externalActivity.End();
            }
        }

        public async Task SignOutAsync()
        {
            foreach (var account in await _pca.GetAccountsAsync())
            {
                await _pca.RemoveAsync(account);
            }
        }
    }
}
