using eBRestarter.Classes.EBInterfaces;

namespace eBRestarter.Classes.EBFiles
{
    public sealed class Delete : IDelete
    {

        /*
         * This class deletes files and folders
         */
        public void DeleteSingleFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception)
            {

            }
        }

        public void DeleteAllFilesInDirectory(string filePath)
        {
            string[] files = Directory.GetFiles(filePath);

            foreach (string file in files)
            {
                File.Delete(file);
            }
        }

        public void DeleteAllFileAndSubFolderInDirectory(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                //Delete all files from the Directory
                foreach (string file in Directory.GetFiles(filePath))
                {
                    File.Delete(file);
                }

                //Delete all child Directories
                foreach (string directory in Directory.GetDirectories(filePath))
                {
                    DeleteAllFileAndSubFolderInDirectory(directory);
                }

                //Delete a Directory
                Directory.Delete(filePath);

            }
        }
    }
}
