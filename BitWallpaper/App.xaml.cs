﻿using BitWallpaper.Helpers;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Windows.AppLifecycle;
using System.ComponentModel;
using BitWallpaper.Views;
using Microsoft.UI.Xaml.Controls;
using BitWallpaper.ViewModels;
using System.Threading;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Markup;
using System.Threading.Tasks;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using Microsoft.Windows.ApplicationModel.Resources;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Collections.Immutable;
using System.Text.Json.Nodes;

namespace BitWallpaper
{
    public partial class App : Application
    {
        private Window _window;
        public Window MainWindow => _window;

        private MainViewModel _viewModel;
        public MainViewModel ViewModel => _viewModel;

        private MainShell _shell;
        public MainShell Shell => _shell;

        private static readonly Microsoft.UI.Dispatching.DispatcherQueue _currentDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        public static Microsoft.UI.Dispatching.DispatcherQueue CurrentDispatcherQueue => _currentDispatcherQueue;

        private static readonly string _appName = "BitWallpaper";//_resourceLoader.GetString("AppName");
        private static readonly string _appDeveloper = "torum";
        private static readonly string _envDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        public static string AppDataFolder { get; } = _envDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;
        public static string AppConfigFilePath { get; } = AppDataFolder + System.IO.Path.DirectorySeparatorChar + _appName + ".config";

        private static readonly ResourceLoader _resourceLoader = new();

        //private static SynchronizationContext _theSynchronizationContext = SynchronizationContext.Current;
        //public SynchronizationContext TheSynchronizationContext { get => _theSynchronizationContext; }

        public bool IsSaveErrorLog = true;
        public string LogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + "BitWallpaper_errors.txt";
        private readonly StringBuilder Errortxt = new();

        private readonly BitWallpaper.Helpers.NotificationManager notificationManager = new BitWallpaper.Helpers.NotificationManager();

        public App()
        {
            try
            {
                InitializeComponent();

                //this.RequestedTheme = ApplicationTheme.Dark;
                //this.RequestedTheme = ApplicationTheme.Light;

                // For testing.
                //Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
                //Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "ja-JP";

            }
            catch (XamlParseException parseException)
            {
                Debug.WriteLine($"Unhandled XamlParseException in App: {parseException.Message}");
                foreach (var key in parseException.Data.Keys)
                {
                    Debug.WriteLine("{Key}:{Value}", key.ToString(), parseException.Data[key]?.ToString());
                }
                throw;
            }
            catch(Exception ex) 
            {
                Debug.WriteLine("Exception at App(): ", ex);

                AppendErrorLog($"Exception at App()", ex.Message);
                SaveErrorLog();
                throw;
            }

            // This does not fire...because of winui3 bugs. should be fixed in v1.3 WinAppSDK
            // see https://github.com/microsoft/microsoft-ui-xaml/issues/5221
            Microsoft.UI.Xaml.Application.Current.UnhandledException += App_UnhandledException;

            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;


        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            /*
             * https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/applifecycle
            // If this is the first instance launched, then register it as the "main" instance.
            // If this isn't the first instance launched, then "main" will already be registered,
            // so retrieve it.
            var mainInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey("BitWallPaperMain");

            // If the instance that's executing the OnLaunched handler right now
            // isn't the "main" instance.
            if (!mainInstance.IsCurrent)
            {
                // Redirect the activation (and args) to the "main" instance, and exit.
                var activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
                await mainInstance.RedirectActivationToAsync(activatedEventArgs);

                System.Diagnostics.Process.GetCurrentProcess().Kill();
                return;
            }
            else
            {
                // Otherwise, register for activation redirection
                Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().Activated += App_Activated;
            }
            */
            //
            if (!RuntimeHelper.IsMSIX)
            {
                WinUIEx.WindowManager.PersistenceStorage = new FilePersistence(Path.Combine(AppDataFolder, "WinUIExPersistence.json"));
            }

            _window = new MainWindow();
            _viewModel = new MainViewModel();
            _shell = new MainShell(_viewModel);
            _window.Content = _shell;

            // TODO: change theme in the setting.
            TitleBarHelper.UpdateTitleBar(ElementTheme.Default);

            /*
            var manager = WinUIEx.WindowManager.Get(_window);
            // https://stackoverflow.com/questions/74879865/invalidoperationexception-when-closing-a-windowex-window-in-winui-3
            //manager.PersistenceId = "MainWindowPersistanceId";
            manager.MinWidth = 640;
            manager.MinHeight = 480;
            //manager.Backdrop = new WinUIEx.AcrylicSystemBackdrop();
            manager.Backdrop = new WinUIEx.MicaSystemBackdrop();
            */
            //

            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            notificationManager.Init();
            _viewModel.ShowBalloon += (sender, arg) => { ShowBalloon(arg); };

            MainWindow.Activate();
        }

        private void ShowBalloon(ShowBalloonEventArgs arg)
        {

            // https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/notifications/app-notifications/app-notifications-quickstart?tabs=cs
            var appNotification = new AppNotificationBuilder().AddText(arg.Title).AddText(arg.Text).SetTimeStamp(new DateTime(2017, 04, 15, 19, 45, 00, DateTimeKind.Utc)).BuildNotification();
            
            //.AddButton(new AppNotificationButton("OK")
            //.AddArgument("action", "dissmiss"))
            //.SetTimeStamp(new DateTime(2017, 04, 15, 19, 45, 00, DateTimeKind.Utc));

            //.SetScenario(AppNotificationScenario.Alarm)
            AppNotificationManager.Default.Show(appNotification);
        }

