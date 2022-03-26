using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Updater.Properties;
using Updater.Services;
using Updater.Utils;
using Updater.ViewModels;
using Updater.Views;

namespace Updater
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Startup += Desktop_Startup;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async void Desktop_Startup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            string[] commanlineArgs = e.Args;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (commanlineArgs.Length > 0)
                {
                    if (commanlineArgs.Any(x => x == "check" || x == "c"))
                    {
                        var service = new UpdateService();
                        bool upToDate;
                        try
                        {
                            upToDate = await service.CheckUpdate();
                        }
                        catch (Exception error)
                        {
                            Console.WriteLine($"error checking: {error.Message}");
                            throw;
                        }

                        // 1st output : Detect output as bool in target app (the calling app)
                        Console.Out.WriteLine(upToDate);

                        if (!upToDate)
                        {
                            if (commanlineArgs.Any(x => x.Replace("-", "") == "force" || x.Replace("-", "") == "f"))
                            {
                                desktop.MainWindow = new DownloadProgressWindow();
                                desktop.MainWindow.DataContext = new DownloadProgressViewModel();
                                desktop.MainWindow.Topmost = true;
                                desktop.MainWindow.Show();

                                if (Settings.Default.ProgressFullscreen)
                                {
                                    desktop.MainWindow.WindowState = WindowState.FullScreen;
                                }
                            }
                            else
                            {
                                desktop.MainWindow = new UpdateAvailableWindow
                                {
                                    DataContext = new UpdateAvailableViewModel()
                                };
                                desktop.MainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                                desktop.MainWindow.Topmost = true;

                                desktop.MainWindow.Show();
                                desktop.MainWindow.SetAlwaysOnTop();
                            }
                        }
                        else
                        {
                            if (desktop.MainWindow == null)
                            {
                                desktop.Shutdown();
                            }
                        }
                    }
                }
                else
                {
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = new MainWindowViewModel(),
                    };
                }
            }
        }

        public static async Task ShowAlert(string msg)
        {
            var alert = new AlertWindow();
            alert.DataContext = new AlertViewModel(msg);

            var desktop = ((IClassicDesktopStyleApplicationLifetime)Current!.ApplicationLifetime!);
            if (desktop.MainWindow != null)
            {
                await alert.ShowDialog(desktop.MainWindow);
            }
            else
            {
                desktop.MainWindow = alert;
                desktop.MainWindow.Topmost = true;
                desktop.MainWindow.SetAlwaysOnTop();

                desktop.MainWindow.Show();
            }
        }
    }
}


