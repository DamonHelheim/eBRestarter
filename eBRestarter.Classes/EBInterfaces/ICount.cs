namespace eBRestarter.Classes.EBInterfaces
{
    public interface ICount
    {
        public T CountFilesInDirectoryI<T>(string path);
        public T CountNumberOfFilesInSpecifiedDirectories<T>(List<string> directoryPaths);
    }
}
