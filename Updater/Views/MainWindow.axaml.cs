using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using Updater.Utils;
using Updater.ViewModels;

namespace Updater.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.FindFolder.RegisterHandler(DoShowFolderDialogAsync)));
            this.WhenActivated(d => d(ViewModel!.Close.Subscribe((_)=> Close())));
        }

        public async Task<string?> GetPath()
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            dialog.Title = "Client App Path";
            dialog.Directory = AppDomain.CurrentDomain.BaseDirectory;

            string? result = await dialog.ShowAsync(this);

            return result;
        }

        public async Task DoShowFolderDialogAsync(InteractionContext<Unit, string?> interaction)
        {
            var result = await GetPath();
            interaction.SetOutput(result);
        }

        public override void Show()
        {
            base.Show();

            this.SetWindowStartupLocationWorkaround();
        }
    }
}
