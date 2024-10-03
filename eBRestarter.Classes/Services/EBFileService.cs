using eBRestarter.Classes.EBInterfaces;

namespace eBRestarter.Classes.Services
{
    public class EBFileService(IEBFile eBFile)
    {
        private readonly IEBFile _eBFile = eBFile;

        public void CreateDefaultDirectoryIfNotExist()
        {
            _eBFile.CreateDefaultDirectoryIfNotExist();
        }

        public string ReadFile(string filePath)
        {
            string fileInformation = "";

            if (File.Exists(filePath))
            {
                fileInformation = _eBFile.ReadFile(filePath);              
            }

            return fileInformation;
        }

        public string[]? ReadAPIFile()
        {
            return _eBFile.ReadAPIFile();
        }

        public void CreateFile(string filePath, string value)
        {
            if (!File.Exists(filePath))
            {             
                _eBFile.CreateFile(filePath, value);
            }
        }

        public void WriteFile(string filePath, string value)
        {
            if (File.Exists(filePath))
            {
                _eBFile.WriteFile(filePath, value);
            }
        }

        public void RenameFileAndSaveInBackup(string filePathOnly, string fileNameOnly, string fileExtension, string directoryNameBackup)
        {           
            _eBFile.RenameFileAndSaveInBackup(filePathOnly, fileNameOnly, fileExtension, directoryNameBackup);
        }

        public void CreateSpecificFileExtension(string username, string apikey, string filePathOnly, string filenameOnly, string fileExtension)
        {
            _eBFile.CreateSpecificFileExtension(username, apikey, filePathOnly, filenameOnly, fileExtension);
        }
    }
}
