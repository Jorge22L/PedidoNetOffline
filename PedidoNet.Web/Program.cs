using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PedidoNet.UI.Shared.Auth;
using PedidoNet.UI.Shared.Clientes;
using PedidoNet.UI.Shared.Device;
using PedidoNet.UI.Shared.Offline;
using PedidoNet.UI.Shared.Offline.Productos;
using PedidoNet.UI.Shared.Pedidos;
using PedidoNet.UI.Shared.Productos;
using PedidoNet.Web;
using PedidoNet.Web.Models.Auth;
using PedidoNet.Web.Services;
using PedidoNet.Web.Services.Auth;
using PedidoNet.Web.Services.Offline;
using static PedidoNet.Web.Services.Auth.MsalJsExternalLoginProvider;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl no está configurado");

var entraTenantId = builder.Configuration["EntraId:TenantId"];
var entraClientId = builder.Configuration["EntraId:ClientId"];
var entraApiScope = builder.Configuration["EntraId:ApiScope"];

if (!string.IsNullOrWhiteSpace(entraTenantId) &&
    !string.IsNullOrWhiteSpace(entraClientId) &&
    !string.IsNullOrWhiteSpace(entraApiScope))
{
    builder.Services.AddSingleton(new EntraWebOptions
    {
        TenantId = entraTenantId,
        ClientId = entraClientId,
        ApiScope = entraApiScope
    });

    builder.Services.AddScoped<IExternalLoginProvider, MsalJsExternalLoginProvider>();
}
else
{
    builder.Services.AddScoped<IExternalLoginProvider, UnsupportedExternalLoginProvider>();
}

// =========================================================
// HTTP CLIENTS
//
// PedidoNetApi:              login / refresh (SIN handler).
// PedidoNetAuthenticatedApi: endpoints protegidos
//                            (Bearer + refresh automático).
// =========================================================

builder.Services.AddTransient<AuthenticatedHttpHandler>();

builder.Services.AddHttpClient("PedidoNetApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient("PedidoNetAuthenticatedApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthenticatedHttpHandler>();

// =========================================================
// AUTENTICACIÓN
// =========================================================

builder.Services.AddScoped<ITokenStorage, TokenStorage>();

// AuthApiClient NO utiliza AuthenticatedHttpHandler (evita refresh recursivo).
builder.Services.AddScoped<AuthApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

    return new AuthApiClient(httpClientFactory.CreateClient("PedidoNetApi"));
});

builder.Services.AddScoped<IAuthService, AuthService>();

// Habilita [Authorize], AuthorizeRouteView y AuthorizeView.
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<PedidoNetAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<PedidoNetAuthenticationStateProvider>());

// =========================================================
// PRODUCTOS
// =========================================================

builder.Services.AddScoped<ProductosApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

    return new ProductosApiClient(httpClientFactory.CreateClient("PedidoNetAuthenticatedApi"));
});

builder.Services.AddScoped<IConnectivityService, ConnectivityService>();
builder.Services.AddScoped<IProductoOfflineStore, IndexedDbProductoStore>();
builder.Services.AddScoped<IProductoSyncQueue, IndexedDbProductoSyncQueue>();
builder.Services.AddScoped<ProductoSyncService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddSingleton<IDevicePermissionService, UnsupportedDevicePermissionService>();
builder.Services.AddSingleton<ICameraService, UnsupportedCameraService>();

// La lectura de códigos de barras es solo para la app móvil.
builder.Services.AddSingleton<IBarcodeScannerService, UnsupportedBarcodeScannerService>();

// =========================================================
// CLIENTES Y PEDIDOS (en línea: la API usa procedimientos almacenados)
// =========================================================

builder.Services.AddScoped<ClientesApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

    return new ClientesApiClient(httpClientFactory.CreateClient("PedidoNetAuthenticatedApi"));
});

builder.Services.AddScoped<PedidosApiClient>(sp =>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

    return new PedidosApiClient(httpClientFactory.CreateClient("PedidoNetAuthenticatedApi"));
});

builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IPedidoService, PedidoService>();


await builder.Build().RunAsync();
