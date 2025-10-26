using System.Configuration;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Files_Transfer.Pages;

public partial class SettingPage : Page
{
    public readonly Configuration Config;
    public int UsLimit;
    public int DsLimit;
    public int Time;
    private readonly ResourceDictionary _light;
    private readonly ResourceDictionary _dark;
    public SettingPage()
    {
        Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        _light = new ResourceDictionary
        {
            Source = new Uri("Resources/Themes/Light.xaml", UriKind.Relative)
        };
        _dark = new ResourceDictionary
        {
            Source = new Uri("Resources/Themes/Dark.xaml", UriKind.Relative)
        };
        InitializeComponent();
        if (Config.AppSettings.Settings["Theme"].Value == "Dark")
        {
            DarkRadioButton.IsChecked = true;
        }
        else
        {
            LightRadioButton.IsChecked = true;
        }
        StartUpToggleButton.IsChecked = Config.AppSettings.Settings["StartUp"].Value == "true";
        DefaultPath.Text = Config.AppSettings.Settings["DownloadPath"].Value;
        AutoReceiveToggleButton.IsChecked = Config.AppSettings.Settings["AutoReceive"].Value == "true";
        ServerIp.Text = Config.AppSettings.Settings["ServerIp"].Value;
        ServerPort.Text = Config.AppSettings.Settings["ServerPort"].Value;
        ClientIp.Text = Config.AppSettings.Settings["ClientIp"].Value;
        ClientPort.Text = Config.AppSettings.Settings["ClientPort"].Value;
        if (int.TryParse(Config.AppSettings.Settings["UpSpeedLimit"].Value, out UsLimit) && UsLimit > 0)
        {
            UpSpeedLimit.Text = Config.AppSettings.Settings["UpSpeedLimit"].Value;
            UpSpeedLimitToggleButton.IsChecked = Config.AppSettings.Settings["UpSpeedLimitSwitch"].Value == "true";
        }
        else
        {
            UpSpeedLimit.Text = "";
            Config.AppSettings.Settings["UpSpeedLimit"].Value = "";
            UpSpeedLimitToggleButton.IsChecked = false;
        }
        if (int.TryParse(Config.AppSettings.Settings["DownSpeedLimit"].Value, out DsLimit) && DsLimit > 0)
        {
            DownSpeedLimit.Text = Config.AppSettings.Settings["DownSpeedLimit"].Value;
            DownSpeedLimitToggleButton.IsChecked = Config.AppSettings.Settings["DownSpeedLimitSwitch"].Value == "true";
        }
        else
        {
            DownSpeedLimit.Text = "";
            Config.AppSettings.Settings["DownSpeedLimit"].Value = "";
            DownSpeedLimitToggleButton.IsChecked = false;
        }
        FileCompletenessCheckToggleButton.IsChecked = Config.AppSettings.Settings["FileCompletenessCheck"].Value == "true";
        if (int.TryParse(Config.AppSettings.Settings["FreshTime"].Value, out Time) && Time > 0)
        {
            FreshTime.Text = Config.AppSettings.Settings["FreshTime"].Value;
        }
        else
        {
            FreshTime.Text = "500";
            Config.AppSettings.Settings["FreshTime"].Value = "500";
            Time = 500;
        }
        Config.Save(ConfigurationSaveMode.Full);
        ConfigurationManager.RefreshSection("appSettings");
    }
    
    // 输入框动画处理
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

