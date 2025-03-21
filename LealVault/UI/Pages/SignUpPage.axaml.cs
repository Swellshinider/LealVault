
using LealVault.Common;
using LealVault.Common.Database.Models;
using LealVault.Common.Database.Repository;
using MsBox.Avalonia.Enums;

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
            _ = await _window.DisplayMessageBox("Invalid Username", "Username must have at least 3 characters", Icon.Warning, [new() { Name = "Ok" }]);
        else if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password) || password.Length < 6)
            _ = await _window.DisplayMessageBox("Invalid Password", "Password must have at least 6 characters", Icon.Warning, [new() { Name = "Ok" }]);
        else if (password != passwordConfirm)
            _ = await _window.DisplayMessageBox("Invalid Password", "Passwords did not match, please confirm your password", Icon.Warning, [new() { Name = "Ok" }]);
        else
        {
            using var repo = new UserRepository();
            var hashedPassword = password.HashPassword(out var salt);

            if (repo.GetByName(username) != null)
            {
                _ = await _window.DisplayMessageBox("Invalid Username", "Username already exists!", Icon.Warning, [new() { Name = "Ok" }]);
                return;
            }

            var entity = new User()
            {
                Id = Common.Util.GenerateGuid(),
                Username = username,
                Password = hashedPassword,
                Salt = salt
            };
            await repo.InsertAsync(entity);
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