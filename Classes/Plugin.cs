﻿using System;
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
    private List<string> _keywords = [];

    public string Keywords
    {
        get { 
            var kw = string.Join(", ", _keywords);

            if (kw == "")
            {
                kw = BuildKeywords();
            }

            return kw;
        }
        set => _keywords = [.. value.Split(',')];
    }

    public string Name { get; set; } = "Unknown";
    public string Version { get; set; }
    public string FilePath { get; set; }
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

    public bool IsFiltered(string filter)
    {
        //  Filter type
        var typeFilter = false;

        if (Config.Main.IsInstrumentsVisible != Config.Main.IsFXVisible) 
        {

            if (Config.Main.IsInstrumentsVisible && !Categories.Contains("instrument"))
            {
                typeFilter = true;
            }

            if (Config.Main.IsFXVisible && !Categories.Contains("fx"))
            {
                typeFilter = true;
            }

            if (!typeFilter)
            {
                return false;
            }
        }

        if (filter == "")
        {
            return true;
        }

        var isFiltered = true;

        if (_keywords.Count == 0)
        {
            BuildKeywords();
        }

        var filterWords = filter.ToLower().Split(' ');

        foreach (var word in filterWords)
        {
            if (!_keywords.Any(kw => kw.Contains(word)))
            {
                isFiltered = false;
            }
        }

        return isFiltered;
    }


    public string BuildKeywords()
    {
        _keywords = [ Name.ToLower(), ];

        if (Classes == null)
        {
            return "";
        }

        foreach (var _class in Classes)
        {
            if (_class.Vendor != null && _class.Vendor != "" && !_keywords.Contains(_class.Vendor))
            {
                _keywords.Add(_class.Vendor.ToLower());
            }

            if (_class.SubCategories == null)
            {
                continue;
            }

            foreach (var subCategory in _class.SubCategories)
            {
                if (!_keywords.Contains(subCategory))
                {
                    _keywords.Add(subCategory.ToLower());
                }
            }
        }

        return string.Join(", ", _keywords);
    }


    public string[] Categories
    {
        get
        {
            List<string> categories = [];

            foreach (var _class in Classes)
            {
                if (_class.SubCategories == null)
                {
                    continue;
                }

                foreach (var subCategory in _class.SubCategories)
                {
                    if (!categories.Contains(subCategory))
                    {
                        categories.Add(subCategory.ToLower());
                    }
                }
            }

            return [.. categories];
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