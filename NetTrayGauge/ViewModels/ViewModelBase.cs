using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetTrayGauge.ViewModels;

/// <summary>
/// Simple base class for INotifyPropertyChanged.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void Raise([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
