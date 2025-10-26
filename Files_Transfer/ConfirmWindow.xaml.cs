using System.Windows;
using System.Windows.Input;

namespace Files_Transfer;

public partial class ConfirmWindow : Window
{
    public ConfirmWindow(string title, string message)
    {
        InitializeComponent();
        TitleLabel.Content = title;
        Message.Content = message;
    }

    private void Drag(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void Confirm(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void Cancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}