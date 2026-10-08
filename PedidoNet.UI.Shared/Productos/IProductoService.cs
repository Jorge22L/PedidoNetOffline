using PedidoNet.UI.Shared.Models.Productos;

namespace PedidoNet.UI.Shared.Productos
{
    public interface IProductoService
    {
        Task<List<ProductosDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);

        Task<ProductosDto?> ObtenerPorLocalIdAsync(Guid localId, CancellationToken cancellationToken = default);

        Task<Guid> CrearAsync(CrearProductoRequest model, CancellationToken cancellationToken = default);

        Task ActualizarAsync(Guid localId, ActualizarProductoRequest model, CancellationToken cancellationToken = default);

        Task EliminarAsync(Guid localId, CancellationToken cancellationToken = default);

        Task<ProductoImagenDTO> SubirImagenAsync(int productoId, ProductoImageUpload image, CancellationToken cancellationToken = default);

        /// <summary>Imágenes del producto (requiere conexión; no se guardan offline).</summary>
        Task<List<ProductoImagenDTO>> ObtenerImagenesAsync(int productoId, CancellationToken cancellationToken = default);

        Task EliminarImagenAsync(int productoId, int productoImagenId, CancellationToken cancellationToken = default);

        string ObtenerUrlImagen(string ruta);
    }
}
