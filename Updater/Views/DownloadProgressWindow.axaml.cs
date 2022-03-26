using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Updater.ViewModels;
using System;
using CheckIn.Utils;
using Updater.Utils;
using Avalonia.Threading;

namespace Updater.Views
{
    public partial class DownloadProgressWindow : ReactiveWindow<DownloadProgressViewModel>
    {
        public DownloadProgressWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            // Chain call; Core Processing
            this.WhenActivated(d => d(ViewModel!.StartDownload().Subscribe((filePath) => {
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    Dispatcher.UIThread.Post(() => ViewModel.StartExtract(filePath).Subscribe((extracted) =>
                        Dispatcher.UIThread.Post(() => ViewModel.CleanAndFinish(extracted, filePath).Subscribe((_) =>
                        {
                            // "sudo reboot".Cmd();
                        }))
                    ));
                }
            })));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
