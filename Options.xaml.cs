using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AudioPluginsManager;
/// <summary>
/// Interaction logic for Options.xaml
/// </summary>
public partial class Options : Window
{
    private static readonly List<string> Folders = [];

    public Options()
    {
        InitializeComponent();

        Folders.AddRange((IEnumerable<string>)Properties.Settings.Default.Folders);

        foreach (var folder in Folders)
        {
            LBOptionsFolders.Items.Add(folder);
        }
    }

    private void BtnOptsAddFolder_Click(object sender, RoutedEventArgs e)
    {
        if (TBFolder.Text != "")
        {
            if (!Folders.Contains(TBFolder.Text)) 
            {
                Folders.Add(TBFolder.Text);
                LBOptionsFolders.Items.Add(TBFolder.Text);
            }
            TBFolder.Text = "";
        }

    }

    private void BtnOptsRemoveFolder_Click(object sender, RoutedEventArgs e)
    {
        if (LBOptionsFolders.SelectedIndex != -1)
        {
            Folders.RemoveAt(LBOptionsFolders.SelectedIndex);
            LBOptionsFolders.Items.RemoveAt(LBOptionsFolders.SelectedIndex);
        }
    }

    private void BtnOptsOK_Click(object sender, RoutedEventArgs e)
    {
        Properties.Settings.Default.Folders.Clear();
        Properties.Settings.Default.Folders.AddRange([.. Folders]);
        Properties.Settings.Default.Save();
        Close();
    }

    private void BtnOptsCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

}
