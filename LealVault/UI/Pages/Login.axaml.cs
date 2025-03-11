using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LealVault.UI.ViewModels;

namespace LealVault.UI.Pages;

public partial class Login : UserControl
{
    public Login()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public LoginViewModel ViewModel { get; } = new();
}