using System.Configuration;
using System.Data;
using System.Windows;

namespace Files_Transfer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (config.AppSettings.Settings["StartUp"].Value == "true")
        {
            new SplashScreen(@"Assets\PNG\startup.png").Show(true, true);
        }
    }
}