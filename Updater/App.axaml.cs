using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Linq;
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

        private void Desktop_Startup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            string[] commanlineArgs = e.Args;

            if (commanlineArgs.Length > 0)
            {
                if (commanlineArgs.Any(x => x == "check" || x == "c"))
                {

                }
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }
        }
    }
}
