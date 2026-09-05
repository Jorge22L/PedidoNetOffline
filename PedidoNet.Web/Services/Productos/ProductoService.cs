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
        public Task<List<ProductosDto>> ObtenerTodosAsync()
        => _apiClient.GetAllSync();

        public Task<ProductosDto?> ObtenerPorIdAsync(int id)
            => _apiClient.GetByIdAsync(id);

        public Task CrearAsync(CrearProductoRequest model)
            => _apiClient.CreateAsync(model);

        public Task ActualizarAsync(
            int id,
            ActualizarProductoRequest model)
            => _apiClient.UpdateAsync(id, model);

        public Task EliminarAsync(int id)
            => _apiClient.DeleteAsync(id);
    }
}
