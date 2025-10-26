using System.Windows;
using System.Windows.Controls;

namespace Files_Transfer.Pages;

public partial class NetworkPage : Page
{
    public readonly ServerPage ServerPage;
    public readonly ClientPage ClientPage;
    public NetworkPage()
    {
        InitializeComponent();
        var networkDefaultPage = new NetworkDefaultPage();
        ServerPage = new ServerPage();
        ClientPage = new ClientPage();
        Frame.Navigate(networkDefaultPage);
    }

    private void ServerPageRadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(ServerPage);
    }

    private void ClientPageRadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(ClientPage);
    }
}