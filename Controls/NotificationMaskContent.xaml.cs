
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace cn.lixiaotuan.notifyisland.Controls;

public partial class NotificationMaskContent : UserControl, INotifyPropertyChanged
{
    public string _maskContent = "";

    public string MaskContent
    {
        get => _maskContent;
        set
        {
            if (value == _maskContent) return;
            _maskContent = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public NotificationMaskContent()
    {
        InitializeComponent();
        DataContext = this;
    }
}
