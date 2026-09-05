using Microsoft.JSInterop;

namespace PedidoNet.Web.Services
{
    public class ConnectivityService : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private DotNetObjectReference<ConnectivityService>? _dotNetRef;

        public bool IsOnline { get; private set; } = true;
        public event Action? ConnectivityChanged;

        public ConnectivityService(IJSRuntime jSRuntime)
        {
            _jsRuntime = jSRuntime;
        }

        public async Task InitializeAsync()
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            IsOnline = await _jsRuntime.InvokeAsync<bool>("connectivity.initialize", _dotNetRef);

            ConnectivityChanged?.Invoke();
        }

        [JSInvokable]
        public void SetOnlineStatus(bool isOnline)
        {
            IsOnline = isOnline;
            ConnectivityChanged?.Invoke();
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("connectivity.dispose");
            }
            catch
            {

            }

            _dotNetRef?.Dispose();
        }
    }
}
