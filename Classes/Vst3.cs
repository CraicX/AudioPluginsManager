using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AudioPluginsManager.Classes;
public static class Vst3
{
    public static List<string> PluginFolders { get; set; }                   = [];
    public static List<string> PluginPaths { get; set; }                     = [];
    public static List<Plugin> PluginsAll { get; set; }                      = [];
    public static List<Plugin> Plugins { get; set; }                         = [];
    public static Dictionary<string, Vendor> VendorMap { get; set; }         = [];
    public static Dictionary<string, Category> CategoryMap { get; set; }     = [];
    public static Dictionary<string, PluginInfo> PluginInfoMap { get; set; } = [];

    public static void Init()
    {
        if (Properties.Settings.Default.Folders != null && Properties.Settings.Default.Folders.Count > 0)
        {
            PluginFolders.AddRange((IEnumerable<string>)Properties.Settings.Default.Folders);
        }

        LoadMap();

        if (PluginInfoMap.Count == 0)
        {
            GetVst3Plugins();
        }
        else
        {
            GetPluginInfo();
        }
    }

    public static void LoadMap()
    {
        var MapPath = Path.Combine(Config.Vst3InfoPath, "PluginSet.map");

        if (!File.Exists(MapPath))
        {
            return;
        }

        var json = File.ReadAllText(MapPath);

        var pluginSet = System.Text.Json.JsonSerializer.Deserialize<PluginSet>(json);

        foreach (var pluginInfo in pluginSet.Plugins)
        {
            PluginInfoMap[pluginInfo.FilePath] = pluginInfo;
        }
    }

    public static void SaveMap()
    {
        var MapPath = Path.Combine(Config.Vst3InfoPath, "PluginSet.map");

        var pluginSet = new PluginSet();

        pluginSet.Plugins.AddRange(PluginInfoMap.Values);

        var json = System.Text.Json.JsonSerializer.Serialize(pluginSet);

        File.WriteAllText(MapPath, json);
    }


    public static void GetVst3Plugins(bool rescan=false)
    {
        PluginPaths = GetVst3Paths();

        if (rescan)
        {
            PluginInfoMap.Clear();
            Plugins.Clear();
            PluginsAll.Clear();
            VendorMap.Clear();
            CategoryMap.Clear();
            Config.Main.TVVendor.ItemsSource = null;
            Config.Main.TVVendor.Items.Clear();
        }

        BuildPluginInfo(rescan);
    }
    
    private static void GetPluginInfo()
    {
        var JsonOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
        };

        foreach (var pluginInfo in PluginInfoMap.Values)
        {
            List<string> categories = [];

            var plugin = System.Text.Json.JsonSerializer.Deserialize<Plugin>(pluginInfo.Json, JsonOptions);

            plugin.FilePath = pluginInfo.FilePath;

            Plugins.Add(plugin);

            if (VendorMap.TryGetValue(plugin.FactoryInfo.Vendor, out var vendor))
            {
                if (!vendor.Plugins.Contains<Plugin>(plugin))
                {
                    vendor.Plugins.Add(plugin);
                }
            }
            else
            {
                if (plugin.FactoryInfo.Vendor == null)
                {
                    plugin.FactoryInfo.Vendor = "Unknown";
                }

                vendor = new Vendor
                {
                    Name    = plugin.FactoryInfo.Vendor,
                    URL     = plugin.FactoryInfo.URL,
                    EMail   = plugin.FactoryInfo.EMail,
                    Plugins = [plugin],
                };

                VendorMap[plugin.FactoryInfo.Vendor] = vendor;
            }

            foreach (var _class in plugin.Classes)
            {
                if (_class.SubCategories == null)
                {
                    continue;
                }

                foreach (var _subCategory in _class.SubCategories)
                {
                    if (!categories.Contains(_subCategory))
                    {
                        categories.Add(_subCategory);
                    }
                }
            }


            foreach (var _category in categories)
            {
                if (CategoryMap.TryGetValue(_category, out var category))
                {
                    if (!category.Plugins.Contains<Plugin>(plugin))
                    {
                        category.Plugins.Add(plugin);
                    }
                }
                else
                {
                    category = new Category
                    {
                        Name    = _category,
                        Plugins = [plugin],
                    };

                    CategoryMap[_category] = category;
                }
            }
            
            PluginsAll.Add(plugin);
        }

        //  Add commas to the plugins count
        Config.Main.LblActive.Text = string.Concat("Active: ", Plugins.Count.ToString("N0"));

        Config.Main.TVVendor.ItemsSource   = null;
        
        foreach (var vendor in VendorMap.Values)
        {
            Config.Main.TVVendor.Items.Add(vendor);
        }

        //  Sort the TreeView
        Config.Main.TVVendor.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

