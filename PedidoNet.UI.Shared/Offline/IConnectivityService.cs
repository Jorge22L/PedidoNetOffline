using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Offline
{
    /*
     * Contrato compartido de conectividad.
     *
     * Cada host lo implementa con su tecnología:
     * - Web:    eventos online/offline del navegador (JS interop).
     * - Mobile: MAUI Connectivity.
     */
    public interface IConnectivityService
    {
        bool IsOnline { get; }

        event Action? ConnectivityChanged;

        Task InitializeAsync();
    }
}
