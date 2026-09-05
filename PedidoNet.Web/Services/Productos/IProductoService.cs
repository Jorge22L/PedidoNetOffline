using PedidoNet.Web.Models.Productos;

namespace PedidoNet.Web.Services.Productos
{
    public interface IProductoService
    {
        Task<List<ProductosDto>> ObtenerTodosAsync();

        Task<ProductosDto?> ObtenerPorIdAsync(int id);

        Task CrearAsync(CrearProductoRequest model);

        Task ActualizarAsync(int id,ActualizarProductoRequest model);

        Task EliminarAsync(int id);
    }
}
