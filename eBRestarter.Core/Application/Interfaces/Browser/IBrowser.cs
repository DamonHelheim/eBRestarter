using eBRestarter.Core.Domain.Enums;
using eBRestarter.Core.Domain.Models.Records;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Application.Interfaces.Browser
{
    //(Was muss ein Browser können?)
    public interface IBrowser
    {
        BrowserType Type { get; }
        string BrowserVersion { get; }
        bool IsInstalled { get; }
        void Start(string url, string arguments = "");
        void Close();

        // Cache & Pfad Infos
        BrowserPaths GetPaths();
    }
}
