using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Updater.Properties;

namespace Updater.Services
{
    public class UpdateService
    {
        public async Task<bool> CheckUpdate()
        {
            var server = Settings.Default.UpdateServer;

            return false;
        }
    }
}
