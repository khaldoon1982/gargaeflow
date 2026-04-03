using System.Windows;
using GarageFlow.Wpf.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace GarageFlow.Wpf.Views;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        _viewModel.LoginSucceeded += user =>
        {
            var mainWindow = App.GetHost().Services.GetRequiredService<MainWindow>();
            var mainVm = App.GetHost().Services.GetRequiredService<MainViewModel>();
            mainVm.CurrentUser = user;
            mainWindow.DataContext = mainVm;
            System.Windows.Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
        };
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.Password = PasswordBox.Password;
    }
}
