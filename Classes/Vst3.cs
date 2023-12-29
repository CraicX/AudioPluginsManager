using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using JsonFormatterPlus;
using System.Threading.Tasks;

namespace AudioPluginsManager.Classes;
public static class Vst3
{
    public static StringCollection PluginFolders { get; set; }               = [];
    public static List<string> PluginPaths { get; set; }                     = [];
    public static List<Plugin> PluginsAll { get; set; }                      = [];
    public static List<Plugin> Plugins { get; set; }                         = [];
    public static Dictionary<string, Vendor> VendorMap { get; set; }         = [];
    public static Dictionary<string, Category> CategoryMap { get; set; }     = [];
    public static Dictionary<string, PluginInfo> PluginInfoMap { get; set; } = [];

    public static void Init()
    {
        if (Properties.Settings.Default.Folders != null)
        {
            PluginFolders = Properties.Settings.Default.Folders;
        }

        if (PluginFolders.Count == 0)
        {
            var programFiles    = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
            var programFilesX86 = Environment.ExpandEnvironmentVariables("%ProgramFiles(x86)%");

            var vst3Path = Path.Combine(programFiles, "Common Files", "VST3");
            if (Directory.Exists(vst3Path))
            {
                PluginFolders.Add(vst3Path);
            }

            vst3Path = Path.Combine(programFilesX86, "Common Files", "VST3");
            if (Directory.Exists(vst3Path))
            {
                PluginFolders.Add(vst3Path);
            }

            vst3Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Steinberg", "VST3");
            if (Directory.Exists(vst3Path))
            {
                PluginFolders.Add(vst3Path);
            }

            vst3Path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Steinberg", "VST3");
            if (Directory.Exists(vst3Path))
            {
                PluginFolders.Add(vst3Path);
            }

            Properties.Settings.Default.Folders = PluginFolders;
            Properties.Settings.Default.Save();
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

        json = JsonFormatter.Format(json);

        File.WriteAllText(MapPath, json);
    }


    public static async void GetVst3Plugins(bool rescan=false)
    {
        PluginPaths = await GetVst3PathsAsync();

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
        }
        foreach (var vendor in VendorMap.Values)
        {
            Config.Main.TVVendor.Items.Add(vendor);
        }

        //  Sort the TreeView
        Config.Main.TVVendor.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

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

                json = JsonFormatter.Minify(json);

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

    private static async Task<List<string>> GetVst3PathsAsync()
    {
        if (Properties.Settings.Default.Folders != null)
        {
            foreach (var folder in Properties.Settings.Default.Folders)
            {
                if (!PluginFolders.Contains(folder) && Directory.Exists(folder))
                {
                    PluginFolders.Add(folder);
                }
            }
        }

        List<string> _Paths = [];

        foreach (var folder in PluginFolders)
        {
            _Paths.AddRange(await GetAllVst3FilesAsync(folder));
        }


        Process validator = new();

        validator.StartInfo.FileName        = "Lib\\get-plugins.cmd";
        validator.StartInfo.Arguments       = "";
        validator.StartInfo.UseShellExecute = false;
        validator.StartInfo.CreateNoWindow  = true;

        validator.Start();
        await validator.WaitForExitAsync();

        // read text file
        // add each line to PluginsAll list

        var lines = System.IO.File.ReadAllLines("Lib\\plugins.txt");

        foreach (var line in lines)
        {
            if (!_Paths.Contains(line) && File.Exists(line))
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

    public static List<string> GetAllVst3Files(string directoryPath)
    {
        var files = new List<string>();
        if (Directory.Exists(directoryPath))
        {
            files.AddRange(Directory.GetFiles(directoryPath, "*.vst3", SearchOption.AllDirectories));
        }

        for (var i = files.Count; --i >= 0;)
        {
            files[i] = files[i].Replace("\\", "/");
        }


        return files;
    }

    public static async Task<List<string>> GetAllVst3FilesAsync(string directoryPath)
    {
        return await Task.Run(() =>
        {
            var files = new List<string>();
            if (Directory.Exists(directoryPath))
            {
                files.AddRange(Directory.GetFiles(directoryPath, "*.vst3", SearchOption.AllDirectories));
            }

            for (var i = files.Count; --i >= 0;)
            {
                files[i] = files[i].Replace("\\", "/");
            }

            return files;
        });
    }
}
