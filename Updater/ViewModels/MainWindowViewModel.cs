using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Updater.Properties;

namespace Updater.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string appPath;
        private string server;
        private string appName;

        public ICommand SaveConfig { get; }
        public ICommand Find { get; }

        public MainWindowViewModel()
        {
            FindFolder = new Interaction<Unit, string?>();

            AppPath = Settings.Default.ClientAppPath;
            Server = Settings.Default.UpdateServer;
            AppName = Settings.Default.AppName;

            SaveConfig = ReactiveCommand.Create(() =>
            {

                Settings.Default.ClientAppPath = AppPath;
                Settings.Default.UpdateServer = string.IsNullOrWhiteSpace(Server) ? "" : ("http://" + Regex.Replace(Server, @"(https?://)", ""));
                Settings.Default.AppName = AppName;

                Settings.Default.Save();
                Settings.Default.Reload();
            });

            Find = ReactiveCommand.CreateFromTask(async () =>
            {
                var result = await FindFolder.Handle(Unit.Default);
                if (result != null)
                {
                    AppPath = result;
                }
            });
        }

        public string AppPath { get => appPath; set => this.RaiseAndSetIfChanged(ref appPath, value); }
        public string AppName { get => appName; set => this.RaiseAndSetIfChanged(ref appName, value); }
        public string Server { get => server; set => this.RaiseAndSetIfChanged(ref server, value); }
        

        public Interaction<Unit, string?> FindFolder { get; }
    }
}
