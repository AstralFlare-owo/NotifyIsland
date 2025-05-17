
using System.Globalization;
using System.Net;
using System.Windows.Controls;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;
using MaterialDesignThemes.Wpf;

namespace cn.lixiaotuan.notifyisland.Views;

[SettingsPageInfo("notifyisland.settingspage", "NotifyIsland", PackIconKind.Webhook, PackIconKind.Webhook, SettingsPageCategory.Debug)]
public partial class NotifyIslandSettingsPage : SettingsPageBase
{
    public Plugin Plugin { get; }

    public NotifyIslandSettingsPage(Plugin plugin)
    {
        Plugin = plugin;
        InitializeComponent();
        DataContext = this;
    }
}

public class PortValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        int val = 0;
        if (!int.TryParse(value.ToString(), out val)) return new ValidationResult(false, "端口号必须为数字");
        if (val < 1 || val > 65535) return new ValidationResult(false, "端口号必须在1~65535之间");
        return new ValidationResult(true, null);
    }
}

public class HostValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string parsed = value.ToString();
        if (parsed == "*" || parsed == "+") return new ValidationResult(true, null);
        IPAddress parsedIP;
        if (!IPAddress.TryParse(parsed, out parsedIP)) return new ValidationResult(false, "无效的IP地址");
        if (parsedIP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 && !parsed.Contains("[")) return new ValidationResult(false, "IPv6地址需要方括号");
        return new ValidationResult(true, null);
    }
}