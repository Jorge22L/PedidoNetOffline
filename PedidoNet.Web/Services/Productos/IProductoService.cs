using PedidoNet.Web.Models.Productos;

namespace PedidoNet.Web.Services.Productos
{
    public interface IProductoService
    {
        Task<List<ProductosDto>> GetAllAsync();
    }
}
