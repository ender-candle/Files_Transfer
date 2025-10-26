using System.Windows;
using System.Windows.Input;

namespace Files_Transfer;

public partial class MessageWindow : Window
{
    public MessageWindow(string title, string message)
    {
        InitializeComponent();
        TitleLabel.Content = title;
        Message.Content = message;
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