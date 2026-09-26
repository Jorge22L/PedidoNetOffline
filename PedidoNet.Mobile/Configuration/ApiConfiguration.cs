using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Configuration
{
    public static class ApiConfiguration
    {
        public static string GetBaseUrl()
        {
#if ANDROID
            return "http://10.0.2.2:8080";
#elif WINDOWS
            return "http://localhost:8080";
#else
        throw new PlatformNotSupportedException("PedidoNet Mobile solamente soporta Android y Windows")
#endif
        }
    }
}
