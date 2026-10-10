// Vista previa, impresión y descarga de reportes (Blob URL en un iframe).
window.reportViewer = {

    show: async function (frameId, streamRef, contentType) {
        const frame = document.getElementById(frameId);
        if (!frame) return;

        const buffer = await streamRef.arrayBuffer();
        const blob = new Blob([buffer], { type: contentType });

        if (frame.dataset.url) {
            URL.revokeObjectURL(frame.dataset.url);
        }

        const url = URL.createObjectURL(blob);
        frame.dataset.url = url;
        frame.src = url;
    },

    print: function (frameId) {
        const frame = document.getElementById(frameId);
        if (!frame || !frame.dataset.url) return;

        try {
            // Blob URL = mismo origen: se puede imprimir solo el contenido del iframe.
            frame.contentWindow.focus();
            frame.contentWindow.print();
        } catch {
            // Algunos visores de PDF no lo permiten: se abre en otra pestaña.
            window.open(frame.dataset.url, "_blank");
        }
    },

    download: async function (fileName, streamRef, contentType) {
        const buffer = await streamRef.arrayBuffer();
        const blob = new Blob([buffer], { type: contentType });
        const url = URL.createObjectURL(blob);

        const link = document.createElement("a");
        link.href = url;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        link.remove();

        setTimeout(() => URL.revokeObjectURL(url), 1000);
    },

    clear: function (frameId) {
        const frame = document.getElementById(frameId);
        if (frame && frame.dataset.url) {
            URL.revokeObjectURL(frame.dataset.url);
            delete frame.dataset.url;
        }
    }
};