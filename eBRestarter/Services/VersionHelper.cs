using System.Diagnostics;
using System.Reflection;

namespace eBRestarter.Services
{
    public class VersionHelper
    {
        public static string? GetCurrentVersion
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

                return fvi.FileVersion;
            }
        }
    }
}
