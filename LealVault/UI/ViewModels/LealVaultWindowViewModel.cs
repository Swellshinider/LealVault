using Avalonia.Controls;
using System.ComponentModel;

namespace LealVault.UI.ViewModels;

public class LealVaultWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private Control _currentPage = new Login();

    public Control CurrentPage
    {
        get => _currentPage;
        set
        {
            _currentPage = value;
            OnPropertyChanged(nameof(PageType));
        }
    }

    private void OnPropertyChanged(string propertyName) 
        => PropertyChanged?.Invoke(this, new(propertyName));
}