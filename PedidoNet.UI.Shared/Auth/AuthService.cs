namespace PedidoNet.UI.Shared.Auth;

public sealed class AuthService : IAuthService
{
    private static readonly TimeSpan RefreshMargin =
        TimeSpan.FromMinutes(1);

    /*
     * El lock es estático porque IHttpClientFactory resuelve
     * AuthenticatedHttpHandler en su propio scope, por lo que
     * pueden existir varias instancias de AuthService al mismo
     * tiempo. Si cada una tuviera su propio lock, dos refresh
     * simultáneos usarían el mismo refresh token y el segundo
     * fallaría porque la API lo rota (lo revoca) en el primero.
     */
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

    private readonly AuthApiClient _apiClient;
    private readonly ITokenStorage _tokenStorage;

    public AuthService(
        AuthApiClient apiClient,
        ITokenStorage tokenStorage)
    {
        _apiClient = apiClient;
        _tokenStorage = tokenStorage;
    }

    public Task<LoginResponse?> GetSessionAsync()
    {
        return _tokenStorage.GetAsync();
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        var response = await _apiClient.LoginAsync(request);

        if (response is null)
        {
            return LoginResult.Fail(
                "No se recibió respuesta del servidor.");
        }

        if (!response.Success || response.Data is null)
        {
            return LoginResult.Fail(
                response.Message ?? "No fue posible iniciar sesión.");
        }

        await _tokenStorage.SaveAsync(response.Data);

        return LoginResult.Ok(response.Data);
    }

    public async Task<LoginResult> LoginExternoAsync(string externalAccessToken)
    {
        var response = await _apiClient.LoginEntraAsync(externalAccessToken);

        if (response is null)
        {
            return LoginResult.Fail(
                "No se recibió respuesta del servidor.");
        }

        if (!response.Success || response.Data is null)
        {
            return LoginResult.Fail(
                response.Message ?? "No fue posible iniciar sesión con Microsoft.");
        }

        // Misma sesión que el login normal: refresh, roles y offline siguen igual.
        await _tokenStorage.SaveAsync(response.Data);

        return LoginResult.Ok(response.Data);
    }

    public async Task LogoutAsync()
    {
        var session = await _tokenStorage.GetAsync();

        await _tokenStorage.ClearAsync();

        if (session is null ||
            string.IsNullOrWhiteSpace(session.RefreshToken))
        {
            return;
        }

        try
        {
            /*
             * Revocar en el servidor es "best effort":
             * si no hay conexión la sesión local ya fue eliminada
             * y el refresh token expirará por sí solo.
             */
            using var timeout = new CancellationTokenSource(
                TimeSpan.FromSeconds(5));

            await _apiClient.RevokeAsync(new RefreshTokenRequest
            {
                RefreshToken = session.RefreshToken
            }, timeout.Token);
        }
        catch (Exception)
        {
        }
    }

    public async Task<LoginResult> RefreshSessionAsync(
        string? failedAccessToken = null)
    {
        await _refreshLock.WaitAsync();

        try
        {
            var session = await _tokenStorage.GetAsync();

            if (session is null)
            {
                return LoginResult.Fail(
                    "No existe una sesión almacenada.");
            }

            if (string.IsNullOrWhiteSpace(session.RefreshToken))
            {
                return LoginResult.Fail(
                    "La sesión no contiene un refresh token.");
            }

            if (session.RefreshTokenExpiraEn <= DateTime.UtcNow)
            {
                await _tokenStorage.ClearAsync();

                return LoginResult.Fail(
                    "La sesión ha expirado.");
            }

            /*
             * Otra petición pudo haber renovado la sesión
             * mientras esperábamos el lock.
             *
             * - Refresh preventivo: si el access token vuelve a
             *   ser válido no necesitamos otro refresh.
             * - Refresh por 401: si el token almacenado ya no es
             *   el que la API rechazó, alguien más lo renovó.
             */
            var alreadyRefreshed = failedAccessToken is null
                ? session.ExpiraEn > DateTime.UtcNow.Add(RefreshMargin)
                : !string.Equals(
                    session.AccessToken,
                    failedAccessToken,
                    StringComparison.Ordinal);

            if (alreadyRefreshed)
            {
                return LoginResult.Ok(session);
            }

            var request = new RefreshTokenRequest
            {
                RefreshToken = session.RefreshToken
            };

            /*
             * Errores de red, 429 o 5xx lanzan HttpRequestException
             * desde AuthApiClient. NO se borra la sesión en esos
             * casos: el refresh token sigue siendo válido y se
             * reintentará cuando la API vuelva a estar disponible.
             */
            var response =
                await _apiClient.RefreshAsync(request);

            if (response is null)
            {
                return LoginResult.Fail(
                    "No se recibió respuesta al renovar la sesión.");
            }

            if (!response.Success ||
                response.Data is null)
            {
                await _tokenStorage.ClearAsync();

                return LoginResult.Fail(
                    response.Message ??
                    "No fue posible renovar la sesión.");
            }

            // La API rota el refresh token: se guarda la nueva sesión completa.
            await _tokenStorage.SaveAsync(response.Data);

            return LoginResult.Ok(response.Data);
        }
        finally
        {
            _refreshLock.Release();
        }
    }
}
