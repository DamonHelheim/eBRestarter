using eBRestarter.Classes.EBInterfaces;
using Serilog;

namespace eBRestarter.Classes.EBFiles
{
    public sealed class EBFileAccess : ISystemPaths
    {
        /*
         * This class is necessary because that some files can not be changed if the application is open.
         */
        public sealed class FileObject
        {
            public string? FilePath { get; set; }
            public FileStream? FileStream { get; set; }
        }

        public delegate void CreateFileLocksType(string name, string dateipfad);

        public static readonly CreateFileLocksType? CreateFileLock = new(CreateFileLocks);

        public delegate void LockFileType(string filePath);

        public static readonly LockFileType? LockFile = new(LockFileInPath);

        public delegate void AccessFileType(string filePath);

        public static readonly AccessFileType? AccessFile = new(AccessFileInPath);

        private readonly static Dictionary<string, FileObject> FileContainer = [];

        private static readonly EBFileAccess instance = new();

        static EBFileAccess() { }
        private EBFileAccess() { }

        public static EBFileAccess Instance { get { return instance; } }

        public static void CreateFileLocks(string name, string dateipfad)
        {
            if (!FileContainer.ContainsKey(name))
            {
                try
                {
                    FileObject newFileObject = new()
                    {
                        FilePath = dateipfad,
                        FileStream = new(dateipfad, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
                    };

                    FileContainer[name] = newFileObject;

                }
                catch (Exception e)
                {
                    Log.Error(e, "Es ist eine Ausnahme im Fileaccessbereich aufgetreten. Logging...");
                }
            }
            else
            {
                //FileStream Object with name already exist
            }
        }

        private static void AccessFileInPath(string objectName)
        {
            if (FileContainer.TryGetValue(objectName, out FileObject? fileObject))
            {
                fileObject.FileStream!.Close();
            }
        }

        private static void LockFileInPath(string objectName)
        {
            if (FileContainer.TryGetValue(objectName, out _))
            {
                FileContainer[objectName].FileStream = new FileStream(FileContainer[objectName].FilePath!, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            };
        }
    }
}
