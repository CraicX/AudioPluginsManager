using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AdonisUI;
using AdonisUI.Controls;
using AudioPluginsManager.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using JsonFormatterPlus;
using System.Text.RegularExpressions;

namespace AudioPluginsManager;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : AdonisWindow
{
    public bool IsVendorsVisible{ get; set; }     = true;
    public bool IsFXVisible{ get; set; }          = true;
    public bool IsInstrumentsVisible{ get; set; } = true;

    public MainWindow()
    {
        InitializeComponent();

        Config.Init(this);
    }

    private void TglVendors_Click(object sender, RoutedEventArgs e)
    {
        IsVendorsVisible = !IsVendorsVisible;
        UpdateToggleButtons();
    }

    private void TglFX_Click(object sender, RoutedEventArgs e)
    {
        IsFXVisible = !IsFXVisible;
        UpdateToggleButtons();
    }

    private void TglInstruments_Click(object sender, RoutedEventArgs e)
    {
        IsInstrumentsVisible = !IsInstrumentsVisible;
        UpdateToggleButtons();
    }

    private void UpdateToggleButtons()
    {
        BitmapImage bi = new();
        bi.BeginInit();
        bi.UriSource = new Uri("Resources/img/" + (IsVendorsVisible ? "t-vendor.png" : "t-vendor2.png"), UriKind.Relative);
        bi.EndInit();
        ImgVendors.Source     = bi;

        bi = new();
        bi.BeginInit();
        bi.UriSource = new Uri("Resources/img/" + (IsFXVisible ? "t-fx.png" : "t-fx2.png"), UriKind.Relative);
        bi.EndInit();
        ImgFX.Source = bi;

        bi = new();
        bi.BeginInit();
        bi.UriSource = new Uri("Resources/img/" + (IsInstrumentsVisible ? "t-instruments.png" : "t-instruments2.png"), UriKind.Relative);
        bi.EndInit();
        ImgInstruments.Source = bi;

        //ImgFX.Source          = new BitmapImage(new Uri("Resources/img/" + (IsFXVisible ? "t-fx.png" : "t-fx2.png")));
        //ImgInstruments.Source = new BitmapImage(new Uri("Resources/img/" + (IsInstrumentsVisible ? "t-instruments.png" : "t-instruments2.png")));

        Vst3.FilterPlugins(TBFilter.Text);

    }

    private void MnuFileExit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void MnuToolsOptions_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new Options();
        dlg.ShowDialog();
    }

    private void MnuToolsScan_Click(object sender, RoutedEventArgs e)
    {
        Vst3.GetVst3Plugins(true);
    }


    private void TVVendor_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        RTBInfo.Document.Blocks.Clear();
        if (e.NewValue is not Plugin obj)
        {
            return;
        }

        if (Vst3.PluginInfoMap.TryGetValue(obj.FilePath, out var pluginInfo))
        {
            var json = JsonFormatter.Format(pluginInfo.Json);

            // use regex to remove blank lines
            json = MyRegex().Replace(json, "");


            RTBInfo.Document.Blocks.Add(new Paragraph(new Run(json)));
        }
    }

    private void TBFilter_TextChanged(object sender, TextChangedEventArgs e)
    {
        Vst3.FilterPlugins(TBFilter.Text);
    }

    [GeneratedRegex(@"^\s+$[\r\n]*", RegexOptions.Multiline)]
    private static partial Regex MyRegex();
}