using System;
using System.Windows;
using Forms = System.Windows.Forms;
using NetTrayGauge.Models;
using NetTrayGauge.Rendering;
using NetTrayGauge.Utilities;
using NetTrayGauge.ViewModels;
using NetTrayGauge.Views.Windows;

namespace NetTrayGauge.Services;

/// <summary>
/// Hosts the system tray icon and related UI.
/// </summary>
public class NotifyIconService : IDisposable
{
    private readonly SettingsService _settingsService;
    private readonly NetworkMonitor _monitor;
    private readonly TrayRenderer _renderer;
    private readonly LoggingService _logger;
    private readonly PopupViewModel _popupViewModel;
    private readonly AutostartService _autostartService;
    private readonly PopupWindow _popupWindow;
    private readonly SettingsWindow _settingsWindow;
    private readonly Forms.ContextMenuStrip _menu;
    private readonly Forms.NotifyIcon _notifyIcon;
    private GaugeScale _scales = new() { DownloadMax = 1024 * 1024, UploadMax = 1024 * 1024 };
    private double _peakDownload;
    private double _peakUpload;

    public NotifyIconService(
        SettingsService settingsService,
        NetworkMonitor monitor,
        TrayRenderer renderer,
        LoggingService logger,
        PopupViewModel popupViewModel,
        AutostartService autostartService,
        PopupWindow popupWindow,
        SettingsWindow settingsWindow)
    {
        _settingsService = settingsService;
        _monitor = monitor;
        _renderer = renderer;
        _logger = logger;
        _popupViewModel = popupViewModel;
        _autostartService = autostartService;
        _popupWindow = popupWindow;
        _settingsWindow = settingsWindow;

        _menu = BuildMenu();
        _notifyIcon = new Forms.NotifyIcon
        {
            Visible = true,
            Text = "NetTrayGauge",
            ContextMenuStrip = _menu
        };

        _monitor.SnapshotAvailable += OnSnapshot;
        _notifyIcon.MouseClick += NotifyIconOnMouseClick;
    }

