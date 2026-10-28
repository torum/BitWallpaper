using BitWallpaper.Helpers;
using BitWallpaper.Services;
using BitWallpaper.Services.Contracts;
using BitWallpaper.ViewModels;
using BitWallpaper.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;

namespace BitWallpaper;

public partial class App : Application
{
    public const string AppName = "BitWallpaper2";
    private const string AppDeveloper = "torum";
    private static readonly string _envDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    public static string AppDataFolder { get; private set; } = System.IO.Path.Combine(System.IO.Path.Combine(_envDataFolder, AppDeveloper), AppName);
    public static string AppConfigFilePath { get; private set; } = System.IO.Path.Combine(AppDataFolder, AppName + ".config");

    // ErrorLog
    private static readonly string _logFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + System.IO.Path.DirectorySeparatorChar + "BitWallpaper_errors.txt";
    private readonly StringBuilder _errortxt = new();
    private readonly Lock _errorLogLock = new();

    //public static Window Window { get; private set; } = null!;

    //public static nint WindowHandle => WinRT.Interop.WindowNative.GetWindowHandle(Window);

    public App()
    {
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        if (RuntimeHelper.IsMSIX)
        {
            Debug.WriteLine("IsMSIX");
            var envDataFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            AppDataFolder = System.IO.Path.Combine(System.IO.Path.Combine(envDataFolder, AppDeveloper), AppName);
            //AppConfigFilePath = System.IO.Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, AppName + ".config");
            AppConfigFilePath = System.IO.Path.Combine(AppDataFolder, AppName + ".config"); // Ops. should have been this...
        }
        else
        {
            //Debug.WriteLine("Not IsMSIX");
        }

        InitializeComponent();

        var services = new ServiceCollection();

        // Core Services
        services.AddSingleton<IDispatcherService>(new DispatcherService(DispatcherQueue));
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IBitbankStreamService, BitbankStreamService>();
        services.AddHttpClient("BitWallpaper", client =>
        {
            // Addition to this timeout, also use the per-request cancellation token as the fallback timeout mechanism.
            // Though, not a good idea to set timeout here to avoid disposable race. 
            client.Timeout = TimeSpan.FromSeconds(13);//Timeout.InfiniteTimeSpan;//
            // Preemptively close the connection to avoid IOExceptions caused by remote server silently tears down an idle TLS/socket connection.
            // .NET's internal reading loops encounter expected IOExceptions or SocketExceptions on server side closed idle keep-alive sockets.
            client.DefaultRequestHeaders.ConnectionClose = false;
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("BitWallpaper/1.2");
        }).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            ConnectTimeout = TimeSpan.FromSeconds(8),
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                // Optional: nothing to change here by default; handshake time is bounded by ConnectTimeout.
            }
        });
        services.AddSingleton<IBitbankHttpSearvice, BitbankHttpSearvice>();

        // Views and ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<ShellPage>();

        // Build the provider
        ServiceProvider = services.BuildServiceProvider();

        Microsoft.UI.Xaml.Application.Current.UnhandledException += App_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    public static Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; private set; } = null!;

    public IServiceProvider ServiceProvider { get; private set; } = null!;

    public static T GetService<T>() where T : class
    {
        //return ((App)Current).ServiceProvider.GetRequiredService<T>();
        return (App.Current as App)!.ServiceProvider.GetRequiredService<T>();
    }

#if DEBUG
    public bool IsSaveErrorLog { get; set; } = true;
#else
    public bool IsSaveErrorLog { get; set; }
