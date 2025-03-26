using LealVault.Common;
using LealVault.Common.Database.Models;
using LealVault.Common.Database.Repository;
using LealVault.Utilities;
using MsBox.Avalonia.Enums;

namespace LealVault.UI.Pages;

public partial class LoginPage : UserControl
{
    public delegate void ButtonCreateAccount();
    public event ButtonCreateAccount? CreateAccountPressed;

    public delegate void Logged(User user);
    public event Logged? LoggedIn;

    private Window? _window;

    public LoginPage()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (VisualRoot is Window window)
            _window = window;
    }

    private async Task Login()
    {
        var username = TextBoxUsername.Text ?? "";
        var password = TextBoxPassword.Text ?? "";

        if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username) || username.Length < 3)
            _ = await _window!.DisplayMessageBox("Invalid Username", "Username must have at least 3 characters", Icon.Warning, [new() { Name = "Ok" }]);
        else if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password) || password.Length < 6)
            _ = await _window!.DisplayMessageBox("Invalid Password", "Password must have at least 6 characters", Icon.Warning, [new() { Name = "Ok" }]);
        else
        {
            using var repo = new UserRepository();
            var account = await repo.GetByName(username);

            if (account == null)
            {
                _ = await _window.DisplayMessageBox("Account Not Found",
                    "Your account could not be found!\nPlease, check the username and password and try again", Icon.Warning, [new() { Name = "Ok" }]);
                return;
            }

            if (!password.VerifyPassword(account.Salt, account.Password))
            {
                _ = await _window.DisplayMessageBox("Invalid Credentials",
                    "Invalid Credentials!\nPlease, check the username and password and try again", Icon.Warning, [new() { Name = "Ok" }]);
                return;
            }

            LoggedIn?.Invoke(account);
        }
    }

    private async void TextBox_Enter(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await Login();
    }

    private void ButtonToggleTapped(object? sender, TappedEventArgs e)
    {
        var isHidden = TextBoxPassword.PasswordChar == '*';
        TextBoxPassword.PasswordChar = isHidden ? '\0' : '*';
        ImageToggleVisibleConfirmPassword.Source = isHidden
            ? ImageKind.White_OpennedEye.GetImage()
            : ImageKind.White_HorizontalLine.GetImage();
    }

    private async void ButtonLoginTapped(object? sender, TappedEventArgs e)
        => await Login();

    private void TextBlockCreateAccountPointerPressed(object? sender, PointerPressedEventArgs e)
        => CreateAccountPressed?.Invoke();
}