window.productoDb = (() => {
    const dbName = "PedidoNetOffline"
    const version = 1

    const productoStore = "productos"
    const syncStore = "productoSyncQueue"

    function openDb() {
        return new Promise((resolve, reject) => {
            const request = indexedDB.open(dbName, version)

            request.onupgradeneeded = event => {
                const db = event.target.result

                if (!db.objectStoreNames.contains(productoStore)) {
                    const store = db.createObjectStore(productoStore, { keyPath: "localId" })

                    store.createIndex("productoId", "productoId", { unique:false })
                }

                if (!db.objectStoreNames.contains(syncStore)) {
                    db.createObjectStore(syncStore, { keyPath: "operationId" })
                }
            }
            request.onsuccess = () => resolve(request.result)

            request.onerror = () => reject(request.error)
        })
    }

    async function getAll(storeName) {
        const db = await openDb()

        return new Promise((resolve, reject) => {
            const transaction = db.transaction(storeName, "readonly")

            const store = transaction.objectStore(storeName)

            const request = store.getAll()

            request.onsuccess = () => resolve(request.result)

            request.onerror = () => reject(request.error)
        })
    }

    async function get(storeName, key) {
        const db = await openDb()

        return new Promise((resolve, reject) => {
            const transaction = db.transaction(storeName, "readonly")

            const store = transaction.objectStore(storeName)

            const request = store.get(key)

            request.onsuccess = () => resolve(request.result ?? null)

            request.onerror = () => reject(request.error)
        })
    }

    async function put(storeName, value) {
        const db = await openDb()

        return new Promise((resolve, reject) => {
            const transaction = db.transaction(storeName, "readwrite")

            const store = transaction.objectStore(storeName)

            const request = store.put(value)

            request.onsuccess = () => resolve()

            request.onerror = () => reject(request.error)
        })
    }

    async function remove(storeName, key) {
        const db = await openDb()

        return new Promise((resolve, reject) => {
            const transaction = db.transaction(storeName, "readwrite")

            const store = transaction.objectStore(storeName)

            const request = store.delete(key)

            request.onsuccess = () => resolve()

            request.onerror = () => reject(request.error)
        })
    }

    async function clear(storeName) {
        const db = await openDb()

        return new Promise((resolve, reject) => {
            const transaction = db.transaction(storeName, "readwrite")

            const store = transaction.objectStore(storeName)

            const request = store.clear()

            request.onsuccess = () => resolve()

            request.onerror = () => reject(request.error)
        })
    }

    return {
        getProductos: () => getAll(productoStore),

        getProducto: localId => get(productoStore, localId),

        putProducto: producto => put(productoStore, producto),

        deleteProducto: localId => remove(productoStore, localId),

        clearProductos: () => clear(productoStore),

        getSyncOperations: () => getAll(syncStore),

        putSyncOperation: operation => put(syncStore, operation),

        deleteSyncOperation: operationId => remove(syncStore, operationId)
    }
})()