using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using Updater.Services;
using Updater.ViewModels;
using Updater.Views;

namespace Updater
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Startup += Desktop_Startup;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private async void Desktop_Startup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
        {
            string[] commanlineArgs = e.Args;

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (commanlineArgs.Length > 0)
                {
                    if (commanlineArgs.Any(x => x == "check" || x == "c"))
                    {
                        var service = new UpdateService();
                        bool upToDate;
                        try
                        {
                            upToDate = await service.CheckUpdate();
                        }
                        catch (Exception error)
                        {
                            Console.WriteLine($"error checking: {error.Message}");
                            throw;
                        }

                        // 1st output : Detect output as bool in target app (the calling app)
                        Console.Out.WriteLine(upToDate);

                        if (!upToDate)
                        {
                            desktop.MainWindow = new UpdateAvailableWindow
                            {
                                DataContext = new UpdateAvailableViewModel()
                            };
                            desktop.MainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                            desktop.MainWindow.Topmost = true;
                            desktop.MainWindow.Show();

                            if (OperatingSystem.IsLinux())
                            {
                                // Trying to handle always on top on linux here, but can't make it work
                                // If you know how, then please contribute
                                try
                                {
                                    //var platformImpl = desktop.MainWindow.PlatformImpl;
                                    //var handle = platformImpl.Handle.Handle;
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
                        }
                        else
                        {
                            desktop.Shutdown();
                        }
                    }
                }
                else
                {
                    desktop.MainWindow = new MainWindow
                    {
                        DataContext = new MainWindowViewModel(),
                    };
                }
            }
        }
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
