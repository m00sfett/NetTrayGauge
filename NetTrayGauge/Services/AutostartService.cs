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
        var normalizedPath = NormalizePath(_executablePath);
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true) ?? Registry.CurrentUser.CreateSubKey(RunKeyPath);
        key?.SetValue(_appName, $"\"{normalizedPath}\"");
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
        var storedPath = NormalizePath(value);
        var currentPath = NormalizePath(_executablePath);

        if (string.IsNullOrWhiteSpace(storedPath))
        {
            return false;
        }

        return string.Equals(storedPath, currentPath, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string? path) => (path ?? string.Empty).Trim().Trim('"');
}
