using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ServerBase
{
    public class ConfigManager : Singleton<ConfigManager>
    {
        private Dictionary<string/*category*/, Dictionary<string, string>> _configCategory = new Dictionary<string, Dictionary<string, string>>();

        public bool LoadServerConfig()
        {
            string path;
            if (false == FindServerConfigFilePath(out path))
            {
                return false;
            }

            bool result = LoadXml(path);
            return result;
        }

        private bool FindServerConfigFilePath(out string path)
        {
            string fileName = "ServerConfig.xml";
            //현재경로로부터 상위경로로 올라가면서 위 파일을 찾는다.
            var files = SearchFile(fileName);

            bool result = false;
            if(0 < files.Count)
            {
                path = files[0];
                result = true;
            }
            else
            {
                path = string.Empty;
            }

            return result;
        }

        public static List<string> SearchFile(string fileName)
        {
            List<string> files = new List<string>();
            string curDir = "./"; //current directory

            const int MAX_TRY_COUNT = 10;
            int curTryCount = 0;
            while (curTryCount < MAX_TRY_COUNT && files.Count < 1)
            {
                try
                {
                    string[] findResults = Directory.GetFiles(curDir, fileName, SearchOption.AllDirectories);
                    for (int i = 0; i < findResults.Length; ++i)
                    {
                        string absPath = Path.GetFullPath(findResults[i]);
                        files.Add(absPath);
                    }
                }
                catch (Exception ex)
                {
                    Log.AddLog(ex.Message);
                }

                curDir = string.Format($"../{curDir}");
                curTryCount++;
            }

            return files;
        }

        private bool LoadXml(string path)
        {
            XmlDocument xml = new XmlDocument(); // XmlDocument 생성
            xml.Load(path);
            XmlNode root = xml.SelectSingleNode("Config");

            bool result = true;
            try
            {
                foreach (XmlNode xn in root.ChildNodes)
                {
                    if (xn.Name.ToLower() != "category")
                        continue;

                    string category = xn.Attributes["Name"].InnerText.ToLower();
                    if (false == _configCategory.ContainsKey(category))
                    {
                        var dataDict = new Dictionary<string, string>();
                        _configCategory[category] = dataDict;
                    }

                    Dictionary<string, string> itemDict = _configCategory[category];
                    foreach (XmlNode itemNode in xn.ChildNodes)
                    {
                        if (itemNode.Name.ToLower() != "item")
                            continue;

                        string item = itemNode.Attributes["Name"].InnerText.ToLower();

                        if (itemNode.Attributes["Value"] == null)
                            continue;

                        itemDict[item] = itemNode.Attributes["Value"].InnerText.ToLower();
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
                Log.AddLog(ex.Message);
            }

            return result;
        }

        public string GetConfigString(string category, string item, string defaultValue = "")
        {
            string cateKey = category.ToLower();
            string itemKey = item.ToLower();

            if (false == _configCategory.ContainsKey(cateKey))
            {
                return defaultValue;
            }

            if (false == _configCategory[cateKey].ContainsKey(itemKey))
            {
                return defaultValue;
            }

            return _configCategory[cateKey][itemKey];
        }

        public int GetConfigInt(string category, string item, int defaultValue = 0)
        {
            string cateKey = category.ToLower();
            string itemKey = item.ToLower();

            if (false == _configCategory.ContainsKey(cateKey))
            {
                return defaultValue;
            }

            if (false == _configCategory[cateKey].ContainsKey(itemKey))
            {
                return defaultValue;
            }

            int result = defaultValue;
            Int32.TryParse(_configCategory[cateKey][itemKey], out result);

            return result;
        }

        public bool GetConfigBool(string category, string item, bool defaultValue = false)
        {
            string cateKey = category.ToLower();
            string itemKey = item.ToLower();

            if (false == _configCategory.ContainsKey(cateKey))
            {
                return defaultValue;
            }

            if (false == _configCategory[cateKey].ContainsKey(itemKey))
            {
                return defaultValue;
            }

            bool result = defaultValue;
            Boolean.TryParse(_configCategory[cateKey][itemKey], out result);

            return result;
        }
    }
}
