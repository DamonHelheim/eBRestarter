// Lade gespeicherte Werte beim Öffnen
document.addEventListener('DOMContentLoaded', async () => {
  const data = await chrome.storage.local.get(['useCustom', 'customUrl', 'customTimeMin']);
  document.getElementById('useCustom').checked = data.useCustom || false;
  document.getElementById('zielUrl').value = data.customUrl || "";
  document.getElementById('wartezeitMin').value = data.customTimeMin || 3;
  toggleFields();
});

// Felder aktivieren/deaktivieren je nach Checkbox
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

  // URL reparieren (http:// hinzufügen, falls vergessen)
  if (customUrl && !customUrl.startsWith('http://') && !customUrl.startsWith('https://')) {
    customUrl = 'https://' + customUrl;
    document.getElementById('zielUrl').value = customUrl; // Zeigt die Korrektur direkt im Feld an
  }

  chrome.storage.local.set({ useCustom, customUrl, customTimeMin }, () => {
    const status = document.getElementById('status');
    status.textContent = "Gespeichert!";
    setTimeout(() => status.textContent = "", 2000);
  });
});