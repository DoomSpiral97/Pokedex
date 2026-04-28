using Pokedex.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Pokedex;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
    }

    // Ein Handler für alle Slot Buttons 
    private async void Slot_Click(object sender, RoutedEventArgs e)
    {
        int index = int.Parse(((Button)sender).Tag.ToString()); //ButtonTag zu String -> zu int
        await _viewModel.SlotClick(index); // SlotClick Methode im VM
    }
}