using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PedidoNet.UI.Shared.Api;

/// <summary>
/// Utilidades comunes para los clientes HTTP de la API.
/// </summary>
internal static class ApiHttp
{
    /// <summary>
    /// Lanza UnauthorizedAccessException en 401 (la UI redirige a login)
    /// o ApiException con el mensaje que envió la API.
    /// </summary>
    public static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "La sesión ya no es válida.");
        }

        var mensaje = await LeerMensajeAsync(response, cancellationToken);

        throw new ApiException(mensaje, response.StatusCode);
    }

    /// <summary>
    /// Lee respuestas con el formato { success, data, message } y devuelve data.
    /// </summary>
    public static async Task<T?> ReadDataAsync<T>(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        await EnsureSuccessAsync(response, cancellationToken);

        var envelope = await response.Content
            .ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: cancellationToken);

        return envelope is null ? default : envelope.Data;
    }

    private static async Task<string> LeerMensajeAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string? contenido = null;

        try
        {
            contenido = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception)
        {
        }

        if (!string.IsNullOrWhiteSpace(contenido))
        {
            try
            {
                using var documento = JsonDocument.Parse(contenido);

                if (documento.RootElement.ValueKind == JsonValueKind.Object)
                {
                    // Middleware: { statusCode, message } · Controladores: { success, message }
                    // ProblemDetails: { title }
                    foreach (var propiedad in new[] { "message", "Message", "title", "Title" })
                    {
                        if (documento.RootElement.TryGetProperty(propiedad, out var valor) &&
                            valor.ValueKind == JsonValueKind.String &&
                            !string.IsNullOrWhiteSpace(valor.GetString()))
                        {
                            return valor.GetString()!;
                        }
                    }
                }
            }
            catch (JsonException)
            {
            }
        }

        var codigo = (int)response.StatusCode;

        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => "El registro solicitado no existe.",
            HttpStatusCode.Forbidden => "No tienes permisos para realizar esta acción.",
            HttpStatusCode.TooManyRequests => "Demasiadas solicitudes. Intenta nuevamente en unos segundos.",
            _ when codigo >= 500 => "Ocurrió un error en el servidor. Intenta nuevamente.",
            _ => $"La API respondió con el código {codigo}."
        };
    }
}
