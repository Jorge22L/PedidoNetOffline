using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using PedidoNet.Mobile.Configuration;
using PedidoNet.Mobile.Service.Api;
using PedidoNet.Mobile.Service.Auth;
using PedidoNet.Mobile.Service.Network;
using PedidoNet.Mobile.Service.Offline;
using PedidoNet.UI.Shared.Auth;
using PedidoNet.UI.Shared.Offline;
using PedidoNet.UI.Shared.Offline.Productos;
using PedidoNet.UI.Shared.Productos;

namespace PedidoNet.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont(
                        "OpenSans-Regular.ttf",
                        "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // =========================================================
            // CONFIGURACIÓN DE API
            // =========================================================

            string baseUrl = ApiConfiguration.GetBaseUrl();

            var apiOptions = new ApiOptions
            {
                BaseUrl = baseUrl
            };

            builder.Services.AddSingleton(apiOptions);

            // =========================================================
            // STORAGE DE AUTENTICACIÓN
            // =========================================================

            builder.Services.AddSingleton<
                ITokenStorage,
                MauiTokenStorage>();

            // =========================================================
            // HANDLER PARA REQUESTS AUTENTICADOS
            // =========================================================

            builder.Services.AddTransient<
                AuthenticatedHttpHandler>();

            // =========================================================
            // HTTP CLIENTS
            // =========================================================

            ConfigureApi(builder.Services);

            // =========================================================
            // AUTH API CLIENT
            //
            // Utiliza PedidoNetApi:
            // - login
            // - refresh
            //
            // NO utiliza AuthenticatedHttpHandler
            // =========================================================

            builder.Services.AddScoped<UI.Shared.Auth.AuthApiClient>(sp =>
            {
                var httpClientFactory =
                    sp.GetRequiredService<IHttpClientFactory>();

                var httpClient =
                    httpClientFactory.CreateClient(
                        "PedidoNetApi");

                return new UI.Shared.Auth.AuthApiClient(httpClient);
            });

            // =========================================================
            // AUTH SERVICE
            // =========================================================

            builder.Services.AddScoped<
                IAuthService,
                AuthService>();

            // =========================================================
            // ESTADO DE AUTENTICACIÓN / AUTORIZACIÓN
            //
            // Habilita [Authorize], AuthorizeRouteView y AuthorizeView.
            // =========================================================

            builder.Services.AddAuthorizationCore();
            builder.Services.AddCascadingAuthenticationState();

            builder.Services.AddScoped<
                PedidoNetAuthenticationStateProvider>();

            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<PedidoNetAuthenticationStateProvider>());

            // =========================================================
            // PRODUCTOS API CLIENT
            //
            // Utiliza PedidoNetAuthenticatedApi.
            // Este cliente pasa automáticamente por:
            //
            // AuthenticatedHttpHandler
            //      ↓
            // Bearer token
            //      ↓
            // refresh automático
            // =========================================================

            builder.Services.AddScoped<ProductosApiClient>(sp =>
            {
                var httpClientFactory =
                    sp.GetRequiredService<IHttpClientFactory>();

                var httpClient =
                    httpClientFactory.CreateClient(
                        "PedidoNetAuthenticatedApi");

                return new ProductosApiClient(httpClient);
            });

            // =========================================================
            // OFFLINE-FIRST DE PRODUCTOS (SQLite + MAUI Connectivity)
            //
            // RCL = qué hacer   (ProductoService, ProductoSyncService)
            // Host = cómo hacerlo (SQLite, Connectivity)
            // =========================================================

            ConfigureOffline(builder.Services);

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void ConfigureApi(
            IServiceCollection services)
        {
            // =========================================================
            // CLIENTE SIN AUTENTICACIÓN
            //
            // Se usa exclusivamente para:
            // - login
            // - refresh token
            //
            // IMPORTANTE:
            // No agregar AuthenticatedHttpHandler aquí.
            // =========================================================

            services.AddHttpClient(
                "PedidoNetApi",
                (sp, client) =>
                {
                    var options =
                        sp.GetRequiredService<ApiOptions>();

                    client.BaseAddress =
                        new Uri(options.BaseUrl);

                    client.Timeout =
                        TimeSpan.FromSeconds(30);
                });

            // =========================================================
            // CLIENTE AUTENTICADO
            //
            // Se utilizará para:
            // - Productos
            // - Clientes
            // - Pedidos
            // - otros endpoints protegidos
            // =========================================================

            services.AddHttpClient(
                "PedidoNetAuthenticatedApi",
                (sp, client) =>
                {
                    var options =
                        sp.GetRequiredService<ApiOptions>();

                    client.BaseAddress =
                        new Uri(options.BaseUrl);

                    client.Timeout =
                        TimeSpan.FromSeconds(30);
                })
                .AddHttpMessageHandler<
                    AuthenticatedHttpHandler>();

            // =========================================================
            // CLIENTE GENÉRICO EXISTENTE DE MOBILE
            // =========================================================

            services.AddScoped<ApiClient>();
        }

        private static void ConfigureOffline(
            IServiceCollection services)
        {
            // Conectividad nativa de MAUI detrás del contrato compartido.
            services.AddSingleton<IConnectivity>(
                Connectivity.Current);

            services.AddSingleton<
                IConnectivityService,
                MauiConnectivityService>();

            // Una única conexión SQLite para toda la app.
            services.AddSingleton<PedidoNetDatabase>();

            services.AddSingleton<
                IProductoOfflineStore,
                SqliteProductoStore>();

            services.AddSingleton<
                IProductoSyncQueue,
                SqliteProductoSyncQueue>();

            // Lógica compartida (RCL).
            services.AddScoped<ProductoSyncService>();

            services.AddScoped<
                IProductoService,
                ProductoService>();
        }
    }
}
