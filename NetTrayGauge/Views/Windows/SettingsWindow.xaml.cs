using System;
using System.Diagnostics;
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
        try
        {
            _viewModel.Save();
        }
        catch (Exception ex)
        {
            ShowError("Could not save settings.", ex);
            return;
        }

        try
        {
            if (_viewModel.Settings.StartWithWindows)
            {
                _autostartService.Enable();
            }
            else
            {
                _autostartService.Disable();
            }
        }
        catch (Exception ex)
        {
            ShowError("Settings saved, but failed to update Windows startup option.", ex);
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
            try
            {
                _viewModel.SettingsService.Export(dialog.FileName);
            }
            catch (Exception ex)
            {
                ShowError("Could not export settings.", ex);
            }
        }
    }

    private void OnImport(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "JSON|*.json" };
        if (dialog.ShowDialog() == true)
        {
            try
            {
                _viewModel.SettingsService.Import(dialog.FileName);
                DataContext = null;
                DataContext = _viewModel;
            }
            catch (Exception ex)
            {
                ShowError("Could not import settings.", ex);
            }
        }
    }

    private void ShowError(string message, Exception exception)
    {
        Debug.WriteLine($"{message}: {exception}");
        MessageBox.Show(this, message, "NetTrayGauge", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
