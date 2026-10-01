using Microsoft.Extensions.Logging;
using PedidoNet.Mobile.Configuration;
using PedidoNet.Mobile.Service.Api;
using PedidoNet.Mobile.Service.Auth;
using PedidoNet.UI.Shared.Auth;

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

            builder.Services.AddSingleton<UI.Shared.Auth.ITokenStorage, MauiTokenStorage>();

            ConfigureApi(builder.Services);

            builder.Services.AddScoped<PedidoNet.UI.Shared.Auth.AuthApiClient>(sp =>
            {
                var httpClientFactory =
                    sp.GetRequiredService<IHttpClientFactory>();

                var httpClient =
                    httpClientFactory.CreateClient("PedidoNetApi");

                return new PedidoNet.UI.Shared.Auth.AuthApiClient(httpClient);
            });

            builder.Services.AddScoped<IAuthService, AuthService>();

            string baseUrl = ApiConfiguration.GetBaseUrl();

            var apiOptions = new ApiOptions
            {
                BaseUrl = baseUrl
            };

            builder.Services.AddSingleton(apiOptions);

            ConfigureApi(builder.Services);

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }

        private static void ConfigureApi(IServiceCollection services)
        {
            services.AddHttpClient("PedidoNetApi", (sp, client) =>
            {
                var options = sp.GetRequiredService<ApiOptions>();

                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddScoped<ApiClient>();
        }
    }
}