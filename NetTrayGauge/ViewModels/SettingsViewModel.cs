using System.Collections.ObjectModel;
using NetTrayGauge.Models;
using NetTrayGauge.Services;

namespace NetTrayGauge.ViewModels;

/// <summary>
/// View model backing the settings window.
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly NetworkMonitor _monitor;

    public ObservableCollection<NetworkInterfaceItem> Interfaces { get; } = new();

    public Settings Settings => _settingsService.Current;

    public SettingsService SettingsService => _settingsService;

    public SettingsViewModel(SettingsService settingsService, NetworkMonitor monitor)
    {
        _settingsService = settingsService;
        _monitor = monitor;
        RefreshInterfaces();
    }

    public void RefreshInterfaces()
    {
        Interfaces.Clear();
        foreach (var nic in _monitor.GetInterfaces())
        {
            Interfaces.Add(new NetworkInterfaceItem(nic.Id, nic.Name));
        }
    }

    public void Save() => _settingsService.Save();
}

/// <summary>
/// Lightweight network interface entry for data binding.
/// </summary>
public record NetworkInterfaceItem(string Id, string Name)
{
    public override string ToString() => Name;
}
