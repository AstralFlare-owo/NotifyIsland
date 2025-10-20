using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Shared.Interfaces;
using ClassIsland.Shared.Models.Notification;
using Microsoft.Extensions.Hosting;
using MaterialDesignThemes.Wpf;
using cn.lixiaotuan.notifyisland.Controls;
using System.Windows.Controls;
using System.Windows;

namespace cn.lixiaotuan.notifyisland.Services.NotificationProviders;

public class APINotificationProvider : INotificationProvider, IHostedService
{
    public string Name { get; set; } = "API提醒";
    public string Description { get; set; } = "通过HTTP接口触发的提醒。";
    public Guid ProviderGuid { get; set; } = new Guid("534B80F5-8775-A978-95A3-F6A7BC5A1166");
    public object? SettingsElement { get; set; }
    public object? IconElement { get; set; } = new PackIcon()
    {
        Kind = PackIconKind.Webhook,
        Width = 24,
        Height = 24
    };

    public Plugin Plugin { get; }

    private INotificationHostService NotificationHostService { get; }

    public APINotificationProvider(INotificationHostService notificationHostService, Plugin plugin)
    {
        Plugin = plugin;
        NotificationHostService = notificationHostService;
        NotificationHostService.RegisterNotificationProvider(this);
    }

    public async Task StartAsync(CancellationToken cancellationToken) {
        if (Plugin.Settings.Enabled) {
            NotificationAPIServer.NotificationAPIServer.Current ??= new NotificationAPIServer.NotificationAPIServer(
                "http://"+Plugin.Settings.Host+":"+Plugin.Settings.Port.ToString()+"/",Plugin.Settings.Token);
            NotificationAPIServer.NotificationAPIServer.Current.NotificationReceived += NotificationAPIOnReceived;
        }
        Plugin.Settings.PropertyChanged += (s, e) =>
        {
            if (NotificationAPIServer.NotificationAPIServer.Current != null)
            {
                NotificationAPIServer.NotificationAPIServer.Current.NotificationReceived -= NotificationAPIOnReceived;
                NotificationAPIServer.NotificationAPIServer.Current?.Dispose();
            }
            if (Plugin.Settings.Enabled)
            {
                NotificationAPIServer.NotificationAPIServer.Current ??= new NotificationAPIServer.NotificationAPIServer(
                    "http://" + Plugin.Settings.Host + ":" + Plugin.Settings.Port.ToString() + "/",Plugin.Settings.Token);
                NotificationAPIServer.NotificationAPIServer.Current.NotificationReceived += NotificationAPIOnReceived;
            }
        };
    }
    
    public async Task StopAsync(CancellationToken cancellationToken) {
        if (NotificationAPIServer.NotificationAPIServer.Current != null)
        {
            NotificationAPIServer.NotificationAPIServer.Current.NotificationReceived -= NotificationAPIOnReceived;
            NotificationAPIServer.NotificationAPIServer.Current?.Dispose();
        }
    }

    private void NotificationAPIOnReceived(object? sender, NotificationAPIServer.NotificationReceivedEventArgs e)
    {
        NotificationHostService.ShowNotification(new NotificationRequest()
        {
            MaskContent = new NotificationMaskContent()
            {
                MaskContent = e.notification.title
            },
            MaskDuration = TimeSpan.FromSeconds(e.notification.title_duration),
            OverlayContent = new TextBlock()
            {
                Text = e.notification.content,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
            OverlayDuration = TimeSpan.FromSeconds(e.notification.content_duration),
            MaskSpeechContent = e.notification.title_voice ?? e.notification.title,
            OverlaySpeechContent = e.notification.content_voice ?? e.notification.content,
            RequestNotificationSettings = new ClassIsland.Shared.Models.Notification.NotificationSettings()
            {
                IsSettingsEnabled = true,
                IsNotificationSoundEnabled = e.notification.sound_enabled,
                IsNotificationEffectEnabled = e.notification.effect_enabled
            }
        });
    }
}