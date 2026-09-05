using PedidoNet.Web.Services.Api;

namespace PedidoNet.Web.Models.Auth
{
    public class AuthService : IAuthService
    {
        private readonly AuthApiClient _apiClient;
        private readonly ITokenStorage _tokenStorage;

        public AuthService(AuthApiClient apiClient, ITokenStorage tokenStorage)
        {
            _apiClient = apiClient;
            _tokenStorage = tokenStorage;
        }
        public Task<LoginResponse?> GetSessionAsync()
        {
            return _tokenStorage.GetAsync();
        }

        public async Task<bool> LoginAsync(LoginRequest request)
        {
            var response = await _apiClient.LoginAsync(request);
            if(response is null || !response.Success || response.Data is null)
            {
                return false;
            }

            await _tokenStorage.SaveAsync(response.Data);

            return true;
        }

        public Task LogoutAsync()
        {
            return _tokenStorage.ClearAsync();
        }
    }
}
