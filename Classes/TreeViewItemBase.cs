using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioPluginsManager.Classes;

public class TreeViewItemBase : INotifyPropertyChanged
{
    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (value != isSelected)
            {
                isSelected = value;
                NotifyPropertyChanged("IsSelected");
            }
        }
    }

    private bool isExpanded;
    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (value != isExpanded)
            {
                isExpanded = value;
                NotifyPropertyChanged("IsExpanded");
            }
        }
    }


    public event PropertyChangedEventHandler PropertyChanged;

    public void NotifyPropertyChanged(string propName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}