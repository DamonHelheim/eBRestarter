namespace eBRestarter.Classes.EBInterfaces
{
    public interface IEBFile
    {
        public void CreateDefaultDirectoryIfNotExist();
        public string ReadFile(string filePath);

        public string[]? ReadAPIFile();

        public void CreateFile(string filePath, string value);
        
        public void WriteFile(string filePath, string value);

        public void RenameFileAndSaveInBackup(string filePathOnly, string fileNameOnly, string fileExtension, string directoryNameBackup);

        public void CreateSpecificFileExtension(string username, string apikey, string filePathOnly, string fileNameOnly, string fileExtension);
    }
}
