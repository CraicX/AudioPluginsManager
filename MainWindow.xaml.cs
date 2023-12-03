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

namespace AudioPluginsManager;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : AdonisWindow
{
    public MainWindow()
    {
        InitializeComponent();

        Config.Init(this);
    }
}