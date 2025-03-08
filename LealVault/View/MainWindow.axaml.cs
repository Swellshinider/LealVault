using Avalonia.Controls;

namespace LealVault.View;

public partial class MainWindow : Window
{
    private bool _leftPanelToggle = false;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void CollapseButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _leftPanelToggle = !_leftPanelToggle;

        if (_leftPanelToggle)
        {
            LeftPanel.Width = 40;
            CollapseButton.Content = ">";
        }
        else
        {
            LeftPanel.Width = double.NaN;
            CollapseButton.Content = "<";
        }
    }
}