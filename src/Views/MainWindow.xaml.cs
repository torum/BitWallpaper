using BitWallpaper.Models;
using BitWallpaper.Services;
using BitWallpaper.Services.Contracts;
using BitWallpaper.ViewModels;
using LiveChartsCore.Themes;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WinRT;

namespace BitWallpaper.Views;

public sealed partial class MainWindow : Window
{
    private readonly ShellPage _shell;
    private readonly IConfigurationService _configurationService;
    private readonly IBitbankStreamService _bitbankStreamService;
    private readonly MainViewModel _vm;
    private int _winRestoreWidth = 1024;
    private int _winRestoreHeight = 768;
    private int _winRestoreTop = 100;
    private int _winRestoreLeft = 100;
    private ElementTheme _theme = ElementTheme.Default;
    private bool _isClosing;

    [DynamicWindowsRuntimeCast(typeof(OverlappedPresenter))]
    public MainWindow(IConfigurationService configurationService, IBitbankStreamService bitbankStreamService)
    {
        _vm = App.GetService<MainViewModel>();
        _shell = App.GetService<ShellPage>();
        _configurationService = configurationService;
        _bitbankStreamService = bitbankStreamService;

        InitializeComponent();

        AppWindow.SetIcon("Assets/BitWallpaper.ico");
        ExtendsContentIntoTitleBar = true;

        this.Content = _shell;

        LoadSettings();

        _shell.RequestedTheme = _theme;

        //// Let's not set caption button color here, because it will affect normal dim when activate/deactivate. Only set caption button color when theme is changed.
        SetCapitionButtonColor();

        _shell.Loaded += Shell_Loaded;

        this.AppWindow.Closing += AppWindow_Closing;
        this.Closed += Window_Closed;
        this.SizeChanged += Window_SizeChanged;
        this.AppWindow.Changed += AppWindow_Changed;
        this.Activated += Window_Activated;

        if (this.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.PreferredMinimumWidth = 778;
            presenter.PreferredMinimumHeight = 600;
        }
    }

    private void Shell_Loaded(object sender, RoutedEventArgs e)
    {
        _shell.Loaded -= Shell_Loaded;

        _shell.Initialize(this);

    }

