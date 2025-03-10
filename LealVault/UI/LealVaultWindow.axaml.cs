using Avalonia.Controls;
using LealVault.UI.ViewModels;

namespace LealVault.UI;

public partial class LealVaultWindow : Window
{
    public LealVaultWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public LealVaultWindowViewModel ViewModel { get; } = new();
}