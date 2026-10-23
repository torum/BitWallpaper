using BitWallpaper.Models;
using BitWallpaper.Services;
using BitWallpaper.Services.Contracts;
using BitWallpaper.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using WinRT;

namespace BitWallpaper.Views;

public sealed partial class ShellPage : Page
{
    private readonly List<(string Tag, Type Page)> _pages =
[
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
        ("gridview", typeof(GridViewPage)),
        ("settings", typeof(SettingsPage)),
    ];

    private readonly INavigationService _navigationService;
    private static readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);

    public ShellPage(INavigationService navigationService)
    {
        ViewModel = App.GetService<MainViewModel>();
        _navigationService = navigationService;

        InitializeComponent();

        _navigationService.Initialize(NavigationFrame, _pages.ToDictionary(p => p.Tag, p => p.Page));

        this.Loaded += ShellPage_Loaded;
    }

    public MainViewModel ViewModel { get; }

    private async void ShellPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        this.Loaded -= ShellPage_Loaded;

        try
        {
            //_ = RunSafeAsync(ViewModel.InitializeAndStartAsync, nameof(ViewModel.InitializeAndStartAsync));
            await ViewModel.InitializeAndStartAsync();

            var pairViewModel = ViewModel.EnabledPairs.FirstOrDefault();

            if (pairViewModel is not null)
            {
                var item = _pages.FirstOrDefault(p => p.Tag.Equals(pairViewModel.PairCode.ToString()));
                var page = item.Page;

                if (NavigationFrame.Navigate(page, pairViewModel))
                {
                    NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().Where(x => x.Tag?.ToString()?.Equals(pairViewModel.PairCode.ToString()) == true).FirstOrDefault();//.First();

                    //_ = RunSafeAsync(() => ViewModel.SetSelectedPairFromCodeAsync(pairViewModel.PairCode), nameof(ViewModel.SetSelectedPairFromCodeAsync));
                    await ViewModel.SetSelectedPairFromCodeAsync(pairViewModel.PairCode);
                }
                else
                {
                    Debug.WriteLine($"ShellPage_Loaded: Navigation to {item.Tag} failed.");
                }
            }
            else
            {
                // Go to SettingsPage if no enabled pairs are found.
                if (_navigationService.NavigateTo("settings", null, SlideNavigationTransitionEffect.FromBottom)) //new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo()
                {
                    //
                }
                else
                {
                    Debug.WriteLine($"ShellPage_Loaded: Navigation to SettingsPage failed.");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ShellPage_Loaded: Exception occurred - {ex}");
            (App.Current as App)?.AppendErrorLog("ShellPage_Loaded", ex.ToString());
        }
    }

    public void Initialize(Microsoft.UI.Xaml.Window window)
    {
        // Perform any initialization logic here

        //window.SetTitleBar(AppTitleBar);
    }


    private async void NavigationViewControl_Loaded(object sender, RoutedEventArgs e)
    {
        /*
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


                if (((NavigationViewItem)NavigationViewControl.SelectedItem).Tag.ToString() == pairViewModel.PairCode.ToString())
                {
                    NavigationFrame.Navigate(_page, pairViewModel, new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo());
                }
            }
        }
        */
    }

    private async void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.IsSettingsInvoked == true)
        {
            if (_navigationService.NavigateTo("settings", null, SlideNavigationTransitionEffect.FromBottom)) //new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo()
            {
                //
            }
        }
        else if (args.InvokedItemContainer != null && (args.InvokedItemContainer?.Tag != null))
        {
            if (_pages is null)
            {
                return;
            }

            await _asyncLock.WaitAsync();

            try
            {
                var tag = args.InvokedItemContainer.Tag.ToString();

                if (string.IsNullOrWhiteSpace(tag))
                {
                    Debug.WriteLine("NavigationViewControl_ItemInvoked: Invoked item has no tag.");
                    return;
                }

                if (tag.Equals("gridview", StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (NavigationFrame.Navigate(typeof(GridViewPage)))
                    //if (_navigationService.NavigateTo("gridview", ViewModel, SlideNavigationTransitionEffect.FromBottom)) //new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo()
                    {
                        // Set all to Active Pair in ViewModel and Start all.
                        await ViewModel.SetSelectedAllPairsFromCodeAsync();
                        //_ = RunSafeAsync(() => ViewModel.SetSelectedAllPairsFromCodeAsync(), nameof(ViewModel.SetSelectedAllPairsFromCodeAsync));
                    }
                }
                else
                {
                    var pairvm = ViewModel.Pairs.FirstOrDefault(x => x.PairCode.ToString().Equals(tag, StringComparison.OrdinalIgnoreCase));
                    var item = _pages.FirstOrDefault(p => p.Tag.Equals(pairvm?.PairCode.ToString()));

                    if (NavigationFrame.Navigate(item.Page, pairvm))
                    //if (_navigationService.NavigateTo(args.InvokedItemContainer, pairvm, SlideNavigationTransitionEffect.FromBottom)) //new Microsoft.UI.Xaml.Media.Animation.EntranceNavigationTransitionInfo()
                    {
                        var _activePair = Enum.TryParse<PairCodes>(tag.ToString(), out var parsedPair);
                        if (_activePair)//typeof(GridViewPage)
                        {
                            // Set Active Pair in ViewModel and Start.
                            await ViewModel.SetSelectedPairFromCodeAsync(parsedPair);
                            //_ = RunSafeAsync(() => ViewModel.SetSelectedPairFromCodeAsync(parsedPair), nameof(ViewModel.SetSelectedPairFromCodeAsync));
                        }
                        else
                        {
                            Debug.WriteLine($"NavigationView_ItemInvoked: Failed to parse PairCodes from tag '{tag}'.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"NavigationViewControl_ItemInvoked: Exception occurred - {ex}");
                (App.Current as App)?.AppendErrorLog("NavigationViewControl_ItemInvoked", ex.ToString());
            }
            finally
            {
                _asyncLock.Release();
            }
        }
        else
        {
            Debug.WriteLine("NavigationViewControl_ItemInvoked: No valid item invoked.");
        }
    }

    [DynamicWindowsRuntimeCast(typeof(NavigationViewItem))]
    private void NavigationFrame_Navigated(object sender, NavigationEventArgs e)
    {
        if (e.SourcePageType == typeof(SettingsPage) || (NavigationFrame.SourcePageType == typeof(SettingsPage)))
        {
            // SettingsItem is not part of NavView.MenuItems, and doesn't have a Tag.
            NavigationViewControl.SelectedItem = (NavigationViewItem)NavigationViewControl.SettingsItem;

            return;
        }
        else if (e.SourcePageType != null)
        {
            //NavigationViewControl.Header = null;

            //var item = _pages.FirstOrDefault(p => p.Page == e.SourcePageType);

            //NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First(n => n.Tag.Equals(item.Tag));

            //NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();

            // Do nothing.
        }



    }

    private void NavigationFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        Debug.WriteLine("NavigationFrame_NavigationFailed: " + e.Exception.Message + " - " + e.Exception.StackTrace);

        (App.Current as App)?.AppendErrorLog("NavigationFrame_NavigationFailed", e.Exception.Message + ", StackTrace: " + e.Exception.StackTrace);

        e.Handled = true;
    }
}
