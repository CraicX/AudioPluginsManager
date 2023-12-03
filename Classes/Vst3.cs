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
    public static List<string> PluginPaths { get; set; }             = [];
    public static List<Plugin> PluginsAll { get; set; }              = [];
    public static List<Plugin> Plugins { get; set; }                 = [];
    public static Dictionary<string, Vendor> VendorMap { get; set; } = [];
    public static Dictionary<string, Category> CategoryMap { get; set; } = [];

    public static void GetVst3Plugins()
    {
        PluginPaths = GetVst3Paths();

        BuildPluginInfo();

        GetPluginInfo();
    }
    
    private static void GetPluginInfo()
    {
        //  Get list of json files in Vst3InfoPath

        List<string> jsonFiles = [];

        Dictionary<string, string> VSTMap = [];

        foreach (var file in PluginPaths)
        {
            VSTMap[string.Concat(Path.GetFileName(file), ".json")] = file;
        }

        var JsonOptions = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
        };

        foreach (var file in Directory.GetFiles(Config.Vst3InfoPath))
        {
            if (file.EndsWith(".json"))
            {
                jsonFiles.Add(file);
            }
        }

        //  Parse each json file

        foreach (var file in jsonFiles)
        {
            var json = File.ReadAllText(file);

            if (string.IsNullOrEmpty(json?.Trim()))
            {
                continue;
            }

            List<string> categories = [];

            var plugin = System.Text.Json.JsonSerializer.Deserialize<Plugin>(json, JsonOptions);

            if (VSTMap.TryGetValue(Path.GetFileName(file), out var pluginPath))
            {
                plugin.FilePath = pluginPath;

                Plugins.Add(plugin);

                if (VendorMap.TryGetValue(plugin.FactoryInfo.Vendor, out var vendor))
                {
                    if (!vendor.Plugins.Contains<Plugin>(plugin)) {
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
            }

            PluginsAll.Add(plugin);
        }

        //  Add each plugin to the TreeView

        //  Add commas to the plugins count
        Config.Main.LblActive.Text = string.Concat("Active: ", Plugins.Count.ToString("N0"));

        //Config.Main.TVName.ItemsSource     = Plugins;
        Config.Main.TVVendor.ItemsSource   = VendorMap.Values;
        //Config.Main.TVCategory.ItemsSource = CategoryMap.Values;

        //  Sort the TreeView

        //Config.Main.TVName.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        Config.Main.TVVendor.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        //Config.Main.TVCategory.Items.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

        Debug.WriteLine("Done");

    }

    private static async void BuildPluginInfo()
    {
        var TotalPlugins = PluginPaths.Count;
        var CurrentCount = 0;

        Config.Main.PBar.Maximum = TotalPlugins;
        Config.Main.PBar.Value   = 0;

        foreach (var pluginPath in PluginPaths)
        {
            Config.Main.PBar.Value = ++CurrentCount;

            var pluginInfoPath = Path.Combine(Config.Vst3InfoPath, string.Concat(Path.GetFileName(pluginPath), ".json"));

            if (File.Exists(pluginInfoPath))
            {
                continue;
            }

            Config.Main.LblStatus.Text = string.Concat("Building plugin info for ", Path.GetFileName(pluginPath));

            Process moduleInfo = new();

            moduleInfo.StartInfo.FileName              = "Lib\\moduleinfotool.exe";
            moduleInfo.StartInfo.Arguments             = string.Concat("-create -version 3 -output \"", pluginInfoPath, "\" -path \"", pluginPath, "\"");
            moduleInfo.StartInfo.UseShellExecute       = false;
            moduleInfo.StartInfo.CreateNoWindow        = true;
            moduleInfo.StartInfo.RedirectStandardError = true;
            //moduleInfo.StartInfo.WorkingDirectory    = "Lib";
            moduleInfo.Start();

            await moduleInfo.WaitForExitAsync();

            if (moduleInfo.ExitCode != 0)
            {
                Debug.WriteLine(string.Concat("Error: ", moduleInfo.ExitCode));
                //Debug.WriteLine(moduleInfo.StandardError.ReadToEnd());
            }

            moduleInfo.Close();

        }

        Config.Main.LblStatus.Text = "Done Scanning VST3";
        Config.Main.PBar.Value     = 0;
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

}