    private void DefaultPathTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        Animation(DefaultPathBorder1, true);
    }

    private void DefaultPathTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        Config.AppSettings.Settings["DownloadPath"].Value = DefaultPath.Text;
        Animation(DefaultPathBorder1, false);
    }

    private void DefaultPathTextBox_OnMouseEnter(object sender, MouseEventArgs e)
    {
        Animation(DefaultPathBorder2, true);
    }

    private void DefaultPathTextBox_OnMouseLeave(object sender, MouseEventArgs e)
    {
        Animation(DefaultPathBorder2, false);
    }
    
    private void ServerIpTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        Animation(ServerIpBorder1, true);
    }

    private void ServerIpTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        Config.AppSettings.Settings["ServerIp"].Value = ServerIp.Text;
        Animation(ServerIpBorder1, false);
    }

    private void ServerIpTextBox_OnMouseEnter(object sender, MouseEventArgs e)
    {
        Animation(ServerIpBorder2, true);
    }

    private void ServerIpTextBox_OnMouseLeave(object sender, MouseEventArgs e)
    {
        Animation(ServerIpBorder2, false);
    }
    
    private void ServerPortTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        Animation(ServerPortBorder1, true);
    }

    private void ServerPortTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        Config.AppSettings.Settings["ServerPort"].Value = ServerPort.Text;
        Animation(ServerPortBorder1, false);
    }

    private void ServerPortTextBox_OnMouseEnter(object sender, MouseEventArgs e)
    {
        Animation(ServerPortBorder2, true);
    }

    private void ServerPortTextBox_OnMouseLeave(object sender, MouseEventArgs e)
    {
        Animation(ServerPortBorder2, false);
    }
    
    private void ClientIpTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        Animation(ClientIpBorder1, true);
    }

    private void ClientIpTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        Config.AppSettings.Settings["ClientIp"].Value = ClientIp.Text;
        Animation(ClientIpBorder1, false);
    }

    private void ClientIpTextBox_OnMouseEnter(object sender, MouseEventArgs e)
    {
        Animation(ClientIpBorder2, true);
    }

    private void ClientIpTextBox_OnMouseLeave(object sender, MouseEventArgs e)
    {
        Animation(ClientIpBorder2, false);
    }
    
    private void ClientPortTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        Animation(ClientPortBorder1, true);
    }

    private void ClientPortTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        Config.AppSettings.Settings["ClientPort"].Value = ClientPort.Text;
        Animation(ClientPortBorder1, false);
    }

    private void ClientPortTextBox_OnMouseEnter(object sender, MouseEventArgs e)
    {
        Animation(ClientPortBorder2, true);
    }

    private void ClientPortTextBox_OnMouseLeave(object sender, MouseEventArgs e)
    {
        Animation(ClientPortBorder2, false);
    }
    
    private void UpSpeedLimitTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        Animation(UpSpeedLimitBorder1, true);
    }

    private void UpSpeedLimitTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(UpSpeedLimit.Text, out UsLimit) && UsLimit > 0)
        {
            Config.AppSettings.Settings["UpSpeedLimit"].Value = UpSpeedLimit.Text;
        }
        else
        {
            UpSpeedLimit.Text = "";
            Config.AppSettings.Settings["UpSpeedLimit"].Value = "";
            UpSpeedLimitToggleButton.IsChecked = false;
        }
        Animation(UpSpeedLimitBorder1, false);
    }

    private void UpSpeedLimitTextBox_OnMouseEnter(object sender, MouseEventArgs e)
    {
        Animation(UpSpeedLimitBorder2, true);
    }

    private void UpSpeedLimitTextBox_OnMouseLeave(object sender, MouseEventArgs e)
    {
        Animation(UpSpeedLimitBorder2, false);
    }
    
    private void DownSpeedLimitTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        Animation(DownSpeedLimitBorder1, true);
    }

    private void DownSpeedLimitTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(DownSpeedLimit.Text, out DsLimit) && DsLimit > 0)
        {
            Config.AppSettings.Settings["DownSpeedLimit"].Value = DownSpeedLimit.Text;
        }
        else
        {
            DownSpeedLimit.Text = "";
            Config.AppSettings.Settings["DownSpeedLimit"].Value = "";
            DownSpeedLimitToggleButton.IsChecked = false;
        }
        Animation(DownSpeedLimitBorder1, false);
    }

    private void DownSpeedLimitTextBox_OnMouseEnter(object sender, MouseEventArgs e)
    {
        Animation(DownSpeedLimitBorder2, true);
    }

    private void DownSpeedLimitTextBox_OnMouseLeave(object sender, MouseEventArgs e)
    {
        Animation(DownSpeedLimitBorder2, false);
    }
    
    private void FreshTimeTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        Animation(FreshTimeBorder1, true);
    }

    private void FreshTimeTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(FreshTime.Text, out Time) && Time > 0)
        {
            Config.AppSettings.Settings["FreshTime"].Value = FreshTime.Text;
        }
        else
        {
            FreshTime.Text = "500";
            Config.AppSettings.Settings["FreshTime"].Value = "500";
            Time = 500;
        }
        Animation(FreshTimeBorder1, false);
    }

    private void FreshTimeTextBox_OnMouseEnter(object sender, MouseEventArgs e)
    {
        Animation(FreshTimeBorder2, true);
    }

    private void FreshTimeTextBox_OnMouseLeave(object sender, MouseEventArgs e)
    {
        Animation(FreshTimeBorder2, false);
    }
    
    // 配置文件更新处理、主题切换
    private void LightRadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        Application.Current.Resources.MergedDictionaries.Remove(_dark);
        Application.Current.Resources.MergedDictionaries.Add(_light);
        Config.AppSettings.Settings["Theme"].Value = "Light";
    }

    private void DarkRadioButton_OnChecked(object sender, RoutedEventArgs e)
    {
        Application.Current.Resources.MergedDictionaries.Remove(_light);
        Application.Current.Resources.MergedDictionaries.Add(_dark);
        Config.AppSettings.Settings["Theme"].Value = "Dark";
    }

    private void StartUpToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        Config.AppSettings.Settings["StartUp"].Value = "true";
    }

    private void StartUpToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        Config.AppSettings.Settings["StartUp"].Value = "false";
    }

    private void ChooseDirectory(object sender, RoutedEventArgs e)
    {
        var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
        folderBrowserDialog.ShowDialog();
        DefaultPath.Text = folderBrowserDialog.SelectedPath;
        Config.AppSettings.Settings["DownloadPath"].Value = DefaultPath.Text;
    }

    private void AutoReceiveToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(DefaultPath.Text))
        {
            new MessageWindow("错误", "请先填写有效默认路径！")
            {
                Owner = Application.Current.MainWindow
            }.ShowDialog();
            new Thread(AutoReceiveToggleButtonClose).Start();
            return;
        }
        Config.AppSettings.Settings["AutoReceive"].Value = "true";
    }

    private void AutoReceiveToggleButtonClose()
    {
        Dispatcher.Invoke(() =>
        {
            AutoReceiveToggleButton.IsChecked = false;
        });
    }

    private void AutoReceiveToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        Config.AppSettings.Settings["AutoReceive"].Value = "false";
    }

    private void UpSpeedLimitToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(UpSpeedLimit.Text, out UsLimit) && UsLimit > 0)
        {
            Config.AppSettings.Settings["UpSpeedLimitSwitch"].Value = "true";
        }
        else
        {
            new MessageWindow("错误", "请先填写有效上传速度！")
            {
                Owner = Application.Current.MainWindow
            }.ShowDialog();
            new Thread(UpSpeedLimitToggleButtonClose).Start();
        }
    }

    private void UpSpeedLimitToggleButtonClose()
    {
        Dispatcher.Invoke(() =>
        {
            UpSpeedLimitToggleButton.IsChecked = false;
        });
    }

    private void UpSpeedLimitToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        Config.AppSettings.Settings["UpSpeedLimitSwitch"].Value = "false";
    }

    private void DownSpeedLimitToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        if (int.TryParse(DownSpeedLimit.Text, out DsLimit) && DsLimit > 0)
        {
            Config.AppSettings.Settings["DownSpeedLimitSwitch"].Value = "true";
        }
        else
        {
            new MessageWindow("错误", "请先填写有效下载速度！")
            {
                Owner = Application.Current.MainWindow
            }.ShowDialog();
            new Thread(DownSpeedLimitToggleButtonClose).Start();
        }
    }
    
    private void DownSpeedLimitToggleButtonClose()
    {
        Dispatcher.Invoke(() =>
        {
            DownSpeedLimitToggleButton.IsChecked = false;
        });
    }

    private void DownSpeedLimitToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        Config.AppSettings.Settings["DownSpeedLimitSwitch"].Value = "false";
    }

    private void FileCompletenessCheckToggleButton_OnChecked(object sender, RoutedEventArgs e)
    {
        Config.AppSettings.Settings["FileCompletenessCheck"].Value = "true";
    }

    private void FileCompletenessCheckToggleButton_OnUnchecked(object sender, RoutedEventArgs e)
    {
        Config.AppSettings.Settings["FileCompletenessCheck"].Value = "false";
    }

    private void SettingPage_OnUnloaded(object sender, RoutedEventArgs e)
    {
        Config.Save(ConfigurationSaveMode.Full);
        ConfigurationManager.RefreshSection("appSettings");
    }
}