namespace eBRestarter.Classes.EBInterfaces
{
    public interface IEBXMLFile
    {
        public void CreateDefaultXMLConfig();
        public void SaveXMLConfigFileByTagValue(string tagName, string tagValue);
        public void SaveXMLConfigFileByAttributeValue(string filePath, string tagName, string attributeName, string attributeValue);
        public T ReadTagValueFromXMLFile<T>(string filePath, string tagName);
        public T ReadAttributeValueFromXMLFile<T>(string filePath, string tagName, string attributeName);
    }
}
