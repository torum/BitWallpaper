using BitWallpaper.Helpers;
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


        private Microsoft.UI.Dispatching.DispatcherQueue _currentDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        public Microsoft.UI.Dispatching.DispatcherQueue CurrentDispatcherQueue { get => _currentDispatcherQueue; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            // TODO: change theme in the setting.
            this.RequestedTheme = ApplicationTheme.Dark;

            // This does not fire...because of winui3 bugs. should be fixed in v1.3 WinAppSDK
            // see https://github.com/microsoft/microsoft-ui-xaml/issues/5221
            UnhandledException += App_UnhandledException;

            // For testing.
            //Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
            //Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "ja-JP";
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // If this is the first instance launched, then register it as the "main" instance.
            // If this isn't the first instance launched, then "main" will already be registered,
            // so retrieve it.
            var mainInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey("main");

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

            // Otherwise, register for activation redirection
            Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().Activated += App_Activated;

            //
            _window = new MainWindow();
            _viewModel = new MainViewModel();
            _shell = new MainShell(_viewModel);
            _window.Content = _shell;

            //TitleBarHelper.UpdateTitleBar(ElementTheme.Default);

            var manager = WinUIEx.WindowManager.Get(_window);
            manager.PersistenceId = "MainWindowPersistanceId";
            manager.MinWidth = 640;
            manager.MinHeight = 480;
            //manager.Backdrop = new WinUIEx.AcrylicSystemBackdrop();
            manager.Backdrop = new WinUIEx.MicaSystemBackdrop();

            _window.Activate();
        }

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

            Debug.WriteLine("App_UnhandledException: " + e.Message + " - " + e.Exception.StackTrace + " - " + e.Exception.Source);

            AppendErrorLog("App_UnhandledException", e.Message);
            AppendErrorLog("StackTrace", e.Exception.StackTrace);
            AppendErrorLog("Source", e.Exception.Source);

            SaveErrorLog();
        }

        public bool IsSaveErrorLog = true;
        public string LogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + "BitWallpaper_errors.txt";
        private StringBuilder Errortxt = new StringBuilder();

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

    }
}
