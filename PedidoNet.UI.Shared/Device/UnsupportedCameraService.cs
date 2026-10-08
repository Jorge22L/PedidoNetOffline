using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Device
{
    public sealed class UnsupportedCameraService : ICameraService
    {
        public bool IsSupported => false;

        public Task<DevicePhoto?> CapturePhotoAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<DevicePhoto?>(null);

        public Task<DevicePhoto?> PickPhotoAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<DevicePhoto?>(null);
    }
}
