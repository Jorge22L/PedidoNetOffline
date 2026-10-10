using PedidoNet.UI.Shared.Device;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PedidoNet.Mobile.Device
{
    public sealed class MauiLocationService : ILocationService
    {
        private static readonly TimeSpan AntiguedadMaxima = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan TiempoLimite = TimeSpan.FromSeconds(15);
        private const double PrecisionAceptableMetros = 100;

        private readonly IDevicePermissionService _permissions;

        public MauiLocationService(IDevicePermissionService permissions)
        {
            _permissions = permissions;
        }

        public bool IsSupported => true;

        public async Task<GeoLocation?> GetCurrentLocationAsync(CancellationToken cancellationToken = default)
        {
            var permiso = await _permissions.RequestAsync(DevicePermission.Location);

            if (permiso != DevicePermissionStatus.Granted)
                throw new DeviceFeatureException("Debes conceder el permiso de ubicación para usar esta función.");

            try
            {
                var location = await MainThread.InvokeOnMainThreadAsync(() =>
                    Geolocation.Default.GetLastKnownLocationAsync());

                if (!EsUtil(location))
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TiempoLimite);

                    location = await MainThread.InvokeOnMainThreadAsync(() =>
                        Geolocation.Default.GetLocationAsync(request, cancellationToken));
                }

                return location is null
                    ? null
                    : new GeoLocation(
                        location.Latitude,
                        location.Longitude,
                        location.Accuracy,
                        location.Timestamp,
                        location.IsFromMockProvider);
            }
            catch (FeatureNotSupportedException ex)
            {
                throw new DeviceFeatureException("Este dispositivo no tiene GPS.", ex);
            }
            catch (FeatureNotEnabledException ex)
            {
                throw new DeviceFeatureException("Activa la ubicación (GPS) del dispositivo e intenta de nuevo.", ex);
            }
            catch (PermissionException ex)
            {
                throw new DeviceFeatureException("Debes conceder el permiso de ubicación para usar esta función.", ex);
            }
        }

        public async Task OpenMapAsync(double latitud, double longitud, string? etiqueta = null)
        {
            var opciones = new MapLaunchOptions
            {
                Name = etiqueta ?? string.Empty,
                NavigationMode = NavigationMode.None
            };

            var abierto = await MainThread.InvokeOnMainThreadAsync(() =>
                Map.Default.TryOpenAsync(latitud, longitud, opciones));

            if (abierto)
                return;

            // Sin app de mapas: Google Maps en el navegador.
            var url = string.Create(
                CultureInfo.InvariantCulture,
                $"https://www.google.com/maps/search/?api=1&query={latitud},{longitud}");

            var abiertoEnNavegador = await MainThread.InvokeOnMainThreadAsync(() =>
                Browser.Default.OpenAsync(url, BrowserLaunchMode.External));

            if (!abiertoEnNavegador)
                throw new DeviceFeatureException("No hay ninguna aplicación para mostrar el mapa.");
        }

        private static bool EsUtil(Location? location)
        {
            return location is not null
                && DateTimeOffset.UtcNow - location.Timestamp <= AntiguedadMaxima
                && (location.Accuracy ?? double.MaxValue) <= PrecisionAceptableMetros;
        }
    }
}
