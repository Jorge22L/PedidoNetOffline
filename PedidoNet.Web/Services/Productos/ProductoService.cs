using PedidoNet.Web.Models.Productos;
using PedidoNet.Web.Services.Api;

namespace PedidoNet.Web.Services.Productos
{
    public class ProductoService : IProductoService
    {
        private readonly ProductosApiClient _apiClient;
        public ProductoService(ProductosApiClient apiClient)
        {
            _apiClient = apiClient;
        }
        public Task<List<ProductosDto>> GetAllAsync()
        {
            return _apiClient.GetAllSync();
        }
    }
}
