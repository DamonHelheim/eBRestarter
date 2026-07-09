# eBRestarter
**Ein Automatisierungstool für die eBesucher Surfbar zur Optimierung von Stabilität und Leistung.**

<!-- Hero Image: Ein großer, schicker Screenshot des Hauptfensters der App -->
![eBRestarter Hauptfenster - Übersicht](assets/hero_image.png)

## Über eBRestarter
Bei der kontinuierlichen Nutzung der eBesucher Surfbar kann es gelegentlich zu Abstürzen, Aufhängern oder Leistungseinbußen durch einen vollen Browser-Cache kommen. 

eBRestarter ist eine Softwarelösung, die den Betrieb der Surfbar überwacht und den Browser in regelmäßigen, benutzerdefinierten Abständen automatisch neu startet. Zusätzlich werden Cache und Cookies automatisiert bereinigt. Dadurch wird ein kontinuierlicher und stabiler Betrieb der Surfbar gewährleistet, ohne dass manuelle Eingriffe erforderlich sind.

## Kernfunktionen und Vorteile
* **Stabiler Betrieb:** 
Eine automatisierte Bereinigung von Cache und Cookies verhindert Leistungseinbußen, die durch angesammelte temporäre Daten entstehen können.
<br>
* **Automatischer Neustart:** 
Bei Hängern oder Ausfällen der Surfbar führt eBRestarter selbstständig einen Neustart durch und minimiert so Ausfallzeiten.
<br>
* **Punkte Statistiken:** 
Überwachen Sie Ihre gesammelten Punkte (stündlich, monatlich, für das aktuelle Jahr) übersichtlich in einem zentralen Dashboard.

---

## Funktionsumfang
Das Programm bietet sowohl automatisierte Hintergrundprozesse als auch Werkzeuge für die manuelle Konfiguration:

### Automatisierte Prozesse
* **Browser-Neustart:**
Startet den Browser in festgelegten Intervallen automatisch neu.
<br>
* **Systembereinigung:**
Verlauf, Cache und Cookies werden nach einem konfigurierten Zeitplan im Hintergrund gelöscht.
<br>
* **PC-Neustart:**
Optional kann das gesamte System nach einem definierten Zeitplan (z.B. nachts) komplett neu gestartet werden.

<!-- Screenshot: Die neue Chrome Erweiterung in Aktion oder das Einstellungsmenü dafür -->
![Tab-Restarter Browser-Erweiterung](assets/extension_preview.png)

* **Tab-Restarter Browser-Erweiterung:** 
Eine Begleiterweiterung für Chromium-basierte Browser (z.B. Chrome, Edge, Brave, Vivaldi):
<br>
  * *Tab-Überwachung:* 
  Die Erweiterung erkennt eingefrorene Tabs, Endlosschleifen oder Verbindungsfehler und lädt die betroffene Seite bei Bedarf selbstständig neu.
  <br>
  * *Kiosk-Modus:* 
  Unerwünschte Pop-ups oder versehentlich geöffnete Tabs werden automatisch geschlossen, um den Fokus auf die Surfbar zu wahren.

### Monitoring & Statistiken
<!-- Screenshot: Das Dashboard mit den Statistiken (Stündlich, Monatlich, Jahr) und der IP-Adresse -->
![Dashboard mit Punkte-Statistiken und IP-Anzeige](assets/statistics_dashboard.png)

* **API-Integration:** 
Unterstützt die direkte Anbindung an die eBesucher-API zur Abfrage und Anzeige Ihrer Statistiken.
<br>
* **Auswertungen:**
Detaillierte Aufschlüsselung der generierten Punkte nach Stunde, Monaten und aktuelles Jahr.
<br>
* **IP-Anzeige:**
Die aktuelle öffentliche IP-Adresse wird kontinuierlich in der Benutzeroberfläche dargestellt.

### Manuelle Werkzeuge
<!-- Screenshot: Die Download- und Bereinigungstools -->
![Manuelle Tools und Browser-Downloader](assets/manual_tools.png)

* **Browser-Downloader:**
Direkter Download unterstützter Browser bequem aus der Anwendung heraus.
<br>
* **Add-on Installation:**
Integrierte Funktion zur unkomplizierten Installation des eBesucher Add-ons.
<br>
* **Manuelle Bereinigung:**
Möglichkeit zum sofortigen Löschen von Cookies und Cache per Knopfdruck.
<br>
* **Windows Auto-Login Guide:**
Enthält die Möglichkeite zur Einrichtung der automatischen Windows-Anmeldung. (Hinweis: das gilt nur für Anmeldungen ohne Pin eingabe.)

---

## Systemanforderungen

**Software:**
* **Betriebssystem:** Windows 11 (Home/Pro, **nur 64-Bit**)
  * *Getestet auf: Windows 11 (Version 25H2)*

* **Framework:** .NET 10.0 Runtime

**Hardware:**

* **Prozessor (CPU):** Intel oder AMD CPU mit 64-Bit-Architektur 
  *(Erfolgreich getestet u.a. auf Intel N150)*
<br>
* **Arbeitsspeicher (RAM):** 4 GB RAM empfohlen (Betrieb mit weniger RAM auf eigene Verantwortung möglich).
<br>
* **Festplattenspeicher:** Mindestens 300 MB freier Speicherplatz.

> **Hinweise:** 
> * **Sprachen:** Die Software ist in Deutsch und Englisch verfügbar.
> * **Kompatibilität:** Es werden ausschließlich Windows-Betriebssysteme unterstützt.

## Erste Schritte

1. **Download:**
Laden Sie die aktuelle Version von eBRestarter herunter.
<br>
2. **Voraussetzungen prüfen:**
Stellen Sie sicher, dass die .NET 10 Runtime auf Ihrem System installiert ist.
<br>
3. **Einrichtung:**
Starten Sie die Anwendung. Nutzen Sie die integrierten Werkzeuge, um den gewünschten Browser herunterzuladen und das Add-on zu installieren. Hinterlegen Sie optional Ihre API-Zugangsdaten für die Statistikfunktionen.
<br>
4. **Konfiguration:**
Legen Sie die Intervalle für Neustarts und Bereinigungen fest und aktivieren Sie bei Bedarf die Browser-Erweiterung.
<br>
5. **Start:**
Aktivieren Sie den automatisierten Betrieb über die Schaltfläche "Start".

---
*Hinweis: eBRestarter ist eine inoffizielle Software und steht in keiner direkten Verbindung zur TurboAd GmbH.*
