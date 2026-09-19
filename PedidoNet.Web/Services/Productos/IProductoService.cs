using PedidoNet.Web.Models.Productos;
using PedidoNet.UI.Shared.Models.Productos;

namespace PedidoNet.Web.Services.Productos
{
    public interface IProductoService
    {
        Task<List<ProductosDto>> ObtenerTodosAsync(CancellationToken cancellation = default);

        Task<ProductosDto?> ObtenerPorLocalIdAsync(Guid localId, CancellationToken cancellationToken = default);

        Task<Guid> CrearAsync(CrearProductoRequest model, CancellationToken cancellationToken = default);

        Task ActualizarAsync(Guid localId,ActualizarProductoRequest model, CancellationToken cancellationToken = default);

        Task EliminarAsync(Guid localId, CancellationToken cancellationToken = default);

        Task<ProductoImagenDTO> SubirImagenAsync(int productoId, ProductoImageUpload image, CancellationToken cancellationToken = default);

        string ObtenerUrlImagen(string ruta);
    }
}
