using Avalonia;
using Avalonia.Threading;
using FluentHttpClient;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Updater.Properties;
using Updater.Utils;
using UpdaterLib;

namespace Updater.Services
{
    public class UpdateService
    {
        public static bool AlreadyDownloaded = false;

        public static string GetMD5HashFromFile(string fileName)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        public async Task<bool> CheckUpdate()
        {
            var server = Settings.Default.UpdateServer;
            var appName = Settings.Default.AppName;
            var appFolder = Settings.Default.ClientAppPath;

            // ================ Safe-Gaurd Init core config first ======================

            bool exit = false;
            if (string.IsNullOrWhiteSpace(server))
            {
                await App.ShowAlert("Please config server URI first.");
                exit = true;
            }
            else if (!Uri.TryCreate(server, UriKind.Absolute, out var uri))
            {
                await App.ShowAlert("The update server is not valid URI.");
                exit = true;
            }
            if (string.IsNullOrWhiteSpace(appName))
            {
                await App.ShowAlert("Please config app name first.");
                exit = true;
            }
            if (string.IsNullOrWhiteSpace(appFolder))
            {
                await App.ShowAlert("Please config the app folder path first.");
                exit = true;
            }

            if (exit)
            {
                return false;
            }

            // ====================================================================

            DateTimeOffset? lastMod = null;
            string? version = null;

            string? checksum = null;
            var currentFile = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.tar.gz")
                .OrderByDescending(x => new FileInfo(x).LastWriteTimeUtc)
                .FirstOrDefault();
            var lastVersion = Settings.Default.LastVersion;
            if (!string.IsNullOrWhiteSpace(currentFile))
            {
                var fileInfo = new FileInfo(currentFile);
                lastMod = fileInfo.LastWriteTimeUtc;
                version = GetVersionFromFileName(currentFile);
                checksum = GetMD5HashFromFile(currentFile);
            }
            else
            {
                if (lastVersion != null)
                {
                    lastMod = lastVersion.Modified;
                    version = lastVersion.Version;
                }
            }

            var url = $"{server}/update/{appName}/check";

            int statusCode = 0;
            try
            {
                var client = new HttpClient(new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    CheckCertificateRevocationList = false,
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                });
                var uptodate = await client.UsingRoute(url)
                    .WithJsonContent(new { Version = version, Modified = lastMod, Checksum = checksum })
                    .WithRequestTimeout(5)
                    .PostAsync()
                    .OnFailureAsync(async (res) =>
                    {
                        statusCode = (int)res.StatusCode;
                        Logger.LogError($"error calling server ({statusCode}): {await res.GetResponseStringAsync()}");
                        await App.ShowAlert($"Error on calling server ({statusCode}). Please contact administrator.");
                    }, false)
                    .DeserializeJsonAsync<bool>();

                if (!uptodate)
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(currentFile))
                {
                    AlreadyDownloaded = true;
                    return CheckVersion(lastVersion, currentFile);
                }

                return true;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (Exception ex) when (
                ex is TaskCanceledException ||
                ex is TimeoutException
            )
            {
                await App.ShowAlert($"Error on calling server ({statusCode}). Please contact administrator.");
                return false;
            }
            
        }


        public delegate void OnProgress(long currentSize, long totalSize, float percent);

