using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using AdonisUI.Controls;
using AudioPluginsManager.Classes;
using JsonFormatterPlus;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

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
            
            TBVName.Text     = obj.Name;
            TBVFile.Text     = obj.FilePath;
            TBVCategory.Text = string.Join(", ", obj.Categories.Distinct());
            TBVVendor.Text   = obj.FactoryInfo.Vendor;
            TBVUrl.Text      = obj.FactoryInfo.URL;
            TBVSDK.Text      = obj.Classes[0].SDKVersion;
            TBVVersion.Text  = obj.Classes[0].Version;

            RTBInfo.Document.Blocks.Add(new Paragraph(new Run(json)));
        }
    }

    private void TBFilter_TextChanged(object sender, TextChangedEventArgs e)
    {
        Vst3.FilterPlugins(TBFilter.Text);
    }

    [GeneratedRegex(@"^\s+$[\r\n]*", RegexOptions.Multiline)]
    private static partial Regex MyRegex();

    private async void BtnScrape_Click(object sender, RoutedEventArgs e)
    {
        //  Search bing for images of the VST using webview2

        //  Create serveless webview2
        var query = UrlEncoder.Default.Encode(TBVName.Text + " vst");

        var wv2 = new WebPageParser();
        await wv2.NavigateAndParseAsync("https://www.bing.com/images/search?q=" + query);
        
        //  Get the images
        var images = wv2.GetImages();

        //  Get the links
        var links = wv2.GetLinks();

    }
}