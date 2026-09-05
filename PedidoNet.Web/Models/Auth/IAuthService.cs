namespace PedidoNet.Web.Models.Auth
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(LoginRequest request);
        Task LogoutAsync();
        Task<LoginResponse?> GetSessionAsync();
    }
}
