using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Models.Productos
{
    public class ProductoImagenDTO
    {
        public int ProductoImagenId { get; set; }
        public string Ruta { get; set; } = string.Empty;
        public string? NombreOriginal { get; set; }
        public string? ContentType { get; set; }
        public bool EsPrincipal { get; set; }

    }
}
