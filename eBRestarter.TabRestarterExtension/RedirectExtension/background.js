chrome.runtime.onInstalled.addListener(() => {
    chrome.storage.local.set({ isActive: true });
});

function updateIcon(isActive) {
    const path = isActive
        ? { "16": "icons/eBRestarter_Redirect_On_small.png", "32": "icons/eBRestarter_Redirect_On_small.png" }
        : { "16": "icons/icon16_off.png", "32": "icons/icon32_off.png" }; // Passe die Pfade für OFF an, falls du eigene hast
    chrome.action.setIcon({ path: path });
}

// Lauscht auf Änderungen am Schalter (An/Aus)
chrome.storage.onChanged.addListener((changes, namespace) => {
    if (namespace === 'local' && changes.isActive !== undefined) {
        updateIcon(changes.isActive.newValue);
    }
});

// Zieht sich die Einstellungen (entweder aus der UI oder der config.json)
async function getActiveSettings() {
    const uiData = await chrome.storage.local.get(['useCustom', 'customUrl', 'customTimeMin']);

    if (uiData.useCustom && uiData.customUrl && uiData.customTimeMin) {
        return {
            ZIEL_URL: uiData.customUrl,
            WARTEZEIT_MS: uiData.customTimeMin * 60000
        };
    }

    try {
        const response = await fetch(chrome.runtime.getURL('config.json'));
        const config = await response.json();
        if (!config.ZIEL_URL.startsWith('http://') && !config.ZIEL_URL.startsWith('https://')) {
            config.ZIEL_URL = 'https://' + config.ZIEL_URL;
        }
        return config;
    } catch (error) { return null; }
}

// =========================================================================
// NEU: WATCHDOG FÜR HÄNGENDE LADEVORGÄNGE UND WEITERLEITUNGSSCHLEIFEN
// =========================================================================
const loadingTimers = {};

chrome.tabs.onUpdated.addListener(async (tabId, changeInfo, tab) => {

    // 1. Wenn der Tab anfängt zu laden (Spinner dreht sich)
    if (changeInfo.status === 'loading') {
        const result = await chrome.storage.local.get(['isActive']);
        if (result.isActive === false) return; // Wenn Erweiterung aus ist, nichts tun

        const config = await getActiveSettings();
        if (!config) return;

        // Alten Timer löschen, falls sich der Tab innerhalb der Wartezeit neu lädt
        if (loadingTimers[tabId]) {
            clearTimeout(loadingTimers[tabId]);
        }

        // Notfall-Timer starten
        loadingTimers[tabId] = setTimeout(() => {
            console.log(`Tab ${tabId} hängt im Ladevorgang! Forciere Umleitung nach: ${config.ZIEL_URL}`);
            chrome.tabs.update(tabId, { url: config.ZIEL_URL });
            delete loadingTimers[tabId];
        }, config.WARTEZEIT_MS);
    }

    // 2. Wenn der Tab ERFOLGREICH fertig geladen hat
    else if (changeInfo.status === 'complete') {
        // Watchdog-Timer abbrechen.
        // Ab hier übernimmt die content.js auf der eigentlichen Webseite.
        if (loadingTimers[tabId]) {
            clearTimeout(loadingTimers[tabId]);
            delete loadingTimers[tabId];
        }
    }
});

// Aufräumen: Wenn der Nutzer den Tab manuell schließt, bevor er fertig geladen hat
chrome.tabs.onRemoved.addListener((tabId) => {
    if (loadingTimers[tabId]) {
        clearTimeout(loadingTimers[tabId]);
        delete loadingTimers[tabId];
    }
});
// =========================================================================

// Der klassische Fehler-Abfänger (z.B. DNS-Fehler, keine Internetverbindung)
chrome.webNavigation.onErrorOccurred.addListener(async (details) => {
    if (details.frameId === 0) { // Nur Haupt-Tabs, keine iFrames
        const result = await chrome.storage.local.get(['isActive']);
        if (result.isActive) {
            const config = await getActiveSettings();
            if (config) {
                console.log("Fehler erkannt! Leite um nach: " + config.ZIEL_URL);
                chrome.tabs.update(details.tabId, { url: config.ZIEL_URL });
            }
        }
    }
});