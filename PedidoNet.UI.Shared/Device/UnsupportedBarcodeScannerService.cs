using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Device
{
    public sealed class UnsupportedBarcodeScannerService : IBarcodeScannerService
    {
        public bool IsSupported => false;

        public Task<BarcodeScanResult?> ScanAsync(
            BarcodeScanKind kind = BarcodeScanKind.CodigoBarras,
            CancellationToken cancellationToken = default)
            => Task.FromResult<BarcodeScanResult?>(null);
    }
}
