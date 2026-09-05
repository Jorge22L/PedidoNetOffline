window.connectivity = {
    dotNetRef: null,

    initialize: function(dotNetRef){
        this.dotNetRef = dotNetRef;

        window.addEventListener("online", this.handleOnline);
        window.addEventListener("offline", this.handleOffline);

        return navigator.onLine;
    },

    handleOnline: function(){
        if(window.connectivity.dotNetRef){
            window.connectivity.dotNetRef.invokeMethodAsync("SetOnlineStatus", true);
        }
    },

    handleOffline: function () {
        if (window.connectivity.dotNetRef) {
            window.connectivity.dotNetRef.invokeMethodAsync("SetOnlineStatus", false);
        }
    },

    dispose: function () {
        window.removeEventListener("online", this.handleOnline);

        window.removeEventListener("offline", this.handleOffline);

        this.dotNetRef = null
    }

}