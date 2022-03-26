using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Updater.ViewModels;
using System;
using Updater.Utils;
using Avalonia.Controls.ApplicationLifetimes;

namespace Updater.Views
{
    public partial class UpdateAvailableWindow : ReactiveWindow<UpdateAvailableViewModel>
    {
        public UpdateAvailableWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            this.WhenActivated(d => d(ViewModel!.Cancel.Subscribe((_) => Close())));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void Show()
        {
            var screenSize = (Application.Current!.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)!
                    .MainWindow.Screens.Primary.Bounds;
            Width = screenSize.Width * 0.5f;
            Height = screenSize.Height * 0.5f;

            base.Show();

            this.SetWindowStartupLocationWorkaround();
        }
    }
}
