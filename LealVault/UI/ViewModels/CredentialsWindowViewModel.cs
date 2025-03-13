using Avalonia.Controls;
using System.ComponentModel;

namespace LealVault.UI.ViewModels;

public class CredentialsWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) 
        => PropertyChanged?.Invoke(this, new(propertyName));
}