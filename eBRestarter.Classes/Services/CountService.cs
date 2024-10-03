using eBRestarter.Classes.EBInterfaces;

namespace eBRestarter.Classes.Services
{
    public class CountService(ICount count)
    {
        private readonly ICount _count = count;

        public T CountFilesInSpecificDirectory<T>(string directoryPath)
        {
            return (T)(object)_count.CountFilesInDirectoryI<T>(directoryPath)!;
        }

        public T CountFiles<T>(List<string> directoryPaths)
        {
            return (T)(object)_count.CountNumberOfFilesInSpecifiedDirectories<T>(directoryPaths)!;
        }
    }
}
