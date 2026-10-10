using PedidoNet.UI.Shared.Device;
using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Device
{
    /// <summary>
    /// IBarcodeScannerService para MAUI: abre BarcodeScannerPage como modal
    /// y devuelve el primer código leído.
    /// </summary>
    public sealed class MauiBarcodeScannerService : IBarcodeScannerService
    {
        private readonly IDevicePermissionService _permissions;

        public MauiBarcodeScannerService(IDevicePermissionService permissions)
        {
            _permissions = permissions;
        }

        public bool IsSupported => true;

        public async Task<BarcodeScanResult?> ScanAsync(
            BarcodeScanKind kind = BarcodeScanKind.CodigoBarras,
            CancellationToken cancellationToken = default)
        {
            var permiso = await _permissions.RequestAsync(DevicePermission.Camera);

            if (permiso != DevicePermissionStatus.Granted)
                throw new DeviceFeatureException("Debes conceder el permiso de cámara para escanear");

            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var navigation = Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation
                    ?? throw new DeviceFeatureException("No fue posible abrir el escáner");

                var page = new BarcodeScannerPage(kind);

                await navigation.PushModalAsync(page);

                using (cancellationToken.Register(() =>
                    MainThread.BeginInvokeOnMainThread(() => _ = page.CancelarAsync())))
                {
                    return await page.Result;
                }
            });
        }
    }
}
