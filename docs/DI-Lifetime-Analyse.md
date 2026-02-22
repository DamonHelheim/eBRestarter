# DI Service-Lifetime Analyse – eBRestarter

## Kurzfassung

- **Captive Dependencies**: Mehrere als `Transient` registrierte Dienste werden ausschließlich in Singletons injiziert und damit faktisch zu Singletons. Empfehlung: auf `Singleton` umstellen, um die tatsächliche Nutzung abzubilden und Verwechslungen zu vermeiden.
- **IRestClientService**: Wird von drei Singletons genutzt; Implementierung ist zustandslos (erstellt pro Request einen neuen RestClient). → `Singleton`.
- **IDialogService / IThemeService**: Zustandslos bzw. App-weiter Zustand; werden von Singleton-ViewModels gehalten. → `Singleton`.
- **ViewModelAbout**: Wird im About-Dialog verwendet; für einheitliches Verhalten mit anderen Dialog-ViewModels → `Transient`.

---

## 1. Core – ApplicationServiceExtensions

| Klasse/Interface | Aktuelle Lifetime | Empfohlene Lifetime | Begründung |
|------------------|-------------------|---------------------|------------|
| IComputerRestartScheduler / ComputerRestartScheduler | Singleton | Singleton ✓ | Hält `PeriodicTimer`, `CancellationTokenSource`, Background-Task; ein Scheduler pro App. |
| IRestartCalculationService / RestartCalculationService | Singleton | Singleton ✓ | Zustandslos, reine Berechnungslogik. |
| ICacheDeletionIntervalValidator / CacheDeletionIntervalValidator | Singleton | Singleton ✓ | Zustandslos. |
| IBrowserDisplayNameResolver / BrowserDisplayNameResolverService | Singleton | Singleton ✓ | Zustandslos. |
| IBrowserCleanupScheduleService / BrowserCleanupScheduleService | Singleton | Singleton ✓ | Zustandslos. |
| IRestartTaskDisplayStateService / RestartTaskDisplayStateService | Singleton | Singleton ✓ | Zustandslos, nutzt nur andere Services. |

---

## 2. Infrastructure – InfrastructureServiceRegistration

