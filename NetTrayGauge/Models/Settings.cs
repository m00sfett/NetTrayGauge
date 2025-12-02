using System;
using System.Text.Json.Serialization;

namespace NetTrayGauge.Models;

/// <summary>
/// Represents persisted application settings.
/// </summary>
public class Settings
{
    public bool StartWithWindows { get; set; } = true;
    public int UpdateIntervalMs { get; set; } = 1000;
    public int SmoothingSamples { get; set; } = 3;
    public Theme Theme { get; set; } = Theme.Dark;
    public bool ShowDigitalOverlay { get; set; } = true;
    public string FontFamily { get; set; } = "Segoe UI";
    public TrayIconSize TrayIconSize { get; set; } = TrayIconSize.Large;
    public UnitMode UnitMode { get; set; } = UnitMode.Auto;
    public double? MaxDownloadScale { get; set; };
    public double? MaxUploadScale { get; set; };
    public string? PreferredInterfaceId { get; set; };
    public double TickDensity { get; set; } = 0.35;
    public double NeedleThickness { get; set; } = 1.8;
    public double ArcThickness { get; set; } = 2.4;
    public double WindowLeft { get; set; } = double.NaN;
    public double WindowTop { get; set; } = double.NaN;
    public double WindowWidth { get; set; } = 320;
    public double WindowHeight { get; set; } = 240;

    [JsonIgnore]
    public double MaxScaleDecay { get; set; } = 0.92;
}

/// <summary>
/// Unit display mode.
/// </summary>
public enum UnitMode
{
    Auto,
    Bps,
    KiBps,
    MiBps,
    bps,
    Kbps,
    Mbps
}

/// <summary>
/// Theme selector.
/// </summary>
public enum Theme
{
    Light,
    Dark,
    Neon
}

/// <summary>
/// Tray icon base size.
/// </summary>
public enum TrayIconSize
{
    Small = 16,
    Medium = 24,
    Large = 32
}
