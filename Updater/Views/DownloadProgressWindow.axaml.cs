using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;
using Updater.ViewModels;
using System;
using Updater.Utils;
using Avalonia.Threading;
using Updater.Properties;

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
                            // 3rd output : Use this output code to detect in calling app thread, eg. to auto re-start/run the updated instance
                            Console.Out.WriteLine("!!Finish!!");
                            if (Settings.Default.AutoReboot)
                            {
                                if (OperatingSystem.IsLinux())
                                {
                                    // For embeded environment, rebooting may be the best choice
                                    "sudo reboot".Cmd();
                                }
                                else if (OperatingSystem.IsMacOS())
                                {
                                    "sudo shutdown -r now".Cmd();
                                }
                                else
                                {
                                    "shutdown /r /t:0".Cmd();
                                }
                            }
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
