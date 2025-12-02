using NetTrayGauge.Services;

namespace NetTrayGauge.App;

internal static class TrayMonitorApp
{
    public static Task RunAsync(string[] args)
    {
        var monitor = new NetworkMonitor();
        monitor.PrintStatus();

        if (args.Length > 0)
        {
            Console.WriteLine($"Arguments: {string.Join(' ', args)}");
        }

        return Task.CompletedTask;
    }
}
