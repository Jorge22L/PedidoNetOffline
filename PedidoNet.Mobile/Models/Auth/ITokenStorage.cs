using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Models.Auth
{
    public interface ITokenStorage
    {
        Task SaveAsync(LoginResponse session);
        Task<LoginResponse?> GetAsync();
        Task<string?> GetAccessTokenAsync();
        Task ClearAsync();
    }
}
