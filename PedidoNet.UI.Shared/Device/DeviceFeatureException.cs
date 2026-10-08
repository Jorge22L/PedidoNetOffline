using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Device
{
    /// <summary>
    /// Error de capacidad del dispositivo con mensaje apto para el usuario
    /// </summary>
    public sealed class DeviceFeatureException : Exception
    {
        public DeviceFeatureException(string message, Exception? inner = null) : base(message, inner) { }
    }
}
