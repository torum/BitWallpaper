// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BitWallpaper4.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Windows;
using System.Threading;
using ABI.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BitWallpaper4
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {

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

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {

            m_window = new MainWindow();

            TitleBarHelper.UpdateTitleBar(ElementTheme.Default);

            UnhandledException += App_UnhandledException;

            m_window.Activate();
        }

        private Window m_window;

        public Window MainWindow => m_window;

        private Microsoft.UI.Dispatching.DispatcherQueue _currentDispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        public Microsoft.UI.Dispatching.DispatcherQueue CurrentDispatcherQueue { get=> _currentDispatcherQueue; }


        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // TODO: Log and handle exceptions as appropriate.
            // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.

            System.Diagnostics.Debug.WriteLine("App_UnhandledException: " + e.Message);

            AppendErrorLog("App_DispatcherUnhandledException", e.Message);

            SaveErrorLog();
        }

        private StringBuilder Errortxt = new StringBuilder();
        public bool IsSaveErrorLog = true;
        public string LogFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + "BitWallpaper_errors.txt";

        public void AppendErrorLog(string errorTxt, string kindTxt)
        {
            DateTime dt = DateTime.Now;
            string nowString = dt.ToString("yyyy/MM/dd HH:mm:ss");

            Errortxt.AppendLine(nowString + " - " + kindTxt + " - " + errorTxt);
        }

        public void SaveErrorLog()
        {
            if (!IsSaveErrorLog)
                return;

            if (string.IsNullOrEmpty(LogFilePath))
                return;

            string s = Errortxt.ToString();
            if (!string.IsNullOrEmpty(s))
                File.WriteAllText(LogFilePath, s);
        }

    }
}
