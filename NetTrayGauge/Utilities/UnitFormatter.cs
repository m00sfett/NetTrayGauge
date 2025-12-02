namespace NetTrayGauge.Utilities;

using System.Globalization;
using NetTrayGauge.Models;

public static class UnitFormatter
{
    /// <summary>
    /// Formats a throughput given as BYTES PER SECOND according to the configured UnitMode.
    /// Returns a compact string like "12.3 Mbit/s" or "1.1 MiB/s".
    /// </summary>
    public static string Format(double bytesPerSecond, UnitMode unitMode, int decimals = 1)
    {
        var (val, unit) = ToValueAndUnit(bytesPerSecond, unitMode);
        if (double.IsNaN(val) || double.IsInfinity(val)) val = 0;

        // Build numeric format like "0.#" or "0.##"
        var fmt = decimals <= 0 ? "0" : "0." + new string('#', decimals);
        var num = val.ToString(fmt, CultureInfo.InvariantCulture);
        return $"{num} {unit}";
    }

    /// <summary>
    /// Converts BYTES PER SECOND to a numeric value and its unit label,
    /// depending on UnitMode. Auto prefers bits for network speeds.
    /// </summary>
    public static (double value, string unit) ToValueAndUnit(double bytesPerSecond, UnitMode unitMode)
    {
        if (bytesPerSecond < 0 || double.IsNaN(bytesPerSecond) || double.IsInfinity(bytesPerSecond))
            bytesPerSecond = 0;

        double bps = bytesPerSecond; // bytes per second

        switch (unitMode)
        {
            case UnitMode.Bps:   return (bps, "B/s");
            case UnitMode.KiBps: return (bps / 1024d, "KiB/s");
            case UnitMode.MiBps: return (bps / (1024d * 1024d), "MiB/s");

            case UnitMode.bps:   return (bps * 8d, "bit/s");
            case UnitMode.Kbps:  return (bps * 8d / 1_000d, "Kbit/s");
            case UnitMode.Mbps:  return (bps * 8d / 1_000_000d, "Mbit/s");

            case UnitMode.Auto:
            default:
                // Auto: pick a readable *bit/s* unit
                double bits = bps * 8d;
                if (bits >= 1_000_000d) return (bits / 1_000_000d, "Mbit/s");
                if (bits >= 1_000d)     return (bits / 1_000d,     "Kbit/s");
                return (bits, "bit/s");
        }
    }
}
