using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Updater.Properties;

namespace Updater.Services
{
    public class SettingService
    {
        public void SetByArgs(string[] args)
        {
            string? key = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (i == 0)
                {
                    if (!IsSettingCommand(args[i]))
                    {
                        throw new Exception("Invalid arg format.");
                    }
                    continue;
                }
                var arg = args[i];
                var split = arg.Split("=");
                if (split.Length > 1)
                {
                    Set(split[0], split[1]);
                }
                else
                {
                    if (key == null)
                    {
                        key = arg;
                        continue;
                    }
                    else
                    {
                        Set(key, arg);
                        key = null;
                    }
                }
            }

            if (key != null)
            {
                throw new Exception("Invalid arg format.");
            }
        }

        public void Set(string key, string value)
        {
            object converted = value;
            if (key == nameof(Settings.Default.UpdateServer))
            {
                converted = string.IsNullOrWhiteSpace(value) ? "" : (Regex.IsMatch(value, @"^https?://") ? value : "http://" + value);
            }
            else if (key == nameof(Settings.Default.AutoReboot) || key == nameof(Settings.Default.ProgressFullscreen))
            {
                converted = Convert.ToBoolean(value);
            }
            Settings.Default[key] = converted;
        }

        public bool IsSettingCommand(string arg)
        {
            var normalized = arg.Replace("-", "").ToLower();
            return normalized == "set" || normalized == "s";
        }
    }
}