    [DynamicWindowsRuntimeCast(typeof(OverlappedPresenter))]
    [DynamicWindowsRuntimeCast(typeof(FrameworkElement))]
    private void LoadSettings()
    {
        if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
        {
            _vm.IsAcrylicSupported = true;

            _vm.IsBackdropSupported = true;
        }
        if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
        {
            _vm.IsMicaSupported = true;

            _vm.IsBackdropSupported = true;
        }

        if (!System.IO.File.Exists(_configurationService.ConfigFilePath))
        {
            // Sets default.
            if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
            {
                _vm.IsAcrylicSupported = true;
                SystemBackdrop = new DesktopAcrylicBackdrop();
                _vm.Material = SystemBackdropOption.Acrylic;

                _vm.IsBackdropSupported = true;
            }
            else if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
            {
                _vm.IsMicaSupported = true;
                SystemBackdrop = new MicaBackdrop()
                {
                    Kind = MicaKind.Base
                };
                _vm.Material = SystemBackdropOption.Mica;

                _vm.IsBackdropSupported = true;
            }

            _theme = ElementTheme.Default;

            return;
        }

        //OverlappedPresenterState? winState = null; // For AOT workaround, don't set default, = OverlappedPresenterState.Restored;

        var config = _configurationService.CurrentConfiguration;

        //Debug.WriteLine($"MainWindow Configuration: Width={config.MainWindow.Width}, Height={config.MainWindow.Height}, Top={config.MainWindow.Top}, Left={config.MainWindow.Left}, RestoreWidth={config.MainWindow.RestoreWidth}, RestoreHeight={config.MainWindow.RestoreHeight}, RestoreTop={config.MainWindow.RestoreTop}, RestoreLeft={config.MainWindow.RestoreLeft}");

        if (config.MainWindow.Width < 500)
        {
            config.MainWindow.Width = 500;
        }
        if (config.MainWindow.Height < 500)
        {
            config.MainWindow.Height = 500;
        }
        if (config.MainWindow.Top < 0)
        {
            config.MainWindow.Top = 0;
        }
        if (config.MainWindow.Left < 0)
        {
            config.MainWindow.Left = 0;
        }

        if (config.MainWindow.RestoreWidth < 500)
        {
            config.MainWindow.RestoreWidth = 500;
        }
        if (config.MainWindow.RestoreHeight < 500)
        {
            config.MainWindow.RestoreHeight = 500;
        }
        if (config.MainWindow.RestoreTop < 0)
        {
            config.MainWindow.RestoreTop = 0;
        }
        if (config.MainWindow.RestoreLeft < 0)
        {
            config.MainWindow.RestoreLeft = 0;
        }

        // Restore window size and position
        Microsoft.UI.Windowing.AppWindow? appWindow = this.AppWindow;
        if (appWindow != null)
        {
            // AOT bad
            /*
            // https://github.com/microsoft/CsWinRT/issues/1930
            // Window state
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
            }
            */

            if (config.MainWindow.WindowState == OverlappedPresenterState.Maximized)
            {
                // Sets restore size and position.
                appWindow.MoveAndResize(new Windows.Graphics.RectInt32(config.MainWindow.Left, config.MainWindow.Top, config.MainWindow.Width, config.MainWindow.Height));
                // Maximize the window.
                (appWindow.Presenter as OverlappedPresenter)!.Maximize();
            }
            else if (config.MainWindow.WindowState == OverlappedPresenterState.Minimized)
            {
                // This should not happen.

                // DONT restore minimized size and pos. bad.

                (appWindow.Presenter as OverlappedPresenter)!.Restore();

                appWindow.MoveAndResize(new Windows.Graphics.RectInt32(config.MainWindow.Left, config.MainWindow.Top, config.MainWindow.Width, config.MainWindow.Height));
            }
            else if (config.MainWindow.WindowState == OverlappedPresenterState.Restored)
            {
                // TODO:
                //var dpi = Windows.Win32.PInvoke.GetDpiForWindow(new Windows.Win32.Foundation.HWND(WinRT.Interop.WindowNative.GetWindowHandle(this)));
                //var scalingFactor = (float)dpi / 96;
                //AppWindow.Resize(new Windows.Graphics.SizeInt32((int)(400.0f * scalingFactor), (int)(300.0f * scalingFactor)));

                // Sets restore size and position.
                appWindow.MoveAndResize(new Windows.Graphics.RectInt32(config.MainWindow.Left, config.MainWindow.Top, config.MainWindow.Width, config.MainWindow.Height));
            }
        }

        // Apply theme and backdrop
        if (config.SystemBackdrop != SystemBackdropOption.None)
        {
            _theme = config.Theme;
            _vm.Theme = config.Theme;
        }
        _vm.Material = config.SystemBackdrop;
        SwitchBackdrop(config.SystemBackdrop);
    }

    public void SwitchBackdrop(SystemBackdropOption backdrop)
    {
        //var vm = App.GetService<MainViewModel>();

        if (backdrop == SystemBackdropOption.Mica)
        {
            if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
            {
                this.SystemBackdrop = new MicaBackdrop()
                {
                    Kind = MicaKind.Base
                };
                _vm.Material = SystemBackdropOption.Mica;

                _vm.IsBackdropSupported = true;
            }
        }
        else if (backdrop == SystemBackdropOption.MicaAlt)
        {
            if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
            {
                this.SystemBackdrop = new MicaBackdrop()
                {
                    Kind = MicaKind.BaseAlt
                };
                _vm.Material = SystemBackdropOption.MicaAlt;

                _vm.IsBackdropSupported = true;
            }
        }
        else if (backdrop == SystemBackdropOption.Acrylic)
        {
            if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
            {
                this.SystemBackdrop = new DesktopAcrylicBackdrop();

                _vm.Material = SystemBackdropOption.Acrylic;

                _vm.IsBackdropSupported = true;
            }
        }
        else if (backdrop == SystemBackdropOption.None)
        {
            this.SystemBackdrop = null;

            _vm.Material = SystemBackdropOption.None;
            _vm.IsBackdropSupported = false;
            _vm.Theme = ElementTheme.Default;
            if (this.Content is ShellPage root)
            {
                root.RequestedTheme = _vm.Theme;

                SetCapitionButtonColor();
            }

            this.SystemBackdrop = null;
        }
    }

