using System;
using System.Collections.Generic;
using System.Text;

namespace PedidoNet.UI.Shared.Offline
{
    public enum SyncStatus
    {
        Synced = 0,
        PendingCreate = 1,
        PendingUpdate = 2,
        PendingDelete = 3,
        Failed = 4,
    }
}
