// MSAL.js (msal-browser v3) como módulo ES desde jsDelivr.
// Si prefieres no depender de la CDN, descarga el paquete y sírvelo desde wwwroot/lib.
import {
    PublicClientApplication,
    InteractionRequiredAuthError
} from "https://cdn.jsdelivr.net/npm/@azure/msal-browser@3/+esm";

let client = null;
let clientKey = null;

async function getClient(clientId, tenantId) {
    const key = `${clientId}|${tenantId}`;

    if (client && clientKey === key) {
        return client;
    }

    client = new PublicClientApplication({
        auth: {
            clientId: clientId,
            authority: `https://login.microsoftonline.com/${tenantId}`,
            // Debe coincidir con la URI registrada en PedidoNet-Web (SPA)
            redirectUri: new URL("blank.html", document.baseURI).href
        },
        cache: {
            // La sesión propia de PedidoNet ya vive en localStorage;
            // la cuenta de Microsoft solo se recuerda mientras dure la pestaña.
            cacheLocation: "sessionStorage"
        }
    });

    await client.initialize();

    clientKey = key;

    return client;
}

/**
 * Devuelve el access token de Entra ID para la API, o null si el usuario cerró el popup.
 */
export async function acquireToken(clientId, tenantId, scope) {
    const pca = await getClient(clientId, tenantId);

    const request = { scopes: [scope] };

    const account = pca.getAllAccounts()[0];

    // 1. Sin interfaz si ya hubo login en esta pestaña.
    if (account) {
        try {
            const silent = await pca.acquireTokenSilent({ ...request, account });
            return silent.accessToken;
        } catch (error) {
            if (!(error instanceof InteractionRequiredAuthError)) {
                console.warn("MSAL silent:", error);
            }
        }
    }

    // 2. Popup de Microsoft.
    try {
        const result = await pca.acquireTokenPopup({ ...request, prompt: "select_account" });
        return result.accessToken;
    } catch (error) {
        if (error?.errorCode === "user_cancelled") {
            return null;
        }

        // popup_window_error = el navegador bloqueó el popup
        throw new Error(error?.errorCode
            ? `${error.errorCode}: ${error.errorMessage}`
            : String(error));
    }
}

/**
 * Borra la cuenta de Microsoft de la caché local SIN cerrar la sesión
 * del usuario en Microsoft (no navega ni abre ventanas).
 */
export async function signOut() {
    if (!client) {
        return;
    }

    const account = client.getAllAccounts()[0];

    if (account) {
        await client.logoutRedirect({
            account,
            onRedirectNavigate: () => false
        });
    }
}