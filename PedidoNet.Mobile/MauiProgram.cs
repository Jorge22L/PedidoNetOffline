using Microsoft.Extensions.Logging;
using PedidoNet.Mobile.Configuration;
using PedidoNet.Mobile.Service.Api;

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
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            string baseUrl = ApiConfiguration.GetBaseUrl();

            var apiOptions = new ApiOptions { BaseUrl = baseUrl };

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
            var apiOptions = new ApiOptions
            {
                BaseUrl = ApiConfiguration.GetBaseUrl()
            };

            services.AddHttpClient("PedidoNetApi", (sp, client) =>
            {
                var options = sp.GetRequiredService<ApiOptions>();
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });
#if DEBUG
            services.AddBlazorWebViewDeveloperTools();
#endif

            services.AddScoped<ApiClient>();
        }
    }
}
