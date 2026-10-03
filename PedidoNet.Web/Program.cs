using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PedidoNet.UI.Shared.Auth;
using PedidoNet.UI.Shared.Offline;
using PedidoNet.UI.Shared.Offline.Productos;
using PedidoNet.UI.Shared.Productos;
using PedidoNet.Web;
using PedidoNet.Web.Models.Auth;
using PedidoNet.Web.Services;
using PedidoNet.Web.Services.Offline;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl no está configurado");

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


await builder.Build().RunAsync();
