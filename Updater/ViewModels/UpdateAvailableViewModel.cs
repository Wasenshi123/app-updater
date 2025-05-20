using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.Reactive;
using Updater.Properties;
using Updater.Views;

namespace Updater.ViewModels
{
    public class UpdateAvailableViewModel: ViewModelBase
    {
        private string currentVersion;
        private string latestVersion;

        public ReactiveCommand<Unit, Unit> Confirm { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public UpdateAvailableViewModel(string currentVersion, string latestVersion)
        {
            this.currentVersion = currentVersion;
            this.latestVersion = latestVersion;

            Confirm = ReactiveCommand.Create(() =>
            {
                // 2nd output : Use this output code to detect it in the target app, eg. to automatically close the running instance
                Console.Out.WriteLine("!!Update!!");

                var desktop = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!;
                var current = desktop.MainWindow;

                desktop.MainWindow = new DownloadProgressWindow();
                desktop.MainWindow.DataContext = new DownloadProgressViewModel();
                desktop.MainWindow.Topmost = true;
                desktop.MainWindow.Show();

                if (Settings.Default.ProgressFullscreen)
                {
                    desktop.MainWindow.WindowState = Avalonia.Controls.WindowState.FullScreen;
                }

                current.Close();
            });

            Cancel = ReactiveCommand.Create(() => { });
        }

        public string CurrentVersion { get => currentVersion; }
        public string LatestVersion { get => latestVersion; }
    }
}
