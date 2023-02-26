using BitWallpaper.Helpers;
using BitWallpaper.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
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
            (PairCodes.omg_jpy.ToString(), typeof(OmgJpyPage)),
            (PairCodes.xym_jpy.ToString(), typeof(XymJpyPage)),
            (PairCodes.link_jpy.ToString(), typeof(LinkJpyPage)),
            (PairCodes.mkr_jpy.ToString(), typeof(MkrJpyPage)),
            (PairCodes.boba_jpy.ToString(), typeof(BobaJpyPage)),
            (PairCodes.enj_jpy.ToString(), typeof(EnjJpyPage)),
            (PairCodes.matic_jpy.ToString(), typeof(MaticJpyPage)),
            (PairCodes.dot_jpy.ToString(), typeof(DotJpyPage)),
            (PairCodes.doge_jpy.ToString(), typeof(DogeJpyPage)),
            (PairCodes.astr_jpy.ToString(), typeof(AstrJpyPage)),
            (PairCodes.ada_jpy.ToString(), typeof(AdaJpyPage)),
            (PairCodes.avax_jpy.ToString(), typeof(AvaxJpyPage)),
            (PairCodes.axs_jpy.ToString(), typeof(AxsJpyPage)),
            (PairCodes.flr_jpy.ToString(), typeof(FlrJpyPage)),
            (PairCodes.sand_jpy.ToString(), typeof(SandJpyPage)),
            ("settings", typeof(SettingsPage)),
        };
        /*
        private string _envDataFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private string _appDataFolder;
        private string _appConfigFilePath;
        

        private static readonly ResourceLoader _resourceLoader = new();
        */
        public MainShell(MainViewModel mainVM)
        {
            MainVM = mainVM;
            /*
            var _appName = _resourceLoader.GetString("AppName");
            var _appDeveloper = "torum";
            
            _appDataFolder = _envDataFolder + System.IO.Path.DirectorySeparatorChar + _appDeveloper + System.IO.Path.DirectorySeparatorChar + _appName;
            
            _appConfigFilePath = _appDataFolder + System.IO.Path.DirectorySeparatorChar + _appName + ".config";
            */

            #region == Load settings ==

            double height = 640; //(App.Current as App).MainWindow.GetAppWindow().Size.Height;
            double width = 480;//(App.Current as App).MainWindow.GetAppWindow().Size.Width;
            bool navigationViewControl_IsPaneOpen = false;  

            if (System.IO.File.Exists(App.AppConfigFilePath))
            {
                XDocument xdoc = XDocument.Load(App.AppConfigFilePath);

                //Debug.WriteLine(xdoc.ToString());

                // Main window
                if (App.MainWindow != null)
                {
                    // Main Window element
                    var mainWindow = xdoc.Root.Element("MainWindow");
                    if (mainWindow != null)
                    {
                        /*
                        var hoge = mainWindow.Attribute("top");
                        if (hoge != null)
                        {
                            (sender as Window).Top = double.Parse(hoge.Value);
                        }
                        */
                        /*
                        hoge = mainWindow.Attribute("left");
                        if (hoge != null)
                        {
                            (sender as Window).Left = double.Parse(hoge.Value);
                        }
                        */
                        var hoge = mainWindow.Attribute("height");
                        if (hoge != null)
                        {
                            height = double.Parse(hoge.Value);
                        }

                        hoge = mainWindow.Attribute("width");
                        if (hoge != null)
                        {
                            width = double.Parse(hoge.Value);
                        }
                        /*
                        hoge = mainWindow.Attribute("state");
                        if (hoge != null)
                        {
                            if (hoge.Value == "Maximized")
                            {
                                (sender as Window).WindowState = WindowState.Maximized;
                            }
                            else if (hoge.Value == "Normal")
                            {
                                (sender as Window).WindowState = WindowState.Normal;
                            }
                            else if (hoge.Value == "Minimized")
                            {
                                (sender as Window).WindowState = WindowState.Normal;
                            }
                        }
                        */

                    }

                    var xNavView = mainWindow.Element("NavigationViewControl");
                    if (xNavView != null)
                    {
                        if (xNavView.Attribute("IsPaneOpen") != null)
                        {
                            var xvalue = xNavView.Attribute("IsPaneOpen").Value;
                            if (!string.IsNullOrEmpty(xvalue))
                            {
                                if (xvalue == "True")
                                    navigationViewControl_IsPaneOpen = true;
                                else
                                    navigationViewControl_IsPaneOpen = false;
                            }
                        }
                    }
                }

                // Options
                var opts = xdoc.Root.Element("Opts");
                if (opts != null)
                {
                    var xvalue = opts.Attribute("IsChartTooltipVisible");
                    if (xvalue != null)
                    {
                        if (!string.IsNullOrEmpty(xvalue.Value))
                        {
                            if (xvalue.Value == "True")
                                MainVM.IsChartTooltipVisible = true;
                            else

                                MainVM.IsChartTooltipVisible = false;
                        }
                    }

                    xvalue = opts.Attribute("IsDebugSaveLog");
                    if (xvalue != null)
                    {
                        if (!string.IsNullOrEmpty(xvalue.Value))
                        {
                            if (xvalue.Value == "True")
                                MainVM.IsDebugSaveLog = true;
                            else

                                MainVM.IsDebugSaveLog = false;
                        }
                    }
                }

                // Pairs
                var xPairs = xdoc.Root.Element("Pairs");
                if (xPairs != null)
                {
                    var pairList = xPairs.Elements("Pair");

                    foreach (var hoge in pairList)
                    {
                        var xvalue = hoge.Attribute("PairCode");
                        if (xvalue != null)
                        {
                            if (!string.IsNullOrEmpty(xvalue.Value))
                            {
                                try
                                {
                                    //
                                    PairCodes pc = MainVM.GetPairs[xvalue.Value];

                                    var pair = MainVM.Pairs.FirstOrDefault(x => x.PairCode == pc);

                                    // TODO:
                                    pair.IsChartTooltipVisible = MainVM.IsChartTooltipVisible;

                                    var attrv = hoge.Attribute("IsEnabled");
                                    if (attrv != null)
                                    {
                                        if (!string.IsNullOrEmpty(attrv.Value))
                                        {
                                            if (attrv.Value == "True")
                                                pair.IsEnabled = true;
                                            else
                                                pair.IsEnabled = false;
                                        }
                                    }

                                    attrv = hoge.Attribute("IsPaneVisible");
                                    if (attrv != null)
                                    {
                                        if (!string.IsNullOrEmpty(attrv.Value))
                                        {
                                            if (attrv.Value == "True")
                                                pair.IsPaneVisible = true;
                                            else
                                                pair.IsPaneVisible = false;
                                        }
                                    }

                                    attrv = hoge.Attribute("CandleType");
                                    if (attrv != null)
                                    {
                                        if (!string.IsNullOrEmpty(attrv.Value))
                                        {
                                            var candleTypeString = attrv.Value;

                                            if (candleTypeString == "OneMin")
                                                pair.SelectedCandleType = Models.CandleTypes.OneMin;
                                            else if (candleTypeString == "FiveMin")
                                                pair.SelectedCandleType = Models.CandleTypes.FiveMin;
                                            else if (candleTypeString == "FifteenMin")
                                                pair.SelectedCandleType = Models.CandleTypes.FifteenMin;
                                            else if (candleTypeString == "ThirtyMin")
                                                pair.SelectedCandleType = Models.CandleTypes.ThirtyMin;
                                            else if (candleTypeString == "OneHour")
                                                pair.SelectedCandleType = Models.CandleTypes.OneHour;
                                            else if (candleTypeString == "FourHour")
                                                pair.SelectedCandleType = Models.CandleTypes.FourHour;
                                            else if (candleTypeString == "EightHour")
                                                pair.SelectedCandleType = Models.CandleTypes.EightHour;
                                            else if (candleTypeString == "TwelveHour")
                                                pair.SelectedCandleType = Models.CandleTypes.TwelveHour;
                                            else if (candleTypeString == "OneDay")
                                                pair.SelectedCandleType = Models.CandleTypes.OneDay;
                                            else if (candleTypeString == "OneWeek")
                                                pair.SelectedCandleType = Models.CandleTypes.OneWeek;
                                            else if (candleTypeString == "OneMonth")
                                                pair.SelectedCandleType = Models.CandleTypes.OneMonth;

                                        }
                                    }

                                    // TODO: 個別設定が必要
                                    if (pair.PairCode == PairCodes.btc_jpy)
                                    {
                                        MainVM.IsOnBtcJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.xrp_jpy)
                                    {
                                        MainVM.IsOnXrpJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.eth_jpy)
                                    {
                                        MainVM.IsOnEthJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.ltc_jpy)
                                    {
                                        MainVM.IsOnLtcJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.bcc_jpy)
                                    {
                                        MainVM.IsOnBccJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.mona_jpy)
                                    {
                                        MainVM.IsOnMonaJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.xlm_jpy)
                                    {
                                        MainVM.IsOnXlmJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.qtum_jpy)
                                    {
                                        MainVM.IsOnQtumJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.bat_jpy)
                                    {
                                        MainVM.IsOnBatJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.omg_jpy)
                                    {
                                        MainVM.IsOnOmgJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.xym_jpy)
                                    {
                                        MainVM.IsOnXymJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.link_jpy)
                                    {
                                        MainVM.IsOnLinkJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.mkr_jpy)
                                    {
                                        MainVM.IsOnMkrJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.boba_jpy)
                                    {
                                        MainVM.IsOnBobaJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.enj_jpy)
                                    {
                                        MainVM.IsOnEnjJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.matic_jpy)
                                    {
                                        MainVM.IsOnMaticJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.dot_jpy)
                                    {
                                        MainVM.IsOnDotJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.doge_jpy)
                                    {
                                        MainVM.IsOnDogeJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.astr_jpy)
                                    {
                                        MainVM.IsOnAstrJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.ada_jpy)
                                    {
                                        MainVM.IsOnAdaJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.avax_jpy)
                                    {
                                        MainVM.IsOnAvaxJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.axs_jpy)
                                    {
                                        MainVM.IsOnAxsJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.flr_jpy)
                                    {
                                        MainVM.IsOnFlrJpy = pair.IsEnabled;
                                    }
                                    else if (pair.PairCode == PairCodes.sand_jpy)
                                    {
                                        MainVM.IsOnSandJpy = pair.IsEnabled;
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
            }

            (App.Current as App).IsSaveErrorLog = MainVM.IsDebugSaveLog;

            #endregion

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


            //(App.Current as App).MainWindow.ExtendsContentIntoTitleBar = true;
            App.MainWindow.SetTitleBar(AppTitleBar);
            App.MainWindow.Activated += MainWindow_Activated;
            App.MainWindow.Closed += MainWindow_Closed;
            //AppTitleBarText.Text = "AppDisplayName".GetLocalized();

            //
            MainVM.NavigationViewControl_IsPaneOpen = navigationViewControl_IsPaneOpen;
            /*
            if (((width > 100) && (height > 100)) && ((width < 2000) && (height < 2000)))
            {
                // Be carefull!
                (App.Current as App).MainWindow.CenterOnScreen(width, height);
            }
            */
        }

        private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
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
            // 設定ファイル用のXMLオブジェクト
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

            // Root Document Element
            XmlElement root = doc.CreateElement(string.Empty, "App", string.Empty);
            doc.AppendChild(root);

            //XmlAttribute attrs = doc.CreateAttribute("Version");
            //attrs.Value = _appVer;
            //root.SetAttributeNode(attrs);
            XmlAttribute attrs;

            // Main window
            if (App.MainWindow != null)
            {
                // Main Window element
                XmlElement mainWindow = doc.CreateElement(string.Empty, "MainWindow", string.Empty);

                // Main Window attributes
                attrs = doc.CreateAttribute("width");
                /*
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Width.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Width.ToString();
                }
                */
                attrs.Value = App.MainWindow.GetAppWindow().Size.Width.ToString();
                mainWindow.SetAttributeNode(attrs);

                attrs = doc.CreateAttribute("height");
                /*
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Height.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Height.ToString();
                }
                */
                attrs.Value = App.MainWindow.GetAppWindow().Size.Height.ToString();
                mainWindow.SetAttributeNode(attrs);

                /*
                attrs = doc.CreateAttribute("top");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Top.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Top.ToString();
                }
                mainWindow.SetAttributeNode(attrs);
                */
                /*
                attrs = doc.CreateAttribute("left");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = (sender as Window).RestoreBounds.Left.ToString();
                }
                else
                {
                    attrs.Value = (sender as Window).Left.ToString();
                }
                mainWindow.SetAttributeNode(attrs);
                */
                /*
                attrs = doc.CreateAttribute("state");
                if ((sender as Window).WindowState == WindowState.Maximized)
                {
                    attrs.Value = "Maximized";
                }
                else if ((sender as Window).WindowState == WindowState.Normal)
                {
                    attrs.Value = "Normal";

                }
                else if ((sender as Window).WindowState == WindowState.Minimized)
                {
                    attrs.Value = "Minimized";
                }
                mainWindow.SetAttributeNode(attrs);
                */


                XmlElement xNavigationViewControl = doc.CreateElement(string.Empty, "NavigationViewControl", string.Empty);
               
                XmlAttribute xAttrs = doc.CreateAttribute("IsPaneOpen");
                xAttrs.Value = MainVM.NavigationViewControl_IsPaneOpen.ToString();
                xNavigationViewControl.SetAttributeNode(xAttrs);

                mainWindow.AppendChild(xNavigationViewControl);

                // set Main Window element to root.
                root.AppendChild(mainWindow);

            }

            // Options
            XmlElement xOpts = doc.CreateElement(string.Empty, "Opts", string.Empty);

            attrs = doc.CreateAttribute("IsChartTooltipVisible");
            attrs.Value = MainVM.IsChartTooltipVisible.ToString();
            xOpts.SetAttributeNode(attrs);

            attrs = doc.CreateAttribute("IsDebugSaveLog");
            attrs.Value = MainVM.IsDebugSaveLog.ToString();
            xOpts.SetAttributeNode(attrs);

            root.AppendChild(xOpts);

            // Each pairs
            XmlElement xPairs = doc.CreateElement(string.Empty, "Pairs", string.Empty);
            foreach (var hoge in MainVM.Pairs)
            {
                XmlElement xPair = doc.CreateElement(string.Empty, "Pair", string.Empty);

                XmlAttribute xAttrs = doc.CreateAttribute("PairCode");
                xAttrs.Value = hoge.PairCode.ToString();
                xPair.SetAttributeNode(xAttrs);

                xAttrs = doc.CreateAttribute("IsEnabled");
                xAttrs.Value = hoge.IsEnabled.ToString();
                xPair.SetAttributeNode(xAttrs);

                xAttrs = doc.CreateAttribute("IsPaneVisible");
                xAttrs.Value = hoge.IsPaneVisible.ToString();
                xPair.SetAttributeNode(xAttrs);

                xAttrs = doc.CreateAttribute("CandleType");
                xAttrs.Value = hoge.SelectedCandleType.ToString();
                xPair.SetAttributeNode(xAttrs);


                xAttrs = doc.CreateAttribute("IsChartTooltipVisible");
                // TODO:
                //xAttrs.Value = hoge.IsChartTooltipVisible.ToString();
                //xAttrs.Value = MainVM.IsChartTooltipVisible.ToString();
                xPair.SetAttributeNode(xAttrs);


                xPairs.AppendChild(xPair);

                // Since we are here, we might as well clean up.
                hoge.IsEnabled = false;
                hoge.CleanUp();
            }
            root.AppendChild(xPairs);

            try
            {
                doc.Save(App.AppConfigFilePath);
            }
            //catch (System.IO.FileNotFoundException) { }
            catch (Exception ex)
            {
                Debug.WriteLine("■■■■■ Error  設定ファイルの保存中: " + ex + " while opening : " + App.AppConfigFilePath);
            }

            MainVM.CleanUp();

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

        private async void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
        {

            await Task.Delay(100);

            if (NavigationViewControl.MenuItems.Count > 0)
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
            settings.Content = "";//"Setting".GetLocalized();
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
                else if (_page == typeof(OmgJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.omg_jpy);
                }
                else if (_page == typeof(XymJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.xym_jpy);
                }
                else if (_page == typeof(LinkJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.link_jpy);
                }
                else if (_page == typeof(MkrJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.mkr_jpy);
                }
                else if (_page == typeof(BobaJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.boba_jpy);
                }
                else if (_page == typeof(EnjJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.enj_jpy);
                }
                else if (_page == typeof(MaticJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.matic_jpy);
                }
                else if (_page == typeof(DotJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.dot_jpy);
                }
                else if (_page == typeof(DogeJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.doge_jpy);
                }
                else if (_page == typeof(AstrJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.astr_jpy);
                }
                else if (_page == typeof(AdaJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.ada_jpy);
                }
                else if (_page == typeof(AvaxJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.avax_jpy);
                }
                else if (_page == typeof(AxsJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.axs_jpy);
                }
                else if (_page == typeof(FlrJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.flr_jpy);
                }
                else if (_page == typeof(SandJpyPage))
                {
                    vm = MainVM.Pairs.FirstOrDefault(x => x.PairCode == PairCodes.sand_jpy);
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
            else if (e.SourcePageType == typeof(OmgJpyPage))
            {
                _activePair = PairCodes.omg_jpy;
            }
            else if (e.SourcePageType == typeof(XymJpyPage))
            {
                _activePair = PairCodes.xym_jpy;
            }
            else if (e.SourcePageType == typeof(LinkJpyPage))
            {
                _activePair = PairCodes.link_jpy;
            }
            else if (e.SourcePageType == typeof(MkrJpyPage))
            {
                _activePair = PairCodes.mkr_jpy;
            }
            else if (e.SourcePageType == typeof(BobaJpyPage))
            {
                _activePair = PairCodes.boba_jpy;
            }
            else if (e.SourcePageType == typeof(EnjJpyPage))
            {
                _activePair = PairCodes.enj_jpy;
            }
            else if (e.SourcePageType == typeof(MaticJpyPage))
            {
                _activePair = PairCodes.matic_jpy;
            }
            else if (e.SourcePageType == typeof(DotJpyPage))
            {
                _activePair = PairCodes.dot_jpy;
            }
            else if (e.SourcePageType == typeof(DogeJpyPage))
            {
                _activePair = PairCodes.doge_jpy;
            }
            else if (e.SourcePageType == typeof(AstrJpyPage))
            {
                _activePair = PairCodes.astr_jpy;
            }
            else if (e.SourcePageType == typeof(AdaJpyPage))
            {
                _activePair = PairCodes.ada_jpy;
            }
            else if (e.SourcePageType == typeof(AvaxJpyPage))
            {
                _activePair = PairCodes.avax_jpy;
            }
            else if (e.SourcePageType == typeof(AxsJpyPage))
            {
                _activePair = PairCodes.axs_jpy;
            }
            else if (e.SourcePageType == typeof(FlrJpyPage))
            {
                _activePair = PairCodes.flr_jpy;
            }
            else if (e.SourcePageType == typeof(SandJpyPage))
            {
                _activePair = PairCodes.sand_jpy;
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
