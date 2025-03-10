using System.ComponentModel;

namespace LealVault.UI.ViewModels;

public class LealVaultWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private PageType _pageType = PageType.LogIn;

    public PageType PageType
    {
        get => _pageType;
        set
        {
            _pageType = value;
            OnPropertyChanged(nameof(PageType));
        }
    }

    private void OnPropertyChanged(string propertyName) 
        => PropertyChanged?.Invoke(this, new(propertyName));
}