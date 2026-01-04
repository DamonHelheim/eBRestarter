using eBRestarter.Core.Domain.Models;
using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eBRestarter.Desktop.WinUI3.Services.Interfaces
{
    public interface IDialogService
    {
        Task<bool> ShowYesNoDialogAsync(string title, string message);
        Task ShowMessageAsync(string title, string message);

        Task<AutoLogonDialogResult> ShowAutoLogonDialogAsync(string defaultUser = null, string defaultDomain = null);

        Task ShowAboutDialogAsync();
        Task ShowBrowserDeleteContentDialogAsync();
    }
}
