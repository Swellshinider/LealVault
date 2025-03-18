using System.ComponentModel;

namespace LealVault.UI.ViewModels;

public class AthenticationWindowViewModel : INotifyPropertyChanged
{
    private Control? _containerPage;

    public Control? ContainerPage
    {
        get => _containerPage;
        set
        {
            _containerPage = value;
            OnPropertyChanged(nameof(ContainerPage));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) 
        => PropertyChanged?.Invoke(this, new(propertyName));
}