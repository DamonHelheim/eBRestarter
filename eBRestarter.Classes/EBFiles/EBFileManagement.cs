using eBRestarter.Classes.EBInterfaces;
using Serilog;

namespace eBRestarter.Classes.EBFiles
{
    public sealed class EBFileManagement : IEBFile, ISystemPaths
    {
        private static int fileCount = 1;

        public void CreateDefaultDirectoryIfNotExist()
        {
            try
            {
                if (Directory.Exists(ISystemPaths.FilePathApplicationOnly))
                {
                    //Debug.WriteLine("That path exists already.");
                    return;
                }

                DirectoryInfo di = Directory.CreateDirectory(ISystemPaths.FilePathApplicationOnly);

            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im EBFileManagementbereich aufgetreten. Logging...");
            }
            finally { }
        }

        public string[]? ReadAPIFile()
        {
            string username = string.Empty;
            string apikey = string.Empty;
            string[] APIFileDataBuffer = new string[2];

            if (File.Exists(ISystemPaths.File_Path_API) == true)
            {
                try
                {
                    using (BinaryReader reader = new(File.Open(ISystemPaths.File_Path_API, FileMode.Open)))
                    {
                        APIFileDataBuffer[0] = username = reader.ReadString();
                        APIFileDataBuffer[1] = apikey = reader.ReadString();
                    }

                    return APIFileDataBuffer;

                }
                catch (Exception)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }


        public string ReadFile(string filePath)
        {
            try
            {
                return File.ReadAllText(filePath);
            }
            catch (Exception)
            {
                return null!;
            }
        }

        public void CreateFile(string filePath, string value)
        {
            try
            {
                using StreamWriter sw = File.CreateText(filePath);
                sw.Write(value); // "-" standard value for very first start of the application                                          
            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im EBFileManagementbereich aufgetreten. Logging...");
            }
        }

        public void WriteFile(string filePath, string value)
        {
            try
            {
                using StreamWriter sw = File.CreateText(filePath);
                sw.Write(value); // "-" standard value for very first start of the application                               
            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im EBFileManagementbereich aufgetreten. Logging...");
            }
        }

        public void RenameFileAndSaveInBackup(string filePathOnly, string fileNameOnly, string fileExtension, string directoryNameBackup)
        {
            try
            {
                string? pathDirectoryBackup = @Path.GetDirectoryName(filePathOnly + "\\" + directoryNameBackup + "\\" + fileNameOnly + fileExtension);

                string? newFullPathForBackup = filePathOnly + fileNameOnly + fileExtension;

                Directory.CreateDirectory(filePathOnly + "\\" + directoryNameBackup); //Create a backup directory if not exist if delete API file

                while (File.Exists(newFullPathForBackup))
                {
                    string? tempFileName = string.Format("{0}({1})", fileNameOnly, fileCount++);

                    newFullPathForBackup = Path.Combine(pathDirectoryBackup!, tempFileName + fileExtension);
                }

                File.Move(filePathOnly + "\\" + fileNameOnly + fileExtension, newFullPathForBackup); //Moves the old file in a folder backup

            }
            catch (Exception e)
            {

                Log.Error(e, "Es ist eine Ausnahme im EBFileManagementbereich aufgetreten. Logging...");
            }
        }

        public void CreateSpecificFileExtension(string username, string apikey, string filePathOnly, string filenameOnly, string fileExtension)
        {
            /*
             * The input string must not be null or empty and it must have the format of a key, otherwise it will be rejected.
             */

            try
            {
                using BinaryWriter binWriter = new(File.Open(filePathOnly + "\\" + filenameOnly + fileExtension, FileMode.Create));
                binWriter.Write(username);
                binWriter.Write(apikey);
            }
            catch (Exception e)
            {
                try
                {
                    File.Delete(filePathOnly + "\\" + filenameOnly + fileExtension);
                }
                catch (Exception e2)
                {
                    Log.Error(e2, "Es ist eine Ausnahme im EBFileManagementbereich aufgetreten. Logging...");
                }

                Log.Error(e, "Es ist eine Ausnahme im EBFileManagementbereich aufgetreten. Logging...");
            }
        }
    }
}
