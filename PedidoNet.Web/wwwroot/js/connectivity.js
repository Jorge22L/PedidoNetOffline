window.connectivity = {
    dotNetRef: null,

    initialize: function(dotNetRef){
        this.dotNetRef = dotNetRef;
        this.onlineHandler = () => {
            console.log("Evento ONLINE");
            this.dotNetRef?.invokeMethodAsync("SetOnlineStatus", true);
        }

        this.offlineHandler = () => {
            console.log("Evento OFFLINE");
            this.dotNetRef?.invokeMethodAsync("SetOnlineStatus", false);
        }

        window.addEventListener("online", this.onlineHandler);
        window.addEventListener("offline", this.offlineHandler);

        console.log("Connectivity inicializado. Estado: ", navigator.onLine);

        return navigator.onLine;
    },

    dispose: function () {
        if (this.onlineHandler) {
            window.removeEventListener("online", this.onlineHandler);
        }

        if (this.offlineHandler) {
            window.removeEventListener("offline", this.offlineHandler);
        }

        this.dotNetRef = null;
    }

    

}