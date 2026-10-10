using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Device
{
    public sealed class UnsupportedLocationService
    {
        public bool IsSupported => false;

        public Task<GeoLocation?> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
        {
            throw new DeviceFeatureException("La geolocalización solo está disponible en la app móvil.");
        }

        public Task OpenMapAsync(double latitud, double longitud, string? etiqueta = null)
        {
            throw new DeviceFeatureException("El mapa solo está disponible en la app móvil.");
        }
    }
}
