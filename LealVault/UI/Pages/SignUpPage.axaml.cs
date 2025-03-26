
using Avalonia.Controls;
using LealVault.Common;
using LealVault.Common.Database.Models;
using LealVault.Common.Database.Repository;
using LealVault.Utilities;
using MsBox.Avalonia.Enums;

namespace LealVault;

public partial class SignUpPage : UserControl
{
    public delegate void ButtonEnterAccount();
    public event ButtonEnterAccount? EnterAccountPressed;

    public delegate void Logged(User user);
    public event Logged? LoggedIn;

    private Window? _window;

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
            _ = await _window!.DisplayMessageBox("Invalid Username", "Username must have at least 3 characters", Icon.Warning, [new() { Name = "Ok" }]);
        else if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password) || password.Length < 6)
            _ = await _window!.DisplayMessageBox("Invalid Password", "Password must have at least 6 characters", Icon.Warning, [new() { Name = "Ok" }]);
        else if (password != passwordConfirm)
            _ = await _window!.DisplayMessageBox("Invalid Password", "Passwords did not match, please confirm your password", Icon.Warning, [new() { Name = "Ok" }]);
        else
        {
            using var repo = new UserRepository();
            var hashedPassword = password.HashPassword(out var salt);
            var checkAccount = await repo.GetByName(username);

            if (checkAccount != null)
            {
                _ = await _window.DisplayMessageBox("Invalid Username", "Username already exists!", Icon.Warning, [new() { Name = "Ok" }]);
                return;
            }
            else
            {
                var confirmMessage = $"Once an account is created you will never be able to modify your password.\n\nPlease, confirm your account:\nUsername: {username}\nPassword: {password}";
                var res = await _window.DisplayMessageBox("Confirm", confirmMessage, Icon.Warning,
                    [new() { Name = "Continue", IsDefault = true }, new() { Name = "Cancel", IsCancel = true }]);

                if (res != "Continue")
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

            LoggedIn?.Invoke(entity);
        }
    }

    private async void TextBox_Enter(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            await CreateAccount();
    }

    private void CheckBox_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not CheckBox cb)
            return;

        TextBoxPassword.PasswordChar = cb.IsChecked!.Value ? '\0' : '*';
        TextBoxPasswordConfirm.PasswordChar = cb.IsChecked!.Value ? '\0' : '*';
    }

    private void ButtonClick_GeneratePassword(object? sender, RoutedEventArgs e)
    {
        var password = Password.GenerateSecurePassword(12);
        TextBoxPassword.Text = password;
        TextBoxPasswordConfirm.Text = password;
    }

    private void TextBlock_PointerPressed(object? sender, PointerPressedEventArgs e)
        => EnterAccountPressed?.Invoke();

    private void ButtonToggleConfirmTapped(object? sender, TappedEventArgs e)
    {
        var isHidden = TextBoxPassword.PasswordChar == '*';
        TextBoxPassword.PasswordChar = isHidden ? '\0' : '*';
        TextBoxPasswordConfirm.PasswordChar = isHidden ? '\0' : '*';
        ImageToggleVisibleConfirmPassword.Source = isHidden
            ? ImageKind.White_OpennedEye.GetImage()
            : ImageKind.White_HorizontalLine.GetImage();
    }

    private async void Button_PointerPressed(object? sender, PointerPressedEventArgs e)
        => await CreateAccount();
}