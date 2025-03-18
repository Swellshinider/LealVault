namespace LealVault.UI.Pages;

public partial class LoginPage : UserControl
{
    public delegate void ButtonCreateAccount();
    public event ButtonCreateAccount? CreateAccountPressed;

    public LoginPage()
    {
        InitializeComponent();
    }

    private void TextBlock_PointerPressed(object? sender, PointerPressedEventArgs e)
        => CreateAccountPressed?.Invoke();
}