        Debug.WriteLine("Done");

    }

    public static void FilterPlugins(string filter)
    {
        Config.Main.TVVendor.ItemsSource = null;
        Config.Main.TVVendor.Items.Clear();

        using (Config.Main.TVVendor.Items.DeferRefresh())
        {
            Plugins.Clear();
            VendorMap.Clear();

            foreach (var plugin in PluginsAll)
            {
                if (plugin.IsFiltered(filter))
                {
                    Plugins.Add(plugin);

                    if (VendorMap.TryGetValue(plugin.FactoryInfo.Vendor, out var vendor))
                    {
                        if (!vendor.Plugins.Contains<Plugin>(plugin))
                        {
                            vendor.Plugins.Add(plugin);
                        }
                    }
                    else
                    {
                        if (plugin.FactoryInfo.Vendor == null)
                        {
                            plugin.FactoryInfo.Vendor = "Unknown";
                        }

                        vendor = new Vendor
                        {
                            Name    = plugin.FactoryInfo.Vendor,
                            URL     = plugin.FactoryInfo.URL,
                            EMail   = plugin.FactoryInfo.EMail,
                            Plugins = [plugin],
                        };

                        VendorMap[plugin.FactoryInfo.Vendor] = vendor;
                    }
                }
            }

            // Add to treeview

            Config.Main.LblActive.Text = string.Concat("Active: ", Plugins.Count.ToString("N0"));
            //Config.Main.TVVendor.ItemsSource = VendorMap.Values;
        }
        foreach (var vendor in VendorMap.Values)
        {
            Config.Main.TVVendor.Items.Add(vendor);
        }

        //  Sort the TreeView
        Config.Main.TVVendor.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

        //Config.Main.TVVendor.Items.Refresh();
    }


    private static async void BuildPluginInfo(bool rebuild=false)
    {
        var CurrentCount   = 0;
        var PluginsToScan  = 0;

        foreach (var pluginPath in PluginPaths)
        {
            if (!rebuild && PluginInfoMap.ContainsKey(pluginPath))
            {
                continue;
            }
            
            PluginsToScan++;
        }

        Config.Main.PBar.Maximum = PluginsToScan;
        Config.Main.PBar.Value   = 0;

        foreach (var pluginPath in PluginPaths)
        {
            if (!rebuild && PluginInfoMap.ContainsKey(pluginPath))
            {
                continue;
            }

            var TimeOut      = 30000;
     
            Config.Main.PBar.Value = ++CurrentCount;

            var PathHash = GetHash(pluginPath);

            var pluginInfoPath = Path.Combine(Config.Vst3InfoPath, string.Concat(Path.GetFileName(pluginPath), "_", PathHash, ".json"));

            Config.Main.LblStatus.Text = string.Concat("Scanning plugin [", CurrentCount, "/", PluginsToScan, "]:  ", Path.GetFileName(pluginPath));

            Process moduleInfo = new();

            moduleInfo.StartInfo.FileName              = "Lib\\moduleinfotool.exe";
            moduleInfo.StartInfo.Arguments             = string.Concat("-create -version 3 -output \"", pluginInfoPath, "\" -path \"", pluginPath, "\"");
            moduleInfo.StartInfo.UseShellExecute       = false;
            moduleInfo.StartInfo.CreateNoWindow        = true;
            moduleInfo.StartInfo.RedirectStandardError = true;
            moduleInfo.Start();

            while (!moduleInfo.HasExited && --TimeOut > 0)
            {
                await Task.Delay(1);
            }

            if (TimeOut <= 0)
            {
                moduleInfo.Kill();
                Debug.WriteLine("Error: Timed out");
            }


            if (moduleInfo.ExitCode != 0)
            {
                Debug.WriteLine(string.Concat("Error: ", moduleInfo.ExitCode));
            }
            else if (File.Exists(pluginInfoPath))
            {
                var json = File.ReadAllText(pluginInfoPath);

                if (string.IsNullOrEmpty(json?.Trim()))
                {
                    continue;
                }

                if (PluginInfoMap.TryGetValue(pluginPath, out _))
                {
                    PluginInfoMap[pluginPath].Json = json;
                }
                else
                {
                    PluginInfoMap[pluginPath] = new PluginInfo()
                    {
                        Enabled  = true,
                        FilePath = pluginPath,
                        Json     = json,
                    };
                }
            }

            moduleInfo.Close();

            if (CurrentCount % 25 == 0)
            {
                SaveMap();
            }

            File.Delete(pluginInfoPath);
        }

        Config.Main.LblStatus.Text = "Done Scanning VST3";
        Config.Main.PBar.Value     = 0;

        SaveMap();

        GetPluginInfo();

    }

    private static List<string> GetVst3Paths()
    {
        List<string> _Paths = [];

        Process validator = new();

        validator.StartInfo.FileName        = "Lib\\get-plugins.cmd";
        validator.StartInfo.Arguments       = "";
        validator.StartInfo.UseShellExecute = false;
        validator.StartInfo.CreateNoWindow  = true;

        validator.Start();
        validator.WaitForExit();

        // read text file
        // add each line to PluginsAll list

        var lines = System.IO.File.ReadAllLines("Lib\\plugins.txt");

        foreach (var line in lines)
        {
            if (File.Exists(line))
            {
                _Paths.Add(line);
            }
        }

        return _Paths;
    }


    public static string GetHash(string input) 
    { 
        return string.Format("{0:X}", input.GetHashCode());
    }
}
