using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Auth
{
    /// <summary>
    /// Obtiene un access token de Microsfot Entra ID para intercambiarlo
    /// en la api por la sesion propia de PedidoNet
    /// </summary>
    public interface IExternalLoginProvider
    {
        bool IsSupported { get; }

        /// <returns>El access token, o null si el usuario canceló.</returns>
        Task<string?> AcquireTokenAsync(CancellationToken cancellationToken = default);

        /// <summary>Olvida la cuenta en caché para poder elegir otra la próxima vez.</summary>
        Task SignOutAsync();
    }
}
