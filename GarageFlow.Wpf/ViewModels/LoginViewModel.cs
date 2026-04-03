using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GarageFlow.Application.DTOs;
using GarageFlow.Application.Interfaces;

namespace GarageFlow.Wpf.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthenticationService _authService;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string? _errorMessage;
    [ObservableProperty] private bool _isLoggingIn;

    public event Action<UserDto>? LoginSucceeded;

    public LoginViewModel(IAuthenticationService authService) { _authService = authService; }

    [RelayCommand]
    private async Task Login()
    {
        ErrorMessage = null;
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Gebruikersnaam en wachtwoord zijn verplicht.";
            return;
        }
        IsLoggingIn = true;
        var user = await _authService.LoginAsync(Username, Password);
        IsLoggingIn = false;
        if (user is null)
        {
            ErrorMessage = "Ongeldige gebruikersnaam of wachtwoord.";
            return;
        }
        LoginSucceeded?.Invoke(user);
    }
}
