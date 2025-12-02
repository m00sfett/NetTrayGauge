using System.Windows;
using System.Windows.Media;

namespace NetTrayGauge.Utilities;

/// <summary>
/// Provides simple helpers for DPI scaling.
/// </summary>
public static class DpiHelper
{
    public static double GetScale()
    {
        var source = PresentationSource.FromVisual(Application.Current.MainWindow ?? (System.Windows.Media.Visual?)null);
        if (source?.CompositionTarget != null)
        {
            return source.CompositionTarget.TransformToDevice.M11;
        }

        return 1.0;
    }
}
