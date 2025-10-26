using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static System.Int32;

namespace Files_Transfer.Pages;

public partial class ClientPage : Page
{
    public Socket _mainSocket;
    private string _ip;
    private int _port;
    private bool _timeKeep;
    public ClientPage()
    {
        InitializeComponent();
        if (MainWindow._settingPage.Config.AppSettings.Settings["ClientIp"].Value != "")
        {
            IpTextBox.Foreground = (Brush)Application.Current.FindResource("SecondTextBrush");
            IpTextBox.Text = MainWindow._settingPage.Config.AppSettings.Settings["ClientIp"].Value;
        }

        if (MainWindow._settingPage.Config.AppSettings.Settings["ClientPort"].Value != "")
        {
            PortTextBox.Foreground = (Brush)Application.Current.FindResource("SecondTextBrush");
            PortTextBox.Text = MainWindow._settingPage.Config.AppSettings.Settings["ClientPort"].Value;
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

    private void Start(object sender, RoutedEventArgs e)
    {
        StartButton.IsEnabled = false;
        MainWindow._networkPage.ServerPage.StartButton.IsEnabled = false;
        var regexV4 = new Regex("^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
        var regexV6 = new Regex("(([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4})|(([0-9a-fA-F]{1,4}:){6}:[0-9a-fA-F]{1,4})|(([0-9a-fA-F]{1,4}:){5}(:[0-9a-fA-F]{1,4}){1,2})|(([0-9a-fA-F]{1,4}:){4}(:[0-9a-fA-F]{1,4}){1,3})|(([0-9a-fA-F]{1,4}:){3}(:[0-9a-fA-F]{1,4}){1,4})|(([0-9a-fA-F]{1,4}:){2}(:[0-9a-fA-F]{1,4}){1,5})|([0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6}))|(:((:[0-9a-fA-F]{1,4}){1,7}))");
        if (regexV4.IsMatch(IpTextBox.Text) || regexV6.IsMatch(IpTextBox.Text))
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
            MainWindow._networkPage.ServerPage.StartButton.IsEnabled = true;
            return;
        }

        if (TryParse(PortTextBox.Text, out var port))
        {
            if (port is >= 1 and <= 65535)
            {
                PortHintRightLabel.Visibility = Visibility.Visible;
                PortHintErrorLabel.Visibility = Visibility.Collapsed;
                PortHintErrorText.Visibility = Visibility.Hidden;
                // 继续----------------------------------------
                ClientStart(port);
            }
            else
            {
                PortHintRightLabel.Visibility = Visibility.Collapsed;
                PortHintErrorLabel.Visibility = Visibility.Visible;
                PortHintErrorText.Visibility = Visibility.Visible;
                PortHintErrorText.Content = "请注意范围！";
                StartButton.IsEnabled = true;
                MainWindow._networkPage.ServerPage.StartButton.IsEnabled = true;
            }
        }
        else
        {
            PortHintRightLabel.Visibility = Visibility.Collapsed;
            PortHintErrorLabel.Visibility = Visibility.Visible;
            PortHintErrorText.Visibility = Visibility.Visible;
            PortHintErrorText.Content = "请输入整数！";
            StartButton.IsEnabled = true;
            MainWindow._networkPage.ServerPage.StartButton.IsEnabled = true;
        }
    }

    public void Over(object sender, RoutedEventArgs e)
    {
        _mainSocket.Close();
        MainWindow._networkPage.ClientPageRadioButton.BorderThickness = new Thickness(0);
        ConnectStateLabel.Foreground = (Brush)Application.Current.FindResource("ConnectStateBrush2");
        ConnectStateLabel.Content = "已关闭";
        ConnectTargetLabel.Content = "";
        StartButton.IsEnabled = true;
        MainWindow._networkPage.ServerPage.StartButton.IsEnabled = true;
        OverButton.IsEnabled = false;
        MainWindow.IsServer = false;
        MainWindow.IsClient = false;
        IpTextBox.IsEnabled = true;
    }

    private void ClientStart(int port)
    {
        _mainSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        ConnectStateLabel.Foreground = (Brush)Application.Current.FindResource("SecondTextBrush");
        ConnectStateLabel.Content = "正在寻找服务端...";
        OverButton.IsEnabled = true;
        MainWindow._networkPage.ServerPage.StartButton.IsEnabled = false;
        _ip = IpTextBox.Text;
        _port = port;
        new Thread(WaitConnect){IsBackground = true}.Start();
        new Thread(TimeKeep){IsBackground = true}.Start();
        _timeKeep = true;
    }

    private void WaitConnect()
    {
        try
        {
            _mainSocket.Connect(new IPEndPoint(IPAddress.Parse(_ip), _port));
        }
        catch (SocketException)
        {
            _timeKeep = false;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ConnectStateLabel.Foreground = (Brush)Application.Current.FindResource("ConnectStateBrush2");
                ConnectStateLabel.Content = "未找到服务端";
                _mainSocket.Close();
                StartButton.IsEnabled = true;
                MainWindow._networkPage.ServerPage.StartButton.IsEnabled = true;
                OverButton.IsEnabled = false;
            }));
            return;
        }
        _timeKeep = false;
        if (_mainSocket.RemoteEndPoint is not IPEndPoint endPoint) return;
        Dispatcher.BeginInvoke(new Action(() =>
        {
            MainWindow._networkPage.ClientPageRadioButton.BorderThickness = new Thickness(1);
            ConnectStateLabel.Foreground = (Brush)Application.Current.FindResource("ConnectStateBrush1");
            ConnectStateLabel.Content = "已连接";
            ConnectTargetLabel.Content = "IP=>" + endPoint.Address + "    端口=>" + endPoint.Port;
            MainWindow.IsServer = false;
            MainWindow.IsClient = true;
            IpTextBox.IsEnabled = false;
            MainWindow._taskPage.StartClientReceive(_mainSocket,IpTextBox.Text);
        }));
    }

    private void TimeKeep()
    {
        var time = 0;
        while (_timeKeep)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ConnectStateLabel.Content = "正在寻找服务端...  " + time++ + "秒";
            }));
            Thread.Sleep(1000);
        }
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