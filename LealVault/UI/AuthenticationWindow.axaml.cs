using Avalonia.Controls;
using LealVault.UI.ViewModels;

namespace LealVault.UI;

public partial class AuthenticationWindow : Window
{
    public AuthenticationWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public AthenticationWindowViewModel ViewModel { get; } = new();
}