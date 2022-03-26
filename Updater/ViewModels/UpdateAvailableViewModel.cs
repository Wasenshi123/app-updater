using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;
using System.Reactive;
using Updater.Views;

namespace Updater.ViewModels
{
    public class UpdateAvailableViewModel: ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> Confirm { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }

        public UpdateAvailableViewModel()
        {
            Confirm = ReactiveCommand.Create(() =>
            {
                Console.Out.WriteLine("!!Update!!");

                var desktop = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!;
                var current = desktop.MainWindow;

                desktop.MainWindow = new DownloadProgressWindow();
                desktop.MainWindow.DataContext = new DownloadProgressViewModel();
                desktop.MainWindow.Topmost = true;
                desktop.MainWindow.Show();

                desktop.MainWindow.WindowState = Avalonia.Controls.WindowState.FullScreen;

                current.Close();
            });

            Cancel = ReactiveCommand.Create(() => { });
        }
    }
}
