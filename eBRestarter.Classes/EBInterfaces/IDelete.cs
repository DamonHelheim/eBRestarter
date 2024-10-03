namespace eBRestarter.Classes.EBInterfaces
{
    public interface IDelete
    {
        public void DeleteSingleFile(string filePath);
        public void DeleteAllFilesInDirectory(string filePath);
        public void DeleteAllFileAndSubFolderInDirectory(string filePath);
    }
}
