using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Device
{
    public enum ExternalActivityKind
    {
        Camera,
        Gallery,
        MicrosoftLogin
    }

    public sealed record ExternalActivityMarker(
        ExternalActivityKind Kind,
        string? ReturnRoute,
        DateTime StartedUtc);

    /// <summary>
    /// Marca en Preferences que la app salió a otra app (cámara, galería,
    /// navegador del login de Microsoft).
    ///
    /// Si Android cierra PedidoNet mientras tanto, la marca sobrevive al
    /// reinicio y Home.razor la usa para volver a donde estaba el usuario
    /// en lugar de enviarlo siempre a /login.
    /// </summary>
    public sealed class ExternalActivityTracker
    {
        private const string KindKey = "app.external_activity.kind";
        private const string RouteKey = "app.external_activity.route";
        private const string StartedKey = "app.external_activity.started_utc";

        // Solo se considera un reinicio "durante" la actividad si fue reciente.
        private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(5);

        public void Begin(ExternalActivityKind kind, string? returnRoute = null)
        {
            Preferences.Default.Set(KindKey, kind.ToString());
            Preferences.Default.Set(RouteKey, returnRoute ?? string.Empty);
            Preferences.Default.Set(StartedKey, DateTime.UtcNow.Ticks);
        }

        public void End()
        {
            Preferences.Default.Remove(KindKey);
            Preferences.Default.Remove(RouteKey);
            Preferences.Default.Remove(StartedKey);
        }

        /// <summary>
        /// Lee y elimina la marca. Devuelve null si no hay marca o si es antigua.
        /// </summary>
        public ExternalActivityMarker? Consume()
        {
            if (!Preferences.Default.ContainsKey(StartedKey))
                return null;

            var ticks = Preferences.Default.Get(StartedKey, 0L);
            var kindText = Preferences.Default.Get(KindKey, string.Empty);
            var route = Preferences.Default.Get(RouteKey, string.Empty);

            End();

            if (ticks <= 0 || !Enum.TryParse<ExternalActivityKind>(kindText, out var kind))
                return null;

            var started = new DateTime(ticks, DateTimeKind.Utc);

            if (DateTime.UtcNow - started > MaxAge)
                return null;

            return new ExternalActivityMarker(
                kind,
                string.IsNullOrWhiteSpace(route) ? null : route,
                started);
        }
    }
}
