using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Service.Api
{
    public sealed class ApiClient
    {
        private readonly HttpClient _httpClient;

        public ApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("PedidoNetApi");
        }

        public async Task<HttpResponseMessage> GetAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            return await _httpClient.GetAsync(endpoint, cancellationToken);
        }
    }
}
