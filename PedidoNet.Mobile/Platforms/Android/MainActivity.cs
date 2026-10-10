using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.View;
using Microsoft.Identity.Client;

namespace PedidoNet.Mobile
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ConfigurarBarraDeEstado();
        }
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Entrega a MSAL la respuesta del login de Microsoft en el navegador.
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(requestCode, resultCode, data);
        }

        private void ConfigurarBarraDeEstado()
        {
            if(Window is null)
            {
                return;
            }

            if (!OperatingSystem.IsAndroidVersionAtLeast(35))
            {
#pragma warning disable CA1422 // Obsoleto en API 35
                Window.SetStatusBarColor(Android.Graphics.Color.AliceBlue);
#pragma warning restore CA1422
            }

            var controller = WindowCompat.GetInsetsController(Window, Window.DecorView);
            controller.AppearanceLightStatusBars = true;
        }
    }
}
