using Microsoft.AspNetCore.Components;
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
        private readonly ExternalActivityTracker _externalActivity;
        private readonly NavigationManager _navigation;

        public MauiCameraService(
            IDevicePermissionService permissions,
            ExternalActivityTracker externalActivity,
            NavigationManager navigation)
        {
            _permissions = permissions;
            _externalActivity = externalActivity;
            _navigation = navigation;
        }
        public bool IsSupported => MediaPicker.Default.IsCaptureSupported;

        public async Task<DevicePhoto?> CapturePhotoAsync(CancellationToken cancellationToken = default)
        {
            if (!IsSupported)
                throw new DeviceFeatureException("Este dispositivo no tiene cámara disponible");

            var permiso = await _permissions.RequestAsync(DevicePermission.Camera);

            if (permiso != DevicePermissionStatus.Granted)
                throw new DeviceFeatureException("Debes conceder el permiso de cámara");

            // Si Android cierra la app mientras la cámara está abierta,
            // al reiniciar se vuelve a esta misma página (Home.razor).
            _externalActivity.Begin(ExternalActivityKind.Camera, CurrentRoute());

            FileResult? file;

            try
            {
                file = await MainThread.InvokeOnMainThreadAsync(() =>
                MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Foto del producto"
                }));
            }
            finally
            {
                _externalActivity.End();
            }

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

        /// <summary>Ruta actual sin query string, p. ej. "/productos/editar/{id}".</summary>
        private string CurrentRoute()
        {
            var relative = _navigation.ToBaseRelativePath(_navigation.Uri);

            return "/" + relative.Split('?', '#')[0];
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
            _externalActivity.Begin(ExternalActivityKind.Gallery, CurrentRoute());

            FileResult? file;

            try
            {
                var results = await MainThread.InvokeOnMainThreadAsync(() =>
                MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
                {
                    Title = "Seleccionar imagen"
                }));

                file = results?.FirstOrDefault();
            }
            finally
            {
                _externalActivity.End();
            }

            return file is null ? null : await ToDevicePhotoAsync(file, cancellationToken);
        }
    }
}
