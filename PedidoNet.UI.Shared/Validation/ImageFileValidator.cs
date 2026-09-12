using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Text;

namespace PedidoNet.UI.Shared.Validation
{
    public static class ImageFileValidator
    {
        private const int HeaderLength = 12;

        private const long MaxFileSize = 5 * 1024 * 1024;

        public static async Task<ImageValidationResult> ValidateAsync(IBrowserFile file, CancellationToken cancellationToken = default)
        {
            if(file.Size == 0)
            {
                return ImageValidationResult.Invalid("El archivo está vacío.");
            }

            if (file.Size > MaxFileSize)
            {
                return ImageValidationResult.Invalid("La imagen no puede superar los 5MB.");
            }

            var extension = Path.GetExtension(file.Name).ToLowerInvariant();

            if(!TryGetExpected(extension, file.ContentType, out var expectedType))
            {
                return ImageValidationResult.Invalid("Solo se permiten archivos JPG, PNG o WEBP");
            }

            await using var stream = file.OpenReadStream(MaxFileSize, cancellationToken);

            var header = new byte[HeaderLength];

            var bytesRead = await stream.ReadAsync(header.AsMemory(), cancellationToken);

            var detectedType = DetectImageType(header.AsSpan(0, bytesRead));

            if(detectedType is null)
            {
                return ImageValidationResult.Invalid("El contenido del arcivo no corresponde a una imagen");
            }

            if(!string.Equals(expectedType, detectedType, StringComparison.OrdinalIgnoreCase))
            {
                return ImageValidationResult.Invalid("La extension del archivo no coincide con su contenido real.");
            }

            return ImageValidationResult.Valid();
        }

        private static string? DetectImageType(ReadOnlySpan<byte> header)
        {
            if (header.Length >= 3 &&
                header[0] == 0xFF &&
                header[1] == 0xD8 &&
                header[2] == 0xFF)
            {
                return "image/jpeg";
            }

            ReadOnlySpan<byte> png =
            [
                0x89,
                0x50,
                0x4E,
                0x47,
                0x0D,
                0x0A,
                0x1A,
                0x0A
            ];

            if (header.Length >= png.Length &&
                header[..png.Length]
                    .SequenceEqual(png))
            {
                return "image/png";
            }

            if (header.Length >= 12 &&
                header[0] == (byte)'R' &&
                header[1] == (byte)'I' &&
                header[2] == (byte)'F' &&
                header[3] == (byte)'F' &&
                header[8] == (byte)'W' &&
                header[9] == (byte)'E' &&
                header[10] == (byte)'B' &&
                header[11] == (byte)'P')
            {
                return "image/webp";
            }

            return null;
        }

        public static bool TryGetExpected(string extension, string contentType, out string expectedType)
        {
            expectedType = string.Empty;

            var normalized =
                contentType.ToLowerInvariant();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    if (normalized != "image/jpeg")
                        return false;

                    expectedType = "image/jpeg";
                    return true;

                case ".png":
                    if (normalized != "image/png")
                        return false;

                    expectedType = "image/png";
                    return true;

                case ".webp":
                    if (normalized != "image/webp")
                        return false;

                    expectedType = "image/webp";
                    return true;

                default:
                    return false;
            }
        }

        public sealed record ImageValidationResult(bool IsValid, string? Error)
        {
            public static ImageValidationResult Valid() => new(true, null);

            public static ImageValidationResult Invalid(string error) => new(false, error);
        }
    }
}
