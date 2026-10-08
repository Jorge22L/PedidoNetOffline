using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Device
{
    public enum DevicePermission
    {
        Camera,
        Location,
        Notifications
    }

    public enum DevicePermissionStatus
    {
        Unknown,
        Granted,
        Denied,
        Restricted
    }
    public interface IDevicePermissionService
    {
        Task<DevicePermissionStatus> CheckAsync(DevicePermission permission);
        /// Pide permiso solo si aún no está concedido
        Task<DevicePermissionStatus> RequestAsync(DevicePermission permission);
    }
}
