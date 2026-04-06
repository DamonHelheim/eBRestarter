// Der Timer wird jetzt absolut zuverlässig im Hintergrund (background.js)
// über die Chrome Alarms API gesteuert.
// Dieses Skript ist vorerst inaktiv.

//let timerId = null;

//async function getActiveSettings() {
//    const uiData = await chrome.storage.local.get(['useCustom', 'customUrl', 'customTimeMin']);

//    if (uiData.useCustom && uiData.customUrl && uiData.customTimeMin) {
//        console.log("Nutze Werte aus dem Optionsmenü.");
//        return {
//            ZIEL_URL: uiData.customUrl,
//            WARTEZEIT_MS: uiData.customTimeMin * 60000
//        };
//    }

//    console.log("Nutze Werte aus config.json (C#-Programm).");
//    try {
//        const response = await fetch(chrome.runtime.getURL('config.json'));
//        const config = await response.json();

//        if (!config.ZIEL_URL.startsWith('http://') && !config.ZIEL_URL.startsWith('https://')) {
//            config.ZIEL_URL = 'https://' + config.ZIEL_URL;
//        }
//        return config;
//    } catch (error) {
//        console.error("Fehler beim Laden der config.json!", error);
//        return null;
//    }
//}

//async function handleTimer(isActive) {
//    if (isActive) {
//        if (!timerId) {
//            const config = await getActiveSettings();
//            if (!config) return;

//            console.log(`Timer gestartet. Warte ${config.WARTEZEIT_MS} ms. Ziel: ${config.ZIEL_URL}`);
//            timerId = setTimeout(() => {
//                // NEU: Statt den Tab neu zu laden, funken wir die Background.js an!
//                chrome.runtime.sendMessage({
//                    action: "replaceTab",
//                    url: config.ZIEL_URL
//                });
//            }, config.WARTEZEIT_MS);
//        }
//    } else {
//        if (timerId) {
//            clearTimeout(timerId);
//            timerId = null;
//        }
//    }
//}

//chrome.storage.local.get(['isActive'], (result) => { handleTimer(result.isActive !== false); });
//chrome.storage.onChanged.addListener((changes, namespace) => {
//    if (namespace === 'local' && changes.isActive) handleTimer(changes.isActive.newValue);
//});