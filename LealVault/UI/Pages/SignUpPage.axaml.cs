namespace LealVault;

public partial class SignUpPage : UserControl
{
    public delegate void ButtonEnterAccount();
    public event ButtonEnterAccount? EnterAccountPressed;

    public SignUpPage()
    {
        InitializeComponent();
    }

    private void TextBlock_PointerPressed(object? sender, PointerPressedEventArgs e)
        => EnterAccountPressed?.Invoke();
}