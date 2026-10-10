using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Device
{
    public sealed record GeoLocation(
        double Latitud,
        double Longitud,
        double? PrecisionMetros,
        DateTimeOffset Fecha,
        bool EsSimulada
    );

    public interface ILocationService
    {
        bool IsSupported { get; }
        Task<GeoLocation?> GetCurrentLocationAsync(CancellationToken cancellationToken = default);

        Task OpenMapAsync(double latitud, double longitud, string? etiqueta = null);
    }
}
