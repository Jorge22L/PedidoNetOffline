using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Auth
{
    public interface IAuthService
    {
        Task<LoginResult> LoginAsync(LoginRequest request);

        Task LogoutAsync();

        Task<LoginResponse?> GetSessionAsync();

        /// <summary>
        /// Renueva la sesión usando el refresh token.
        /// </summary>
        /// <param name="failedAccessToken">
        /// Access token que la API rechazó con 401.
        /// Si es null se trata de un refresh preventivo
        /// (solo se renueva si el token está por expirar).
        /// </param>
        Task<LoginResult> RefreshSessionAsync(string? failedAccessToken = null);
    }
}
