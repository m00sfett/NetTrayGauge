# NetTrayGauge

NetTrayGauge ist eine leichtgewichtige Windows-11-Tray-App, die Upload- und Download-Geschwindigkeiten als analoge Halbkreis-Instrumente direkt im Systemtray anzeigt. Die App ist WPF-basiert, DPI-bewusst (PerMonitorV2) und bietet ein Mini-Dashboard sowie ein umfangreiches Einstellungsmenü in deutscher Sprache.

## Features
- Zwei analoge Halbkreis-Gauges (Download oben, Upload unten) als dynamisch gerenderte Tray-Grafik mit optionaler digitaler Overlay-Anzeige.
- Mini-Dashboard per Linksklick mit größeren Gauges und aktuellen Zahlenwerten.
- Rechtsklick-Kontextmenü mit Interface-Auswahl, Einheiten, Design, Glättung, Autostart, Protokollzugriff u.v.m.
- Netzwerkmessung über `System.Net.NetworkInformation.NetworkInterface` ohne Treiber, inkl. Glättung über gleitende Mittelwerte.
- Auto- oder feste Skalierung mit Peak-Decay, Farbverlauf grün→gelb→orange→rot abhängig von der Geschwindigkeit.
- Einstellungen als JSON unter `%APPDATA%\NetTrayGauge\settings.json`, Export/Import aus der Einstellungen-UI.
- Autostart ohne Admin via `HKCU\...\Run`.
- Logging in `%APPDATA%\NetTrayGauge\logs`.

## Projektstruktur
- `App.xaml` / `App.xaml.cs`: Einstiegspunkt, verdrahtet Services.
- `Services/`: Netzwerkmonitor, Tray-/NotifyIcon-Verhalten, Einstellungen, Logging, Autostart.
- `Rendering/TrayRenderer.cs`: Zeichnet die beiden Halbkreise, Nadeln und digitale Overlay-Anzeige ins Tray-Icon.
- `Views/Windows/PopupWindow.xaml`: Mini-Dashboard.
- `Views/Windows/SettingsWindow.xaml`: Einstellungen (Tabs Allgemein, Netzwerk, Design).
- `Views/Controls/GaugeControl`: Wiederverwendbarer Gauge für das Dashboard.
- `Models/`: Settings, Snapshot-Daten, Skalierungsmodelle.

## Build
Voraussetzung: .NET 8 SDK unter Windows.

```bash
dotnet build NetTrayGauge.sln
```

### Publish als Single File (self-contained)
Erzeugt ein einzelnes Exe-Deployment ohne zusätzliche Abhängigkeiten:

```bash
dotnet publish NetTrayGauge/NetTrayGauge.csproj -c Release -r win-x64 -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Das veröffentlichte Artefakt liegt anschließend unter `NetTrayGauge/bin/Release/net8.0-windows/win-x64/publish/NetTrayGauge.exe`.

## Nutzung
- Beim Start erscheint nur das Tray-Icon. Linksklick öffnet/schließt das Mini-Dashboard, Rechtsklick öffnet das Kontextmenü.
- Interface-Auswahl: In den Einstellungen (Tab "Netzwerk") gewünschtes Interface wählen. Ohne Auswahl wird automatisch das schnellste aktive Interface genommen.
- Einheiten: Über Kontextmenü "Einheiten…" oder im Settings-JSON (`unitMode`).
- Digitalanzeige: Kontextmenü "Digitalanzeige an/aus" oder `showDigitalOverlay` im JSON.
- Autostart: Kontextmenü "Autostart an/aus" oder `startWithWindows` im JSON. Hinterlegt den Pfad in `HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run`.

## DPI & Rendering
- Manifest aktiviert PerMonitorV2. Tray-Renderer berücksichtigt den aktuellen DPI-Scale und erstellt je Tick ein Bitmap in entsprechender Größe (16/24/32 px Basis plus DPI-Faktor).
- Farbthemen: Light, Dark, Neon. Nadel/Skala-Farbverlauf richtet sich nach der aktuellen Geschwindigkeit und der gewählten Max-Skala.

## Troubleshooting
- **VPN / mehrere NICs:** Wähle in den Einstellungen explizit das Interface, das überwacht werden soll. Bei Wechseln übernimmt die App automatisch das aktive Interface, falls kein Präferenzwert gesetzt ist.
- **Keine Daten nach Standby:** Die Messung setzt sich nach Sleep/Resume automatisch zurück; bei Bedarf Kontextmenü öffnen/schließen, damit die Nadeln neu anlaufen.
- **DPI unscharf:** Stelle sicher, dass Windows-Skalierung korrekt erkannt wird. Das Manifest erzwingt PerMonitorV2; nach Monitorkacheln ggf. Tray-Icon neu initialisieren (Kontextmenü kurz öffnen).
- **Rechte:** Es sind keine Adminrechte nötig. Autostart erfolgt ausschließlich unter HKCU.

## Lizenz
[MIT](LICENSE)
