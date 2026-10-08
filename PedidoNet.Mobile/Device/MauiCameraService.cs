using PedidoNet.UI.Shared.Device;
using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Device
{
    public sealed class MauiCameraService : ICameraService
    {
        // Mismo limite que la API
        private const long MaxFileSize = 5 * 1024 * 1024;

        private readonly IDevicePermissionService _permissions;

        public MauiCameraService(IDevicePermissionService permissions)
        {
            _permissions = permissions;
        }
        public bool IsSupported => MediaPicker.Default.IsCaptureSupported;

        public async Task<DevicePhoto?> CapturePhotoAsync(CancellationToken cancellationToken = default)
        {
            if (!IsSupported)
                throw new DeviceFeatureException("Este dispositivo no tiene cámara disponible");

            var permiso = await _permissions.RequestAsync(DevicePermission.Camera);

            if (permiso != DevicePermissionStatus.Granted)
                throw new DeviceFeatureException("Debes conceder el permiso de cámara");

            var file = await MainThread.InvokeOnMainThreadAsync(() =>
            MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = "Foto del producto"
            }));

            return file is null ? null : await ToDevicePhotoAsync(file, cancellationToken);
        }

        private async Task<DevicePhoto?> ToDevicePhotoAsync(FileResult file, CancellationToken cancellationToken)
        {
            await using var source = await file.OpenReadAsync();
            using var memory = new MemoryStream();

            await source.CopyToAsync(memory, cancellationToken);

            if (memory.Length > MaxFileSize)
                throw new DeviceFeatureException("La imagen no puede superar los 5MB");

            var contentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? GuessContentType(file.FileName)
                : file.ContentType;

            return new DevicePhoto(file.FileName, contentType, memory.ToArray());
        }

        private static string GuessContentType(string fileName) =>
            Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "image/jpeg"
            };

        public async Task<DevicePhoto?> PickPhotoAsync(CancellationToken cancellationToken = default)
        {
            var results = await MainThread.InvokeOnMainThreadAsync(() =>
            MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                Title = "Seleccionar imagen"
            }));

            var file = results?.FirstOrDefault();

            return file is null ? null : await ToDevicePhotoAsync(file, cancellationToken);
        }
    }
}
