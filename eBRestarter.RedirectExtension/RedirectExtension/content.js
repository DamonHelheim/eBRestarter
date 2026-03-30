let timerId = null;

// Die intelligente Abfrage: Woher kommen die Daten?
async function getActiveSettings() {
  // 1. Prüfe, ob die UI aktiviert wurde
  const uiData = await chrome.storage.local.get(['useCustom', 'customUrl', 'customTimeMin']);
  
  if (uiData.useCustom && uiData.customUrl && uiData.customTimeMin) {
    console.log("Nutze Werte aus dem Optionsmenü.");
    return {
      ZIEL_URL: uiData.customUrl,
      WARTEZEIT_MS: uiData.customTimeMin * 60000 // Minuten in Millisekunden umrechnen
    };
  }

  // 2. Fallback: Wenn UI aus ist, lade config.json
  console.log("Nutze Werte aus config.json (C#-Programm).");
  try {
    const response = await fetch(chrome.runtime.getURL('config.json'));
    const config = await response.json();
    
    // URL aus der JSON reparieren
    if (!config.ZIEL_URL.startsWith('http://') && !config.ZIEL_URL.startsWith('https://')) {
      config.ZIEL_URL = 'https://' + config.ZIEL_URL;
    }
    return config;
  } catch (error) {
    console.error("Fehler beim Laden der config.json!", error);
    return null; // Bricht ab, wenn Datei kaputt ist
  }
}

async function handleTimer(isActive) {
  if (isActive) {
    if (!timerId) {
      const config = await getActiveSettings();
      if (!config) return; // Wenn keine Daten da sind, tu nichts

      console.log(`Timer gestartet. Warte ${config.WARTEZEIT_MS} ms. Ziel: ${config.ZIEL_URL}`);
      timerId = setTimeout(() => {
        window.location.href = config.ZIEL_URL;
      }, config.WARTEZEIT_MS);
    }
  } else {
    if (timerId) {
      clearTimeout(timerId);
      timerId = null;
    }
  }
}

chrome.storage.local.get(['isActive'], (result) => { handleTimer(result.isActive !== false); });
chrome.storage.onChanged.addListener((changes, namespace) => {
  if (namespace === 'local' && changes.isActive) handleTimer(changes.isActive.newValue);
});