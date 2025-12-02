using System;
using System.Windows;
using Microsoft.Win32;
using NetTrayGauge.Models;
using NetTrayGauge.Services;
using NetTrayGauge.ViewModels;

namespace NetTrayGauge.Views.Windows;

/// <summary>
/// Settings window with basic configuration tabs.
/// </summary>
public partial class SettingsWindow : Window
{
    public static Array ThemeValues => Enum.GetValues(typeof(Theme));
    public static Array TrayIconSizeValues => Enum.GetValues(typeof(TrayIconSize));

    private readonly SettingsViewModel _viewModel;
    private readonly AutostartService _autostartService;

    public SettingsWindow(SettingsViewModel viewModel, AutostartService autostartService)
    {
        _viewModel = viewModel;
        _autostartService = autostartService;
        DataContext = viewModel;
        InitializeComponent();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        _viewModel.Save();
        if (_viewModel.Settings.StartWithWindows)
        {
            _autostartService.Enable();
        }
        else
        {
            _autostartService.Disable();
        }
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private void RefreshInterfaces_OnClick(object sender, RoutedEventArgs e)
    {
        _viewModel.RefreshInterfaces();
    }

    private void OnExport(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = "JSON|*.json" };
        if (dialog.ShowDialog() == true)
        {
            _viewModel.SettingsService.Export(dialog.FileName);
        }
    }

    private void OnImport(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "JSON|*.json" };
        if (dialog.ShowDialog() == true)
        {
            _viewModel.SettingsService.Import(dialog.FileName);
            DataContext = null;
            DataContext = _viewModel;
        }
    }
}
