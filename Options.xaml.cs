using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    private static StringCollection Folders = [];

    public Options()
    {
        InitializeComponent();

        if (Properties.Settings.Default.Folders == null)
        {
            Properties.Settings.Default.Folders = [];
        }


        Folders = Properties.Settings.Default.Folders;

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
        Properties.Settings.Default.Folders = Folders;
        Properties.Settings.Default.Save();
        Close();
    }

    private void BtnOptsCancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

}
