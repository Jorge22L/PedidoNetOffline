using PedidoNet.Mobile.Models.Auth;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace PedidoNet.Mobile.Service.Auth
{
    public sealed class MauiTokenStorage : ITokenStorage
    {
        private const string SessionKey = "pedidonet_session";

        public Task ClearAsync()
        {
            SecureStorage.Default.Remove(SessionKey);
            return Task.CompletedTask;
        }

        public async Task<string?> GetAccessTokenAsync()
        {
            var session = await GetAsync();
            return session?.AccessToken;
        }

        public async Task<LoginResponse?> GetAsync()
        {
            var json = await SecureStorage.Default.GetAsync(SessionKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize<LoginResponse>(json);
        }

        public async Task SaveAsync(LoginResponse session)
        {
            var json = JsonSerializer.Serialize(session);

            await SecureStorage.Default.SetAsync(SessionKey, json);
        }
    }
}
