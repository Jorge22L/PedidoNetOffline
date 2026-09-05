using System.ComponentModel.DataAnnotations;

namespace PedidoNet.Web.Models.Auth
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El usuario es requerido.")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        public string Password { get; set; } = string.Empty;
    }
}
