using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Api
{
    public sealed class ApiResponse<T>
    {
        public bool Success { get; set; }

        public T? Data { get; set; }

        public string? Message { get; set; }
    }
}
