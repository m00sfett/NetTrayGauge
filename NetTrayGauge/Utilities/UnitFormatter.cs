using System;
using NetTrayGauge.Models;

namespace NetTrayGauge.Utilities;

/// <summary>
/// Converts numeric speeds to human readable strings.
/// </summary>
public static class UnitFormatter
{
    public static (double value, string unit) Format(double bytesPerSecond, UnitMode mode)
    {
        return mode switch
        {
            UnitMode.Bps => (bytesPerSecond, "B/s"),
            UnitMode.KiBps => (bytesPerSecond / 1024d, "KiB/s"),
            UnitMode.MiBps => (bytesPerSecond / 1024d / 1024d, "MiB/s"),
            UnitMode.bps => (bytesPerSecond * 8d, "bit/s"),
            UnitMode.Kbps => (bytesPerSecond * 8d / 1000d, "Kbit/s"),
            UnitMode.Mbps => (bytesPerSecond * 8d / 1000d / 1000d, "Mbit/s"),
            _ => Auto(bytesPerSecond)
        };
    }

    private static (double value, string unit) Auto(double bytesPerSecond)
    {
        double abs = Math.Abs(bytesPerSecond);
        if (abs >= 1024d * 1024d)
        {
            return (bytesPerSecond / 1024d / 1024d, "MiB/s");
        }
        if (abs >= 1024d)
        {
            return (bytesPerSecond / 1024d, "KiB/s");
        }
        return (bytesPerSecond, "B/s");
    }

    public static string FormatText(double bytesPerSecond, UnitMode mode, int precision = 1)
    {
        var (value, unit) = Format(bytesPerSecond, mode);
        return $"{value:F{precision}} {unit}";
    }
}
