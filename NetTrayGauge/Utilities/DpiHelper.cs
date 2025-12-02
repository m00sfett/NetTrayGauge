using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NetTrayGauge.Utilities;

/// <summary>
/// Provides simple helpers for DPI scaling.
/// </summary>
public static class DpiHelper
{
    public static double GetScale()
    {
        var visual = GetAvailableVisual();
        if (visual != null)
        {
            return GetScaleFromVisual(visual);
        }

        using var temporarySource = CreateHiddenSource();
        if (temporarySource?.RootVisual is Visual temporaryRoot)
        {
            return GetScaleFromVisual(temporaryRoot);
        }

        return VisualTreeHelper.GetDpi(new DrawingVisual()).DpiScaleX;
    }

    private static Visual? GetAvailableVisual()
    {
        var application = Application.Current;
        if (application == null)
        {
            return null;
        }

        if (IsUsableVisual(application.MainWindow))
        {
            return application.MainWindow!;
        }

        return application.Windows
            .OfType<Window>()
            .FirstOrDefault(IsUsableVisual);
    }

    private static bool IsUsableVisual(Window? window) => window is { IsLoaded: true };

    private static double GetScaleFromVisual(Visual visual)
    {
        var source = PresentationSource.FromVisual(visual);
        if (source?.CompositionTarget != null)
        {
            return source.CompositionTarget.TransformToDevice.M11;
        }

        return VisualTreeHelper.GetDpi(visual).DpiScaleX;
    }

    private static HwndSource? CreateHiddenSource()
    {
        var parameters = new HwndSourceParameters("DpiHelperHiddenWindow")
        {
            Width = 0,
            Height = 0,
            PositionX = 0,
            PositionY = 0,
            WindowStyle = 0
        };

        var source = new HwndSource(parameters)
        {
            RootVisual = new DrawingVisual()
        };

        return source;
    }
}
