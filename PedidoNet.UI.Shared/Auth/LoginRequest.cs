using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PedidoNet.UI.Shared.Auth
{
    public sealed class LoginRequest
    {
        [Required(ErrorMessage = "El usuario es requerido.")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        public string Password { get; set; } = string.Empty;
    }
}