| Klasse/Interface | Aktuelle Lifetime | Empfohlene Lifetime | Begründung |
|------------------|-------------------|---------------------|------------|
| IProcessWrapper / RealProcessWrapper | Transient | **Singleton** | Wird nur von WindowsProcessService (Singleton) injiziert → Captive Dependency. RealProcessWrapper ist zustandslos (delegiert an `Process`). |
| IWindowsProcessControlService / WindowsProcessService | Singleton | Singleton ✓ | Hält keinen dauerhaften Zustand, Kapselung von Prozessoperationen. |
| IWindowsSystemInfoService / WindowsSystemInfoService | Singleton | Singleton ✓ | Zustandslos, liest Registry. |
| IProcessInfoService / ProcessInfoService | Transient | **Singleton** | Wird von WindowsStartupService (Singleton) injiziert → Captive. Implementierung zustandslos. |
| IWindowsRegistryService / WindowsRegistryService | Transient | **Singleton** | Wird von WindowsStartupService, WindowsSystemInfoService, OperatingSystemFacade (alle Singleton) injiziert → Captive. Zustandslos. |
| IWindowsFileSystemService / WindowsFileSystemService | Transient | **Singleton** | Wird von JsonCredentialStore, OperatingSystemFacade (Singleton) injiziert → Captive. Zustandslos. |
| IWindowsStartupManagerService / WindowsStartupService | Singleton | Singleton ✓ | Zustandslos, Kapselung Registry/Startup. |
| IWindowsAutoLogonService / WindowsAutoLogonService | Transient | **Singleton** | Wird von ViewModelOptions (Singleton) injiziert → Captive. Zustandslos. |
| IEncryptionService / WindowsEncryptionService | Transient | **Singleton** | Wird von EVisitorConfigService (Singleton) injiziert → Captive. Zustandslos. |
| WindowsWmiHardwareService | Singleton | Singleton ✓ | WMI-Zugriff, eine Instanz reicht. |
| IHardwareInfoService / (WindowsWmiHardwareService) | Singleton | Singleton ✓ | Wie oben. |
| IOsEditionService / (WindowsWmiHardwareService) | Singleton | Singleton ✓ | Wie oben. |
| IOperatingSystemFacade / OperatingSystemFacade | Singleton | Singleton ✓ | Fassade, zustandslos. |
| IWindowsNetworkInfoService / WindowsNetworkInfoService | Singleton | Singleton ✓ | Netzwerk-Infos, zustandslos. |
| ChromeBrowser, FirefoxBrowser, … (Transient) | Transient | Transient ✓ | BrowserFactory löst sie per `GetRequiredService` pro Aufruf; kurzlebige Nutzung. |
| IBrowserFactory / BrowserFactory | Singleton | Singleton ✓ | Factory mit IServiceProvider; korrekt. |
| IBrowserService / WindowsBrowserService | Singleton | Singleton ✓ | Orchestriert Browser, zustandslos. |
| IBrowserDownloadService / HttpClientDownloadService | Singleton | Singleton ✓ | Hält einen `HttpClient`; Singleton vermeidet Socket Exhaustion. |
| **IRestClientService / RestSharpClientService** | Transient | **Singleton** | Wird von EVisitorApiService, EVisitorApiAdapter, GitHubUpdateAdapter (alle Singleton) injiziert → Captive. Erstellt pro Aufruf neuen RestClient (`using var client`), also zustandslos und sicher als Singleton. |
| IPathService / WindowsPathService | Singleton | Singleton ✓ | Pfad-Logik, zustandslos. |
| IEVisitorConfigService / EVisitorConfigService | Singleton | Singleton ✓ | Lädt/speichert Config; eine Instanz pro App. |
| IAppInfoService / AppInfoService | Singleton | Singleton ✓ | Zustandslos. |
| IFileDeletionService / FileDeletionService | Singleton | Singleton ✓ | Zustandslos. |
| IApiAuthenticationService / EVisitorApiService | Singleton | Singleton ✓ | Zustandslos. |
| ICredentialStore / JsonCredentialStore | Singleton | Singleton ✓ | Speicherpfad einmalig; eine Instanz. |
| IEVisitorApiService / EVisitorApiAdapter | Singleton | Singleton ✓ | Zustandslos. |
| IUpdateService / GitHubUpdateAdapter | Singleton | Singleton ✓ | Zustandslos. |
| ICredentialValidationService / PrincipalContextCredentialValidationService | Singleton | Singleton ✓ | Zustandslos (PrincipalContext pro Aufruf). |
| IAppVersionInfoService / WindowsAppVersionInfoService | Singleton | Singleton ✓ | Zustandslos. |

---

## 3. Desktop.WinUI3

### 3.1 ViewModelServiceExtensions

| Klasse | Aktuelle Lifetime | Empfohlene Lifetime | Begründung |
|--------|-------------------|---------------------|------------|
| MainViewModel | Singleton | Singleton ✓ | Haupt-ViewModel, eine Instanz. |
| EBRestarter (Window) | Singleton | Singleton ✓ | Hauptfenster, eine Instanz. |
| ViewModelRestartTask | Singleton | Singleton ✓ | An Page gebunden. |
| ViewModelRestarterProperties | Singleton | Singleton ✓ | An Page gebunden. |
| ViewModelGeneralOverview | Singleton | Singleton ✓ | An Page gebunden. |
| ViewModelInstalledBrowsers | Singleton | Singleton ✓ | An Page gebunden. |
| ViewModelOptions | Singleton | Singleton ✓ | An Page gebunden. |
| ViewModelInfocenter | Singleton | Singleton ✓ | An Page gebunden. |
| **ViewModelAbout** | Singleton | **Transient** | Wird in AboutDialog per `GetRequiredService` geholt; pro Dialogöffnung neues ViewModel für konsistentes Verhalten mit anderen Dialog-VMs. |
| ViewModelNetworkTraffic | Transient | Transient ✓ | Wird in UC_Networktraffic per GetRequiredService geholt; IDisposable, kurzlebig. |
| ViewModelDeleteBrowserContent | Transient | Transient ✓ | Dialog-ViewModel. |
| ViewModelInstallAddOn | Transient | Transient ✓ | Dialog-ViewModel, IDisposable. |
| ViewModelActivateApi | Transient | Transient ✓ | Dialog-ViewModel. |
| ViewModelImportApi | Transient | Transient ✓ | Dialog-ViewModel. |
| ViewModelTurnOffEdgeStartupBoost | Transient | Transient ✓ | Dialog-ViewModel. |

