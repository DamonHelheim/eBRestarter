chrome.runtime.onInstalled.addListener(() => {
  chrome.storage.local.set({ isActive: true });
});

function updateIcon(isActive) {
  const path = isActive 
    ? { "16": "icons/icon16_on.png", "32": "icons/icon32_on.png" }
    : { "16": "icons/icon16_off.png", "32": "icons/icon32_off.png" };
  chrome.action.setIcon({ path: path });
}

// NEU: Lauscht auf Änderungen (weil das Popup jetzt den Status umschaltet)
chrome.storage.onChanged.addListener((changes, namespace) => {
  if (namespace === 'local' && changes.isActive !== undefined) {
    updateIcon(changes.isActive.newValue);
  }
});

// Gleiche intelligente Abfrage wie im Timer
async function getActiveSettings() {
  const uiData = await chrome.storage.local.get(['useCustom', 'customUrl']);
  if (uiData.useCustom && uiData.customUrl) {
    return { ZIEL_URL: uiData.customUrl };
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

chrome.webNavigation.onErrorOccurred.addListener(async (details) => {
  if (details.frameId === 0) { 
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