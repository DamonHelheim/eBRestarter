using eBRestarter.Domain.Enums;
using eBRestarter.Domain.ValueObject;
using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Domain.Interfaces
{
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
