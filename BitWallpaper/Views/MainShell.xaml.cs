using BitWallpaper.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Storage;
using BitWallpaper.Helpers;
using WinUIEx;

namespace BitWallpaper.Views
{
    public sealed partial class MainShell : Page
    {
        public MainViewModel MainVM
        {
            get;
        }

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

        // load a setting for each pair.
        ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        public MainShell(MainViewModel mainVM)
        {
            MainVM = mainVM;

            // Load saved settings.

            Windows.Storage.ApplicationDataCompositeValue compositeOpts = (ApplicationDataCompositeValue)localSettings.Values["Opts"];
            if (compositeOpts != null)
            {
                if (compositeOpts["IsChartTooltipVisible"] != null)
                    MainVM.IsChartTooltipVisible = (bool)compositeOpts["IsChartTooltipVisible"];
            }

            foreach (var hoge in MainVM.Pairs)
            {
                Windows.Storage.ApplicationDataCompositeValue compositePairs = (ApplicationDataCompositeValue)localSettings.Values[hoge.PairCode.ToString()];
                if (compositePairs != null)
                {
                    //hoge.IsChartTooltipVisible = MainVM.IsChartTooltipVisible;

                    if (compositePairs["IsPaneVisible"] != null)
                        hoge.IsPaneVisible = (bool)compositePairs["IsPaneVisible"];
                    if (compositePairs["IsEnabled"] != null)
                    {
                        //hoge.IsEnabled = (bool)compositePairs["IsEnabled"]; // not gonna work.
                        if (hoge.PairCode == PairCodes.btc_jpy)
                        {
                            MainVM.IsOnBtcJpy = (bool)compositePairs["IsEnabled"];
                        }
                        else if (hoge.PairCode == PairCodes.xrp_jpy)
                        {
                            MainVM.IsOnXrpJpy = (bool)compositePairs["IsEnabled"];
                        }
                        else if (hoge.PairCode == PairCodes.eth_jpy)
                        {
                            MainVM.IsOnEthJpy = (bool)compositePairs["IsEnabled"];
                        }
                        else if (hoge.PairCode == PairCodes.ltc_jpy)
                        {
                            MainVM.IsOnLtcJpy = (bool)compositePairs["IsEnabled"];
                        }
                        else if (hoge.PairCode == PairCodes.bcc_jpy)
                        {
                            MainVM.IsOnBccJpy = (bool)compositePairs["IsEnabled"];
                        }
                        else if (hoge.PairCode == PairCodes.mona_jpy)
                        {
                            MainVM.IsOnMonaJpy = (bool)compositePairs["IsEnabled"];
                        }
                        else if (hoge.PairCode == PairCodes.xlm_jpy)
                        {
                            MainVM.IsOnXlmJpy = (bool)compositePairs["IsEnabled"];
                        }
                        else if (hoge.PairCode == PairCodes.qtum_jpy)
                        {
                            MainVM.IsOnQtumJpy = (bool)compositePairs["IsEnabled"];
                        }
                        else if (hoge.PairCode == PairCodes.bat_jpy)
                        {
                            MainVM.IsOnBatJpy = (bool)compositePairs["IsEnabled"];
                        }
                    }
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

            //AppTitleBarText.Text = "AppDisplayName".GetLocalized();
            //

            //(App.Current as App).MainWindow.ExtendsContentIntoTitleBar = true;
            (App.Current as App).MainWindow.SetTitleBar(AppTitleBar);
            (App.Current as App).MainWindow.Activated += MainWindow_Activated;
            (App.Current as App).MainWindow.Closed += MainWindow_Closed;
            //AppTitleBarText.Text = "AppDisplayName".GetLocalized();

            // load window setting
            Windows.Storage.ApplicationDataCompositeValue compositeMainWin = (ApplicationDataCompositeValue)localSettings.Values["MainWindow"];
            if (compositeMainWin != null)
            {
                //String fontName = composite["Font"] as string;
                if ((compositeMainWin["Width"] != null) && (compositeMainWin["Height"] != null))
                {
                    double width = (double)compositeMainWin["Width"];
                    double height = (double)compositeMainWin["Height"];

                    // Be carefull!
                    (App.Current as App).MainWindow.CenterOnScreen(width, height);
                }

                if (compositeMainWin["NavigationViewControl_IsPaneOpen"] != null)
                    MainVM.NavigationViewControl_IsPaneOpen = (bool)compositeMainWin["NavigationViewControl_IsPaneOpen"];
            }
            else
            {
                // let's not.
                //(App.Current as App).MainWindow.CenterOnScreen(1360, 768);
            }

            /*
            //
            var manager = WinUIEx.WindowManager.Get((App.Current as App).MainWindow);
            manager.PersistenceId = "MainWindowPersistanceId";
            manager.MinWidth = 640;
            manager.MinHeight = 480;
            //manager.Backdrop = new WinUIEx.AcrylicSystemBackdrop();
            manager.Backdrop = new WinUIEx.MicaSystemBackdrop();
            */
        }

        private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            //TitleBarHelper.UpdateTitleBar(RequestedTheme);
            TitleBarHelper.UpdateTitleBar(ElementTheme.Default);

            //KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.Left, VirtualKeyModifiers.Menu));
            //KeyboardAccelerators.Add(BuildKeyboardAccelerator(VirtualKey.GoBack));


        }
        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

            AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];

        }
        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            // TODO

