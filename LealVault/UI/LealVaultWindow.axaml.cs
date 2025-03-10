using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using LealVault.UI.ViewModels;

namespace LealVault;

public partial class LealVaultWindow : Window
{
    public LealVaultWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public LealVaultWindowViewModel ViewModel { get; } = new();
}