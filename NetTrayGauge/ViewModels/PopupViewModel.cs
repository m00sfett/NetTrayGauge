using NetTrayGauge.Models;
using NetTrayGauge.Utilities;

namespace NetTrayGauge.ViewModels;

/// <summary>
/// View model for the popup dashboard.
/// </summary>
public class PopupViewModel : ViewModelBase
{
    private double _download;
    private double _upload;
    private double _downloadScale = 1024 * 1024;
    private double _uploadScale = 1024 * 1024;
    private string _downloadText = "-";
    private string _uploadText = "-";
    private UnitMode _unitMode = UnitMode.Auto;

    public double Download
    {
        get => _download;
        private set { _download = value; Raise(); }
    }

    public double Upload
    {
        get => _upload;
        private set { _upload = value; Raise(); }
    }

    public double DownloadScale
    {
        get => _downloadScale;
        private set { _downloadScale = value; Raise(); }
    }

    public double UploadScale
    {
        get => _uploadScale;
        private set { _uploadScale = value; Raise(); }
    }

    public string DownloadText
    {
        get => _downloadText;
        private set { _downloadText = value; Raise(); }
    }

    public string UploadText
    {
        get => _uploadText;
        private set { _uploadText = value; Raise(); }
    }

    public UnitMode UnitMode
    {
        get => _unitMode;
        set { _unitMode = value; Raise(); }
    }

    public void Update(NetworkSnapshot snapshot, UnitMode unitMode)
    {
        UnitMode = unitMode;
        if (!snapshot.IsValid)
        {
            Download = 0;
            Upload = 0;
            DownloadText = snapshot.InterfaceName;
            UploadText = snapshot.InterfaceName;
            return;
        }

        Download = snapshot.DownloadBytesPerSecond;
        Upload = snapshot.UploadBytesPerSecond;
        DownloadText = UnitFormatter.FormatText(snapshot.DownloadBytesPerSecond, unitMode, 1);
        UploadText = UnitFormatter.FormatText(snapshot.UploadBytesPerSecond, unitMode, 1);
    }

    public void UpdateScales(double downloadMax, double uploadMax)
    {
        DownloadScale = downloadMax;
        UploadScale = uploadMax;
    }
}
