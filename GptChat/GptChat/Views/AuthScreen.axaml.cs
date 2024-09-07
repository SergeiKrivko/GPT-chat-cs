using System;
using Auth;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace GptChat.Views;

public partial class AuthScreen : UserControl
{
    public AuthScreen()
    {
        InitializeComponent();
    }

    private async void SignIn()
    {
        await AuthService.Instance.SignIn(new SignInRequestBody()
        {
            email = SignIiEmailBox.Text ?? "",
            password = SignIiPasswordBox.Text ?? "",
        });
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        SignIn();
    }
}