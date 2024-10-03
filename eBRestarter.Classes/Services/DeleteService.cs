using eBRestarter.Classes.EBInterfaces;

namespace eBRestarter.Classes.Services
{
    public class DeleteService(IDelete delete)
    {
        private readonly IDelete delete = delete;

        public void DeleteSingleFile(string filePath)
        {
            delete.DeleteSingleFile(filePath);
        }

        public void DeleteAllFilesInDirectory(string filePath)
        {
            delete.DeleteAllFilesInDirectory(filePath);
        }

        public void DeleteAllFileAndSubFolderInDirectory(string filePath)
        {
            delete.DeleteAllFileAndSubFolderInDirectory(filePath);
        }
    }
}
