using eBRestarter.Classes.EBInterfaces;
using eBRestarter.Classes.OperatingSystem;
using Serilog;
using System.Xml;
using System.Xml.Linq;

namespace eBRestarter.Classes.EBFiles
{
    public sealed class EBXMLFileManagement : IEBXMLFile, ISystemPaths, IConfigConstants
    {
        public void CreateDefaultXMLConfig()
        {
            try
            {

                #pragma warning disable CA1416
                WindowsOS windows = new();

                using (XmlTextWriter writer = new(ISystemPaths.File_Path_eBRestarter_Settings, null))
                {
                    // Define the format of the XML file
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;

                    // Write the start element tag of the root element
                    writer.WriteStartDocument();
                    writer.WriteStartElement(IConfigConstants.EBRestarterConfigName);

                    WriteXMLElements(writer, IConfigConstants.UsernameTag, IConfigConstants.UsernameTag_Name_Attribut, "", null!);

                    List<string> xmlLineRestartAndRuntimeSettings =
                    [
                        IConfigConstants.BrowserRuntimeTag + ":1",
                        IConfigConstants.BrowserRestTag + ":20"
                    ];

#pragma warning disable CA1416
                    WriteXMLElements(writer, IConfigConstants.BrowserTag, IConfigConstants.BrowserTag_Selected_Attribut, windows.StandardBrowser(), xmlLineRestartAndRuntimeSettings);

                    List<string> xmlLinesOptionsSetting =
                    [
                        IConfigConstants.ThemeTag+":Light",
                        IConfigConstants.StartWithWindowsTag+":false",
                    ];

                    WriteXMLElements(writer, IConfigConstants.SettingsTag, null!, null!, xmlLinesOptionsSetting);

                    List<string> xmlLinesSchedularSettings =
                    [
                        IConfigConstants.CheckBrowserAliveRoutineTag + ":false",
                        IConfigConstants.DeleteCookiesAndCacheTag + ":false",
                        IConfigConstants.RestartComputerTag + ":false",
                        IConfigConstants.RestartTimeTag+":1",
                        IConfigConstants.StartRestarterWithProgrammStartTag+":false",
                    ];

                    WriteXMLElements(writer, IConfigConstants.SchedulerTag, null!, null!, xmlLinesSchedularSettings);

                    // Write the end element tag of the root element
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                };

            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im EBXMLFileManagementbereich aufgetreten. Logging...");
            }
        }

        public void SaveXMLConfigFileByTagValue(string tagName, string tagValue)
        {
            try
            {
                XDocument xdocChangePrice = XDocument.Load(ISystemPaths.File_Path_eBRestarter_Settings);
                //XElement elementPrice = xdocChangePrice.Descendants(tagName).Where(x => x.Value == tagValue).First();

                XElement elementPrice = xdocChangePrice.Descendants(tagName).First();

                elementPrice.SetValue(tagValue);
                xdocChangePrice.Save(ISystemPaths.File_Path_eBRestarter_Settings);

            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im EBXMLFileManagementbereich aufgetreten. Logging...");
            }

        }

        public void SaveXMLConfigFileByAttributeValue(string filePath, string tagName, string attributeName, string attributeValue)
        {
            try
            {
                // Load the XML file
                XDocument doc = XDocument.Load(filePath);

                XElement? configElement = doc.Descendants(tagName).FirstOrDefault();

                if (configElement != null)
                {
                    // Replace the value of the id attribute with another value
                    configElement.Attribute(attributeName)!.Value = attributeValue;

                    // Save the modified XML file
                    doc.Save(filePath);

                    //The XML file has been successfully updated
                }
                else
                {
                    //The element was not found
                }

            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im EBXMLFileManagementbereich aufgetreten. Logging...");
            }
        }


        public T ReadTagValueFromXMLFile<T>(string filePath, string tagName)
        {
            try
            {
                XDocument xdoc = XDocument.Load(filePath);

                XElement xmlTagelement = xdoc.Descendants(tagName).First();

                if (typeof(T) == typeof(int))
                {
                    int tagValue = int.Parse(xmlTagelement.Value);

                    return (T)(object)tagValue;
                }
                else if (typeof(T) == typeof(string))
                {

                    string tagValue = xmlTagelement.Value;

                    return (T)(object)tagValue;
                }
                else if (typeof(T) == typeof(bool))
                {
                    bool tagValue = bool.Parse(xmlTagelement.Value);

                    return (T)(object)tagValue;

                }
                else
                {
                    //throw new NotSupportedException("Unsupported type.");
                    Log.Error("Es ist eine Ausnahme im EBXMLFileManagementbereich aufgetreten, dieser Typ wird nicht unterstützt. Logging...");
                    string tagValue = string.Empty;
                    return (T)(object)tagValue;
                }

            }

            catch (Exception e)
            {
                //throw new NotImplementedException();
                Log.Error(e, "Es ist eine Ausnahme im EBXMLFileManagementbereich aufgetreten. Logging...");
                string tagValue = string.Empty;
                return (T)(object)tagValue;
            }
        }


        public T ReadAttributeValueFromXMLFile<T>(string filePath, string tagName, string attributeName)
        {

            try
            {
                XDocument xdoc = XDocument.Load(filePath);

                if (typeof(T) == typeof(int))
                {
                    int tagValue = int.Parse((from configXML in xdoc.Descendants(tagName) where configXML.Attribute(attributeName) != null select configXML?.Attribute(attributeName)?.Value).FirstOrDefault()!);

                    return (T)(object)tagValue;

                }
                if (typeof(T) == typeof(string))
                {
                    string tagValue = (from configXML in xdoc.Descendants(tagName) where configXML.Attribute(attributeName) != null select configXML?.Attribute(attributeName)?.Value).FirstOrDefault()!;

                    return (T)(object)tagValue;
                }
                if (typeof(T) == typeof(bool))
                {
                    bool tagValue = bool.Parse((from configXML in xdoc.Descendants(tagName) where configXML.Attribute(attributeName) != null select configXML?.Attribute(attributeName)?.Value).FirstOrDefault()!);

                    return (T)(object)tagValue;
                }
                else
                {
                    //throw new NotSupportedException("Unsupported type.");
                    Log.Error("Es ist eine Ausnahme im EBXMLFileManagementbereich aufgetreten, dieser Typ wird nicht unterstützt. Logging...");
                    string tagValue = "";
                    return (T)(object)tagValue;
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im EBXMLFileManagementbereich aufgetreten. Logging...");
                string tagValue = "";
                return (T)(object)tagValue;
            }
        }

        public static void WriteXMLElements(XmlTextWriter writer, string startElement, string attributName, string attributValue, List<string> xmlElements)
        {
            try
            {
                writer.WriteStartElement(startElement);

                if (startElement is not null && attributName is not null)
                {
                    writer.WriteAttributeString(attributName, attributValue);
                }

                if (xmlElements is not null)
                {
                    foreach (string element in xmlElements)
                    {
                        string[] parts = element.Split(':');

                        if (parts.Length == 2)
                        {
                            string elementName = parts[0];
                            string elementValue = parts[1];

                            writer.WriteElementString(elementName, elementValue);
                        }
                    }
                }

                writer.WriteEndElement();
            }
            catch (Exception e)
            {
                Log.Error(e, "Es ist eine Ausnahme im EBXMLFileManagementbereich aufgetreten. Logging...");
            }
        }
    }
}
