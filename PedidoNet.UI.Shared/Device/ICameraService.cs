using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Device
{
    public sealed record DevicePhoto(string FileName, string ContentType, byte[] Content)
    {
        public string ToDataUrl() => $"data:{ContentType};base64,{Convert.ToBase64String(Content)}";
    }
    public interface ICameraService
    {
        bool IsSupported { get; }
        /// null si el usuario canceló
        Task<DevicePhoto?> CapturePhotoAsync(CancellationToken cancellationToken = default);
        Task<DevicePhoto?> PickPhotoAsync(CancellationToken cancellationToken = default);
    }
}
