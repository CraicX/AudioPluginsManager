using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace AudioPluginsManager.Classes;
public class Plugin : TreeViewItemBase
{
    public string Name { get; set; } = "Unknown";
    public string Version { get; set; }
    public string FilePath { get; set;
    }
    public string Image
    {
        get
        {
            if (Categories.Contains("instrument"))
            {
                return "Resources/img/am-instrumentv3a.png";
            }
            else
            {
                return "Resources/img/am-fxv3a.png";
            }
        }
    }

    public string[] Categories
    {
        get
        {
            List<string> categories = [];

            foreach (var _class in Classes)
            {
                if (_class.SubCategories == null) continue;

                foreach (var subCategory in _class.SubCategories)
                {
                    if (!categories.Contains(subCategory))
                    {
                        categories.Add(subCategory.ToLower());
                    }
                }
            }

            return categories.ToArray();
        }
    }




    [JsonPropertyName("Factory Info")]
    public FactoryInfo FactoryInfo { get; set; }
    public List<Classes> Classes { get; set; }
}

public class FactoryInfo
{
    public string Vendor { get; set; }
    public string URL { get; set; }

    [JsonPropertyName("E-Mail")]
    public string EMail { get; set; }
    public Dictionary<string, bool> Flags { get; set; } = [];
}

public class Classes
{
    public string CID { get; set; }
    public string Category { get; set; }
    public string Name { get; set; }
    public string Vendor { get; set; }
    public string Version { get; set; }
    public string SDKVersion { get; set; }

    [JsonPropertyName("Sub Categories")]
    public List<string> SubCategories { get; set; }

    [JsonPropertyName("Class Flags")]
    public int ClassFlags { get; set; }
    public int Cardinality { get; set; }
}