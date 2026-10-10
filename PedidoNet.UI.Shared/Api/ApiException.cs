using System.Net;

namespace PedidoNet.UI.Shared.Api;

/// <summary>
/// Error devuelto por la API con un mensaje apto para mostrar al usuario
/// (p. ej. las validaciones de los procedimientos almacenados: stock
/// insuficiente, pedido no pendiente, etc.).
/// Hereda de HttpRequestException para que las páginas existentes
/// que capturan HttpRequestException sigan funcionando.
/// </summary>
public sealed class ApiException : HttpRequestException
{
    public ApiException(string message, HttpStatusCode statusCode)
        : base(message, inner: null, statusCode)
    {
    }
}
