using PedidoNet.UI.Shared.Offline;
using PedidoNet.UI.Shared.Offline.Productos;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Service.Offline
{
    /*
     * Fila SQLite de Producto.
     *
     * Se mantiene separada de ProductoLocal (RCL) para no
     * acoplar el modelo compartido a los atributos de sqlite-net.
     */
    [Table("Productos")]
    public sealed class ProductoLocalEntity
    {
        [PrimaryKey]
        public Guid LocalId { get; set; }

        [Indexed]
        public int? ProductoId { get; set; }

        [MaxLength(20)]
        public string? Codigo { get; set; }

        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public decimal PrecioVenta { get; set; }

        public int Existencias { get; set; }

        public bool? TieneIVA { get; set; }

        public bool? TieneISC { get; set; }

        public SyncStatus SyncStatus { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime LastModifiedUtc { get; set; }

        public static ProductoLocalEntity FromModel(ProductoLocal producto)
        {
            return new ProductoLocalEntity
            {
                LocalId = producto.LocalId,
                ProductoId = producto.ProductoId,
                Codigo = producto.Codigo,
                Nombre = producto.Nombre,
                PrecioVenta = producto.PrecioVenta,
                Existencias = producto.Existencias,
                TieneIVA = producto.TieneIVA,
                TieneISC = producto.TieneISC,
                SyncStatus = producto.SyncStatus,
                IsDeleted = producto.isDeleted,
                LastModifiedUtc = producto.LastModifiedUtc
            };
        }

        public ProductoLocal ToModel()
        {
            return new ProductoLocal
            {
                LocalId = LocalId,
                ProductoId = ProductoId,
                Codigo = Codigo,
                Nombre = Nombre,
                PrecioVenta = PrecioVenta,
                Existencias = Existencias,
                TieneIVA = TieneIVA,
                TieneISC = TieneISC,
                SyncStatus = SyncStatus,
                isDeleted = IsDeleted,
                LastModifiedUtc = LastModifiedUtc
            };
        }
    }
}
