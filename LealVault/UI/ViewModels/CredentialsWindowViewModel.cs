using Avalonia.Controls;
using System.ComponentModel;

namespace LealVault.UI.ViewModels;

public class CredentialsWindowViewModel : INotifyPropertyChanged
{
    private bool _isLogin = true;
    private string _username = "";
    private string _password = "";

    public bool IsLogin
    {
        get => _isLogin;
        set
        {
            _isLogin = value;
            OnPropertyChanged(nameof(IsLogin));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) 
        => PropertyChanged?.Invoke(this, new(propertyName));
}