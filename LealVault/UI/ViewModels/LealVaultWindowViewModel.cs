using Avalonia.Controls;
using LealVault.UI.Pages;
using System.ComponentModel;

namespace LealVault.UI.ViewModels;

public class LealVaultWindowViewModel : INotifyPropertyChanged
{

    private Control _currentPage = new Login();

    public Control CurrentPage
    {
        get => _currentPage;
        set
        {
            _currentPage = value;
            OnPropertyChanged(nameof(CurrentPage));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) 
        => PropertyChanged?.Invoke(this, new(propertyName));
}