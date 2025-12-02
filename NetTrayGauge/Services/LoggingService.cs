using System;
using System.IO;
using System.Text;

namespace NetTrayGauge.Services;

/// <summary>
/// Simple rolling file logger.
/// </summary>
public class LoggingService
{
    private readonly string _logDirectory;
    private readonly object _sync = new();
    private readonly int _maxFiles = 5;
    private readonly long _maxFileSize = 512 * 1024;

    public LoggingService(string appFolder)
    {
        _logDirectory = Path.Combine(appFolder, "logs");
        Directory.CreateDirectory(_logDirectory);
    }

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message, Exception? ex = null) => Write("ERR", message + (ex != null ? $" :: {ex}" : string.Empty));

    private void Write(string level, string message)
    {
        lock (_sync)
        {
            var file = Path.Combine(_logDirectory, "nettraygauge.log");
            RotateIfNeeded(file);
            var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";
            File.AppendAllText(file, line, Encoding.UTF8);
        }
    }

    private void RotateIfNeeded(string file)
    {
        try
        {
            if (File.Exists(file) && new FileInfo(file).Length > _maxFileSize)
            {
                for (int i = _maxFiles - 1; i >= 1; i--)
                {
                    var src = Path.Combine(_logDirectory, $"nettraygauge.log.{i}");
                    var dst = Path.Combine(_logDirectory, $"nettraygauge.log.{i + 1}");
                    if (File.Exists(dst))
                    {
                        File.Delete(dst);
                    }
                    if (File.Exists(src))
                    {
                        File.Move(src, dst);
                    }
                }

                File.Move(file, Path.Combine(_logDirectory, "nettraygauge.log.1"), true);
            }
        }
        catch
        {
            // Swallow logging failures.
        }
    }
}
