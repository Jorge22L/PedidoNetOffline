using PedidoNet.UI.Shared.Offline.Productos;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Service.Offline
{
    [Table("ProductoSyncQueue")]
    public sealed class ProductoSyncOperationEntity
    {
        [PrimaryKey]
        public Guid OperationId { get; set; }

        [Indexed]
        public Guid ProductoLocalId { get; set; }

        public ProductoSyncOperationType Type { get; set; }

        public DateTime CreatedUtc { get; set; }

        public int RetryCount { get; set; }

        public string? LastError { get; set; }

        public bool RequiresAttention { get; set; }

        public static ProductoSyncOperationEntity FromModel(ProductoSyncOperation operation)
        {
            return new ProductoSyncOperationEntity
            {
                OperationId = operation.OperationId,
                ProductoLocalId = operation.ProductoLocalId,
                Type = operation.Type,
                CreatedUtc = operation.CreatedUtc,
                RetryCount = operation.RetryCount,
                LastError = operation.LastError,
                RequiresAttention = operation.RequiresAttention
            };
        }

        public ProductoSyncOperation ToModel()
        {
            return new ProductoSyncOperation
            {
                OperationId = OperationId,
                ProductoLocalId = ProductoLocalId,
                Type = Type,
                CreatedUtc = CreatedUtc,
                RetryCount = RetryCount,
                LastError = LastError,
                RequiresAttention = RequiresAttention
            };
        }
    }
}
