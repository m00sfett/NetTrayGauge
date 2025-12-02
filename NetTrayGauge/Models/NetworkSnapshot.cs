using System;
using System.Net.NetworkInformation;

namespace NetTrayGauge.Models;

/// <summary>
/// Represents a measured network speed snapshot.
/// </summary>
public class NetworkSnapshot
{
    public string InterfaceId { get; init; } = string.Empty;
    public string InterfaceName { get; init; } = string.Empty;
    public double DownloadBytesPerSecond { get; init; };
    public double UploadBytesPerSecond { get; init; };
    public DateTime Timestamp { get; init; };
    public bool IsValid { get; init; };

    public static NetworkSnapshot Empty(string message = "") => new()
    {
        InterfaceId = message,
        InterfaceName = message,
        Timestamp = DateTime.UtcNow,
        IsValid = false
    };
}

/// <summary>
/// Simple record for rendering decisions.
/// </summary>
public class GaugeScale
{
    public double DownloadMax { get; set; }
    public double UploadMax { get; set; }
}
