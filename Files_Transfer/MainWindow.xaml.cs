using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Files_Transfer.Pages;
using Files_Transfer.UserControls;

namespace Files_Transfer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public static bool IsServer = false;
    public static bool IsClient = false;
    public static NetworkPage _networkPage; // 供ServerPage、ClientPage访问
    public static TaskPage _taskPage; // 供ServerPage、ClientPage访问
    public static SendPage _sendPage;
    public static SettingPage _settingPage;
    public MainWindow()
    {
        InitializeComponent();
        _settingPage = new SettingPage();
        _networkPage = new NetworkPage();
        _taskPage = new TaskPage();
        _sendPage = new SendPage();
        new Thread(SpeedIndicatorThread){IsBackground = true}.Start();
    }

    // 窗口关闭按钮
    private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
    { 
        var hint = _taskPage.TransferSendList.Any(variable => variable.Work) || _taskPage.TransferReceiveList.Any(variable => variable.Work);
        if (hint)
        {
             e.Cancel = !(bool)new ConfirmWindow("确定要关闭吗？", "存在未完成任务，是否强制关闭程序？")
            {
                Owner = Application.Current.MainWindow
            }.ShowDialog()!;
        }
    }

    // 窗口状态变化触发
    private void MainWindow_OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Maximized)
        {
            Maximize.Visibility = Visibility.Collapsed;
            MinimizeWindow.Visibility = Visibility.Visible;
        }
        else
        {
            MinimizeWindow.Visibility = Visibility.Collapsed;
            Maximize.Visibility = Visibility.Visible;
        }
    }
    
    // 窗口按钮变化
    private void CheckBorder_OnMouseEnter(object sender, MouseEventArgs e)
    {
        var point = Mouse.GetPosition(CheckBorder);
        if (point.Y > 5)
        {
            Minimize.Foreground = (Brush)Application.Current.FindResource("WindowButtonForegroundBrushActive");
            MinimizeWindow.Foreground = (Brush)Application.Current.FindResource("WindowButtonForegroundBrushActive");
            Maximize.Foreground = (Brush)Application.Current.FindResource("WindowButtonForegroundBrushActive");
            CloseWindow.Foreground = (Brush)Application.Current.FindResource("WindowButtonForegroundBrushActive");
        }
        else
        {
            Minimize.Background = (Brush)Application.Current.FindResource("BackgroundBrush");
            MinimizeWindow.Background = (Brush)Application.Current.FindResource("BackgroundBrush");
            Maximize.Background = (Brush)Application.Current.FindResource("BackgroundBrush");
            CloseWindow.Background = (Brush)Application.Current.FindResource("BackgroundBrush");
        }
    }

    private void CheckBorder_OnMouseLeave(object sender, MouseEventArgs e)
    {
         var point = Mouse.GetPosition(CheckBorder);
         if (point.Y > 5)
         {
             Minimize.Foreground = (Brush)Application.Current.FindResource("WindowButtonForegroundBrush");
             MinimizeWindow.Foreground = (Brush)Application.Current.FindResource("WindowButtonForegroundBrush");
             Maximize.Foreground = (Brush)Application.Current.FindResource("WindowButtonForegroundBrush");
             CloseWindow.Foreground = (Brush)Application.Current.FindResource("WindowButtonForegroundBrush");
         }
         else
         {
             switch (point.X)
             {
                 case >= 10 and <= 57:
                     Minimize.Background = (Brush)Application.Current.FindResource("WindowButtonBackgroundBrushActive1");
                     break;
                 case >= 58 and <= 104:
                     MinimizeWindow.Background = (Brush)Application.Current.FindResource("WindowButtonBackgroundBrushActive1");
                     Maximize.Background = (Brush)Application.Current.FindResource("WindowButtonBackgroundBrushActive1");
                     break;
                 case >= 105:
                     CloseWindow.Background = (Brush)Application.Current.FindResource("WindowButtonBackgroundBrushActive2");
                     break;
             }
         }
    }

    private void ClearWindowButton()
    {
        Minimize.Background = (Brush)Application.Current.FindResource("BackgroundBrush");
        MinimizeWindow.Background = (Brush)Application.Current.FindResource("BackgroundBrush");
        Maximize.Background = (Brush)Application.Current.FindResource("BackgroundBrush");
        CloseWindow.Background = (Brush)Application.Current.FindResource("BackgroundBrush");
        Minimize.Foreground = (Brush)Application.Current.FindResource("WindowButtonForegroundBrush");
        MinimizeWindow.Foreground = (Brush)Application.Current.FindResource("WindowButtonForegroundBrush");
        Maximize.Foreground = (Brush)Application.Current.FindResource("WindowButtonForegroundBrush");
        CloseWindow.Foreground = (Brush)Application.Current.FindResource("WindowButtonForegroundBrush");
    }

    private void MainWindow_OnMouseEnter(object sender, MouseEventArgs e)
    {
        var point = Mouse.GetPosition(CheckBorder);
        if (point.X < 10 || point.Y > 10) ClearWindowButton();
    }

    // 侧边栏按钮导航
    private void NetworkPageSwitchButton(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(_networkPage);
    }

    private void TaskPageSwitchButton(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(_taskPage);
    }

    private void SendPageSwitchButton(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(_sendPage);
    }

    private void SettingPageSwitchButton(object sender, RoutedEventArgs e)
    {
        Frame.Navigate(_settingPage);
    }

    private void InfoWindowSwitchButton(object sender, RoutedEventArgs e)
    {
        new InfoWindow
        {
            Owner = this
        }.ShowDialog();
    }
    
    // 速度表控件拖动处理
    private bool _isMouseDown;
    private Point _standardPoint;
    private void DragMouseDown(object sender, MouseButtonEventArgs e)
    {
        _isMouseDown = true;
        _standardPoint = e.GetPosition(Indicator);
    }

    private void DragMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isMouseDown) return;
        var point = e.GetPosition(DragCanvas);
        Indicator.Margin = new Thickness(point.X - _standardPoint.X, point.Y - _standardPoint.Y, 0, 0);
    }

    private void DragMouseUp(object sender, MouseButtonEventArgs e)
    {
        _isMouseDown = false;
    }

    private void MainWindow_OnMouseLeave(object sender, MouseEventArgs e)
    {
        _isMouseDown = false;
    }
    
    // 速度表动画处理
    private void Animation(SpeedIndicator speedIndicator, bool work)
    { 
        var widthAnimation = new DoubleAnimation
        {
            Duration = new Duration(TimeSpan.FromSeconds(0.3)),
            To = work? 160 : 40,
            EasingFunction = new CubicEase
            {
                EasingMode = EasingMode.EaseInOut
            }
        };
        var widthStoryboard = new Storyboard();
        widthStoryboard.Children.Add(widthAnimation);
        Storyboard.SetTargetName(widthAnimation, speedIndicator.Name);
        Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(WidthProperty));
        widthStoryboard.Begin(this);
        
        var indicatorMargin = Indicator.Margin;
        _marginLeft = indicatorMargin.Left + (work ? -120 : 120);
        var marginAnimation = new ThicknessAnimation
        {
            Duration = new Duration(TimeSpan.FromSeconds(0.3)),
            To = new Thickness(_marginLeft, indicatorMargin.Top, 0, 0),
            EasingFunction = new CubicEase
            {
                EasingMode = EasingMode.EaseInOut
            }
        };
        var marginStoryboard = new Storyboard();
        marginStoryboard.Children.Add(marginAnimation);
        Storyboard.SetTargetName(marginAnimation, speedIndicator.Name);
        Storyboard.SetTargetProperty(marginAnimation, new PropertyPath(MarginProperty));
        marginStoryboard.FillBehavior = FillBehavior.Stop;
        marginStoryboard.Completed += PositionKeep;
        marginStoryboard.Begin(this);
    }

    private double _marginLeft;
    private void PositionKeep(object? sender, EventArgs e)
    {
        Indicator.Margin = Indicator.Margin with { Left = _marginLeft };
    }
    
    // 速度表线程
    private bool _open;
    private void SpeedIndicatorThread()
    {
        while (true)
        {
            var totalUpSpeed = _taskPage.TransferSendList.Sum(variable => variable.Speed);
            var totalDownSpeed = _taskPage.TransferReceiveList.Sum(variable => variable.Speed);
            if (totalUpSpeed == 0 && totalDownSpeed == 0)
            {
                if (_open)
                {
                    _open = false;
                    Dispatcher.Invoke(() =>
                    {
                        Indicator.Icon.Foreground = (Brush)Application.Current.FindResource("SpeedIndicatorIconBrush");
                        Animation(Indicator, false);
                    });
                }
            }
            else
            {
                if (!_open)
                {
                    _open = true;
                    Dispatcher.Invoke(() =>
                    {
                        Indicator.Icon.Foreground = (Brush)Application.Current.FindResource("SpeedIndicatorActiveBrush");
                        Animation(Indicator, true);
                    });
                }
                Dispatcher.Invoke(() =>
                {
                    Indicator.UsText.Text = $"\u2191 {SpeedConvert(totalUpSpeed, 1)}";
                    Indicator.DsText.Text = $"\u2193 {SpeedConvert(totalDownSpeed, 1)}";
                });
            }
            Thread.Sleep(500);
        }
        // ReSharper disable once FunctionNeverReturns
    }
    
    private static string SpeedConvert(double speed, int num)
    {
        return speed switch
        {
            < 1024 => $"{speed} B/s",
            < 1024 * 1024 => $"{Math.Round(speed / 1024, num)} KB/s",
            < 1024 * 1024 * 1024 => $"{Math.Round(speed / 1024 / 1024, num)} MB/s",
            < 1024 * 1024 * 1024 * 1024L => $"{Math.Round(speed / 1024 / 1024 / 1024, num)} GB/s",
            _ => $"{Math.Round(speed / 1024 / 1024 / 1024 / 1024, num)} TB/s"
        };
    }
}