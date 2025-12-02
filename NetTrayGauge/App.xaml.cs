using System;
using System.IO;
using System.Windows;
using NetTrayGauge.Models;
using NetTrayGauge.Rendering;
using NetTrayGauge.Services;
using NetTrayGauge.ViewModels;
using NetTrayGauge.Views.Windows;

namespace NetTrayGauge;

/// <summary>
/// App entry point bootstrapping services.
/// </summary>
public partial class App : Application
{
    private SettingsService? _settingsService;
    private LoggingService? _loggingService;
    private NetworkMonitor? _networkMonitor;
    private NotifyIconService? _notifyIconService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetTrayGauge");
        _loggingService = new LoggingService(appData);
        _settingsService = new SettingsService(appData);
        _settingsService.Load();

        var autostart = new AutostartService("NetTrayGauge", Environment.ProcessPath ?? string.Empty);
        if (_settingsService.Current.StartWithWindows && !autostart.IsEnabled())
        {
            autostart.Enable();
        }

        _networkMonitor = new NetworkMonitor(() => _settingsService.Current, _loggingService);
        var popupViewModel = new PopupViewModel();
        var settingsViewModel = new SettingsViewModel(_settingsService, _networkMonitor);
        var popupWindow = new PopupWindow(popupViewModel, _settingsService);
        var settingsWindow = new SettingsWindow(settingsViewModel, autostart);
        var renderer = new TrayRenderer(_settingsService.Current.FontFamily);
        _notifyIconService = new NotifyIconService(_settingsService, _networkMonitor, renderer, _loggingService, popupViewModel, autostart, popupWindow, settingsWindow);

        _networkMonitor.Start();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _networkMonitor?.Dispose();
        _notifyIconService?.Dispose();
        _loggingService?.Info("Application exited");
        base.OnExit(e);
    }
}