    private Forms.ContextMenuStrip BuildMenu()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Öffnen / Mini-Dashboard", null, (_, _) => TogglePopup());
        menu.Items.Add("Interface wählen…", null, (_, _) => OpenSettings());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Digitalanzeige an/aus", null, (_, _) => ToggleOverlay());
        var units = new Forms.ToolStripMenuItem("Einheiten…");
        foreach (var mode in Enum.GetValues<UnitMode>())
        {
            units.DropDownItems.Add(new Forms.ToolStripMenuItem(mode.ToString(), null, (_, _) => SetUnitMode(mode)));
        }
        menu.Items.Add(units);
        menu.Items.Add("Design & Größe…", null, (_, _) => OpenSettings());
        menu.Items.Add("Skalierung…", null, (_, _) => OpenSettings());
        menu.Items.Add("Intervall & Glättung…", null, (_, _) => OpenSettings());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Autostart an/aus", null, (_, _) => ToggleAutostart());
        menu.Items.Add("Protokolle/Diagnose…", null, (_, _) => ShowLogs());
        menu.Items.Add(new Forms.ToolStripSeparator());
        menu.Items.Add("Beenden", null, (_, _) => System.Windows.Application.Current.Shutdown());
        return menu;
    }

    private void NotifyIconOnMouseClick(object? sender, Forms.MouseEventArgs e)
    {
        if (e.Button == Forms.MouseButtons.Left)
        {
            TogglePopup();
        }
    }

    private void TogglePopup()
    {
        if (_popupWindow.IsVisible)
        {
            _popupWindow.Hide();
        }
        else
        {
            PositionPopup();
            _popupWindow.Show();
            _popupWindow.Activate();
        }
    }

    private void PositionPopup()
    {
        var settings = _settingsService.Current;
        if (!double.IsNaN(settings.WindowLeft) && !double.IsNaN(settings.WindowTop))
        {
            _popupWindow.Left = settings.WindowLeft;
            _popupWindow.Top = settings.WindowTop;
        }
        _popupWindow.Width = settings.WindowWidth;
        _popupWindow.Height = settings.WindowHeight;
    }

    private void OnSnapshot(object? sender, NetworkSnapshot snapshot)
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
        {
            return;
        }

        _ = dispatcher.InvokeAsync(() =>
        {
            _popupViewModel.Update(snapshot, _settingsService.Current.UnitMode);
            UpdateScales(snapshot);
            _popupViewModel.UpdateScales(_scales.DownloadMax, _scales.UploadMax);
            var icon = _renderer.Render(snapshot, _settingsService.Current, _scales, DpiHelper.GetScale());
            _notifyIcon.Icon = icon;
            _notifyIcon.Text = BuildTooltip(snapshot);
        });
    }

    private string BuildTooltip(NetworkSnapshot snapshot)
    {
        const int maxLength = 63; // WinForms limit for NotifyIcon.Text
        var unitMode = _settingsService.Current.UnitMode;

        var down = Utilities.UnitFormatter.FormatText(snapshot.DownloadBytesPerSecond, unitMode, 1);
        var up = Utilities.UnitFormatter.FormatText(snapshot.UploadBytesPerSecond, unitMode, 1);
        var tooltip = ComposeTooltip(down, up);

        if (tooltip.Length <= maxLength)
        {
            return tooltip;
        }

        down = Utilities.UnitFormatter.FormatText(snapshot.DownloadBytesPerSecond, unitMode, 0);
        up = Utilities.UnitFormatter.FormatText(snapshot.UploadBytesPerSecond, unitMode, 0);
        tooltip = ComposeTooltip(down, up);

        if (tooltip.Length <= maxLength)
        {
            return tooltip;
        }

        int maxDownLength = Math.Max(0, maxLength - "Up: ".Length - 1);
        string downLine = ClampLine("Down: ", down, maxDownLength);

        int remaining = Math.Max(0, maxLength - downLine.Length - 1);
        string upLine = ClampLine("Up: ", up, remaining);

        return $"{downLine}\n{upLine}";
    }

    private static string ComposeTooltip(string down, string up) => $"Down: {down}\nUp: {up}";

    private static string ClampLine(string label, string value, int maxLength)
    {
        var line = $"{label}{value}";
        if (line.Length <= maxLength)
        {
            return line;
        }

        int available = Math.Max(0, maxLength - label.Length);
        if (available <= 0)
        {
            return label.TrimEnd();
        }

        var trimmedValue = value.Substring(0, Math.Min(value.Length, available));
        return $"{label}{trimmedValue}";
    }

    private void UpdateScales(NetworkSnapshot snapshot)
    {
        var settings = _settingsService.Current;
        double decay = settings.MaxScaleDecay;
        if (settings.MaxDownloadScale.HasValue)
        {
            _scales.DownloadMax = settings.MaxDownloadScale.Value;
        }
        else
        {
            _peakDownload = Math.Max(_peakDownload * decay, snapshot.DownloadBytesPerSecond);
            _scales.DownloadMax = Math.Max(_peakDownload * 1.1, 1024 * 8);
        }

        if (settings.MaxUploadScale.HasValue)
        {
            _scales.UploadMax = settings.MaxUploadScale.Value;
        }
        else
        {
            _peakUpload = Math.Max(_peakUpload * decay, snapshot.UploadBytesPerSecond);
            _scales.UploadMax = Math.Max(_peakUpload * 1.1, 1024 * 8);
        }
    }

    private void ToggleOverlay()
    {
        _settingsService.Update(s => s.ShowDigitalOverlay = !s.ShowDigitalOverlay);
    }

    private void SetUnitMode(UnitMode mode)
    {
        _settingsService.Update(s => s.UnitMode = mode);
    }

    private void ToggleAutostart()
    {
        try
        {
            if (_autostartService.IsEnabled())
            {
                _autostartService.Disable();
                _settingsService.Update(s => s.StartWithWindows = false);
            }
            else
            {
                _autostartService.Enable();
                _settingsService.Update(s => s.StartWithWindows = true);
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Autostart toggle failed", ex);
        }
    }

    private void ShowLogs()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\NetTrayGauge\\logs",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _logger.Error("Unable to open log folder", ex);
        }
    }

    private void OpenSettings()
    {
        _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    public void Dispose()
    {
        _monitor.SnapshotAvailable -= OnSnapshot;
        _notifyIcon.MouseClick -= NotifyIconOnMouseClick;
        _notifyIcon.Dispose();
        _menu.Dispose();
        _renderer.Dispose();
    }
}
