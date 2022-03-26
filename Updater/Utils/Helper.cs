using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Updater.Utils
{
    public static class Helper
    {
        public static readonly string[] SizeSuffixes =
                   { "bytes", "kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string SizeSuffix(Int64 value, int decimalPlaces = 1)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }
            if (value == 0) { return string.Format("{0:n" + decimalPlaces + "} bytes", 0); }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }

        public static void SetAlwaysOnTop(this Window window)
        {
            var platformImpl = window.PlatformImpl;
            var handle = platformImpl.Handle.Handle;
            if (OperatingSystem.IsLinux())
            {
                // Trying to handle always on top on linux here, but can't make it work
                // If you know how, then please contribute
                try
                {
                    //var display = Xlib.XOpenDisplay(null);
                    ////var root = Xlib.XDefaultRootWindow(display); 
                    //Xlib.XGetWindowAttributes(display, (X11.Window)handle, out var attr);
                    //var root = attr.root;
                    //if (root == X11.Window.None)
                    //{
                    //    root = Xlib.XDefaultRootWindow(display);
                    //}

                    //Atom stateAbove = Xmu.XmuInternAtom(display, Xmu.XmuMakeAtom("_NET_WM_STATE_ABOVE"));
                    //if (stateAbove == Atom.None)
                    //{
                    //    Console.Error.WriteLine("state above is null");
                    //    return;
                    //}
                    //Atom netState = Xmu.XmuInternAtom(display, Xmu.XmuMakeAtom("_NET_WM_STATE"));
                    //if (netState == Atom.None)
                    //{
                    //    Console.Error.WriteLine("net state is null");
                    //    return;
                    //}
                    //XClientMessageEvent evOnTop = new XClientMessageEvent
                    //{
                    //    type = (int)Event.ClientMessage,
                    //    message_type = netState,
                    //    format = 32,
                    //    window = (X11.Window)handle,
                    //    display = display,
                    //};

                    //var data = new ClientMessageData { l = new int[] { 1, (int)stateAbove, 0, 0, 1 } };
                    ////var data = new int[] { 1, (int)stateAbove, 0, 0, 1 };
                    ////var dataHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    //int size = Marshal.SizeOf(data.l[0] * data.l.Length);
                    //var dataP = Marshal.AllocHGlobal(size);
                    ////Marshal.Copy(data.l, 0, dataP, size);
                    //Marshal.StructureToPtr(data.l, dataP, false);
                    //evOnTop.data = dataP;

                    //var eventP = Marshal.AllocHGlobal(Marshal.SizeOf(evOnTop));
                    //Marshal.StructureToPtr(evOnTop, eventP, false);
                    //Xlib.XSendEvent(display, root, true, (long)(EventMask.SubstructureNotifyMask | EventMask.SubstructureRedirectMask), eventP);

                    //Xlib.XFlush(display);

                    //Marshal.FreeHGlobal(dataP);
                    //Marshal.FreeHGlobal(eventP);

                }
                catch (Exception error)
                {
                    Console.Error.WriteLine(error);
                    throw;
                }
            }
            // Workaround to try to simulate always on top behavior
            var cancelSource = new CancellationTokenSource();
            Task.Run(async () =>
            {
                if (cancelSource.IsCancellationRequested)
                {
                    return;
                }
                await Task.Delay(300);
                if (window != null)
                {
                    Dispatcher.UIThread.Post(() =>
                    { 
                        window.BringIntoView();
                        window.Topmost = true; 
                    });
                }
            }, cancelSource.Token);
            window.Closed += (sender, e) => cancelSource.Cancel();
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ClientMessageData
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 20)]
        public char[] b;
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 10)]
        public short[] s;
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.LPArray, SizeConst = 5)]
        public int[] l;
    }
}
