using LealVault.Common.Database.Models;
using LealVault.UI.Pages;
using LealVault.UI.ViewModels;

namespace LealVault.UI;

public partial class AuthenticationWindow : Window
{
    private readonly LoginPage _loginPage = new();
    private readonly SignUpPage _signUpPage = new();

    public AuthenticationWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;

        _loginPage.CreateAccountPressed += LoginPage_CreateAccountPressed;
        _signUpPage.EnterAccountPressed += SignUpPage_EnterAccountPressed;
        _signUpPage.LoggedIn += SignUpPage_LoggedIn;

        ViewModel.ContainerPage = _loginPage;
    }

    public AthenticationWindowViewModel ViewModel { get; } = new();

    private void SignUpPage_EnterAccountPressed()
        => ViewModel.ContainerPage = _loginPage;

    private void LoginPage_CreateAccountPressed() 
        => ViewModel.ContainerPage = _signUpPage;

    private void SignUpPage_LoggedIn(User user)
    {

    }
}