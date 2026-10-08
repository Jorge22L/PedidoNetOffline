using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Device
{
    public sealed class UnsupportedDevicePermissionService : IDevicePermissionService
    {
        public Task<DevicePermissionStatus> CheckAsync(DevicePermission permission)
            => Task.FromResult(DevicePermissionStatus.Restricted);

        public Task<DevicePermissionStatus> RequestAsync(DevicePermission permission)
            => Task.FromResult(DevicePermissionStatus.Restricted);
    }
}