        protected void OnProcessExit(object sender, EventArgs e)
        {
            notificationManager.Unregister();
        }

        // Activated from other instance.
        private void App_Activated(object sender, Microsoft.Windows.AppLifecycle.AppActivationArguments e)
        {
            CurrentDispatcherQueue?.TryEnqueue(() =>
            {
                _window.Activate(); 
                //_window.BringToFront();

                // TODO: need .BringToForeground()
                //_window.SetIsAlwaysOnTop(true);
                //_window.SetIsAlwaysOnTop(false);

            });

            /*
            // Bring the window to the foreground... first get the window handle...
            var hwnd = (Windows.Win32.Foundation.HWND)WinRT.Interop.WindowNative.GetWindowHandle(m_window);

            // Restore window if minimized... requires Microsoft.Windows.CsWin32 NuGet package and a NativeMethods.txt file with ShowWindow method
            Windows.Win32.PInvoke.ShowWindow(hwnd, Windows.Win32.UI.WindowsAndMessaging.SHOW_WINDOW_CMD.SW_RESTORE);

            // And call SetForegroundWindow... requires Microsoft.Windows.CsWin32 NuGet package and a NativeMethods.txt file with SetForegroundWindow method
            Windows.Win32.PInvoke.SetForegroundWindow(hwnd);
            */
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // This does not fire...because of winui3 bugs. should be fixed in v1.3 WinAppSDK
            // see https://github.com/microsoft/microsoft-ui-xaml/issues/5221

            Debug.WriteLine("App_UnhandledException", e.Message + $"StackTrace: {e.Exception.StackTrace}, Source: {e.Exception.Source}");
            AppendErrorLog("App_UnhandledException", e.Message + $"StackTrace: {e.Exception.StackTrace}, Source: {e.Exception.Source}");

            try
            {
                SaveErrorLog();
            }catch(Exception) { }
        }
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var exception = e.Exception.InnerException as Exception;

            Debug.WriteLine("TaskScheduler_UnobservedTaskException: " + exception.Message);
            AppendErrorLog("TaskScheduler_UnobservedTaskException", exception.Message);
            SaveErrorLog();

            e.SetObserved();
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (exception is TaskCanceledException)
            {
                // can ignore.
                Debug.WriteLine("CurrentDomain_UnhandledException (TaskCanceledException): " + exception.Message);
                AppendErrorLog("CurrentDomain_UnhandledException (TaskCanceledException)", exception.Message);
            }
            else
            {
                Debug.WriteLine("CurrentDomain_UnhandledException: " + exception.Message);
                AppendErrorLog("CurrentDomain_UnhandledException", exception.Message);
                SaveErrorLog();
            }
        }

        public void AppendErrorLog(string kindTxt, string errorTxt)
        {
            Errortxt.AppendLine(kindTxt + ": " + errorTxt);
            DateTime dt = DateTime.Now;
            Errortxt.AppendLine($"Occured at {dt.ToString("yyyy/MM/dd HH:mm:ss")}");
            Errortxt.AppendLine("");
        }

        private void SaveErrorLog()
        {
            if (!IsSaveErrorLog)
                return;

            if (string.IsNullOrEmpty(LogFilePath))
                return;

            Errortxt.AppendLine("");
            DateTime dt = DateTime.Now;
            Errortxt.AppendLine($"Saved at {dt.ToString("yyyy/MM/dd HH:mm:ss")}");

            string s = Errortxt.ToString();
            if (!string.IsNullOrEmpty(s))
                File.WriteAllText(LogFilePath, s);
        }

        public void SaveErrorLogIfAny()
        {
            if (Errortxt.Length > 0)
            {
                SaveErrorLog();
            }
        }

        // for the WinUIEx, unpackaged.
        private class FilePersistence : IDictionary<string, object>
        {
            private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
            private readonly string _file;

            public FilePersistence(string filename)
            {
                _file = filename;
                try
                {
                    if (File.Exists(filename))
                    {
                        var jo = System.Text.Json.Nodes.JsonObject.Parse(File.ReadAllText(filename)) as JsonObject;
                        foreach (var node in jo)
                        {
                            if (node.Value is JsonValue jvalue && jvalue.TryGetValue<string>(out string value))
                                _data[node.Key] = value;
                        }
                    }
                }
                catch { }
            }
            private void Save()
            {
                JsonObject jo = new JsonObject();
                foreach (var item in _data)
                {
                    if (item.Value is string s) // In this case we only need string support. TODO: Support other types
                        jo.Add(item.Key, s);
                }
                File.WriteAllText(_file, jo.ToJsonString());
            }
            public object this[string key] { get => _data[key]; set { _data[key] = value; Save(); } }

            public ICollection<string> Keys => _data.Keys;

            public ICollection<object> Values => _data.Values;

            public int Count => _data.Count;

            public bool IsReadOnly => false;

            public void Add(string key, object value)
            {
                _data.Add(key, value); Save();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                _data.Add(item.Key, item.Value); Save();
            }

            public void Clear()
            {
                _data.Clear(); Save();
            }

            public bool Contains(KeyValuePair<string, object> item) => _data.Contains(item);

            public bool ContainsKey(string key) => _data.ContainsKey(key);

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotImplementedException(); // TODO

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => throw new NotImplementedException(); // TODO

            public bool Remove(string key) => throw new NotImplementedException(); // TODO

            public bool Remove(KeyValuePair<string, object> item) => throw new NotImplementedException(); // TODO

            public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => throw new NotImplementedException(); // TODO

            IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException(); // TODO
        }
    }
}