            // Save a setting locally on the device
            ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            // Save a composite setting locally on the device
            Windows.Storage.ApplicationDataCompositeValue composite = new Windows.Storage.ApplicationDataCompositeValue();
            composite["Width"] = (App.Current as App).MainWindow.Bounds.Width;
            composite["Height"] = (App.Current as App).MainWindow.Bounds.Height;
            //composite["Top"] = 
            //composite["Left"] = ;
            composite["NavigationViewControl_IsPaneOpen"] = MainVM.NavigationViewControl_IsPaneOpen;
            localSettings.Values["MainWindow"] = composite;


            composite = new();
            composite["IsChartTooltipVisible"] = MainVM.IsChartTooltipVisible;
            // more
            localSettings.Values["Opts"] = composite;

            foreach (var hoge in MainVM.Pairs)
            {
                composite = new();
                composite["IsPaneVisible"] = hoge.IsPaneVisible;
                composite["IsEnabled"] = hoge.IsEnabled;
                //composite["IsChartTooltipVisible"] = MainVM.IsChartTooltipVisible;
                // more

                localSettings.Values[hoge.PairCode.ToString()] = composite;
            }

            // error logs.
            (App.Current as App).SaveErrorLogIfAny();
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

        private void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (NavigationViewControl.MenuItems.Count >0)
            {
                // Since we use ItemInvoked, we set selecteditem manually
                NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().Where(x => x.Visibility == Visibility.Visible).FirstOrDefault();//.First();

                if (NavigationViewControl.SelectedItem is null)
                {
                    // pass mainviewmodel for setting page.
                    NavigationFrame.Navigate(typeof(SettingsPage), MainVM, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());

                    return;
                }

                // Pass vm to destination Frame when navigate.
                //var pairViewModel = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.btc_jpy);
                //NavigationFrame.Navigate(typeof(BtcJpyPage), pairViewModel, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
                var pairViewModel = MainVM.Pairs.FirstOrDefault(x => x.IsEnabled == true);


                if (pairViewModel != null)
                {
                    var item = _pages.FirstOrDefault(p => p.Tag.Equals(pairViewModel.PairCode.ToString()));
                    var _page = item.Page;

                    if ((NavigationViewControl.SelectedItem as NavigationViewItem).Tag.ToString() == pairViewModel.PairCode.ToString())
                    {
                        NavigationFrame.Navigate(_page, pairViewModel, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
                    }
                }
            }


            // Listen to the window directly so the app responds to accelerator keys regardless of which element has focus.
            //Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=  CoreDispatcher_AcceleratorKeyActivated;

            //Window.Current.CoreWindow.PointerPressed += CoreWindow_PointerPressed;

            //SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;


            // Any other way?
            var settings = (Microsoft.UI.Xaml.Controls.NavigationViewItem)NavigationViewControl.SettingsItem;
            settings.Content = "Setting".GetLocalized();
        }

        private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                // pass mainviewmodel for setting page.
                NavigationFrame.Navigate(typeof(SettingsPage), MainVM, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
                //NavigationFrame.Navigate(typeof(SettingsPage), ViewModel, args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null && (args.InvokedItemContainer?.Tag != null))
            {
                if (_pages is null)
                    return;

                var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

                var _page = item.Page;

                if (_page is null)
                    return;

                // Pass pairviewmodel when navigate to each pair page.
                PairViewModel vm;
                if (_page == typeof(BtcJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.btc_jpy);
                }
                else if (_page == typeof(XrpJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.xrp_jpy);
                }
                else if (_page == typeof(EthJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.eth_jpy);
                }
                else if (_page == typeof(LtcJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.ltc_jpy);
                }
                else if (_page == typeof(MonaJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.mona_jpy);
                }
                else if (_page == typeof(BccJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.bcc_jpy);
                }
                else if (_page == typeof(XlmJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.xlm_jpy);
                }
                else if (_page == typeof(QtumJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.qtum_jpy);
                }
                else if (_page == typeof(BatJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.bat_jpy);
                }
                else
                {
                    throw new NotImplementedException();
                }

                NavigationFrame.Navigate(_page, vm, new SlideNavigationTransitionInfo() { Effect = SlideNavigationTransitionEffect.FromLeft });
                //NavigationFrame.Navigate(_page, vm, args.RecommendedNavigationTransitionInfo);
                //NavigationFrame.Navigate(_page, vm, new SuppressNavigationTransitionInfo());
            }
        }

        private void NavigationFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (NavigationFrame.SourcePageType == typeof(SettingsPage))
            {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
                //NavigationViewControl.SelectedItem = (NavigationViewItem)NavigationViewControl.SettingsItem;
                //NavigationViewControl.Header = "設定";
                return;
            }
            else if (NavigationFrame.SourcePageType != null)
            {
                //NavigationViewControl.Header = null;

                //var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

                //NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First(n => n.Tag.Equals(item.Tag));

                //NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();

                // Do nothing.
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

            // Set IsActive and Init & Start.
            MainVM.SetSelectedPairFromCode(_activePair);
        }

        private void NavigationFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            Debug.WriteLine("NavigationFrame_NavigationFailed: " + e.Exception.Message + " - " + e.Exception.StackTrace);

            (App.Current as App).AppendErrorLog("NavigationFrame_NavigationFailed", e.Exception.Message + ", StackTrace: " + e.Exception.StackTrace);

            e.Handled = true;
        }

    }
}
