using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using NetTrayGauge.Models;
using NetTrayGauge.Utilities;

namespace NetTrayGauge.Rendering;

/// <summary>
/// Renders the live tray icon gauges.
/// </summary>
public class TrayRenderer : IDisposable
{
    private readonly Font _overlayFont;
    private Icon? _lastIcon;

    public TrayRenderer(string fontFamily)
    {
        _overlayFont = new Font(string.IsNullOrWhiteSpace(fontFamily) ? "Segoe UI" : fontFamily, 8, FontStyle.Bold, GraphicsUnit.Pixel);
    }

    public Icon Render(NetworkSnapshot snapshot, Settings settings, GaugeScale scales, double dpiScale)
    {
        var size = (int)((int)settings.TrayIconSize * dpiScale);
        size = Math.Max(size, 16);
        var bmp = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(GetBackground(settings.Theme));

        var center = new PointF(size / 2f, size / 2f);
        var radius = size / 2f - 2;

        DrawGauge(g, center, radius, snapshot.DownloadBytesPerSecond, scales.DownloadMax, true, settings);
        DrawGauge(g, center, radius, snapshot.UploadBytesPerSecond, scales.UploadMax, false, settings);

        if (settings.ShowDigitalOverlay && snapshot.IsValid)
        {
            DrawOverlayText(g, size, snapshot, settings);
        }

        _lastIcon?.Dispose();
        _lastIcon = Icon.FromHandle(bmp.GetHicon());
        return _lastIcon;
    }

    private void DrawGauge(Graphics g, PointF center, float radius, double value, double maxValue, bool upper, Settings settings)
    {
        var startAngle = upper ? 180f : 0f;
        var sweep = 180f;
        var rect = new RectangleF(center.X - radius, center.Y - radius, radius * 2, radius * 2);
        var arcThickness = (float)Math.Max(1, settings.ArcThickness * (radius / 16f));

        using var arcPen = new Pen(Color.FromArgb(80, Color.Gray), arcThickness);
        g.DrawArc(arcPen, rect, startAngle, sweep);

        var norm = maxValue <= 0 ? 0 : Math.Min(1.0, Math.Abs(value) / maxValue);
        var needleColor = GetGradientColor(norm);

        using var needlePen = new Pen(needleColor, (float)Math.Max(1, settings.NeedleThickness * (radius / 16f)));
        var angle = startAngle + sweep * Math.Min(1.0, Math.Abs(value) / Math.Max(1, maxValue));
        if (!upper)
        {
            angle = startAngle + sweep - sweep * Math.Min(1.0, Math.Abs(value) / Math.Max(1, maxValue));
        }
        var radians = angle * Math.PI / 180.0;
        var needleLength = radius - 4;
        var end = new PointF(
            center.X + (float)(Math.Cos(radians) * needleLength),
            center.Y + (float)(Math.Sin(radians) * needleLength));
        g.DrawLine(needlePen, center, end);

        // tick marks
        var ticks = Math.Max(3, (int)(settings.TickDensity * radius));
        using var tickPen = new Pen(Color.FromArgb(120, Color.LightGray), 1);
        for (int i = 0; i <= ticks; i++)
        {
            float frac = i / (float)ticks;
            var a = startAngle + sweep * frac;
            var rads = a * Math.PI / 180.0;
            var inner = new PointF(center.X + (float)Math.Cos(rads) * (radius - 4), center.Y + (float)Math.Sin(rads) * (radius - 4));
            var outer = new PointF(center.X + (float)Math.Cos(rads) * (radius - 1), center.Y + (float)Math.Sin(rads) * (radius - 1));
            g.DrawLine(tickPen, inner, outer);
        }
    }

    private void DrawOverlayText(Graphics g, int size, NetworkSnapshot snapshot, Settings settings)
    {
        var dl = UnitFormatter.Format(snapshot.DownloadBytesPerSecond, settings.UnitMode);
        var ul = UnitFormatter.Format(snapshot.UploadBytesPerSecond, settings.UnitMode);
        var text = $"{dl.value:F1} {dl.unit}\n{ul.value:F1} {ul.unit}";
        var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var brush = settings.Theme == Theme.Light ? new SolidBrush(Color.Black) : new SolidBrush(Color.White);
        g.DrawString(text, _overlayFont, brush, new RectangleF(0, 0, size, size), format);
    }

    private Color GetBackground(Theme theme) => theme switch
    {
        Theme.Light => Color.FromArgb(245, 245, 245),
        Theme.Neon => Color.FromArgb(10, 12, 26),
        _ => Color.FromArgb(30, 30, 32)
    };

    private Color GetGradientColor(double normalized)
    {
        normalized = Math.Clamp(normalized, 0, 1);
        if (normalized < 0.33)
        {
            return Interpolate(Color.Lime, Color.Yellow, normalized / 0.33);
        }
        if (normalized < 0.66)
        {
            return Interpolate(Color.Yellow, Color.Orange, (normalized - 0.33) / 0.33);
        }
        return Interpolate(Color.OrangeRed, Color.Red, (normalized - 0.66) / 0.34);
    }

    private static Color Interpolate(Color a, Color b, double t)
    {
        int r = (int)(a.R + (b.R - a.R) * t);
        int g = (int)(a.G + (b.G - a.G) * t);
        int bl = (int)(a.B + (b.B - a.B) * t);
        return Color.FromArgb(r, g, bl);
    }

    public void Dispose()
    {
        _overlayFont.Dispose();
        _lastIcon?.Dispose();
    }
}