        public async Task<string> Download(OnProgress onUpdateProgress)
        {
            var server = Settings.Default.UpdateServer;
            var appName = Settings.Default.AppName;
            var downloadPath = AppDomain.CurrentDomain.BaseDirectory;

            // if already downloaded skip download again
            string? currentFile = GetCurrentFile();
            if (!string.IsNullOrWhiteSpace(currentFile) && AlreadyDownloaded)
            {
                Console.WriteLine("Already downloaded, so skip and extract current file...");
                var info = new FileInfo(currentFile);
                Dispatcher.UIThread.Post(() =>
                {
                    onUpdateProgress?.Invoke(info.Length, info.Length, 100f);
                });

                return currentFile;
            }

            var url = $"{server}/update/{appName}/download";

            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = delegate
            {
                return true;
            };

            var client = new HttpClient(handler);
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            using (var res = await client.GetAsync(url))
            {
                long? totalToReceive = res.Content.Headers.ContentLength;
                long totalDownloaded = 0;
                string fileName = res.Content.Headers.ContentDisposition?.FileName ?? "unknown.gz";
                var lastModified = res.Content.Headers.LastModified!.Value.UtcDateTime;
                string filePath = Path.Combine(downloadPath, fileName);
                using (var stream = await res.Content.ReadAsStreamAsync())
                {
                    if (!totalToReceive.HasValue)
                    {
                        totalToReceive = stream.Length;
                    }
                    const int step = 1024; // buffer size, progress step set at 1 kb
                    var buffer = new byte[step];
                    using (var sw = File.Create(filePath))
                    {
                        int read = 0;
                        Stopwatch timer = new Stopwatch();
                        timer.Start();
                        do
                        {
                            read = await stream.ReadAsync(buffer, 0, step);
                            totalDownloaded += read;

                            sw.Write(buffer, 0, read);
                            var percent = (double)totalDownloaded / totalToReceive * 100;
                            if (timer.ElapsedMilliseconds > 16.65 || percent == 100) // throttle to maintain FPS at 60 (1000ms/60 = 16.66ms)
                            {
                                Dispatcher.UIThread.Post(() =>
                                {
                                    onUpdateProgress?.Invoke(totalDownloaded, totalToReceive.Value , (float)percent);
                                });
                                timer.Restart();
                            }
                            
                        } while (read > 0);
                        timer.Stop();
                    }
                }

                File.SetLastWriteTimeUtc(filePath, lastModified);

                return filePath;
            }
        }

