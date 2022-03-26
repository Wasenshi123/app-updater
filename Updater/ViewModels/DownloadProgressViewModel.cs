using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Updater.Properties;
using Updater.Services;
using Updater.Utils;
using UpdaterLib;

namespace Updater.ViewModels
{
    public class DownloadProgressViewModel : ViewModelBase
    {
        private float percent;
        private string progressTxt;
        private string labelTxt;
        private bool isFailed;

        public float Percent { get => percent; set => this.RaiseAndSetIfChanged(ref percent, value); }
        public string ProgressTxt { get => progressTxt; set => this.RaiseAndSetIfChanged(ref progressTxt, value); }
        public string LabelTxt { get => labelTxt; set => this.RaiseAndSetIfChanged(ref labelTxt, value); }

        public bool IsFailed { get => isFailed; set => this.RaiseAndSetIfChanged(ref isFailed, value); }
        public ReactiveCommand<Unit, string?> Retry { get; private set; }
        public bool AutoReboot => Settings.Default.AutoReboot;

        public DownloadProgressViewModel()
        {
            Retry = ReactiveCommand.CreateFromObservable(StartDownload);
        }

        public IObservable<string?> StartDownload()
        {
            IsFailed = false;
            return Observable.StartAsync(async () =>
            {
                try
                {
                    var update = new UpdateService();
                    var filePath = await update.Download(OnDownloadProgress);
                    Console.WriteLine($"download complete! file: {filePath}");

                    return filePath;
                }
                catch (Exception e)
                {
                    Logger.LogError("download failed", e);
                }

                IsFailed = true;

                return null;
            });
        }

        public IObservable<string> StartExtract(string sourcePath)
        {
            return Observable.StartAsync(async () =>
            {
                try
                {
                    Console.WriteLine("Start extracting...");
                    var destination = Settings.Default.ClientAppPath;
                    var update = new UpdateService();
                    var extracted = await update.ExtractTarballFile(sourcePath, destination, OnExtractProgress, OnInstallProgress);
                    Console.WriteLine($"extract and install complete!");
                    return extracted;
                }
                catch (Exception e)
                {
                    Logger.LogError("extract failed", e);
                    Console.WriteLine($"Failed extracting: {e}");
                    throw;
                }
            });
        }

        private void OnDownloadProgress(long downloaded, long totalSize, float percent)
        {
            var current = Helper.SizeSuffix(downloaded);
            var total = Helper.SizeSuffix(totalSize);
            ProgressTxt = $"{current}/{total}";
            LabelTxt = $"Downloading Update... {percent / 100:P2}";
            Percent = percent;
        }

        private void OnExtractProgress(long progress, long totalSize, float percent)
        {
            var current = Helper.SizeSuffix(progress);
            var total = Helper.SizeSuffix(totalSize);
            ProgressTxt = $"{current}/{total}";
            LabelTxt = $"Extracting... {percent / 100:P2}";
            Percent = percent;
        }

        private void OnInstallProgress(long progress, long totalSize, float percent)
        {
            ProgressTxt = "";
            LabelTxt = $"Installing Update... {percent / 100:P2}";
            Percent = percent;
        }

        public IObservable<Unit> CleanAndFinish(string extracted, string sourcePath)
        {
            return Observable.Start(() =>
            {
                try
                {
                    Console.WriteLine("Save version and clean up...");
                    File.Delete(extracted);
                    var info = new FileInfo(sourcePath);
                    var splits = Path.GetFileNameWithoutExtension(info.Name).Split('-');
                    var version = splits.Length > 1 ? splits.Last().Replace(".tar", "") : null;
                    Console.WriteLine($"modified: {info.LastWriteTimeUtc}");

                    var newVersion = new UpdateInfo
                    {
                        Modified = info.LastWriteTimeUtc,
                        Version = version
                    };
                    Settings.Default.LastVersion = newVersion;
                    Settings.Default.Save();

                    File.Delete(sourcePath);

                    Console.WriteLine("Saved and Cleaned!");
                }
                catch (Exception e)
                {
                    Logger.LogError("cleanning failed", e);
                    Console.WriteLine($"Failed cleaning: {e}");
                    throw;
                }
            });
        }
    }
}
