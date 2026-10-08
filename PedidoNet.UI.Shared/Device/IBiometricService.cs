using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Device
{
    public interface IBiometricService
    {
        Task<bool> IsAvailableAsync();
        /// retorna true si el usuario se autenticó
        Task<bool> AuthenticateAsync(string reason, CancellationToken cancellationToken = default);
    }
}