    [DynamicWindowsRuntimeCast(typeof(FrameworkElement))]
    public void SetCapitionButtonColor()
    {
        // This is a workaround for the WinAppSDK/WinUI3 bug that caption button color is not updated when theme is changed.
        // Use AppWindow.TitleBar.PreferredTheme instead of Color. It will affect normal dim when activate/deactivate.

        if (this.Content is null)
        {
            return;
        }

        var currentTheme = ((FrameworkElement)Content).ActualTheme;

        if (currentTheme == ElementTheme.Dark)
        {
            this.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.Dark;
        }
        else if (currentTheme == ElementTheme.Light)
        {
            this.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.Light;
        }
        else
        {
            if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                this.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.Dark;
            }
            else
            {
                this.AppWindow.TitleBar.PreferredTheme = TitleBarTheme.Light;
            }
        }
    }

    [DynamicWindowsRuntimeCast(typeof(OverlappedPresenter))]
    private void Window_Activated(object sender, WindowActivatedEventArgs args)
    {
        // Ensure that the restore position and sizes are updated when the window is activated, especially after being minimized or restored.
        Microsoft.UI.Windowing.AppWindow? appWindow = this.AppWindow;
        if (appWindow == null)
        {
            return;
        }
        if (appWindow.Presenter is not OverlappedPresenter presenter)
        {
            return;
        }
        if (presenter.State == OverlappedPresenterState.Restored)
        {
            _winRestoreHeight = appWindow.Size.Height;
            _winRestoreWidth = appWindow.Size.Width;
            _winRestoreTop = appWindow.Position.Y;
            _winRestoreLeft = appWindow.Position.X;
        }
    }

    [DynamicWindowsRuntimeCast(typeof(OverlappedPresenter))]
    private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        if (this.AppWindow.Presenter is OverlappedPresenter presenter)
        {
            if (presenter.State == OverlappedPresenterState.Restored)
            {
                _winRestoreHeight = (int)this.AppWindow.Size.Height;
                _winRestoreWidth = (int)this.AppWindow.Size.Width;
                _winRestoreTop = (int)this.AppWindow.Position.Y;
                _winRestoreLeft = (int)this.AppWindow.Position.X;
            }
        }
    }

    [DynamicWindowsRuntimeCast(typeof(OverlappedPresenter))]
    private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
    {
        if (!args.DidPositionChange)
        {
            return;
        }

        //Microsoft.UI.Windowing.AppWindow? appWindow = this.AppWindow;
        if (sender == null)
        {
            return;
        }
        if (sender.Presenter is not OverlappedPresenter presenter)
        {
            return;
        }

        if (presenter.State == OverlappedPresenterState.Restored)
        {
            _winRestoreHeight = sender.Size.Height;
            _winRestoreWidth = sender.Size.Width;
            _winRestoreTop = sender.Position.Y;
            _winRestoreLeft = sender.Position.X;
        }
    }

    [DynamicWindowsRuntimeCast(typeof(OverlappedPresenter))]
    private async void AppWindow_Closing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        Debug.WriteLine("AppWindow_Closing");

        if (_isClosing) return;
        _isClosing = true;

        args.Cancel = true;

        _vm.UpdateCurrentSettings();

        #region == Save Configuration ==

        //SaveSettings();
        var config = _configurationService.CurrentConfiguration;

        OverlappedPresenterState? winState = null; // For AOT workaround, don't set default.  = OverlappedPresenterState.Restored;

        //Microsoft.UI.Windowing.AppWindow? appWindow = this.AppWindow;
        if (sender != null)
        {
            // AOT bad
            // https://github.com/microsoft/CsWinRT/issues/1930
            if (sender.Presenter is OverlappedPresenter presenter)
            {
                // AOT bad
                //OverlappedPresenter? presenter = appWindow.Presenter as OverlappedPresenter;

                if (presenter?.State == OverlappedPresenterState.Maximized)
                {
                    winState = OverlappedPresenterState.Maximized;
                }
                else if (presenter?.State == OverlappedPresenterState.Minimized)
                {
                    winState = OverlappedPresenterState.Minimized;
                }
                else if (presenter?.State == OverlappedPresenterState.Restored)
                {
                    winState = OverlappedPresenterState.Restored;
                }
            }
            else
            {
                //App.AppendErrorLog("AOT Error",$"{appWindow.Presenter}"); //Microsoft.UI.Windowing.AppWindowPresenter
                //App.SaveErrorLog();

                try
                {
                    // For AOT tmp workaround
                    OverlappedPresenter presenterAOT = this.AppWindow.Presenter.As<OverlappedPresenter>();

                    if (presenterAOT.State == OverlappedPresenterState.Maximized)
                    {
                        winState = OverlappedPresenterState.Maximized;
                    }
                    else if (presenterAOT.State == OverlappedPresenterState.Minimized)
                    {
                        winState = OverlappedPresenterState.Minimized;
                    }
                    else if (presenterAOT.State == OverlappedPresenterState.Restored)
                    {
                        winState = OverlappedPresenterState.Restored;
                    }
                }
                catch (Exception ex)
                {
                    (Application.Current as App)?.AppendErrorLog("AOT Error@SaveSettings", $"{ex}");
                    (Application.Current as App)?.SaveErrorLog();
                }

            }

            // For AOT, check if winState is null or not.
            if (winState != null)
            {
                // Main Window attributes
                if (winState == OverlappedPresenterState.Restored)
                {
                    config.MainWindow.Width = sender.Size.Width;
                }
                else
                {
                    config.MainWindow.Width = _winRestoreWidth;
                }
                config.MainWindow.RestoreWidth = _winRestoreWidth;

                if (winState == OverlappedPresenterState.Restored)
                {
                    config.MainWindow.Height = sender.Size.Height;
                }
                else
                {
                    config.MainWindow.Height = _winRestoreHeight;
                }
                config.MainWindow.RestoreHeight = _winRestoreHeight;

                if (winState == OverlappedPresenterState.Restored)
                {
                    config.MainWindow.Top = sender.Position.Y;
                }
                else
                {
                    config.MainWindow.Top = _winRestoreTop;
                }
                config.MainWindow.RestoreTop = _winRestoreTop;

                if (winState == OverlappedPresenterState.Restored)
                {
                    config.MainWindow.Left = sender.Position.X;
                }
                else
                {
                    config.MainWindow.Left = _winRestoreLeft;
                }
                config.MainWindow.RestoreLeft = _winRestoreLeft;

                if (winState == OverlappedPresenterState.Maximized)
                {
                    //attrs.Value = "Maximized";
                    config.MainWindow.WindowState = OverlappedPresenterState.Maximized;
                }
                else if (winState == OverlappedPresenterState.Restored)
                {
                    //attrs.Value = "Normal";
                    config.MainWindow.WindowState = OverlappedPresenterState.Restored;
                }
                else if (winState == OverlappedPresenterState.Minimized)
                {
                    //attrs.Value = "Minimized";
                    config.MainWindow.WindowState = OverlappedPresenterState.Restored; // Ignore actual value.
                }
            }
        }

        await _configurationService.SaveConfigurationAsync(_vm.Pairs);

        // Wait for the ticker service to stop gracefully
        await _bitbankStreamService.StopAsync();

        #endregion

        this.Close();
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        Debug.WriteLine("MainWindow_Closed");

        this.AppWindow.Closing -= AppWindow_Closing;
        this.Closed -= Window_Closed;
        this.SizeChanged -= Window_SizeChanged;
        this.AppWindow.Changed -= AppWindow_Changed;
        this.Activated -= Window_Activated;

        try
        {
            _vm.Dispose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error disposing MainViewModel: {ex}");
            (Application.Current as App)?.AppendErrorLog("MainWindow_Closed", $"{ex}");
            (Application.Current as App)?.SaveErrorLog();
        }
    }

}
