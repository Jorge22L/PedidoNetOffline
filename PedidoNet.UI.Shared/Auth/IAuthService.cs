using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Auth
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginRequest request);

        Task LogoutAsync();

        Task<LoginResponse?> GetSessionAsync();
    }
}
