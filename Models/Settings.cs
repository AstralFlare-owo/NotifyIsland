
using CommunityToolkit.Mvvm.ComponentModel;

namespace cn.lixiaotuan.notifyisland.Models;

public partial class Settings : ObservableObject
{
    [ObservableProperty] private string _host = "*";
    [ObservableProperty] private int _port = 1379;
    [ObservableProperty] private string _token = "";
    [ObservableProperty] private bool _enabled = false;
}