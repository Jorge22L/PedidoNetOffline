using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using PedidoNet.Web;
using PedidoNet.Web.Models.Auth;
using PedidoNet.Web.Services.Api;
using PedidoNet.Web.Services.Productos;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl no está configurado");

builder.Services.AddScoped(sp => new HttpClient
{
    /*BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)*/
    BaseAddress = new Uri(apiBaseUrl)
});

builder.Services.AddScoped<AuthApiClient>();

builder.Services.AddScoped<ITokenStorage, TokenStorage>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ProductosApiClient>();
builder.Services.AddScoped<IProductoService, ProductoService>();


await builder.Build().RunAsync();
