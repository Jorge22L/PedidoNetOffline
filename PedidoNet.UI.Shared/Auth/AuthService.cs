namespace PedidoNet.UI.Shared.Auth;

public sealed class AuthService : IAuthService
{
    private readonly AuthApiClient _apiClient;
    private readonly ITokenStorage _tokenStorage;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

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

    public Task LogoutAsync()
    {
        return _tokenStorage.ClearAsync();
    }

    public async Task<LoginResult> RefreshSessionAsync()
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
             * Si el access token vuelve a ser válido,
             * no necesitamos hacer otro refresh.
             */
            if (session.ExpiraEn >
                DateTime.UtcNow.AddMinutes(1))
            {
                return LoginResult.Ok(session);
            }

            var request = new RefreshTokenRequest
            {
                RefreshToken = session.RefreshToken
            };

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

            await _tokenStorage.SaveAsync(response.Data);

            return LoginResult.Ok(response.Data);
        }
        finally
        {
            _refreshLock.Release();
        }
    }
}