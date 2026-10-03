using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Auth
{
    public sealed class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
