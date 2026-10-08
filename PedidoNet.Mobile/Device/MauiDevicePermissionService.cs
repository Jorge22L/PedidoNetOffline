using PedidoNet.UI.Shared.Device;
using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Device
{
    public sealed class MauiDevicePermissionService : IDevicePermissionService
    {
        public Task<DevicePermissionStatus> CheckAsync(DevicePermission permission)
        {
            return MainThread.InvokeOnMainThreadAsync(async () => Map(permission switch
            {
                DevicePermission.Camera => await Permissions.CheckStatusAsync<Permissions.Camera>(),
                DevicePermission.Location => await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>(),
                DevicePermission.Notifications => await CheckNotificationsAsync(),
                _ => PermissionStatus.Unknown
            }));
        }

        public Task<DevicePermissionStatus> RequestAsync(DevicePermission permission)
        {
            // Los diálogos de permisos deben abrirse en el hilo principal
            return MainThread.InvokeOnMainThreadAsync(async () => Map(permission switch
            {
                DevicePermission.Camera => await RequestIfNeededAsync<Permissions.Camera>(),
                DevicePermission.Location => await RequestIfNeededAsync<Permissions.LocationWhenInUse>(),
                DevicePermission.Notifications => await RequestNotificationsAsync(),
                _ => PermissionStatus.Unknown
            }));

        }

        private static async Task<PermissionStatus> RequestIfNeededAsync<TPermission>()
            where TPermission : Permissions.BasePermission, new()
        {
            var status = await Permissions.CheckStatusAsync<TPermission>();
            return status == PermissionStatus.Granted 
                ? status : await Permissions.RequestAsync<TPermission>();
        }

        private static Task<PermissionStatus> CheckNotificationsAsync()
        {
#if ANDROID
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
                return Permissions.CheckStatusAsync<Permissions.PostNotifications>();
#endif
            return Task.FromResult(PermissionStatus.Granted);
        }

        private static Task<PermissionStatus> RequestNotificationsAsync()
        {
#if ANDROID
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
                return RequestIfNeededAsync<Permissions.PostNotifications>();
#endif
            return Task.FromResult(PermissionStatus.Granted);
        }

        private static DevicePermissionStatus Map(PermissionStatus status) => status switch
        {
            PermissionStatus.Granted => DevicePermissionStatus.Granted,
            PermissionStatus.Limited => DevicePermissionStatus.Granted,
            PermissionStatus.Denied => DevicePermissionStatus.Denied,
            PermissionStatus.Restricted => DevicePermissionStatus.Restricted,
            _ => DevicePermissionStatus.Unknown
        };
    }
}
