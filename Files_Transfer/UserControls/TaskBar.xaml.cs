using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Files_Transfer.UserControls;

public partial class TaskBar
{
    public TaskBar()
    {
        InitializeComponent();
    }
    // 依赖属性注册-------------------------------------------------------------------------------------------------------
    public static readonly DependencyProperty FileNameProperty =
        DependencyProperty.Register(nameof(FileName), typeof(string), typeof(TaskBar));
    
    public string FileName
    {
        get => (string)GetValue(FileNameProperty);
        set => SetValue(FileNameProperty, value);
    }
    
    public static readonly DependencyProperty InstantSizeProperty =
        DependencyProperty.Register(nameof(InstantSize), typeof(string), typeof(TaskBar));
    
    public string InstantSize
    {
        get => (string)GetValue(InstantSizeProperty);
        set => SetValue(InstantSizeProperty, value);
    }
    
    public static readonly DependencyProperty PercentProperty =
        DependencyProperty.Register(nameof(Percent), typeof(string), typeof(TaskBar), new PropertyMetadata("0.0 %"));
    
    public string Percent
    {
        get => (string)GetValue(PercentProperty);
        set => SetValue(PercentProperty, value);
    }
    
    public static readonly DependencyProperty UpSpeedProperty =
        DependencyProperty.Register(nameof(UpSpeed), typeof(string), typeof(TaskBar), new PropertyMetadata("\u2191 0 B/s"));
    
    public string UpSpeed
    {
        get => (string)GetValue(UpSpeedProperty);
        set => SetValue(UpSpeedProperty, value);
    }
    
    public static readonly DependencyProperty DownSpeedProperty =
        DependencyProperty.Register(nameof(DownSpeed), typeof(string), typeof(TaskBar), new PropertyMetadata("\u2193 0 B/s"));
    
    public string DownSpeed
    {
        get => (string)GetValue(DownSpeedProperty);
        set => SetValue(DownSpeedProperty, value);
    }
    
    public static readonly DependencyProperty RestTimeProperty =
        DependencyProperty.Register(nameof(RestTime), typeof(string), typeof(TaskBar), new PropertyMetadata("0天 0时 0分 0秒"));
    
    public string RestTime
    {
        get => (string)GetValue(RestTimeProperty);
        set => SetValue(RestTimeProperty, value);
    }
    
    public static readonly DependencyProperty PathProperty =
        DependencyProperty.Register(nameof(Path), typeof(string), typeof(TaskBar));
    
    public string Path
    {
        get => (string)GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }
    
    public static readonly DependencyProperty ProgressValueProperty =
        DependencyProperty.Register(nameof(ProgressValue), typeof(double), typeof(TaskBar));
    
    public double ProgressValue
    {
        get => (double)GetValue(ProgressValueProperty);
        set => SetValue(ProgressValueProperty, value);
    }
    
    // 详细信息页切换------------------------------------------------------------------------------------------------------
    private void InfoButton_OnClick(object sender, RoutedEventArgs e)
    {
        MainBorder.Visibility = Visibility.Collapsed;
        InfoBorder.Visibility = Visibility.Visible;
    }

    private void InfoCloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        InfoBorder.Visibility = Visibility.Collapsed;
        MainBorder.Visibility = Visibility.Visible;
    }
    
    // 路径输入框动画------------------------------------------------------------------------------------------------------
    private void Animation(Border border, bool visible)
    {
        var doubleAnimation = new DoubleAnimation
        {
            Duration = new Duration(TimeSpan.FromSeconds(0.25)),
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
    
    // 内外框动画
    private void OutActiveBorder_OnMouseEnter(object sender, MouseEventArgs e)
    {
        Animation(OutActiveBorder, true);
    }

    private void OutActiveBorder_OnMouseLeave(object sender, MouseEventArgs e)
    {
        Animation(OutActiveBorder, false);
    }

    private void InActiveBorder_OnMouseEnter(object sender, MouseEventArgs e)
    {
        Animation(InActiveBorder, true);
    }

    private void InActiveBorder_OnMouseLeave(object sender, MouseEventArgs e)
    {
        Animation(InActiveBorder, false);
    }
    
    // 路径选择-----------------------------------------------------------------------------------------------------------
    private void PathChoose(object sender, RoutedEventArgs e)
    {
        var folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
        folderBrowserDialog.ShowDialog();
        Path = folderBrowserDialog.SelectedPath;
    }
}