// --- DICTIONARY ---
const dict = {
    "DE": {
        title: "Tab Restarter",
        langLabel: "Sprache:",
        langAuto: "Auto (Config)",
        statusLabel: "Tab Restarter Status:",
        btnOn: "AN",
        btnOff: "AUS",
        loading: "LADE...",
        checkboxLabel: "Manuelle Werte nutzen<br><small>(Ignoriert die eBRestarter Tab Restarter config.json)</small>",
        urlLabel: "Ziel-URL:",
        timeLabel: "Wartezeit (Minuten):",
        saveBtn: "Einstellungen Speichern",
        savedMsg: "Gespeichert!"
    },
    "EN": {
        title: "Tab Restarter",
        langLabel: "Language:",
        langAuto: "Auto (Config)",
        statusLabel: "Tab Restarter Status:",
        btnOn: "ON",
        btnOff: "OFF",
        loading: "LOADING...",
        checkboxLabel: "Use manual settings<br><small>(Ignores eBRestarter Tab Restarter config.json)</small>",
        urlLabel: "Target URL:",
        timeLabel: "Wait time (minutes):",
        saveBtn: "Save Settings",
        savedMsg: "Saved!"
    }
};

let currentLang = "DE"; // Aktuelle Anzeigesprache
let configLang = "DE";  // Die Sprache, die in der config.json steht

// Funktion zum Anwenden der Sprache auf das HTML
function applyLanguage() {
    const elements = document.querySelectorAll('[data-i18n]');
    elements.forEach(el => {
        const key = el.getAttribute('data-i18n');
        if (dict[currentLang] && dict[currentLang][key]) {
            el.innerHTML = dict[currentLang][key];
        }
    });
}

document.addEventListener('DOMContentLoaded', async () => {
    // 1. ZUERST DIE SPRACHE AUS DER CONFIG LADEN (als Fallback für AUTO)
    try {
        const response = await fetch(chrome.runtime.getURL('tab_restarter_config.json'));
        const config = await response.json();
        if (config.LANGUAGE && dict[config.LANGUAGE]) {
            configLang = config.LANGUAGE;
        }
    } catch (error) {
        console.log("Konnte config.json für die Sprache nicht laden.");
    }

    // 2. GESPEICHERTE EINSTELLUNGEN LADEN
    const data = await chrome.storage.local.get(['selectedLang', 'isActive', 'useCustom', 'customUrl', 'customTimeMin']);

    // Sprache einstellen (Wenn nichts gewählt ist, nimm "AUTO")
    const userLangChoice = data.selectedLang || "AUTO";
    document.getElementById('languageSelect').value = userLangChoice;

    if (userLangChoice === "AUTO") {
        currentLang = configLang; // Nimm die C# Sprache
    } else {
        currentLang = userLangChoice; // Nimm die vom User erzwungene Sprache
    }

    // Sprache aufs Menü anwenden
    applyLanguage();

    // 3. SCHALTER UND FORMULAR INITIALISIEREN
    let isActive = data.isActive !== false;
    updateToggleButton(isActive);

    document.getElementById('onOffBtn').addEventListener('click', () => {
        isActive = !isActive;
        chrome.storage.local.set({ isActive: isActive });
        updateToggleButton(isActive);
    });

    document.getElementById('useCustom').checked = data.useCustom || false;
    document.getElementById('zielUrl').value = data.customUrl || "";
    document.getElementById('wartezeitMin').value = data.customTimeMin || 3;
    toggleFields();
});

// EVENT-LISTENER: Wenn der Nutzer im Dropdown die Sprache ändert
document.getElementById('languageSelect').addEventListener('change', (e) => {
    const chosen = e.target.value;
    chrome.storage.local.set({ selectedLang: chosen }); // Wahl speichern

    if (chosen === "AUTO") {
        currentLang = configLang;
    } else {
        currentLang = chosen;
    }

    // Sofort alles neu übersetzen
    applyLanguage();

    // Auch den Text im ON/OFF Schalter aktualisieren
    chrome.storage.local.get(['isActive'], (res) => {
        updateToggleButton(res.isActive !== false);
    });
});

// Update-Funktion für den Button
function updateToggleButton(isActive) {
    const btn = document.getElementById('onOffBtn');
    if (isActive) {
        btn.textContent = dict[currentLang].btnOn;
        btn.className = "btn-on";
    } else {
        btn.textContent = dict[currentLang].btnOff;
        btn.className = "btn-off";
    }
}

// Felder aktivieren/deaktivieren
document.getElementById('useCustom').addEventListener('change', toggleFields);
function toggleFields() {
    const isChecked = document.getElementById('useCustom').checked;
    const area = document.getElementById('settingsArea');
    area.style.opacity = isChecked ? "1" : "0.5";
    area.style.pointerEvents = isChecked ? "auto" : "none";
}

// Speichern
document.getElementById('saveBtn').addEventListener('click', () => {
    const useCustom = document.getElementById('useCustom').checked;
    let customUrl = document.getElementById('zielUrl').value.trim();
    const customTimeMin = parseInt(document.getElementById('wartezeitMin').value, 10);

    if (customUrl && !customUrl.startsWith('http://') && !customUrl.startsWith('https://')) {
        customUrl = 'https://' + customUrl;
        document.getElementById('zielUrl').value = customUrl;
    }

    chrome.storage.local.set({ useCustom, customUrl, customTimeMin }, () => {
        const status = document.getElementById('status');
        status.textContent = dict[currentLang].savedMsg;
        setTimeout(() => status.textContent = "", 2000);
    });
});