using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Configuration
{
    /// <summary>
    /// Configuración de Microsoft Entra ID para MSAL.NET.
    /// No son secretos: una app móvil es un cliente público.
    /// </summary>
    public sealed class EntraOptions
    {
        public required string TenantId { get; init; }

        /// <summary>Id. de aplicación (cliente) del registro usado por la app.</summary>
        public required string ClientId { get; init; }

        /// <summary>api://{API_CLIENT_ID}/access_as_user</summary>
        public required string ApiScope { get; init; }

        /// <summary>Redirect URI de Android (registrado en "Aplicaciones móviles y de escritorio").</summary>
        public string AndroidRedirectUri => $"msal{ClientId}://auth";

        /// <summary>Redirect URI de Windows (navegador del sistema).</summary>
        public const string WindowsRedirectUri = "http://localhost";
    }
}
