using System.Windows;
using GarageFlow.Wpf.ViewModels;

namespace GarageFlow.Wpf.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            await vm.LoadInitialDataCommand.ExecuteAsync(null);
    }
}