#endif

    protected async override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        try
        {
            base.OnLaunched(args);

            // Single instance.
            // https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/migrate-to-windows-app-sdk/guides/applifecycle
            var mainInstance = Microsoft.Windows.AppLifecycle.AppInstance.FindOrRegisterForKey("BitWallpaperMain");
            // If the instance that's executing the OnLaunched handler right now
            // isn't the "main" instance.
            if (!mainInstance.IsCurrent)
            {
                // Redirect the activation (and args) to the "main" instance, and exit.
                var activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().GetActivatedEventArgs();
                await mainInstance.RedirectActivationToAsync(activatedEventArgs);

                //System.Diagnostics.Process.GetCurrentProcess().Kill();
                Environment.Exit(0);
                return;
            }
            else
            {
                // Otherwise, register for activation redirection
                Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent().Activated += App_Activated;
            }

            var configurationService = ServiceProvider.GetRequiredService<IConfigurationService>();
            configurationService.Initialize(AppConfigFilePath);
            await configurationService.LoadConfigurationAsync();

            var main = App.GetService<MainWindow>();
            main.Activate();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OnLaunched: {ex}");
            AppendErrorLog("OnLaunched", ex.ToString());
            SaveErrorLog();

            // Startup failed and the app is in an unknown state, so exit instead of leaving a half-initialized process.
            Environment.Exit(1);
        }
    }

    private static void App_Activated(object? sender, Microsoft.Windows.AppLifecycle.AppActivationArguments e)
    {
        DispatcherQueue?.TryEnqueue(() =>
        {
            var main = App.GetService<Views.MainWindow>();

            main.Activate();

        });
    }


    #region == UnhandledException ==

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
        // "A notable limitation is that the UnhandledException event arguments don’t contain as much detail as the original exception as propagated from app code."

        // This does not fire...because of winui3 bugs.
        // see https://github.com/microsoft/microsoft-ui-xaml/issues/5221

        // This kills app... 
        // https://github.com/microsoft/microsoft-ui-xaml/issues/10447


        // e.Exception can lose detail when it crosses the WinRT boundary,
        // but ToString() still gives the best available type, message, HResult and stack trace.
        var details = $"{e.Exception}{Environment.NewLine}HResult: 0x{e.Exception.HResult:X8}{Environment.NewLine}Message: {e.Message}";

        Debug.WriteLine($"App_UnhandledException: {details}");
        AppendErrorLog("App_UnhandledException", details);
        SaveErrorLog();

        // Mark as handled only for exceptions that are known to be recoverable.
        // Leaving it unhandled lets the app terminate, which is safer for unknown states.
        if (e.Exception is OperationCanceledException)
        {
            e.Handled = true;
        }
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        // Flatten so nested AggregateExceptions are unwrapped, and log full details (type, message, stack trace).
        var details = e.Exception.Flatten().ToString();

        Debug.WriteLine($"TaskScheduler_UnobservedTaskException: {details}");
        AppendErrorLog("TaskScheduler_UnobservedTaskException", details);
        SaveErrorLog();

        e.SetObserved();
    }

    private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is not Exception exception)
        {
            var raw = e.ExceptionObject?.ToString() ?? "Unknown exception object";
            Debug.WriteLine($"CurrentDomain_UnhandledException: {raw}");
            AppendErrorLog("CurrentDomain_UnhandledException", raw);
            SaveErrorLog();
            return;
        }

        if (exception is TaskCanceledException)
        {
            // can ignore.
            Debug.WriteLine($"CurrentDomain_UnhandledException (TaskCanceledException): {exception.Message}");
            return;
        }

        // Log full details (type, message, stack trace, inner exceptions).
        var details = exception.ToString();

        Debug.WriteLine($"CurrentDomain_UnhandledException (IsTerminating={e.IsTerminating}): {details}");
        AppendErrorLog("CurrentDomain_UnhandledException", details);
        SaveErrorLog();
    }

    public void AppendErrorLog(string kindTxt, string errorTxt)
    {
        if (!IsSaveErrorLog)
        {
            return;
        }
        var timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
        lock (_errorLogLock)
        {
            _errortxt.AppendLine(kindTxt + ": " + errorTxt);
            _errortxt.AppendLine(CultureInfo.InvariantCulture, $"Occurred at {timestamp}");
        }
    }

    public void SaveErrorLog()
    {
        if (!IsSaveErrorLog)
        {
            return;
        }

        if (string.IsNullOrEmpty(_logFilePath))
        {
            return;
        }

        var timestamp = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);

        lock (_errorLogLock)
        {
            if (_errortxt.Length == 0)
            {
                return;
            }

            _errortxt.AppendLine(CultureInfo.InvariantCulture, $"Saved at {timestamp}");
            _errortxt.AppendLine();

            try
            {
                File.WriteAllText(_logFilePath, _errortxt.ToString());
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // The error logger must never throw from inside an exception handler.
                Debug.WriteLine($"SaveErrorLog failed: {ex}");
            }
        }
    }

    #endregion
}
