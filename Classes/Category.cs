using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;

namespace AudioPluginsManager.Classes;
public class Category : TreeViewItemBase
{
    public Category()
    {
        Plugins = [];
    }

    public string Name { get; set; }

    public ObservableCollection<Plugin> Plugins { get; set; }

}
