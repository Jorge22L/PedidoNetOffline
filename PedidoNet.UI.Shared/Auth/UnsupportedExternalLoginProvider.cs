using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Auth
{
    public sealed class UnsupportedExternalLoginProvider : IExternalLoginProvider
    {
        public bool IsSupported => false;

        public Task<string?> AcquireTokenAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<string?>(null);

        public Task SignOutAsync()
            => Task.CompletedTask;
    }
}
