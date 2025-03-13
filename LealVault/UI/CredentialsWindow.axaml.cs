using Avalonia.Controls;
using LealVault.UI.ViewModels;

namespace LealVault.UI;

public partial class CredentialsWindow : Window
{
    public CredentialsWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public CredentialsWindowViewModel ViewModel { get; } = new();
}