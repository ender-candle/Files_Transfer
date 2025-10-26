using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Files_Transfer.Pages;

public partial class SendPage : Page
{
    public SendPage()
    {
        InitializeComponent();
    }
    
    // 路径输入框动画------------------------------------------------------------------------------------------------------
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
    
    private void PathTextBox_OnGotFocus(object sender, RoutedEventArgs e)
    {
        Animation(PathBorder1, true);
    }

    private void PathTextBox_OnLostFocus(object sender, RoutedEventArgs e)
    {
        Animation(PathBorder1, false);
    }

    private void PathTextBox_OnMouseEnter(object sender, MouseEventArgs e)
    {
        Animation(PathBorder2, true);
    }

    private void PathTextBox_OnMouseLeave(object sender, MouseEventArgs e)
    {
        Animation(PathBorder2, false);
    }

    // 拖入文件处理
    private void DropFile(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        var files = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (files.Length > 1)
        {
            new MessageWindow("提醒", "拖入多文件将只取用首个")
            {
                Owner = Application.Current.MainWindow
            }.ShowDialog();
        }
        if (File.GetAttributes(files[0]) == FileAttributes.Directory)
        {
            new MessageWindow("提醒", "请放入文件而非文件夹")
            {
                Owner = Application.Current.MainWindow
            }.ShowDialog();
            return;
        }
        PathTextBox.Text = files[0];
    }

    // 选择文件处理
    private void ChooseFile(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new System.Windows.Forms.OpenFileDialog();
        if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
        PathTextBox.Text = openFileDialog.FileName;
    }
    
    // 文件存在检测
    private void Fresh(object sender, RoutedEventArgs e)
    {
        if (File.Exists(PathTextBox.Text))
        {
            GetFileInfo();
        }
        else
        {
            FileSizeLabel.Content = "文件不存在";
        }
    }

    // 文件大小获取
    private void GetFileInfo()
    {
        var fileInfo = new FileInfo(PathTextBox.Text);
        var size = fileInfo.Length switch
        {
            < 1024 => $"{fileInfo.Length} B",
            < 1024 * 1024 => $"{fileInfo.Length / 1024.0} KB ({fileInfo.Length} B)",
            < 1024 * 1024 * 1024 => $"{fileInfo.Length / 1024.0 / 1024} MB ({fileInfo.Length} B)",
            < 1024 * 1024 * 1024 * 1024L => $"{fileInfo.Length / 1024.0 / 1024 / 1024} GB ({fileInfo.Length} B)",
            _ => $"{fileInfo.Length / 1024.0 / 1024 / 1024 / 1024} TB ({fileInfo.Length} B)"
        };
        FileSizeLabel.Content = size;
    }

    // 发送
    private void Send(object sender, RoutedEventArgs e)
    {
        if (File.Exists(PathTextBox.Text))
        {
            // 继续------------------------------
            if (MainWindow.IsServer && !MainWindow.IsClient)
            {
                MainWindow._taskPage.StartSend(MainWindow._networkPage.ServerPage._mainSocket, PathTextBox.Text);
                SendButton.IsEnabled = false;
                SendingLabel.Visibility = Visibility.Visible;
                SuccessLabel.Visibility = Visibility.Collapsed;
                FailLabel.Visibility = Visibility.Collapsed;
                InfoLabel.Content = "发送中...";
            } else if (!MainWindow.IsServer && MainWindow.IsClient)
            {
                MainWindow._taskPage.StartSend(MainWindow._networkPage.ClientPage._mainSocket, PathTextBox.Text);
                SendButton.IsEnabled = false;
                SendingLabel.Visibility = Visibility.Visible;
                SuccessLabel.Visibility = Visibility.Collapsed;
                FailLabel.Visibility = Visibility.Collapsed;
                InfoLabel.Content = "发送中...";
            } else if (!MainWindow.IsServer && !MainWindow.IsClient)
            {
                new MessageWindow("警告", "未连接")
                {
                    Owner = Application.Current.MainWindow
                }.ShowDialog();
            }
            else
            {
                new MessageWindow("???", "你不应该能见到这个弹窗，如果见到说明出大问题了")
                {
                    Owner = Application.Current.MainWindow
                }.ShowDialog();
            }
        }
        else
        {
            new MessageWindow("提醒", "文件不存在")
            {
                Owner = Application.Current.MainWindow
            }.ShowDialog();
        }
    }
}