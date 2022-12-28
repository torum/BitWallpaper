using BitWallpaper4.ViewModels;
using BitWallpaper4.Views;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace BitWallpaper4
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : WinUIEx.WindowEx
    {
        private readonly List<(string Tag, Type Page)> _pages = new()
        {
            (PairCodes.btc_jpy.ToString(), typeof(BtcJpyPage)),
            (PairCodes.xrp_jpy.ToString(), typeof(XrpJpyPage)),
            (PairCodes.eth_jpy.ToString(), typeof(EthJpyPage)),
            (PairCodes.ltc_jpy.ToString(), typeof(LtcJpyPage)),
            (PairCodes.mona_jpy.ToString(), typeof(MonaJpyPage)),
            (PairCodes.bcc_jpy.ToString(), typeof(BccJpyPage)),
            (PairCodes.xlm_jpy.ToString(), typeof(XlmJpyPage)),
            (PairCodes.qtum_jpy.ToString(), typeof(QtumJpyPage)),
            (PairCodes.bat_jpy.ToString(), typeof(BatJpyPage)),
            ("settings", typeof(SettingsPage)),
        };

        //private object? _selected;


        public MainViewModel ViewModel
        {
            get;
        }

        public MainWindow()
        {
            AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "App_Icon.ico"));

            ViewModel = new MainViewModel();

            // load a setting for each pair.
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            foreach (var hoge in ViewModel.Pairs)
            {
                Windows.Storage.ApplicationDataCompositeValue compositePairs = (ApplicationDataCompositeValue)localSettings.Values[hoge.PairCode.ToString()];
                if (compositePairs != null)
                {
                    hoge.IsPaneVisible = (bool)compositePairs["IsPaneVisible"];
                }
            }

            try
            {
                InitializeComponent();
            }
            catch (XamlParseException parseException)
            {
                Debug.WriteLine($"Unhandled XamlParseException in MainPage: {parseException.Message}");
                foreach (var key in parseException.Data.Keys)
                {
                    Debug.WriteLine("{Key}:{Value}", key.ToString(), parseException.Data[key]?.ToString());
                }
                throw;
            }
            
            this.ExtendsContentIntoTitleBar = true;
            
            this.SetTitleBar(AppTitleBar);

            // load window setting
            Windows.Storage.ApplicationDataCompositeValue compositeMainWin = (ApplicationDataCompositeValue)localSettings.Values["MainWindow"];
            if (compositeMainWin != null)
            {
                //String fontName = composite["Font"] as string;
                double width = (double)compositeMainWin["Width"];
                double height = (double)compositeMainWin["Height"];

                this.CenterOnScreen(width, height);

                if (compositeMainWin["NavigationViewControl_IsPaneOpen"] != null)
                    NavigationViewControl.IsPaneOpen = (bool)compositeMainWin["NavigationViewControl_IsPaneOpen"];
            }
            else
            {
                //this.SetWindowSize(1360, 768);
                this.CenterOnScreen(1360, 768);
            }

            // need to be here(last).
            var manager = WinUIEx.WindowManager.Get(this);
            manager.PersistenceId = "MainWindowPersistanceId";
            manager.MinWidth = 640;
            manager.MinHeight = 480;
            //manager.Backdrop = new WinUIEx.AcrylicSystemBackdrop();
            manager.Backdrop = new WinUIEx.MicaSystemBackdrop();
        }

        private void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            /*
            AppTitleBar.Margin = new Thickness()
            {
                Left = sender.CompactPaneLength * (sender.DisplayMode == NavigationViewDisplayMode.Minimal ? 2 : 1),
                Top = AppTitleBar.Margin.Top,
                Right = AppTitleBar.Margin.Right,
                Bottom = AppTitleBar.Margin.Bottom
            };
            */
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

            AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];
        }

        private void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Since we use ItemInvoked, we set selecteditem manually
            NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First();

            // Pass vm to destination Frame when navigate.
            NavigationFrame.Navigate(typeof(BtcJpyPage), ViewModel, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());

            // Listen to the window directly so the app responds to accelerator keys regardless of which element has focus.
            //Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=  CoreDispatcher_AcceleratorKeyActivated;

            //Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

            //SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;


            // Any other way?
            var settings = (Microsoft.UI.Xaml.Controls.NavigationViewItem)NavigationViewControl.SettingsItem;
            settings.Content = "";
        }

        private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                NavigationFrame.Navigate(typeof(SettingsPage), args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null && (args.InvokedItemContainer?.Tag != null))
            {
                if (_pages is null)
                    return;

                var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

                var _page = item.Page;

                if (_page is null)
                    return;

                // Pass Frame when navigate.
                //NavigationFrame.Navigate(_page, ViewModel, args.RecommendedNavigationTransitionInfo);
                NavigationFrame.Navigate(_page, ViewModel, new SuppressNavigationTransitionInfo());
            }
        }

        private void NavigationFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (NavigationFrame.SourcePageType == typeof(SettingsPage))
            {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
                //NavigationViewControl.SelectedItem = (NavigationViewItem)NavigationViewControl.SettingsItem;
                //NavigationViewControl.Header = "設定";
            }
            else if (NavigationFrame.SourcePageType != null)
            {
                //NavigationViewControl.Header = null;

                //var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

                //NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First(n => n.Tag.Equals(item.Tag));

                //NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();
            }

            if (e.SourcePageType == typeof(SettingsPage))
            {
                //Selected = NavigationViewService.SettingsItem;
                //_activePair = null;
                return;
            }

            PairCodes _activePair = PairCodes.btc_jpy;

            // （個別設定が必要）
            if (e.SourcePageType == typeof(BtcJpyPage))
            {
                _activePair = PairCodes.btc_jpy;
            }
            else if (e.SourcePageType == typeof(XrpJpyPage))
            {
                _activePair = PairCodes.xrp_jpy;
            }
            else if (e.SourcePageType == typeof(EthJpyPage))
            {
                _activePair = PairCodes.eth_jpy;
            }
            else if (e.SourcePageType == typeof(LtcJpyPage))
            {
                _activePair = PairCodes.ltc_jpy;
            }
            else if (e.SourcePageType == typeof(MonaJpyPage))
            {
                _activePair = PairCodes.mona_jpy;
            }
            else if (e.SourcePageType == typeof(BccJpyPage))
            {
                _activePair = PairCodes.bcc_jpy;
            }
            else if (e.SourcePageType == typeof(XlmJpyPage))
            {
                _activePair = PairCodes.xlm_jpy;
            }
            else if (e.SourcePageType == typeof(QtumJpyPage))
            {
                _activePair = PairCodes.qtum_jpy;
            }
            else if (e.SourcePageType == typeof(BatJpyPage))
            {
                _activePair = PairCodes.bat_jpy;
            }
            else
            {
                throw new NotImplementedException();
            }

            /*
            (App.Current as App)?.CurrentDispatcherQueue?.TryEnqueue(() =>
            {
            });
            */
            ViewModel.SetSelectedPairFromCode(_activePair);

            //ViewModel?.SelectedPair?.InitializeAndStart();
            //Task.Run(() => ViewModel.SelectedPair?.InitializeAndGetChartData(CandleTypes.OneHour));

        }

        private void NavigationFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Debug.WriteLine("NavigationFrame_NavigationFailed: " + e.Exception.Message + " - " + e.Exception.StackTrace);

            (App.Current as App).AppendErrorLog("NavigationFrame_NavigationFailed", e.Exception.Message + ", StackTrace: " + e.Exception.StackTrace);

            e.Handled = true;
        }

        private void WindowEx_Closed(object sender, WindowEventArgs args)
        {
            // Save a setting locally on the device
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            
            localSettings.Values["test_setting"] = "a device specific setting";

            // Save a composite setting locally on the device
            Windows.Storage.ApplicationDataCompositeValue composite = new Windows.Storage.ApplicationDataCompositeValue();
            composite["Width"] = this.Width;
            composite["Height"] = this.Height;
            //composite["Top"] = 
            //composite["Left"] = ;
            composite["NavigationViewControl_IsPaneOpen"] = NavigationViewControl.IsPaneOpen;
            localSettings.Values["MainWindow"] = composite;


            foreach (var hoge in ViewModel.Pairs)
            {
                composite = new();
                composite["IsPaneVisible"] = hoge.IsPaneVisible;
                // more

                localSettings.Values[hoge.PairCode.ToString()] = composite;
            }

            // error logs.
            (App.Current as App).SaveErrorLogIfAny();
        }
    }
}
