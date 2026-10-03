using PedidoNet.UI.Shared.Offline.Productos;
using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Service.Offline
{
    /*
     * Implementación SQLite (MAUI) de IProductoSyncQueue.
     * Equivale a IndexedDbProductoSyncQueue del proyecto Web.
     */
    public sealed class SqliteProductoSyncQueue : IProductoSyncQueue
    {
        private readonly PedidoNetDatabase _database;

        public SqliteProductoSyncQueue(PedidoNetDatabase database)
        {
            _database = database;
        }

        public Task EnqueueAsync(ProductoSyncOperation operation, CancellationToken cancellationToken = default)
        {
            return SaveAsync(operation, cancellationToken);
        }

        public async Task<List<ProductoSyncOperation>> GetPendingAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = await _database.GetConnectionAsync();

            var entities = await connection
                .Table<ProductoSyncOperationEntity>()
                .OrderBy(x => x.CreatedUtc)
                .ToListAsync();

            return entities
                .Select(x => x.ToModel())
                .ToList();
        }

        public async Task RemoveAsync(Guid operationId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = await _database.GetConnectionAsync();

            await connection.DeleteAsync<ProductoSyncOperationEntity>(operationId);
        }

        public Task UpdateAsync(ProductoSyncOperation operation, CancellationToken cancellationToken = default)
        {
            return SaveAsync(operation, cancellationToken);
        }

        private async Task SaveAsync(ProductoSyncOperation operation, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = await _database.GetConnectionAsync();

            await connection.InsertOrReplaceAsync(
                ProductoSyncOperationEntity.FromModel(operation));
        }
    }
}
