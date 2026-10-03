using PedidoNet.UI.Shared.Offline;
using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Service.Network
{
    /*
     * Implementación MAUI del contrato compartido IConnectivityService.
     *
     * El evento de MAUI puede llegar desde un hilo que no es el de la UI;
     * los componentes deben usar InvokeAsync (ya lo hacen).
     */
    public sealed class MauiConnectivityService : IConnectivityService, IDisposable
    {
        private readonly IConnectivity _connectivity;

        public MauiConnectivityService(IConnectivity connectivity)
        {
            _connectivity = connectivity;
            _connectivity.ConnectivityChanged += OnConnectivityChanged;
        }

        public bool IsOnline =>
            _connectivity.NetworkAccess == NetworkAccess.Internet;

        public event Action? ConnectivityChanged;

        public Task InitializeAsync()
        {
            // MAUI no requiere inicialización: el estado se consulta directamente.
            return Task.CompletedTask;
        }

        private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
        {
            ConnectivityChanged?.Invoke();
        }

        public void Dispose()
        {
            _connectivity.ConnectivityChanged -= OnConnectivityChanged;
        }
    }
}
