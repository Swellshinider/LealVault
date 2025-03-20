
using LealVault.Common;

namespace LealVault;

public partial class SignUpPage : UserControl
{
    public delegate void ButtonEnterAccount();
    public event ButtonEnterAccount? EnterAccountPressed;

    public Window? _window;

    public SignUpPage()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (VisualRoot is Window window)
            _window = window;
    }

    private async Task CreateAccount()
    {
        var username = TextBoxUsername.Text ?? "";
        var password = TextBoxPassword.Text ?? "";
        var passwordConfirm = TextBoxPasswordConfirm.Text ?? "";

        if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username) || username.Length < 3)
        {
            _ = await _window.DisplayMessageBox("Invalid Username",
                    "Username must have at least 3 characters",
                    MsBox.Avalonia.Enums.Icon.Warning,
                    [new() { Name = "Ok" }]);
        }
        else if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            _ = await _window.DisplayMessageBox("Invalid Password",
                    "Password must have at least 6 characters",
                    MsBox.Avalonia.Enums.Icon.Warning,
                    [new() { Name = "Ok" }]);
        }
        else if (password != passwordConfirm)
        {
            _ = await _window.DisplayMessageBox("Invalid Password",
                    "Passwords did not match, please confirm your password",
                    MsBox.Avalonia.Enums.Icon.Warning, 
                    [new() { Name = "Ok" }]);
        }
        else
        {

        }
    }

    private async void TextBox_Enter(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await CreateAccount();
    }

    private void ButtonClick_GeneratePassword(object? sender, RoutedEventArgs e)
    {
        var password = Password.GenerateSecurePassword(12);
        TextBoxPassword.Text = password;
        TextBoxPasswordConfirm.Text = password;
    }

    private void TextBlock_PointerPressed(object? sender, PointerPressedEventArgs e)
        => EnterAccountPressed?.Invoke();
}