using eBRestarter.Classes.EBFiles;
using eBRestarter.Classes.EBInterfaces;

namespace eBRestarter.Classes.Services
{
    public class EBXMLFileService(IEBXMLFile eBXMLFile) : ISystemPaths
    {

        private readonly IEBXMLFile _eBXMLFile = eBXMLFile;

        public void CreateDefaultConfigFileIfNotExist()
        {
            if (!File.Exists(ISystemPaths.File_Path_eBRestarter_Settings))
            {
                _eBXMLFile.CreateDefaultXMLConfig();
            }
        }

        public void WriteXMLTagValueInConfig(string tagName, string tagValue)
        {
            if (File.Exists(ISystemPaths.File_Path_eBRestarter_Settings))
            {
                EBFileAccess.AccessFile!(IFileNames.EBRESTARTERCONFIG);

                _eBXMLFile.SaveXMLConfigFileByTagValue(tagName, tagValue);

                EBFileAccess.LockFile!(IFileNames.EBRESTARTERCONFIG);             
            }           
        }

        public void WriteXMLAttributeInConfig(string filePath, string tagName, string attributeName, string attributeValue)
        {
            if (File.Exists(ISystemPaths.File_Path_eBRestarter_Settings))
            {
                EBFileAccess.AccessFile!(IFileNames.EBRESTARTERCONFIG);

                _eBXMLFile.SaveXMLConfigFileByAttributeValue(filePath, tagName, attributeName, attributeValue);

                EBFileAccess.LockFile!(IFileNames.EBRESTARTERCONFIG);
            }
        }

        public T GetXMLTagValueInConfig<T>(string filePath, string tagValue)
        {
            if (File.Exists(ISystemPaths.File_Path_eBRestarter_Settings))
            {
                EBFileAccess.AccessFile!(IFileNames.EBRESTARTERCONFIG);

                object? typObject = (T)(object)_eBXMLFile.ReadTagValueFromXMLFile<T>(filePath, tagValue)!;

                EBFileAccess.LockFile!(IFileNames.EBRESTARTERCONFIG);

                return (T)(object)typObject;
            }
            else
            {
                throw new NotImplementedException();
            }
        }


        public T GetXMLAttributeInConfig<T>(string filePath, string tagName, string attributeName)
        {
            if (File.Exists(ISystemPaths.File_Path_eBRestarter_Settings))
            {
                EBFileAccess.AccessFile!(IFileNames.EBRESTARTERCONFIG);

                object? typObject = (T)(object)_eBXMLFile.ReadAttributeValueFromXMLFile<T>(filePath, tagName, attributeName)!;

                EBFileAccess.LockFile!(IFileNames.EBRESTARTERCONFIG);

                return (T)(object)typObject;
            }
            else
            {
                return (T)(object)null!;
            }
        }
    }
}
