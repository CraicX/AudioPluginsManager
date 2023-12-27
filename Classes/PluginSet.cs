using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioPluginsManager.Classes;
public class PluginSet
{
    public List<PluginInfo> Plugins { get; set; } = [];
}

public class PluginInfo
{
    public string FilePath { get; set; }
    public bool Enabled { get; set; }
    public List<string> Images { get; set; } = [];
    public string Json { get; set; }
}