### 3.2 DialoglServiceExtensions

| Klasse/Interface | Aktuelle Lifetime | Empfohlene Lifetime | Begründung |
|------------------|-------------------|---------------------|------------|
| IDialogService / DialogService | Transient | **Singleton** | Zustandslos (nutzt nur App.MainWindoweBRestarter für XamlRoot). Wird von vielen Singleton-ViewModels injiziert → faktisch eine Instanz; Singleton macht Nutzung explizit. |

### 3.3 ThemeServiceExtension

| Klasse/Interface | Aktuelle Lifetime | Empfohlene Lifetime | Begründung |
|------------------|-------------------|---------------------|------------|
| IThemeService / ThemeService | Transient | **Singleton** | Hält `CurrentTheme`; Theme ist App-weit. In App.xaml.cs bereits als Singleton registriert; Extension sollte einheitlich Singleton verwenden. |

### 3.4 NavigationServiceExtensions

| Klasse/Interface | Aktuelle Lifetime | Empfohlene Lifetime | Begründung |
|------------------|-------------------|---------------------|------------|
| INavigationService / NavigationService | Singleton | Singleton ✓ | Route-Registry, eine Instanz. |

### 3.5 App.xaml.cs

| Registrierung | Hinweis |
|---------------|--------|
| IThemeService, ILanguageService, ILocalizationService | Werden direkt als Singleton registriert. IThemeService doppelt (auch in ThemeServiceExtension). Nach Anpassung: AddThemeService() aufrufen und doppelte IThemeService-Registrierung entfernen. |

---

## 4. Kritische Punkte (vor den Anpassungen)

1. **Captive Dependencies**: Transient-Services, die nur in Singletons leben (z. B. IRestClientService, IProcessWrapper, IWindowsRegistryService, IProcessInfoService, IWindowsFileSystemService, IWindowsAutoLogonService, IEncryptionService), wurden auf Singleton umgestellt.
2. **IRestClientService**: RestSharpClientService erzeugt pro Request einen neuen RestClient und hält keine Verbindung; Singleton ist ressourcen- und architektonisch unkritisch.
3. **ViewModelAbout**: Dialog-ViewModel; Transient sorgt für frischen Zustand pro Dialogöffnung.
4. **IDialogService / IThemeService**: Faktisch einmal pro App genutzt bzw. App-Zustand; Singleton vereinheitlicht die Registrierung.

---

## 5. Durchgeführte Code-Änderungen (Übersicht)

- **InfrastructureServiceRegistration.cs**: IProcessWrapper, IProcessInfoService, IWindowsRegistryService, IWindowsFileSystemService, IWindowsAutoLogonService, IEncryptionService, IRestClientService von Transient auf Singleton.
- **DialoglServiceExtensions.cs**: IDialogService von Transient auf Singleton.
- **ThemeServiceExtension.cs**: IThemeService von Transient auf Singleton.
- **ViewModelServiceExtensions.cs**: ViewModelAbout von Singleton auf Transient.
- **App.xaml.cs**: Doppelte IThemeService-Registrierung entfernt, nur AddThemeService() verwendet (damit eine zentrale Stelle für Theme-Registrierung).
