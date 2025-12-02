using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NetTrayGauge.Views.Controls;

/// <summary>
/// Analog gauge control for popup view.
/// </summary>
public partial class GaugeControl : UserControl
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(double), typeof(GaugeControl), new PropertyMetadata(0d, OnVisualChanged));

    public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
        nameof(MaxValue), typeof(double), typeof(GaugeControl), new PropertyMetadata(1d, OnVisualChanged));

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(GaugeControl), new PropertyMetadata(string.Empty, OnVisualChanged));

    public GaugeControl()
    {
        InitializeComponent();
        Loaded += (_, _) => Redraw();
        SizeChanged += (_, _) => Redraw();
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double MaxValue
    {
        get => (double)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    private static void OnVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GaugeControl control)
        {
            control.Redraw();
        }
    }

    private void Redraw()
    {
        if (!IsLoaded)
        {
            return;
        }

        var canvas = PART_Canvas;
        canvas.Children.Clear();
        var size = Math.Min(ActualWidth, ActualHeight);
        var center = new Point(size / 2, size / 2);
        var radius = size / 2 - 8;

        var background = new System.Windows.Shapes.Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Stroke = new SolidColorBrush(Color.FromArgb(90, 255, 255, 255)),
            StrokeThickness = 2
        };
        Canvas.SetLeft(background, center.X - radius);
        Canvas.SetTop(background, center.Y - radius);
        canvas.Children.Add(background);

        double normalized = MaxValue <= 0 ? 0 : Math.Min(1.0, Value / MaxValue);
        var angle = 180 * normalized;
        var rad = (Math.PI * (180 - angle)) / 180;
        var needle = new System.Windows.Shapes.Line
        {
            X1 = center.X,
            Y1 = center.Y,
            X2 = center.X + Math.Cos(rad) * (radius - 6),
            Y2 = center.Y - Math.Sin(rad) * (radius - 6),
            Stroke = Brushes.DeepSkyBlue,
            StrokeThickness = 3,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
        canvas.Children.Add(needle);

        PART_Label.Text = Title;
    }
}
