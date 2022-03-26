using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckIn.Utils
{
    public static class BashHelper
    {
        public static List<string> Cmd(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            ProcessStartInfo startInfo = new ProcessStartInfo() 
            { 
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            };
            Process proc = new Process() { StartInfo = startInfo, };
            proc.ErrorDataReceived += Proc_ErrorDataReceived;
            proc.Start();
            var result = new List<string>();
            while (!proc.StandardOutput.EndOfStream)
            {
                string? line = proc.StandardOutput.ReadLine();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    result.Add(line);
                }
            }
            
            proc.WaitForExit();

            return result;
        }

        private static void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            throw new Exception(e.Data);
        }
    }
}
