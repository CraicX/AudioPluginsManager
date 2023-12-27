using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AudioPluginsManager.Classes;
public static class Config
{
    public static string Vst3InfoPath { get; set; }

    public static MainWindow Main { get; set; } = null;

    public static void Init(MainWindow _main)
    {
        Main = _main;

        Vst3InfoPath = Path.Combine(AssemblyDirectory, "Resources", "Data", "Vst3Info");

        Vst3.Init();
    }

    public static string AssemblyDirectory
    {
        get
        {
            var codeBase   = Assembly.GetExecutingAssembly().Location;
            var uri = new UriBuilder(codeBase);
            var path       = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
