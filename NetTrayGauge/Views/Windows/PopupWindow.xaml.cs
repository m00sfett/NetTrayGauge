using System;
using System.ComponentModel;
using System.Windows;
using NetTrayGauge.Services;
using NetTrayGauge.ViewModels;

namespace NetTrayGauge.Views.Windows;

/// <summary>
/// Lightweight popup dashboard window.
/// </summary>
public partial class PopupWindow : Window
{
    private readonly SettingsService _settingsService;

    public PopupWindow(PopupViewModel viewModel, SettingsService settingsService)
    {
        _settingsService = settingsService;
        DataContext = viewModel;
        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var settings = _settingsService.Current;
        if (!double.IsNaN(settings.WindowLeft))
        {
            Left = settings.WindowLeft;
        }
        if (!double.IsNaN(settings.WindowTop))
        {
            Top = settings.WindowTop;
        }
        Width = settings.WindowWidth;
        Height = settings.WindowHeight;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
        PersistBounds();
    }

    private void PersistBounds()
    {
        _settingsService.Update(s =>
        {
            s.WindowLeft = Left;
            s.WindowTop = Top;
            s.WindowWidth = Width;
            s.WindowHeight = Height;
        });
    }
}
