using System.Net;
using System.Net.Http.Headers;

namespace PedidoNet.UI.Shared.Auth;

public sealed class AuthenticatedHttpHandler : DelegatingHandler
{
    private readonly IAuthService _authService;

    private static readonly TimeSpan RefreshMargin =
        TimeSpan.FromMinutes(1);

    public AuthenticatedHttpHandler(
        IAuthService authService)
    {
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var session =
            await GetValidSessionAsync(cancellationToken);

        // Tenemos que guardar una copia porque
        // HttpRequestMessage no puede enviarse dos veces.
        var retryRequest =
            await CloneRequestAsync(
                request,
                cancellationToken);

        SetBearerToken(
            request,
            session.AccessToken);

        var response =
            await base.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode !=
            HttpStatusCode.Unauthorized)
        {
            retryRequest.Dispose();

            return response;
        }

        /*
         * El token parecía válido pero la API devolvió 401.
         *
         * Puede ocurrir si:
         * - fue revocado;
         * - existe desfase temporal;
         * - expiró entre validación y petición;
         * - el servidor ya no lo considera válido.
         */

        response.Dispose();

        // Se indica qué token falló para forzar el refresh
        // aunque su fecha de expiración local siga vigente.
        var refreshResult =
            await _authService.RefreshSessionAsync(
                session.AccessToken);

        if (!refreshResult.Success ||
            refreshResult.Session is null)
        {
            retryRequest.Dispose();

            throw new UnauthorizedAccessException(
                refreshResult.Message ??
                "La sesión ya no es válida.");
        }

        SetBearerToken(
            retryRequest,
            refreshResult.Session.AccessToken);

        // Solo se reintenta UNA vez.
        return await base.SendAsync(
            retryRequest,
            cancellationToken);
    }

    private async Task<LoginResponse> GetValidSessionAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var session =
            await _authService.GetSessionAsync();

        if (session is null)
        {
            throw new UnauthorizedAccessException(
                "No existe una sesión activa.");
        }

        if (session.ExpiraEn <=
            DateTime.UtcNow.Add(RefreshMargin))
        {
            var refreshResult =
                await _authService.RefreshSessionAsync();

            if (!refreshResult.Success ||
                refreshResult.Session is null)
            {
                throw new UnauthorizedAccessException(
                    refreshResult.Message ??
                    "No fue posible renovar la sesión.");
            }

            session = refreshResult.Session;
        }

        return session;
    }

    private static void SetBearerToken(
        HttpRequestMessage request,
        string accessToken)
    {
        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var clone =
            new HttpRequestMessage(
                request.Method,
                request.RequestUri);

        clone.Version = request.Version;
        clone.VersionPolicy = request.VersionPolicy;

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(
                header.Key,
                header.Value);
        }

        foreach (var option in request.Options)
        {
            clone.Options.Set(
                new HttpRequestOptionsKey<object?>(
                    option.Key),
                option.Value);
        }

        if (request.Content is not null)
        {
            var contentBytes =
                await request.Content.ReadAsByteArrayAsync(
                    cancellationToken);

            clone.Content =
                new ByteArrayContent(contentBytes);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(
                    header.Key,
                    header.Value);
            }
        }

        return clone;
    }
}
