using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Configuration
{
    public sealed class ApiOptions
    {
        public required string BaseUrl { get; init; } = string.Empty;
    }
}
