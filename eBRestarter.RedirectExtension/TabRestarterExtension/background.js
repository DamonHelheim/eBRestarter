chrome.runtime.onInstalled.addListener(() => {
    chrome.storage.local.set({ isActive: true });
    chrome.alarms.create("master_heartbeat", { periodInMinutes: 1 });
    checkAndCleanTabs();
});

chrome.runtime.onStartup.addListener(() => {
    chrome.alarms.create("master_heartbeat", { periodInMinutes: 1 });
    checkAndCleanTabs();
});

function updateIcon(isActive) {
    const path = isActive
        ? { "16": "icons/eBRestarter_Redirect_On_small.png", "32": "icons/eBRestarter_Redirect_On_small.png" }
        : { "16": "icons/icon16_off.png", "32": "icons/icon32_off.png" };
    chrome.action.setIcon({ path: path });
}

chrome.storage.onChanged.addListener((changes, namespace) => {
    if (namespace === 'local' && changes.isActive !== undefined) {
        updateIcon(changes.isActive.newValue);
        if (changes.isActive.newValue) {
            chrome.alarms.create("master_heartbeat", { periodInMinutes: 1 });
            checkAndCleanTabs();
        } else {
            chrome.alarms.clearAll(); // Wenn aus, alle Wecker löschen
        }
    }
});

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
// KUGELSICHERER KIOSK-MODUS & TAB-KILLER
// =========================================================================
let isEnforcing = false;

function forceSingleTabRedirect(newUrl) {
    if (isEnforcing) return;
    isEnforcing = true;

    // Failsafe: Schloss auf jeden Fall nach 5 Sekunden öffnen, falls Chrome einfriert
    setTimeout(() => { isEnforcing = false; }, 5000);

    console.log(`Erzwinge Redirect auf: ${newUrl}. Schließe alle alten Tabs.`);

    // 1. Zuerst ALLE aktuell offenen Tabs aufschreiben
    chrome.tabs.query({}, (oldTabs) => {
        const oldTabIds = oldTabs.map(tab => tab.id);

        // 2. Den neuen Tab öffnen
        chrome.tabs.create({ url: newUrl }, (newTab) => {
            // Timer für den frisch geschlüpften Tab direkt scharfschalten
            resetTabTimer(newTab.id);

            // 3. Wenn es alte Tabs gab, diese gnadenlos löschen
            if (oldTabIds.length > 0) {
                chrome.tabs.remove(oldTabIds, () => {
                    let ignoreError = chrome.runtime.lastError;

                    // 4. Doppelter Check nach 1 Sekunde! (Falls alte Tabs hängen geblieben sind)
                    setTimeout(() => {
                        chrome.tabs.query({}, (checkTabs) => {
                            // Finde alle Tabs, die NICHT unser neuer Tab sind
                            const strayTabs = checkTabs.filter(t => t.id !== newTab.id).map(t => t.id);
                            if (strayTabs.length > 0) {
                                console.log("Stray Tabs gefunden! Trete nochmal nach...");
                                chrome.tabs.remove(strayTabs);
                            }
                            isEnforcing = false; // Reguläres Entsperren
                        });
                    }, 1000);
                });
            } else {
                isEnforcing = false;
            }
        });
    });
}

// Sucht nach unerlaubten Zweit-Tabs (z.B. beim Start oder durch Popups)
async function checkAndCleanTabs() {
    if (isEnforcing) return;
    const result = await chrome.storage.local.get(['isActive']);
    if (result.isActive === false) return;

    chrome.tabs.query({}, async (tabs) => {
        if (tabs.length > 1) {
            const config = await getActiveSettings();
            if (config) {
                console.log("Sanity-Check: Mehr als 1 Tab offen! Bereinige...");
                forceSingleTabRedirect(config.ZIEL_URL);
            }
        } else if (tabs.length === 1) {
            // Wenn alles sauber ist (1 Tab), sicherstellen dass sein Wecker läuft!
            startTabTimerIfNotRunning(tabs[0].id);
        }
    });
}

chrome.tabs.onCreated.addListener((tab) => {
    checkAndCleanTabs();
});

// =========================================================================
// UNZERSTÖRBARER SYSTEM-WECKER (Chrome Alarms)
// =========================================================================

async function resetTabTimer(tabId) {
    const config = await getActiveSettings();
    if (!config) return;
    // Chrome Alarms brauchen mindestens 1 Minute als Wert.
    const minutes = Math.max(1, config.WARTEZEIT_MS / 60000);
    chrome.alarms.create(`timeout_${tabId}`, { delayInMinutes: minutes });
    console.log(`Wecker für Tab ${tabId} auf ${minutes} Minuten gesetzt.`);
}

async function startTabTimerIfNotRunning(tabId) {
    const alarm = await chrome.alarms.get(`timeout_${tabId}`);
    if (!alarm) {
        resetTabTimer(tabId);
    }
}

chrome.tabs.onUpdated.addListener((tabId, changeInfo, tab) => {
    // Wenn die Seite die URL wechselt (erfolgreiche Weiterleitung etc.),
    // setzen wir den Wecker frisch auf volle z.B. 5 Minuten zurück!
    if (changeInfo.url) {
        resetTabTimer(tabId);
    }
});

chrome.tabs.onRemoved.addListener((tabId) => {
    chrome.alarms.clear(`timeout_${tabId}`);
});

// Wenn der Systemwecker klingelt (Zeit abgelaufen)
chrome.alarms.onAlarm.addListener(async (alarm) => {
    const result = await chrome.storage.local.get(['isActive']);
    if (result.isActive === false) return;

    // 1. Der minütliche Herzschlag-Check
    if (alarm.name === "master_heartbeat") {
        checkAndCleanTabs();
        return;
    }

    // 2. Das Zeitlimit des Tabs ist abgelaufen (Loop oder Stillstand)
    if (alarm.name.startsWith('timeout_')) {
        console.log(`Zeit abgelaufen (${alarm.name})! Tab wird zwangserneuert.`);
        const config = await getActiveSettings();
        if (config) {
            forceSingleTabRedirect(config.ZIEL_URL);
        }
    }
});

// Echte Fehler sofort abfangen (DNS-Ausfall etc.)
chrome.webNavigation.onErrorOccurred.addListener(async (details) => {
    if (details.frameId === 0) {
        const result = await chrome.storage.local.get(['isActive']);
        if (result.isActive) {
            const config = await getActiveSettings();
            if (config) {
                console.log("Navigation Error erkannt! Bereinige sofort.");
                forceSingleTabRedirect(config.ZIEL_URL);
            }
        }
    }
});