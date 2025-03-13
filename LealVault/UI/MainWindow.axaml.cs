using Avalonia.Controls;
using LealVault.UI.ViewModels;

namespace LealVault.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    public MainWindowViewModel ViewModel { get; } = new();
}