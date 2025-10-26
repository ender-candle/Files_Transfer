using System.Windows;
using System.Windows.Input;

namespace Files_Transfer;

public partial class InfoWindow : Window
{
    public InfoWindow()
    {
        InitializeComponent();
    }

    private void Drag(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void CloseButton(object sender, RoutedEventArgs e)
    {
        Close();
    }
}