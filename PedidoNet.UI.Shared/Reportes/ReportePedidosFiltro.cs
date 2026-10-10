using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Reportes
{
    /// <summary>Debe coincidir con Application.Reportes.FormatoReporte de la API.</summary>
    public enum FormatoReporte
    {
        Vista,
        Pdf,
        Excel,
        Word
    }
    public sealed class ReportePedidosFiltro
    {
        public DateOnly? Desde { get; set; }

        public DateOnly? Hasta { get; set; }

        public string? Estado { get; set; }

        public int? ClienteId { get; set; }
    }

    public sealed record ReporteDescargado(byte[] Contenido, string ContentType, string NombreArchivo);
}
