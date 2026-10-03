using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace PedidoNet.UI.Shared.Auth;

/*
 * Estado de autenticación compartido (Web y Mobile).
 *
 * La sesión se considera válida mientras exista un refresh token
 * vigente: el access token puede estar vencido (por ejemplo sin
 * conexión) y AuthenticatedHttpHandler lo renovará cuando haya red.
 * Esto permite que Mobile siga mostrando Productos offline después
 * de haber iniciado sesión.
 */
public sealed class PedidoNetAuthenticationStateProvider : AuthenticationStateProvider
{
    private const string AuthenticationType = "PedidoNet";

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ITokenStorage _tokenStorage;

    // Último estado entregado a la UI (null = todavía no se calculó).
    private bool? _lastIsAuthenticated;

    public PedidoNetAuthenticationStateProvider(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var state = await BuildStateAsync();

        _lastIsAuthenticated = IsAuthenticated(state);

        return state;
    }

    /// <summary>
    /// Vuelve a leer la sesión y notifica SIEMPRE a la UI.
    /// Usar después de login o logout (nunca desde OnInitialized).
    /// </summary>
    public async Task NotifySessionChangedAsync()
    {
        var state = await GetAuthenticationStateAsync();

        /*
         * Se notifica con una tarea YA completada: si se pasa una tarea
         * pendiente, AuthorizeRouteView muestra <Authorizing>, destruye
         * la página actual y la vuelve a crear.
         */
        NotifyAuthenticationStateChanged(Task.FromResult(state));
    }

    /// <summary>
    /// Vuelve a leer la sesión y notifica SOLO si el estado cambió
    /// (por ejemplo, la API invalidó la sesión y se borró del storage).
    /// Seguro de llamar al inicializar una página.
    /// </summary>
    public async Task SyncSessionStateAsync()
    {
        var previous = _lastIsAuthenticated;

        var state = await GetAuthenticationStateAsync();

        if (previous.HasValue &&
            previous.Value != IsAuthenticated(state))
        {
            NotifyAuthenticationStateChanged(Task.FromResult(state));
        }
    }

    private static bool IsAuthenticated(AuthenticationState state)
    {
        return state.User.Identity?.IsAuthenticated == true;
    }

    private async Task<AuthenticationState> BuildStateAsync()
    {
        LoginResponse? session;

        try
        {
            session = await _tokenStorage.GetAsync();
        }
        catch (Exception)
        {
            // Almacenamiento no disponible o sesión corrupta: se trata como anónimo.
            return Anonymous;
        }

        if (!IsValidSession(session))
        {
            return Anonymous;
        }

        return new AuthenticationState(CreatePrincipal(session!));
    }

    public static bool IsValidSession(LoginResponse? session)
    {
        return session is not null &&
               !string.IsNullOrWhiteSpace(session.AccessToken) &&
               !string.IsNullOrWhiteSpace(session.RefreshToken) &&
               session.RefreshTokenExpiraEn > DateTime.UtcNow;
    }

    private static ClaimsPrincipal CreatePrincipal(LoginResponse session)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, session.NombreUsuario)
        };

        if (!string.IsNullOrWhiteSpace(session.Rol))
        {
            claims.Add(new Claim(ClaimTypes.Role, session.Rol));
        }

        var identity = new ClaimsIdentity(
            claims,
            AuthenticationType,
            ClaimTypes.Name,
            ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }
}
