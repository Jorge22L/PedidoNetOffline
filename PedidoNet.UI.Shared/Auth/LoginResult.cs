using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Auth
{
    public class LoginResult
    {
        public bool Success { get; init; }

        public string? Message { get; init; }

        public LoginResponse? Session { get; init; }

        public static LoginResult Ok(LoginResponse session)
        {
            return new LoginResult
            {
                Success = true,
                Session = session
            };
        }

        public static LoginResult Fail(string? message)
        {
            return new LoginResult
            {
                Success = false,
                Message = message
            };
        }
    }
}
