using PedidoNet.UI.Shared.Offline.Productos;
using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Service.Offline
{
    /*
     * Implementación SQLite (MAUI) de IProductoOfflineStore.
     * Equivale a IndexedDbProductoStore del proyecto Web.
     */
    public sealed class SqliteProductoStore : IProductoOfflineStore
    {
        private readonly PedidoNetDatabase _database;

        public SqliteProductoStore(PedidoNetDatabase database)
        {
            _database = database;
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = await _database.GetConnectionAsync();

            await connection.DeleteAllAsync<ProductoLocalEntity>();
        }

        public async Task DeleteAsync(Guid localId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = await _database.GetConnectionAsync();

            await connection.DeleteAsync<ProductoLocalEntity>(localId);
        }

        public async Task<List<ProductoLocal>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = await _database.GetConnectionAsync();

            var entities = await connection
                .Table<ProductoLocalEntity>()
                .ToListAsync();

            return entities
                .Select(x => x.ToModel())
                .ToList();
        }

        public async Task<ProductoLocal?> GetByLocalIdAsync(Guid localId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = await _database.GetConnectionAsync();

            var entity = await connection
                .FindAsync<ProductoLocalEntity>(localId);

            return entity?.ToModel();
        }

        public async Task<ProductoLocal?> GetByserverIdAsync(int productoId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = await _database.GetConnectionAsync();

            int? serverId = productoId;

            var entity = await connection
                .Table<ProductoLocalEntity>()
                .Where(x => x.ProductoId == serverId)
                .FirstOrDefaultAsync();

            return entity?.ToModel();
        }

        public async Task UpsertAsync(ProductoLocal producto, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = await _database.GetConnectionAsync();

            await connection.InsertOrReplaceAsync(
                ProductoLocalEntity.FromModel(producto));
        }

        public async Task UpsertRangeAsync(IEnumerable<ProductoLocal> productos, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = await _database.GetConnectionAsync();

            var entities = productos
                .Select(ProductoLocalEntity.FromModel)
                .ToList();

            await connection.RunInTransactionAsync(db =>
            {
                foreach (var entity in entities)
                {
                    db.InsertOrReplace(entity);
                }
            });
        }
    }
}