        public async Task<string> ExtractTarballFile(string filePath, string destinationPath, OnProgress onExtractProgress, OnProgress onInstallProgress)
        {
            Directory.CreateDirectory(destinationPath);
            var extractedFile = Path.Combine(destinationPath, Path.GetFileNameWithoutExtension(filePath));

            using (FileStream source = File.OpenRead(filePath))
            using (ProgressStream progressStream = new ProgressStream(source))
            using (GZipStream unzipped = new GZipStream(progressStream, CompressionMode.Decompress))
            using (FileStream fs = File.Create(extractedFile))
            {
                var total = source.Length;
                var buffer = new byte[1024];
                int read = 0;
                Stopwatch timer = new Stopwatch();
                timer.Start();
                do
                {
                    read = await unzipped.ReadAsync(buffer, 0, 1024);
                    fs.Write(buffer, 0, read);

                    var percent = (double)progressStream.BytesRead / total * 100;
                    if (timer.ElapsedMilliseconds > 16.65 || percent == 100) // throttle to maintain FPS at 60 (1000ms/60 = 16.66ms)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            onExtractProgress?.Invoke(progressStream.BytesRead, total, (float)percent);
                        });
                        timer.Restart();
                    }
                } while (read > 0);
                timer.Stop();
                Dispatcher.UIThread.Post(() =>
                {
                    onExtractProgress?.Invoke(progressStream.BytesRead, total, (float)100f);
                });
            }

            await Task.Delay(100);
            // Installing (.tar expanding)
            using (FileStream fs = File.OpenRead(extractedFile))
            {
                await ExtractTar(fs, destinationPath, onInstallProgress);
            }

            return extractedFile;
        }

        public static async Task ExtractTar(Stream stream, string outputDir, OnProgress? onProgress = null)
        {
            var buffer = new byte[100];
            long total = stream.Length;
            long bytesRead = 0;
            Dispatcher.UIThread.Post(() =>
            {
                onProgress?.Invoke(bytesRead, total, 0);
            });
            Stopwatch timer = new Stopwatch();
            timer.Start();
            while (true)
            {
                stream.Read(buffer, 0, 100);

                var name = Encoding.ASCII.GetString(buffer).Trim('\0', ' ');
                if (string.IsNullOrWhiteSpace(name))
                    break;

                stream.Seek(24, SeekOrigin.Current);
                stream.Read(buffer, 0, 12);

                long size;

                string hex = Encoding.ASCII.GetString(buffer, 0, 12).Trim('\0', ' ');
                try
                {
                    size = Convert.ToInt64(hex, 8);
                }
                catch (Exception ex)
                {
                    throw new Exception("Could not parse hex: " + hex, ex);
                }

                stream.Seek(376L, SeekOrigin.Current);

                var output = Path.Combine(outputDir, name);

                if (size > 0) // ignores directory entries
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(output));

                    int retry = 0;
                    while (true)
                    {
                        try
                        {
                            using (var fs = File.Create(output))
                            {
                                var blob = new byte[size];
                                bytesRead += await stream.ReadAsync(blob, 0, blob.Length);
                                await fs.WriteAsync(blob, 0, blob.Length);

                                var percent = (double)bytesRead / total * 100;
                                if (timer.ElapsedMilliseconds > 16.65 || percent == 100) // throttle to maintain FPS at 60 (1000ms/60 = 16.66ms)
                                {
                                    Dispatcher.UIThread.Post(() =>
                                    {
                                        onProgress?.Invoke(bytesRead, total, (float)percent);
                                    });
                                    timer.Restart();
                                }
                            }

                            break;
                        }
                        catch (Exception e)
                        {
                            Logger.LogError($"read while extract error (attempt: {retry + 1})", e);
                            await Task.Delay(TimeSpan.FromMilliseconds(250));
                            retry++;
                            if (retry > 2)
                            {
                                try
                                {
                                    // try to forcefully close the file
                                    var processName = GetFileProcessName(output);
                                    Console.WriteLine($"process name to kill: {processName}");
                                    if (processName != null)
                                    {
                                        var p = Process.GetProcessesByName(processName).FirstOrDefault();
                                        p?.Kill();
                                    }
                                }
                                catch (Exception killError)
                                {
                                    Logger.LogError("kill process error", killError);
                                }
                            }
                            if (retry > 8)
                            {
                                throw;
                            }
                        }
                    }

                }

                var pos = stream.Position;

                var offset = 512 - (pos % 512);
                if (offset == 512)
                    offset = 0;

                stream.Seek(offset, SeekOrigin.Current);
            }
            timer.Stop();
            Dispatcher.UIThread.Post(() =>
            {
                onProgress?.Invoke(bytesRead, total, (float)100f);
            });
        }


        // ================== Utils =========================
        private static bool CheckVersion(UpdateInfo? lastVersion, string? currentfile)
        {
            if (lastVersion == null) // This mean first time update case.
            {
                return false;
            }
            if (lastVersion != null && !string.IsNullOrWhiteSpace(currentfile))
            {
                var fileInfo = new FileInfo(currentfile);
                var latestVersion = GetVersionFromFileName(currentfile);
                if (latestVersion != null && !string.IsNullOrWhiteSpace(lastVersion.Version)
                    && Version.Parse(lastVersion.Version) < Version.Parse(latestVersion))
                {
                    return false;
                }

                if (lastVersion.Modified < fileInfo.LastWriteTimeUtc)
                {
                    return false;
                }
            }

            return true;
        }

        public static string? GetVersionFromFileName(string filePath)
        {
            var splits = Path.GetFileNameWithoutExtension(filePath).Split('-');
            return splits.Length > 1 ? splits.Last().Replace(".tar", "") : null;
        }

        private static string? GetCurrentFile()
        {
            var currentFile = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.tar.gz")
                .OrderByDescending(x => new FileInfo(x).LastWriteTimeUtc)
                .FirstOrDefault();
            return currentFile;
        }

        public static string GetFileProcessName(string filePath)
        {
            if (OperatingSystem.IsLinux())
            {
                string fileName = Path.GetFileName(filePath);

                return fileName;
            }
            else
            {
                Process[] procs = Process.GetProcesses();
                string fileName = Path.GetFileName(filePath);

                foreach (Process proc in procs)
                {
                    if (proc.MainWindowHandle != new IntPtr(0) && !proc.HasExited)
                    {
                        ProcessModule[] arr = new ProcessModule[proc.Modules.Count];

                        foreach (ProcessModule pm in proc.Modules)
                        {
                            if (pm.ModuleName == fileName)
                                return proc.ProcessName;
                        }
                    }
                }
            }

            return null;
        }
    }
}
