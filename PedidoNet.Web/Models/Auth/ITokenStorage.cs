namespace PedidoNet.Web.Models.Auth
{
    public interface ITokenStorage
    {
        Task SaveAsync(LoginResponse session);
        Task<LoginResponse?> GetAsync();
        Task<string?> GetAccessTokenAsync();
        Task ClearAsync();
    }
}
