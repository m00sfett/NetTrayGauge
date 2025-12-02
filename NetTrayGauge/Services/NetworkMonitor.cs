using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using NetTrayGauge.Models;

namespace NetTrayGauge.Services;

/// <summary>
/// Samples network interfaces and raises speed updates.
/// </summary>
public class NetworkMonitor : IDisposable
{
    private readonly LoggingService _logger;
    private readonly Func<Settings> _settingsAccessor;
    private readonly ConcurrentQueue<(double rx, double tx)> _history = new();
    private CancellationTokenSource? _cts;
    private Task? _samplingTask;
    private NetworkInterface? _currentInterface;
    private long _lastRx;
    private long _lastTx;
    private DateTime _lastTimestamp;

    public event EventHandler<NetworkSnapshot>? SnapshotAvailable;

    public NetworkMonitor(Func<Settings> settingsAccessor, LoggingService logger)
    {
        _settingsAccessor = settingsAccessor;
        _logger = logger;
    }

    public void Start()
    {
        Stop();
        _cts = new CancellationTokenSource();
        _samplingTask = Task.Run(() => SampleLoopAsync(_cts.Token));
    }

    public void Stop()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    public IEnumerable<NetworkInterface> GetInterfaces()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        n.NetworkInterfaceType != NetworkInterfaceType.Tunnel &&
                        n.OperationalStatus == OperationalStatus.Up);
    }

    private async Task SampleLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var settings = _settingsAccessor();
                var interval = Math.Max(250, settings.UpdateIntervalMs);
                var snapshot = Sample(settings);
                SnapshotAvailable?.Invoke(this, snapshot);
                await Task.Delay(interval, token);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.Error("Network sampling failed", ex);
                await Task.Delay(1000, token);
            }
        }
    }

    private NetworkSnapshot Sample(Settings settings)
    {
        var nic = ResolveInterface(settings);
        if (nic == null)
        {
            Reset();
            return NetworkSnapshot.Empty("Keine Verbindung");
        }

        var stats = nic.GetIPStatistics();
        var now = DateTime.UtcNow;

        if (_lastRx == 0 && _lastTx == 0)
        {
            _lastRx = stats.BytesReceived;
            _lastTx = stats.BytesSent;
            _lastTimestamp = now;
            return NetworkSnapshot.Empty("Warte auf Daten...");
        }

        var elapsed = (now - _lastTimestamp).TotalSeconds;
        _lastTimestamp = now;

        if (elapsed <= 0)
        {
            return NetworkSnapshot.Empty("Warte auf Daten...");
        }

        var rxDelta = stats.BytesReceived - _lastRx;
        var txDelta = stats.BytesSent - _lastTx;
        _lastRx = stats.BytesReceived;
        _lastTx = stats.BytesSent;

        var rxPerSecond = rxDelta / elapsed;
        var txPerSecond = txDelta / elapsed;

        Enqueue(rxPerSecond, txPerSecond, settings.SmoothingSamples);
        var smoothed = GetSmoothed();

        return new NetworkSnapshot
        {
            InterfaceId = nic.Id,
            InterfaceName = nic.Name,
            DownloadBytesPerSecond = smoothed.rx,
            UploadBytesPerSecond = smoothed.tx,
            Timestamp = now,
            IsValid = true
        };
    }

    private NetworkInterface? ResolveInterface(Settings settings)
    {
        if (_currentInterface != null && _currentInterface.OperationalStatus == OperationalStatus.Up)
        {
            return _currentInterface;
        }

        _currentInterface = null;

        var interfaces = GetInterfaces().ToList();
        if (!interfaces.Any())
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(settings.PreferredInterfaceId))
        {
            _currentInterface = interfaces.FirstOrDefault(i => i.Id == settings.PreferredInterfaceId);
            if (_currentInterface != null)
            {
                _logger.Info($"Using preferred interface {_currentInterface.Name}");
                return _currentInterface;
            }
        }

        _currentInterface = interfaces
            .OrderByDescending(i => i.Speed)
            .FirstOrDefault();

        if (_currentInterface != null)
        {
            _logger.Info($"Auto-selected interface {_currentInterface.Name}");
        }

        return _currentInterface;
    }

    private void Enqueue(double rx, double tx, int smoothing)
    {
        _history.Enqueue((rx, tx));
        while (_history.Count > Math.Max(1, smoothing))
        {
            _history.TryDequeue(out _);
        }
    }

    private (double rx, double tx) GetSmoothed()
    {
        if (_history.IsEmpty)
        {
            return (0, 0);
        }

        var rx = _history.Average(h => h.rx);
        var tx = _history.Average(h => h.tx);
        return (rx, tx);
    }

    public void Reset()
    {
        _history.Clear();
        _lastRx = 0;
        _lastTx = 0;
    }

    public void Dispose()
    {
        Stop();
    }
}
