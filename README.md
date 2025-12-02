# NetTrayGauge

Tray Network Monitor prototype targeting a .NET 8 console executable.

## Repository layout

```
NetTrayGauge.sln           Solution entry point
NetTrayGauge/              Console application project
	App/TrayMonitorApp.cs    High-level bootstrapper
	Services/NetworkMonitor.cs  Placeholder service for future logic
```

## Getting started

### Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) or newer

### Build

```bash
dotnet build NetTrayGauge.sln
```

### Run

```bash
dotnet run --project NetTrayGauge/NetTrayGauge.csproj -- [optional-args]
```

The executable currently prints stubbed network monitor output, providing a starting point for future tray integration work.
