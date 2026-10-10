using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Device
{
    public enum BarcodeScanKind
    {
        /// <summary>Códigos lineales: EAN-13, EAN-8, UPC, Code 128, Code 39...</summary>
        CodigoBarras,
        Qr,
        Todos
    }

    public sealed record BarcodeScanResult(string Value, string Format);

    /// <summary>
    /// Lectura de códigos con la cámara.
    /// Mobile: ZXing.Net.Maui · Web: no soportado (IsSupported = false).
    /// </summary>
    public interface IBarcodeScannerService
    {
        bool IsSupported { get; }

        /// <returns>El código leído, o null si el usuario canceló.</returns>
        Task<BarcodeScanResult?> ScanAsync(
            BarcodeScanKind kind = BarcodeScanKind.CodigoBarras,
            CancellationToken cancellationToken = default);
    }
}
