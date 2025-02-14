using Newtonsoft.Json.Linq;
using System.Xml;


string directory = "C:\\Solutions\\Apply.SundBaelt\\src\\Apply.SundBaelt.Web\\uSync\\v9\\DataTypes";
string logPath = Path.Combine("C:\\Scripts", "ChangeSectionsBgColor_Log.txt");

File.AppendAllText(logPath, $"Processing started at {DateTime.Now}\n");


if (!Directory.Exists(directory))
{
    Console.WriteLine($"The directory path '{directory}' does not exist. Exiting script.");
    Environment.Exit(0);
}

var files = Directory.GetFiles(directory);
int fileModifiedCount = 0;

if (files.Length == 0)
{
    Console.WriteLine("No files found in the directory. Exiting script.");
    Environment.Exit(0);
}

foreach (var file in files)
{
    XmlDocument xmlDoc = new XmlDocument();
    xmlDoc.Load(file);

    XmlNode? dataTypeNode = xmlDoc?.SelectSingleNode("//DataType");
    XmlNode? editorAliasNode = xmlDoc?.SelectSingleNode("//DataType/Info/EditorAlias");
    XmlNode? configNode = xmlDoc?.SelectSingleNode("//DataType/Config");
    string? editorAliasValue = editorAliasNode?.InnerText;


    if (editorAliasValue != null && editorAliasValue == "Umbraco.BlockGrid")
    {

        if (configNode != null && configNode.FirstChild is XmlCDataSection cdataSection)
        {
            string? jsonString = cdataSection.Value;
            try
            {
                if (jsonString == null)
                {
                    File.AppendAllText(logPath, $"JSON string is null in file: {file}\n");
                    continue;
                }
                var jsonObject = JObject.Parse(jsonString);
                bool modified = false;
                if (jsonObject.TryGetValue("Blocks", out JToken? blocksToken) && blocksToken is JArray blocksArray)
                {
                    var blocksBgToChange = new List<JToken>();

                    foreach (var block in blocksArray)
                    {

                        block["allowInAreas"] = false;
                        modified = true;
                        block["rowMaxSpan"] = 1;
                        modified = true;
                        block["rowMinSpan"] = 1;
                        modified = true;

                    }

                }

                if (modified)
                {
                    XmlCDataSection? newCdataSection = xmlDoc?.CreateCDataSection(jsonObject.ToString());
                    if (configNode != null && newCdataSection != null)
                    {
                        configNode.RemoveAll();
                        configNode.AppendChild(newCdataSection);

                        xmlDoc?.Save(file);
                        fileModifiedCount++;
                    }
                }
            }
            catch
            {
                Console.WriteLine("We're in the catch");
            }
        }
    }
}
Console.WriteLine($"Processing complete. 'allowInAreas', 'rowMaxSpan', 'rowMinSpan', and 'view' were updated where found. Number of files modified: {fileModifiedCount}.");
