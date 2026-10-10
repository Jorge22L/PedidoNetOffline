using PedidoNet.UI.Shared.Api;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PedidoNet.UI.Shared.Reportes
{
    /// <summary>
    /// Reportes RDLC: los genera la API (ReportViewer Core) y aquí solo se descargan.
    /// </summary>
    public sealed class ReportesApiClient
    {
        private const string BaseUrl = "api/v1/Reportes";

        private readonly HttpClient _httpClient;

        public ReportesApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<ReporteDescargado> ObtenerListadoPedidosAsync(
       ReportePedidosFiltro filtro,
       FormatoReporte formato,
       CancellationToken cancellationToken = default)
        {
            var query = new List<string> { $"formato={Formato(formato)}" };

            if (filtro.Desde is { } desde)
                query.Add($"desde={desde.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

            if (filtro.Hasta is { } hasta)
                query.Add($"hasta={hasta.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

            if (!string.IsNullOrWhiteSpace(filtro.Estado))
                query.Add($"estado={Uri.EscapeDataString(filtro.Estado)}");

            if (filtro.ClienteId is { } clienteId)
                query.Add($"clienteId={clienteId}");

            return DescargarAsync($"{BaseUrl}/pedidos?{string.Join("&", query)}", "pedidos", cancellationToken);
        }

        public Task<ReporteDescargado> ObtenerComprobanteAsync(
        int pedidoId,
        FormatoReporte formato,
        CancellationToken cancellationToken = default)
        {
            return DescargarAsync(
                $"{BaseUrl}/pedidos/{pedidoId}?formato={Formato(formato)}",
                $"pedido_{pedidoId}",
                cancellationToken);
        }

        private async Task<ReporteDescargado> DescargarAsync(
        string url,
        string nombreBase,
        CancellationToken cancellationToken)
        {
            using var response = await _httpClient.GetAsync(url, cancellationToken);

            // 401 → UnauthorizedAccessException; 404/422 → ApiException con el mensaje de la API.
            await ApiHttp.EnsureSuccessAsync(response, cancellationToken);

            var contenido = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

            // El nombre se arma aquí: Content-Disposition no es legible desde otro origen sin exponerlo en CORS.
            var nombre = $"{nombreBase}_{DateTime.Now:yyyyMMdd_HHmm}.{Extension(contentType)}";

            return new ReporteDescargado(contenido, contentType, nombre);
        }

        private static string Formato(FormatoReporte formato) =>
        formato.ToString().ToLowerInvariant();

        private static string Extension(string contentType) => contentType switch
        {
            "application/pdf" => "pdf",
            "text/html" => "html",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => "xlsx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "docx",
            _ => "bin"
        };
    }
}
