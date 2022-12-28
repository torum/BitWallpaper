using BitWallpaper4.Helpers;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BitWallpaper4
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window _window;
        public Window MainWindow => _window;

        private Microsoft.UI.Dispatching.DispatcherQueue _currentDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        public Microsoft.UI.Dispatching.DispatcherQueue CurrentDispatcherQueue { get => _currentDispatcherQueue; }

        /*
        // Prevent multiple instances.
        private bool _mutexOn = true;

        /// <summary>The event mutex name.</summary>
        private const string UniqueEventName = "{50a649c0-84a3-42d8-a161-6908a4c8d3bd}";

        /// <summary>The unique mutex name.</summary>
        private const string UniqueMutexName = "{a97f0455-5e9e-4b8f-82fd-157d20248d6a}";

        /// <summary>The event wait handle.</summary>
        private EventWaitHandle eventWaitHandle;

        /// <summary>The mutex.</summary>
        private Mutex mutex;
        */

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            this.RequestedTheme = ApplicationTheme.Dark;


            /*
            if (_mutexOn)
            {
                this.mutex = new Mutex(true, UniqueMutexName, out bool isOwned);
                this.eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

                // So, R# would not give a warning that this variable is not used.
                GC.KeepAlive(this.mutex);

                if (isOwned)
                {
                    // Spawn a thread which will be waiting for our event
                    var thread = new Thread(
                        () =>
                        {
                            while (this.eventWaitHandle.WaitOne())
                            {
                                //Current.Dispatcher.BeginInvoke((Action)(() => ((MainWindow)Current.MainWindow).BringToForeground()));
                            }
                        });

                    // It is important mark it as background otherwise it will prevent app from exiting.
                    thread.IsBackground = true;

                    thread.Start();
                    return;
                }

                // Notify other instance so it could bring itself to foreground.
                this.eventWaitHandle.Set();

                // Terminate this instance.
                //this.Shutdown();

                App.Current.Exit();
            }
            */

            // This does not fire...because of winui3 bugs. should be fixed in v1.3 WinAppSDK
            // see https://github.com/microsoft/microsoft-ui-xaml/issues/5221
            UnhandledException += App_UnhandledException;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();

            TitleBarHelper.UpdateTitleBar(ElementTheme.Default);
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

        private StringBuilder Errortxt = new StringBuilder();
        public bool IsSaveErrorLog = true;
        public string LogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + "BitWallpaper_errors.txt";

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
