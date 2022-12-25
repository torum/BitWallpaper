using BitWallpaper4.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using BitWallpaper4.Views;
using BitWallpaper4.Models;

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
            ("settings", typeof(SettingsPage)),
        };

        //private object? _selected;
        private PairCodes _activePair = PairCodes.btc_jpy; 


        public MainViewModel ViewModel
        {
            get;
        }

        public MainWindow()
        {
            ViewModel = new MainViewModel();

            this.InitializeComponent();

            this.ExtendsContentIntoTitleBar = true;

            this.SetTitleBar(AppTitleBar);

            this.SetWindowSize(1360, 768);
            this.CenterOnScreen();

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
            //var resource = args.WindowActivationState == WindowActivationState.Deactivated ? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground";

            //AppTitleBarText.Foreground = (SolidColorBrush)App.Current.Resources[resource];


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
            settings.Content = "Ý’è";
        }

        private void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked == true)
            {
                NavigationFrame.Navigate(typeof(SettingsPage), args.RecommendedNavigationTransitionInfo);
            }
            else if (args.InvokedItemContainer != null && (args.InvokedItemContainer.Tag != null))
            {
                /*
                var navItemTag = args.InvokedItemContainer.Tag.ToString();

                if (navItemTag is not null)
                {
                    NavView_Navigate(navItemTag, args.RecommendedNavigationTransitionInfo);

                }
                */
                if (_pages is null)
                    return;

                var item = _pages.FirstOrDefault(p => p.Tag.Equals(args.InvokedItemContainer.Tag.ToString()));

                var _page = item.Page;

                if (_page is null)
                    return;

                // Pass Frame when navigate.
                NavigationFrame.Navigate(_page, ViewModel, args.RecommendedNavigationTransitionInfo);
            }
        }


        private void NavigationFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (NavigationFrame.SourcePageType == typeof(SettingsPage))
            {
                // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
                //NavigationViewControl.SelectedItem = (NavigationViewItem)NavigationViewControl.SettingsItem;
                //NavigationViewControl.Header = "Ý’è";
            }
            else if (NavigationFrame.SourcePageType != null)
            {
                //NavigationViewControl.Header = null;

                var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

                //NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First(n => n.Tag.Equals(item.Tag));

                //NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();
            }

            if (e.SourcePageType == typeof(SettingsPage))
            {
                //Selected = NavigationViewService.SettingsItem;
                //_activePair = null;
                return;
            }
            else if (e.SourcePageType == typeof(Views.BtcJpyPage))
            {
                _activePair = PairCodes.btc_jpy;
            }
            else if (e.SourcePageType == typeof(Views.XrpJpyPage))
            {
                _activePair = PairCodes.xrp_jpy;
            }

            ViewModel.SetSelectedPairFromCode = _activePair;
            ViewModel.SelectedPair.InitializeAndGetChartData(CandleTypes.OneHour);

            /*
            var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
            if (selectedItem != null)
            {
                Selected = selectedItem;
            }
            */
        }

    }
}
