using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater.Utils
{
    public static class WindowHelper
    {
        /// <summary>
        /// Fix center start position not working on Linux. 
        /// </summary>
        public static void SetWindowStartupLocationWorkaround(this Window window)
        {
            if (OperatingSystem.IsWindows())
            {
                // Not needed for Windows
                return;
            }

            var scale = window.PlatformImpl?.DesktopScaling ?? 1.0;
            var pOwner = window.Owner?.PlatformImpl;
            if (pOwner != null)
            {
                scale = pOwner.DesktopScaling;
            }
            var rect = new PixelRect(PixelPoint.Origin,
                PixelSize.FromSize(window.ClientSize, scale));
            if (window.WindowStartupLocation == WindowStartupLocation.CenterScreen)
            {
                var screen = window.Screens.ScreenFromPoint(pOwner?.Position ?? window.Position);
                if (screen == null)
                {
                    return;
                }
                window.Position = screen.WorkingArea.CenterRect(rect).Position;
            }
            else
            {
                if (pOwner == null ||
                    window.WindowStartupLocation != WindowStartupLocation.CenterOwner)
                {
                    return;
                }
                window.Position = new PixelRect(pOwner.Position,
                    PixelSize.FromSize(pOwner.ClientSize, scale)).CenterRect(rect).Position;
            }
        }
    }
}
