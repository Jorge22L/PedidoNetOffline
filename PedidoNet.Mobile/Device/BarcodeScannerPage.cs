using PedidoNet.UI.Shared.Device;
using System;
using System.Collections.Generic;
using System.Text;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace PedidoNet.Mobile.Device
{
    /// <summary>
    /// Página MAUI (no Blazor) con la vista de cámara de ZXing.
    /// Se abre como modal encima del BlazorWebView: la cámara corre
    /// DENTRO de la app, así que Android no la envía a segundo plano.
    /// </summary>
    public sealed class BarcodeScannerPage : ContentPage
    {
        private readonly TaskCompletionSource<BarcodeScanResult?> _tcs =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly CameraBarcodeReaderView _reader;

        public Task<BarcodeScanResult?> Result => _tcs.Task;

        public BarcodeScannerPage(BarcodeScanKind kind)
        {
            Title = "Escanear código";
            BackgroundColor = Colors.Black;

            _reader = new CameraBarcodeReaderView
            {
                Options = new BarcodeReaderOptions
                {
                    Formats = ToFormats(kind),
                    AutoRotate = true,
                    Multiple = false
                },
                IsDetecting = true
            };

            _reader.BarcodesDetected += OnBarcodesDetected;

            var instrucciones = new Label
            {
                Text = kind == BarcodeScanKind.Qr
                    ? "Apunta la cámara al código QR"
                    : "Apunta la cámara al código de barras",
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(16, 16, 16, 8)
            };

            var linterna = new Button { Text = "Linterna" };
            linterna.Clicked += (_, _) => _reader.IsTorchOn = !_reader.IsTorchOn;

            var cancelar = new Button { Text = "Cancelar" };
            cancelar.Clicked += async (_, _) => await CerrarAsync(null);

            var botones = new HorizontalStackLayout
            {
                Spacing = 12,
                Padding = new Thickness(16),
                HorizontalOptions = LayoutOptions.Center,
                Children = { linterna, cancelar }
            };

            var grid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(GridLength.Auto),
                    new RowDefinition(GridLength.Star),
                    new RowDefinition(GridLength.Auto)
                }
            };

            grid.Add(instrucciones, 0, 0);
            grid.Add(_reader, 0, 1);
            grid.Add(botones, 0, 2);

            Content = grid;
        }

        /// <summary>Cierra el escáner sin resultado (cancelación externa).</summary>
        public Task CancelarAsync() => CerrarAsync(null);

        private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
        {
            var primero = e.Results?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Value));

            if (primero is null)
                return;

            // El evento llega desde el hilo de la cámara.
            MainThread.BeginInvokeOnMainThread(async () =>
                await CerrarAsync(new BarcodeScanResult(primero.Value.Trim(), primero.Format.ToString())));
        }

        private async Task CerrarAsync(BarcodeScanResult? resultado)
        {
            // Solo el primer resultado cuenta (la cámara puede detectar varias veces).
            if (!_tcs.TrySetResult(resultado))
                return;

            _reader.IsDetecting = false;
            _reader.BarcodesDetected -= OnBarcodesDetected;

            if (Navigation.ModalStack.Contains(this))
            {
                await Navigation.PopModalAsync();
            }
        }

        protected override bool OnBackButtonPressed()
        {
            // Botón "atrás" de Android = cancelar.
            _ = CerrarAsync(null);
            return true;
        }

        private static BarcodeFormat ToFormats(BarcodeScanKind kind) => kind switch
        {
            BarcodeScanKind.Qr => BarcodeFormat.QrCode,
            BarcodeScanKind.Todos => BarcodeFormats.All,
            _ => BarcodeFormats.OneDimensional
        };
    }
}
