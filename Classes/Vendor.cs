using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;

namespace AudioPluginsManager.Classes;
public class Vendor : TreeViewItemBase
{
    public string Name { get; set; }
    public string URL { get; set; }
    public string EMail { get; set; }
    public Dictionary<string, bool> Flags { get; set; } = [];
    public ObservableCollection<Plugin> Plugins { get; set; } = [];
}
