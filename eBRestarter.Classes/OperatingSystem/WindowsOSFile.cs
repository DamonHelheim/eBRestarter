namespace eBRestarter.Classes.OperatingSystem
{
    public partial class WindowsOS
    {
        public static bool IsDirectory(string path)
        {
            bool isDirectory = (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;

            return isDirectory;
        }

    }
}
