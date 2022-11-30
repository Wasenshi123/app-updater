using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Utils
{
    public static class Logger
    {

        public const string error_log_filename = "error.log";
        public static void LogError(string msg, Exception? e = null)
        {
            var path = CheckAndReturnFile(error_log_filename);

            string errorTxt = $"{DateTimeOffset.Now}: {msg} || {e?.Message ?? "-"}";
            if (e?.InnerException != null)
            {
                errorTxt += $" || {e.InnerException}";
            }
            if (!string.IsNullOrWhiteSpace(e?.StackTrace))
            {
                errorTxt += $"\n{e?.StackTrace}";
            }
            errorTxt += "\n\n";

            File.AppendAllText(path, errorTxt);
        }

        private static string CheckAndReturnFile(string filename, int increment = 0)
        {
            string file = filename;
            if (increment > 0)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var ex = Path.GetExtension(file);
                file = $"{name}-{increment}{ex}";
            }

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var path = Path.Combine(basePath, file);
            if (File.Exists(path))
            {
                if (new FileInfo(path).Length >= 1024 * 1024 * 2)
                {
                    return CheckAndReturnFile(filename, increment + 1);
                }
            }

            return path;
        }
    }
}
