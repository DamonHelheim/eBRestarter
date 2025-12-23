using eBRestarter.Application.Services.Ports.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Serilog;
using System.Diagnostics;
using System.Runtime.Versioning;

namespace eBRestarter.Infrastructure.Services.WindowsOS
{
    /// <summary>
    /// Manages system startup configurations, including Windows Autostart, Edge Startup Boost, 
    /// and AutoLogon settings via the Windows Registry.
    /// <br/>
    /// <b>Layer:</b> Infrastructure (Adapter)
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class WindowsStartupService : IStartupManagerService
    {
        // --- Constants for Registry Paths ---
        // Path for the "Current User" Run key (Standard Autostart)
        private const string RegistryPathRun = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        // Path for Edge Policies (Machine-wide)
        private const string RegistryPathEdgePolicies = @"SOFTWARE\Policies\Microsoft\Edge";

        // Path for Passwordless Sign-in (Machine-wide)
        private const string RegistryPathPasswordLess = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordLess\Device";

        private readonly ILogger<WindowsStartupService> _logger;

        /// <summary>
        /// Initializes the startup service with a logger.
        /// </summary>
        public WindowsStartupService(ILogger<WindowsStartupService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Adds the current application to the Windows Registry "Run" key for the current user.
        /// This ensures the app starts automatically when the user logs in.
        /// </summary>
        public void EnableAutoStart()
        {
            try
            {
                // We open the key with write permissions (true).
                // 'Registry.CurrentUser' does not require Admin rights.
                using var rkApp = Registry.CurrentUser.OpenSubKey(RegistryPathRun, true);

                if (rkApp != null)
                {
                    // "eV Restarter" is the key name, the value is the path to the .exe
                    rkApp.SetValue("eV Restarter", Process.GetCurrentProcess().MainModule!.FileName);
                    _logger.LogInformation("Autostart entry for 'eV Restarter' created.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set Autostart in registry.");
            }
        }

        /// <summary>
        /// Removes the application from the Windows Registry "Run" key.
        /// </summary>
        public void DisableAutoStart()
        {
            try
            {
                using var rkApp = Registry.CurrentUser.OpenSubKey(RegistryPathRun, true);

                // deleteValue(..., false) prevents an exception if the key doesn't exist.
                rkApp?.DeleteValue("eV Restarter", false);
                _logger.LogInformation("Autostart entry for 'eV Restarter' removed.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove Autostart from registry.");
            }
        }

        /// <summary>
        /// Retrieves all current autostart entries for the current user.
        /// </summary>
        /// <returns>A dictionary containing the application name (Key) and the executable path (Value).</returns>
        public Dictionary<string, object> GetStartupEntries()
        {
            var valuesBynames = new Dictionary<string, object>();

            try
            {
                // Open the registry key read-only
                using var rootKey = Registry.CurrentUser.OpenSubKey(RegistryPathRun);

                if (rootKey != null)
                {
                    // Get all entry names (e.g., "Steam", "Discord", "eV Restarter")
                    string[] valueNames = rootKey.GetValueNames();

                    foreach (string currSubKey in valueNames)
                    {
                        // Get the value (the path to the exe)
                        object value = rootKey.GetValue(currSubKey)!;
                        valuesBynames.Add(currSubKey, value);
                    }
                }

                return valuesBynames;
            }
            catch (Exception ex)
            {
                // Replaced static Log.Error with injected _logger
                _logger.LogError(ex, "Failed to retrieve startup entries.");

                // Return empty dictionary instead of null to avoid NullReferenceException in the caller
                return new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// Activates or deactivates the "Startup Boost" feature of Microsoft Edge.
        /// </summary>
        /// <param name="enable">True to enable (Sets registry value to 1), False to disable (Sets registry value to 0).</param>
        /// <remarks>
        /// <b>Requires Administrator Privileges</b> because it writes to <c>HKEY_LOCAL_MACHINE</c>.
        /// </remarks>
        public void SetEdgeStartupBoost(bool enable)
        {
            try
            {
                int dwordValue = enable ? 1 : 0;

                // CreateSubKey automatically opens the key if it exists, or creates it if it doesn't.
                // It simplifies the old if/else logic significantly.
                using RegistryKey key = Registry.LocalMachine.CreateSubKey(RegistryPathEdgePolicies);

                key.SetValue("StartupBoostEnabled", dwordValue, RegistryValueKind.DWord);

                _logger.LogInformation("Edge Startup Boost set to {State} ({Value}).", enable, dwordValue);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Permission denied modifying Edge Policies. Run application as Administrator.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Edge Startup Boost.");
            }
        }

        /// <summary>
        /// Configures the "DevicePasswordLessBuildVersion" setting to allow or disallow automatic logon (removing the Hello requirement).
        /// </summary>
        /// <param name="enable">
        /// <c>true</c>: Sets value to 0 (Enables ability for auto-logon / disables mandatory Hello).
        /// <c>false</c>: Sets value to 2 (Enforces Windows Hello / disables auto-logon).
        /// </param>
        /// <remarks>
        /// <b>Requires Administrator Privileges</b> (HKEY_LOCAL_MACHINE).
        /// </remarks>
        public void SetAutoLogon(bool enable)
        {
            try
            {
                // Logic derived from your previous code:
                // 0 = PasswordLess Disabled (Standard Login / Autologon possible)
                // 2 = PasswordLess Enabled (Windows Hello required)
                int dwordValue = enable ? 0 : 2;

                using RegistryKey key = Registry.LocalMachine.CreateSubKey(RegistryPathPasswordLess);

                key.SetValue("DevicePasswordLessBuildVersion", dwordValue, RegistryValueKind.DWord);

                _logger.LogInformation("Windows PasswordLess Login requirement set to {State} (RegValue: {Value}).", !enable, dwordValue);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Permission denied modifying PasswordLess settings. Run application as Administrator.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error configuring AutoLogon settings.");
            }
        }
    }
}
