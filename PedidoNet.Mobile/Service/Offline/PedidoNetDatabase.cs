using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.Mobile.Service.Offline
{
    /*
     * Conexión SQLite única de la app (registrar como Singleton).
     *
     * Las tablas se crean de forma perezosa la primera vez
     * que se necesita la conexión.
     */
    public sealed class PedidoNetDatabase
    {
        private const string DatabaseFileName = "pedidonet.db3";

        private const SQLiteOpenFlags Flags =
            SQLiteOpenFlags.ReadWrite |
            SQLiteOpenFlags.Create |
            SQLiteOpenFlags.SharedCache;

        private readonly SemaphoreSlim _initLock = new(1, 1);

        private SQLiteAsyncConnection? _connection;

        public static string DatabasePath =>
            Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);

        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            if (_connection is not null)
            {
                return _connection;
            }

            await _initLock.WaitAsync();

            try
            {
                if (_connection is not null)
                {
                    return _connection;
                }

                var connection = new SQLiteAsyncConnection(DatabasePath, Flags);

                await connection.CreateTableAsync<ProductoLocalEntity>();
                await connection.CreateTableAsync<ProductoSyncOperationEntity>();

                _connection = connection;

                return _connection;
            }
            finally
            {
                _initLock.Release();
            }
        }
    }
}
