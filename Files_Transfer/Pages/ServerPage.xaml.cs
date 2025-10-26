using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Files_Transfer.Pages;

public partial class ServerPage : Page
{
    private Socket _listenSocket;
    public Socket _mainSocket;
    public ServerPage()
    {
        InitializeComponent();
        if (MainWindow._settingPage.Config.AppSettings.Settings["ServerIp"].Value != "")
        {
            IpTextBox.Foreground = (Brush)Application.Current.FindResource("SecondTextBrush");
            IpTextBox.Text = MainWindow._settingPage.Config.AppSettings.Settings["ServerIp"].Value;
        }

        if (MainWindow._settingPage.Config.AppSettings.Settings["ServerPort"].Value != "")
        {
            PortTextBox.Foreground = (Brush)Application.Current.FindResource("SecondTextBrush");
            PortTextBox.Text = MainWindow._settingPage.Config.AppSettings.Settings["ServerPort"].Value;
        }
    }

    // 输入框动画处理、提示文本处理
    private void Animation(Border border, bool visible)
    {
        var doubleAnimation = new DoubleAnimation
        {
            Duration = new Duration(TimeSpan.FromSeconds(0.3)),
            To = visible ? 1 : 0
        };
        var storyboard = new Storyboard();
        storyboard.Children.Add(doubleAnimation);
        Storyboard.SetTargetName(doubleAnimation, border.Name);
        Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(OpacityProperty));
        storyboard.Begin(this);
    }

    private void IpTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        Animation(IpBorder1, true);
        if (IpTextBox.Text != "IPv4/6") return;
        IpTextBox.Foreground = (Brush)Application.Current.FindResource("SecondTextBrush");
        IpTextBox.Text = "";
    }

    private void IpTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        Animation(IpBorder1, false);
        if (IpTextBox.Text != "") return;
        IpTextBox.Foreground = (Brush)Application.Current.FindResource("TextBoxHintTextBrush");
        IpTextBox.Text = "IPv4/6";
    }

    private void IpTextBox_OnMouseEnter(object sender, MouseEventArgs e)
    {
        Animation(IpBorder2, true);
    }

    private void IpTextBox_OnMouseLeave(object sender, MouseEventArgs e)
    {
        Animation(IpBorder2, false);
    }

    private void PortTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        Animation(PortBorder1, true);
        if (PortTextBox.Text != "1~65535") return;
        PortTextBox.Foreground = (Brush)Application.Current.FindResource("SecondTextBrush");
        PortTextBox.Text = "";
    }

    private void PortTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        Animation(PortBorder1, false);
        if (PortTextBox.Text != "") return;
        PortTextBox.Foreground = (Brush)Application.Current.FindResource("TextBoxHintTextBrush");
        PortTextBox.Text = "1~65535";
    }

    private void PortTextBox_OnMouseEnter(object sender, MouseEventArgs e)
    {
        Animation(PortBorder2, true);
    }

    private void PortTextBox_OnMouseLeave(object sender, MouseEventArgs e)
    {
        Animation(PortBorder2, false);
    }

    // 开始监听、关闭
    private void Start(object sender, RoutedEventArgs e)
    {
        StartButton.IsEnabled = false;
        MainWindow._networkPage.ClientPage.StartButton.IsEnabled = false;
        var regexV4 = new Regex("^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
        var regexV6 = new Regex("(([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4})|(([0-9a-fA-F]{1,4}:){6}:[0-9a-fA-F]{1,4})|(([0-9a-fA-F]{1,4}:){5}(:[0-9a-fA-F]{1,4}){1,2})|(([0-9a-fA-F]{1,4}:){4}(:[0-9a-fA-F]{1,4}){1,3})|(([0-9a-fA-F]{1,4}:){3}(:[0-9a-fA-F]{1,4}){1,4})|(([0-9a-fA-F]{1,4}:){2}(:[0-9a-fA-F]{1,4}){1,5})|([0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6}))|(:((:[0-9a-fA-F]{1,4}){1,7}))");
        if (regexV4.IsMatch(IpTextBox.Text) || regexV6.IsMatch(IpTextBox.Text) && !IpTextBox.Text.Contains('/'))
        {
            IpHintRightLabel.Visibility = Visibility.Visible;
            IpHintErrorLabel.Visibility = Visibility.Collapsed;
            IpHintErrorText.Visibility = Visibility.Hidden;
        }
        else
        {
            IpHintRightLabel.Visibility = Visibility.Collapsed;
            IpHintErrorLabel.Visibility = Visibility.Visible;
            IpHintErrorText.Visibility = Visibility.Visible;
            StartButton.IsEnabled = true;
            MainWindow._networkPage.ClientPage.StartButton.IsEnabled = true;
            return;
        }

        if (int.TryParse(PortTextBox.Text, out var port))
        {
            if (port is >= 1 and <= 65535)
            {
                PortHintRightLabel.Visibility = Visibility.Visible;
                PortHintErrorLabel.Visibility = Visibility.Collapsed;
                PortHintErrorText.Visibility = Visibility.Hidden;
                // 继续----------------------------------------
                ServerStart(port);
            }
            else
            {
                PortHintRightLabel.Visibility = Visibility.Collapsed;
                PortHintErrorLabel.Visibility = Visibility.Visible;
                PortHintErrorText.Visibility = Visibility.Visible;
                PortHintErrorText.Content = "请注意范围！";
                StartButton.IsEnabled = true;
                MainWindow._networkPage.ClientPage.StartButton.IsEnabled = true;
            }
        }
        else
        {
            PortHintRightLabel.Visibility = Visibility.Collapsed;
            PortHintErrorLabel.Visibility = Visibility.Visible;
            PortHintErrorText.Visibility = Visibility.Visible;
            PortHintErrorText.Content = "请输入整数！";
            StartButton.IsEnabled = true;
            MainWindow._networkPage.ClientPage.StartButton.IsEnabled = true;
        }
    }

    public void Over(object sender, RoutedEventArgs e)
    {
        if (_mainSocket != null) _mainSocket.Close();
        _listenSocket.Close();
        MainWindow._networkPage.ServerPageRadioButton.BorderThickness = new Thickness(0);
        ConnectStateLabel.Foreground = (Brush)Application.Current.FindResource("ConnectStateBrush2");
        ConnectStateLabel.Content = "已关闭";
        ConnectTargetLabel.Content = "";
        StartButton.IsEnabled = true;
        MainWindow._networkPage.ClientPage.StartButton.IsEnabled = true;
        OverButton.IsEnabled = false;
        MainWindow.IsServer = false;
        MainWindow.IsClient = false;
        IpTextBox.IsEnabled = true;
    }

    private void ServerStart(int port)
    {
        _listenSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        try
        {
            _listenSocket.Bind(new IPEndPoint(IPAddress.Parse(IpTextBox.Text), port));
        }
        catch (SocketException)
        {
            PortHintRightLabel.Visibility = Visibility.Collapsed;
            PortHintErrorLabel.Visibility = Visibility.Visible;
            PortHintErrorText.Visibility = Visibility.Visible;
            PortHintErrorText.Content = "端口已占用！";
            StartButton.IsEnabled = true;
            MainWindow._networkPage.ClientPage.StartButton.IsEnabled = true;
            return;
        }
        _listenSocket.Listen(1);
        ConnectStateLabel.Foreground = (Brush)Application.Current.FindResource("SecondTextBrush");
        ConnectStateLabel.Content = "等待连接...";
        OverButton.IsEnabled = true;
        MainWindow._networkPage.ClientPage.StartButton.IsEnabled = false;
        new Thread(WaitConnect){IsBackground = true}.Start();
    }

    private void WaitConnect()
    {
        try
        {
            _mainSocket = _listenSocket.Accept();
        }
        catch (SocketException)
        {
            return;
        }
        _listenSocket.Close();
        if (_mainSocket.RemoteEndPoint is not IPEndPoint endPoint) return;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            MainWindow._networkPage.ServerPageRadioButton.BorderThickness = new Thickness(1);
            ConnectStateLabel.Foreground = (Brush)Application.Current.FindResource("ConnectStateBrush1");
            ConnectStateLabel.Content = "已连接";
            ConnectTargetLabel.Content = "IP=>" + endPoint.Address + "    端口=>" + endPoint.Port;
            MainWindow.IsServer = true;
            MainWindow.IsClient = false;
            IpTextBox.IsEnabled = false;
            MainWindow._taskPage.StartServerReceive(_mainSocket, IpTextBox.Text);
        }));
    }
    
    // 文本框内容改变时关闭提示
    private void IpTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (IpHintRightLabel == null || IpHintErrorLabel == null || IpHintErrorText == null) return;
        IpHintRightLabel.Visibility = Visibility.Collapsed;
        IpHintErrorLabel.Visibility = Visibility.Collapsed;
        IpHintErrorText.Visibility = Visibility.Collapsed;
    }

    private void PortTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        if (PortHintRightLabel == null || PortHintErrorLabel == null || PortHintErrorText == null) return;
        PortHintRightLabel.Visibility = Visibility.Collapsed;
        PortHintErrorLabel.Visibility = Visibility.Collapsed;
        PortHintErrorText.Visibility = Visibility.Collapsed;
    }
}