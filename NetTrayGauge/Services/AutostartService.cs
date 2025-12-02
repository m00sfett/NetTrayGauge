using System;
using Microsoft.Win32;

namespace NetTrayGauge.Services;

/// <summary>
/// Manages HKCU Run autostart registration.
/// </summary>
public class AutostartService
{
    private const string RunKeyPath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
    private readonly string _appName;
    private readonly string _executablePath;

    public AutostartService(string appName, string executablePath)
    {
        _appName = appName;
        _executablePath = executablePath;
    }

    public void Enable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true) ?? Registry.CurrentUser.CreateSubKey(RunKeyPath);
        key?.SetValue(_appName, $"\"{_executablePath}\"");
    }

    public void Disable()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
        key?.DeleteValue(_appName, false);
    }

    public bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
        var value = key?.GetValue(_appName) as string;
        return !string.IsNullOrWhiteSpace(value);
    }
}
