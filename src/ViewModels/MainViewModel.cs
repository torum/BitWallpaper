using BitWallpaper.Helpers;
using BitWallpaper.Models;
using BitWallpaper.Services.Contracts;
using BitWallpaper.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System.Collections.ObjectModel;
using System.Reflection;
using Windows.ApplicationModel;
using WinRT;

namespace BitWallpaper.ViewModels;

public record IsEnabledState
{
    public bool IsEnabled { get; set; }
    public PairCodes PairCode { get; set; }
}

public class ShowBalloonEventArgs(string title, string text) : EventArgs
{
    public string? Title { get; private set; } = title;
    public string? Text { get; private set; } = text;
}

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly bool _isWebSocketOn = true;
    private readonly bool _isLiveChartsOn = true;

    private static readonly SemaphoreSlim _asyncLock = new SemaphoreSlim(1, 1);
    private bool _isNotificationManagerInitialized;
    private bool _isNotificationEnabled = true;

    private readonly IBitbankStreamService _bitbankStreamService;
    private readonly IConfigurationService _configurationService;
    private readonly IDispatcherService _dispatcherService;
    private readonly BitWallpaper.Helpers.NotificationManager notificationManager = new();

    public event EventHandler<ShowBalloonEventArgs>? ShowBalloon;
    public event EventHandler<IsEnabledState>? IsEnabledChanged;

    public MainViewModel(IBitbankStreamService bitbankStreamService, IConfigurationService configurationService, IDispatcherService dispatcherService)
    {
        _bitbankStreamService = bitbankStreamService;
        _configurationService = configurationService;
        _dispatcherService = dispatcherService;

        var config = _configurationService.CurrentConfiguration;
        IsSaveLog = config.IsSaveLog;
        IsNavigationViewMenuOpen = config.IsNavigationViewMenuOpen;

        IsOnBtcJpy = config.IsOnBtcJpy;
        IsOnEthJpy = config.IsOnEthJpy;
        IsOnXrpJpy = config.IsOnXrpJpy;

        IsOnLtcJpy = config.IsOnLtcJpy;
        IsOnMonaJpy = config.IsOnMonaJpy;
        IsOnBccJpy = config.IsOnBccJpy;
        IsOnXlmJpy = config.IsOnXlmJpy;
        IsOnQtumJpy = config.IsOnQtumJpy;
        IsOnBatJpy = config.IsOnBatJpy;
        IsOnOmgJpy = config.IsOnOmgJpy;
        IsOnXymJpy = config.IsOnXymJpy;
        IsOnLinkJpy = config.IsOnLinkJpy;
        IsOnMkrJpy = false;//config.IsOnMkrJpy; // to sky_jpy SKY/JPY
        IsOnBobaJpy = config.IsOnBobaJpy;
        IsOnEnjJpy = false;//config.IsOnEnjJpy; // no longer supported
        IsOnMaticJpy = false;//config.IsOnMaticJpy; // to pol_jpy POL/JPY
        IsOnDotJpy = config.IsOnDotJpy;
        IsOnDogeJpy = config.IsOnDogeJpy;
        IsOnAstrJpy = config.IsOnAstrJpy;
        IsOnAdaJpy = config.IsOnAdaJpy;
        IsOnAvaxJpy = config.IsOnAvaxJpy;
        IsOnAxsJpy = config.IsOnAxsJpy;
        IsOnFlrJpy = config.IsOnFlrJpy;
        IsOnSandJpy = config.IsOnSandJpy;
        OnPropertyChanged(nameof(EnabledPairs));

        // Load the pairs from the configuration service into the Pairs collection
        _configurationService.GetPairs(Pairs);

        IsEnabledChanged += OnIsEnabledChanged;
        ShowBalloon += DoShowBalloon;

        _bitbankStreamService.TickerUpdated += OnBitbankStreamService_TickerUpdated;
        _bitbankStreamService.DepthUpdated += OnBitbankStreamService_DepthUpdated;

        // Will crash due to WinAppSDK bug.
        //https://github.com/microsoft/WindowsAppSDK/issues/6071
        try
        {
            AppNotificationManager.Default.Register();
            _isNotificationManagerInitialized = true;
        }
        catch (System.Runtime.InteropServices.COMException ex)
        {
            _isNotificationEnabled = false;
            _isNotificationManagerInitialized = false;
            Debug.WriteLine("NotificationManager registration failed: " + ex);

            foreach (var pair in Pairs)
            {
                pair.IsNotificationEnabled = _isNotificationEnabled;
            }
        }

        // For developing purposes, we can set this to true to avoid starting the ticker service and charts loading.
        _isWebSocketOn = true;
        _isLiveChartsOn = true;

#if DEBUG
        IsSaveLog = true;

#else
        IsSaveLog = false;
#endif
    }

    #region == Properties ==

    public bool IsSaveLog { get; set; }

    #region == View related ==

    public bool IsNavigationViewMenuOpen { get; set; } = true;

    public int UniformGridColumns
    {
        get
        {
            int count = EnabledPairs.Count;//Pairs.Count(p => p.IsEnabled);
            if (count <= 0) return 1;
            if (count == 1) return 1;
            if (count == 2) return 2;
            if (count <= 4) return 2;
            if (count <= 6) return 3;
            if (count <= 9) return 3;
            if (count <= 12) return 4;
            if (count <= 16) return 4;
            if (count <= 20) return 5;
            return 5; // Default to a maximum of 5 columns for more than 20 pairs.
        }
    }

    #endregion

    #region == Application general ==

    private SystemBackdropOption _material = SystemBackdropOption.Mica;
    public SystemBackdropOption Material
    {
        get => _material;
        set
        {
            if (_material == value)
            {
                return;
            }

            _material = value;
            OnPropertyChanged(nameof(Material));
        }
    }

    private bool _isAcrylicSupported;
    public bool IsAcrylicSupported
    {
        get => _isAcrylicSupported;
        set
        {
            if (_isAcrylicSupported == value)
            {
                return;
            }

            _isAcrylicSupported = value;
            OnPropertyChanged(nameof(IsAcrylicSupported));
        }
    }

    private bool _isMicaSupported;
    public bool IsMicaSupported
    {
        get => _isMicaSupported;
        set
        {
            if (_isMicaSupported == value)
            {
                return;
            }

            _isMicaSupported = value;
            OnPropertyChanged(nameof(IsMicaSupported));
        }
    }


    private bool _isBackdropSupported = true;
    public bool IsBackdropSupported
    {
        get => _isBackdropSupported;
        set
        {
            if (_isBackdropSupported == value)
            {
                return;
            }

            _isBackdropSupported = value;
            OnPropertyChanged(nameof(IsBackdropSupported));
        }
    }

    private ElementTheme _theme = ElementTheme.Default;
    public ElementTheme Theme
    {
        get => _theme;
        set
        {
            if (_theme == value)
            {
                return;
            }

            _theme = value;
            OnPropertyChanged(nameof(ElementTheme));
        }
    }
#pragma warning disable IDE0079 // 不要な抑制を削除します
#pragma warning disable CA1822 // メンバーを static に設定します

    public string VersionText

#pragma warning restore CA1822 // メンバーを static に設定します
#pragma warning restore IDE0079 // 不要な抑制を削除します
    {
        get
        {
            Version version;

            if (RuntimeHelper.IsMSIX)
            {
                var packageVersion = Package.Current.Id.Version;

                version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
            }
            else
            {
                version = Assembly.GetExecutingAssembly().GetName().Version!;
            }

            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }

    #endregion

    #region == Pairs ==

    public ObservableCollection<PairViewModel> Pairs { get; } = [
        new PairViewModel(PairCodes.btc_jpy, 24, "{0:#,0}", "C", 100M, 1000M),
        new PairViewModel(PairCodes.xrp_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.eth_jpy, 24, "{0:#,0}", "C", 100M, 1000M),
        new PairViewModel(PairCodes.ltc_jpy, 24, "{0:#,0.0}", "C1", 100M, 1000M),
        new PairViewModel(PairCodes.mona_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 1M),
        new PairViewModel(PairCodes.bcc_jpy, 24, "{0:#,0}", "C", 10M, 100M),
        new PairViewModel(PairCodes.xlm_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.qtum_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.bat_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.omg_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.xym_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.link_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        //new PairViewModel(PairCodes.mkr_jpy, 24, "{0:#,0}", "C", 100M, 1000M),  // to sky_jpy SKY/JPY
        new PairViewModel(PairCodes.boba_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        //new PairViewModel(PairCodes.enj_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M), // no longer supported
        //new PairViewModel(PairCodes.matic_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M), // to pol_jpy POL/JPY
        new PairViewModel(PairCodes.dot_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.doge_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.astr_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.ada_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.avax_jpy, 24, "{0:#,0.000}", "C3", 100M, 1000M),
        new PairViewModel(PairCodes.axs_jpy, 24, "{0:#,0.000}", "C3", 100M, 1000M),
        new PairViewModel(PairCodes.flr_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.sand_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
    ];

    public ObservableCollection<PairViewModel> EnabledPairs
    {
        get
        {
             return field ?? new ObservableCollection<PairViewModel>(Pairs.Where(x => x.IsEnabled));
        }
        private set
        {
            field = value;
            OnPropertyChanged(nameof(EnabledPairs));
        }
    }

    private PairViewModel? _selectedPair;
    public PairViewModel? SelectedPair
    {
        get => _selectedPair;
        set
        {
            if (_selectedPair == value)
            {
                return;
            }

            _selectedPair = value;

            if (_selectedPair != null)
            {
                foreach (var hoge in Pairs)
                {
                    if (hoge.PairCode == _selectedPair.PairCode)
                    {
                        hoge.IsSelectedActive = true;
                    }
                    else
                    {
                        hoge.IsSelectedActive = false;
                    }
                }
            }

            OnPropertyChanged();
        }
    }

    #endregion

    #region == IsOn ==

    private bool _isOnBtcJpy = true;
    public bool IsOnBtcJpy
    {
        get => _isOnBtcJpy;
        set
        {
            if (_isOnBtcJpy != value)
            {
                _isOnBtcJpy = value;
                OnPropertyChanged(nameof(IsOnBtcJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.btc_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                }
            }
        }
    }

    private bool _isOnXrpJpy = true;
    public bool IsOnXrpJpy
    {
        get => _isOnXrpJpy;
        set
        {
            if (_isOnXrpJpy != value)
            {
                _isOnXrpJpy = value;
                OnPropertyChanged(nameof(IsOnXrpJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.xrp_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                }
            }
        }
    }

    private bool _isOnEthJpy = true;
    public bool IsOnEthJpy
    {
        get => _isOnEthJpy;
        set
        {
            if (_isOnEthJpy != value)
            {
                _isOnEthJpy = value;
                OnPropertyChanged(nameof(IsOnEthJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.eth_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnLtcJpy = false;
    public bool IsOnLtcJpy
    {
        get => _isOnLtcJpy;
        set
        {
            if (_isOnLtcJpy != value)
            {
                _isOnLtcJpy = value;
                OnPropertyChanged(nameof(IsOnLtcJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.ltc_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnBccJpy = false;
    public bool IsOnBccJpy
    {
        get => _isOnBccJpy;
        set
        {
            if (_isOnBccJpy != value)
            {
                _isOnBccJpy = value;
                OnPropertyChanged(nameof(IsOnBccJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.bcc_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnMonaJpy = false;
    public bool IsOnMonaJpy
    {
        get => _isOnMonaJpy;
        set
        {
            if (_isOnMonaJpy != value)
            {
                _isOnMonaJpy = value;
                OnPropertyChanged(nameof(IsOnMonaJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.mona_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnXlmJpy = false;
    public bool IsOnXlmJpy
    {
        get => _isOnXlmJpy;
        set
        {
            if (_isOnXlmJpy != value)
            {
                _isOnXlmJpy = value;
                OnPropertyChanged(nameof(IsOnXlmJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.xlm_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnQtumJpy = false;
    public bool IsOnQtumJpy
    {
        get => _isOnQtumJpy;
        set
        {
            if (_isOnQtumJpy != value)
            {
                _isOnQtumJpy = value;
                OnPropertyChanged(nameof(IsOnQtumJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.qtum_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnBatJpy = false;
    public bool IsOnBatJpy
    {
        get => _isOnBatJpy;
        set
        {
            if (_isOnBatJpy != value)
            {
                _isOnBatJpy = value;
                OnPropertyChanged(nameof(IsOnBatJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.bat_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnOmgJpy = false;
    public bool IsOnOmgJpy
    {
        get => _isOnOmgJpy;
        set
        {
            if (_isOnOmgJpy != value)
            {
                _isOnOmgJpy = value;
                OnPropertyChanged(nameof(IsOnOmgJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.omg_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnXymJpy = false;
    public bool IsOnXymJpy
    {
        get => _isOnXymJpy;
        set
        {
            if (_isOnXymJpy != value)
            {
                _isOnXymJpy = value;
                OnPropertyChanged(nameof(IsOnXymJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.xym_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnLinkJpy = false;
    public bool IsOnLinkJpy
    {
        get => _isOnLinkJpy;
        set
        {
            if (_isOnLinkJpy != value)
            {
                _isOnLinkJpy = value;
                OnPropertyChanged(nameof(IsOnLinkJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.link_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnMkrJpy = false;
    public bool IsOnMkrJpy
    {
        get => _isOnMkrJpy;
        set
        {
            if (_isOnMkrJpy != value)
            {
                _isOnMkrJpy = value;
                OnPropertyChanged(nameof(IsOnMkrJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.mkr_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnBobaJpy = false;
    public bool IsOnBobaJpy
    {
        get => _isOnBobaJpy;
        set
        {
            if (_isOnBobaJpy != value)
            {
                _isOnBobaJpy = value;
                OnPropertyChanged(nameof(IsOnBobaJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.boba_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnEnjJpy = false;
    public bool IsOnEnjJpy
    {
        get => _isOnEnjJpy;
        set
        {
            if (_isOnEnjJpy != value)
            {
                _isOnEnjJpy = value;
                OnPropertyChanged(nameof(IsOnEnjJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.enj_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnMaticJpy = false;
    public bool IsOnMaticJpy
    {
        get => _isOnMaticJpy;
        set
        {
            if (_isOnMaticJpy != value)
            {
                _isOnMaticJpy = value;
                OnPropertyChanged(nameof(IsOnMaticJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.matic_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnDotJpy = false;
    public bool IsOnDotJpy
    {
        get => _isOnDotJpy;
        set
        {
            if (_isOnDotJpy != value)
            {
                _isOnDotJpy = value;
                OnPropertyChanged(nameof(IsOnDotJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.dot_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnDogeJpy = false;
    public bool IsOnDogeJpy
    {
        get => _isOnDogeJpy;
        set
        {
            if (_isOnDogeJpy != value)
            {
                _isOnDogeJpy = value;
                OnPropertyChanged(nameof(IsOnDogeJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.doge_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnAstrJpy = false;
    public bool IsOnAstrJpy
    {
        get => _isOnAstrJpy;
        set
        {
            if (_isOnAstrJpy != value)
            {
                _isOnAstrJpy = value;
                OnPropertyChanged(nameof(IsOnAstrJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.astr_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnAdaJpy = false;
    public bool IsOnAdaJpy
    {
        get => _isOnAdaJpy;
        set
        {
            if (_isOnAdaJpy != value)
            {
                _isOnAdaJpy = value;
                OnPropertyChanged(nameof(IsOnAdaJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.ada_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnAvaxJpy = false;
    public bool IsOnAvaxJpy
    {
        get => _isOnAvaxJpy;
        set
        {
            if (_isOnAvaxJpy != value)
            {
                _isOnAvaxJpy = value;
                OnPropertyChanged(nameof(IsOnAvaxJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.avax_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnAxsJpy = false;
    public bool IsOnAxsJpy
    {
        get => _isOnAxsJpy;
        set
        {
            if (_isOnAxsJpy != value)
            {
                _isOnAxsJpy = value;
                OnPropertyChanged(nameof(IsOnAxsJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.axs_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnFlrJpy = false;
    public bool IsOnFlrJpy
    {
        get => _isOnFlrJpy;
        set
        {
            if (_isOnFlrJpy != value)
            {
                _isOnFlrJpy = value;
                OnPropertyChanged(nameof(IsOnFlrJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.flr_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    private bool _isOnSandJpy = false;
    public bool IsOnSandJpy
    {
        get => _isOnSandJpy;
        set
        {
            if (_isOnSandJpy != value)
            {
                _isOnSandJpy = value;
                OnPropertyChanged(nameof(IsOnSandJpy));
            }

            var pair = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.sand_jpy);
            if (pair is not null)
            {
                if (pair.IsEnabled != value)
                {
                    pair.IsEnabled = value;
                    IsEnabledChanged?.Invoke(this, new IsEnabledState { IsEnabled = value, PairCode = pair.PairCode });
                    OnPropertyChanged(nameof(EnabledPairs));
                    OnPropertyChanged(nameof(UniformGridColumns));
                }
            }
        }
    }

    #endregion

    #region == LTP PercentChange ==

    private string _ltpBtcJpyPercentChange = "";
    public string LtpBtcJpyPercentChange
    {
        get => _ltpBtcJpyPercentChange;
        set
        {
            if (_ltpBtcJpyPercentChange == value)
            {
                return;
            }

            _ltpBtcJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpBtcJpyPercentChange));
        }
    }

    private string _ltpXrpJpyPercentChange = "";
    public string LtpXrpJpyPercentChange
    {
        get => _ltpXrpJpyPercentChange;
        set
        {
            if (_ltpXrpJpyPercentChange == value)
            {
                return;
            }

            _ltpXrpJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpXrpJpyPercentChange));
        }
    }

    private string _ltpEthJpyPercentChange = "";
    public string LtpEthJpyPercentChange
    {
        get => _ltpEthJpyPercentChange;
        set
        {
            if (_ltpEthJpyPercentChange == value)
            {
                return;
            }

            _ltpEthJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpEthJpyPercentChange));
        }
    }

    private string _ltpLtcJpyPercentChange = "";
    public string LtpLtcJpyPercentChange
    {
        get => _ltpLtcJpyPercentChange;
        set
        {
            if (_ltpLtcJpyPercentChange == value)
            {
                return;
            }

            _ltpLtcJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpLtcJpyPercentChange));
        }
    }

    private string _ltpBccJpyPercentChange = "";
    public string LtpBccJpyPercentChange
    {
        get => _ltpBccJpyPercentChange;
        set
        {
            if (_ltpBccJpyPercentChange == value)
            {
                return;
            }

            _ltpBccJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpBccJpyPercentChange));
        }
    }


    private string _ltpMonaJpyPercentChange = "";
    public string LtpMonaJpyPercentChange
    {
        get => _ltpMonaJpyPercentChange;
        set
        {
            if (_ltpMonaJpyPercentChange == value)
            {
                return;
            }

            _ltpMonaJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpMonaJpyPercentChange));
        }
    }

    private string _ltpXlmJpyPercentChange = "";
    public string LtpXlmJpyPercentChange
    {
        get => _ltpXlmJpyPercentChange;
        set
        {
            if (_ltpXlmJpyPercentChange == value)
            {
                return;
            }

            _ltpXlmJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpXlmJpyPercentChange));
        }
    }

    private string _ltpQtumJpyPercentChange = "";
    public string LtpQtumJpyPercentChange
    {
        get => _ltpQtumJpyPercentChange;
        set
        {
            if (_ltpQtumJpyPercentChange == value)
            {
                return;
            }

            _ltpQtumJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpQtumJpyPercentChange));
        }
    }

    private string _ltpBatJpyPercentChange = "";
    public string LtpBatJpyPercentChange
    {
        get => _ltpBatJpyPercentChange;
        set
        {
            if (_ltpBatJpyPercentChange == value)
            {
                return;
            }

            _ltpBatJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpBatJpyPercentChange));
        }
    }

    private string _ltpOmgJpyPercentChange = "";
    public string LtpOmgJpyPercentChange
    {
        get => _ltpOmgJpyPercentChange;
        set
        {
            if (_ltpOmgJpyPercentChange == value)
            {
                return;
            }

            _ltpOmgJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpOmgJpyPercentChange));
        }
    }

    private string _ltpXymJpyPercentChange = "";
    public string LtpXymJpyPercentChange
    {
        get => _ltpXymJpyPercentChange;
        set
        {
            if (_ltpXymJpyPercentChange == value)
            {
                return;
            }

            _ltpXymJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpXymJpyPercentChange));
        }
    }

    private string _ltpLinkJpyPercentChange = "";
    public string LtpLinkJpyPercentChange
    {
        get => _ltpLinkJpyPercentChange;
        set
        {
            if (_ltpLinkJpyPercentChange == value)
            {
                return;
            }

            _ltpLinkJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpLinkJpyPercentChange));
        }
    }

    private string _ltpMkrJpyPercentChange = "";
    public string LtpMkrJpyPercentChange
    {
        get => _ltpMkrJpyPercentChange;
        set
        {
            if (_ltpMkrJpyPercentChange == value)
            {
                return;
            }

            _ltpMkrJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpMkrJpyPercentChange));
        }
    }


    private string _ltpBobaJpyPercentChange = "";
    public string LtpBobaJpyPercentChange
    {
        get => _ltpBobaJpyPercentChange;
        set
        {
            if (_ltpBobaJpyPercentChange == value)
            {
                return;
            }

            _ltpBobaJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpBobaJpyPercentChange));
        }
    }

    private string _ltpEnjJpyPercentChange = "";
    public string LtpEnjJpyPercentChange
    {
        get => _ltpEnjJpyPercentChange;
        set
        {
            if (_ltpEnjJpyPercentChange == value)
            {
                return;
            }

            _ltpEnjJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpEnjJpyPercentChange));
        }
    }


    private string _ltpMaticJpyPercentChange = "";
    public string LtpMaticJpyPercentChange
    {
        get => _ltpMaticJpyPercentChange;
        set
        {
            if (_ltpMaticJpyPercentChange == value)
            {
                return;
            }

            _ltpMaticJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpMaticJpyPercentChange));
        }
    }

    private string _ltpDotJpyPercentChange = "";
    public string LtpDotJpyPercentChange
    {
        get => _ltpDotJpyPercentChange;
        set
        {
            if (_ltpDotJpyPercentChange == value)
            {
                return;
            }

            _ltpDotJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpDotJpyPercentChange));
        }
    }

    private string _ltpDogeJpyPercentChange = "";
    public string LtpDogeJpyPercentChange
    {
        get => _ltpDogeJpyPercentChange;
        set
        {
            if (_ltpDogeJpyPercentChange == value)
            {
                return;
            }

            _ltpDogeJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpDogeJpyPercentChange));
        }
    }

    private string _ltpAstrJpyPercentChange = "";
    public string LtpAstrJpyPercentChange
    {
        get => _ltpAstrJpyPercentChange;
        set
        {
            if (_ltpAstrJpyPercentChange == value)
            {
                return;
            }

            _ltpAstrJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpAstrJpyPercentChange));
        }
    }

    private string _ltpAdaJpyPercentChange = "";
    public string LtpAdaJpyPercentChange
    {
        get => _ltpAdaJpyPercentChange;
        set
        {
            if (_ltpAdaJpyPercentChange == value)
            {
                return;
            }

            _ltpAdaJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpAdaJpyPercentChange));
        }
    }


    private string _ltpAvaxJpyPercentChange = "";
    public string LtpAvaxJpyPercentChange
    {
        get => _ltpAvaxJpyPercentChange;
        set
        {
            if (_ltpAvaxJpyPercentChange == value)
            {
                return;
            }

            _ltpAvaxJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpAvaxJpyPercentChange));
        }
    }


    private string _ltpAxsJpyPercentChange = "";
    public string LtpAxsJpyPercentChange
    {
        get => _ltpAxsJpyPercentChange;
        set
        {
            if (_ltpAxsJpyPercentChange == value)
            {
                return;
            }

            _ltpAxsJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpAxsJpyPercentChange));
        }
    }

    private string _ltpFlrJpyPercentChange = "";
    public string LtpFlrJpyPercentChange
    {
        get => _ltpFlrJpyPercentChange;
        set
        {
            if (_ltpFlrJpyPercentChange == value)
            {
                return;
            }

            _ltpFlrJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpFlrJpyPercentChange));
        }
    }

    private string _ltpSandJpyPercentChange = "";
    public string LtpSandJpyPercentChange
    {
        get => _ltpSandJpyPercentChange;
        set
        {
            if (_ltpSandJpyPercentChange == value)
            {
                return;
            }

            _ltpSandJpyPercentChange = value;
            OnPropertyChanged(nameof(LtpSandJpyPercentChange));
        }
    }

    #endregion

    #region == LTP ==

    private string _ltpBtcJpy = "...";
    public string LtpBtcJpy
    {
        get => _ltpBtcJpy;
        set
        {
            if (_ltpBtcJpy == value)
            {
                return;
            }

            _ltpBtcJpy = value;
            OnPropertyChanged(nameof(LtpBtcJpy));
        }
    }

    private string _ltpXrpJpy = "...";
    public string LtpXrpJpy
    {
        get => _ltpXrpJpy;
        set
        {
            if (_ltpXrpJpy == value)
            {
                return;
            }

            _ltpXrpJpy = value;
            OnPropertyChanged(nameof(LtpXrpJpy));
        }
    }

    private string _ltpEthJpy = "...";
    public string LtpEthJpy
    {
        get => _ltpEthJpy;
        set
        {
            if (_ltpEthJpy == value)
            {
                return;
            }

            _ltpEthJpy = value;
            OnPropertyChanged(nameof(LtpEthJpy));
        }
    }

    private string _ltpLtcJpy = "...";
    public string LtpLtcJpy
    {
        get => _ltpLtcJpy;
        set
        {
            if (_ltpLtcJpy == value)
            {
                return;
            }

            _ltpLtcJpy = value;
            OnPropertyChanged(nameof(LtpLtcJpy));
        }
    }

    private string _ltpMonaJpy = "...";
    public string LtpMonaJpy
    {
        get => _ltpMonaJpy;
        set
        {
            if (_ltpMonaJpy == value)
            {
                return;
            }

            _ltpMonaJpy = value;
            OnPropertyChanged(nameof(LtpMonaJpy));
        }
    }

    private string _ltpBccJpy = "...";
    public string LtpBccJpy
    {
        get => _ltpBccJpy;
        set
        {
            if (_ltpBccJpy == value)
            {
                return;
            }

            _ltpBccJpy = value;
            OnPropertyChanged(nameof(LtpBccJpy));
        }
    }

    private string _ltpXlmJpy = "...";
    public string LtpXlmJpy
    {
        get => _ltpXlmJpy;
        set
        {
            if (_ltpXlmJpy == value)
            {
                return;
            }

            _ltpXlmJpy = value;
            OnPropertyChanged(nameof(LtpXlmJpy));
        }
    }

    private string _ltpQtumJpy = "...";
    public string LtpQtumJpy
    {
        get => _ltpQtumJpy;
        set
        {
            if (_ltpQtumJpy == value)
            {
                return;
            }

            _ltpQtumJpy = value;
            OnPropertyChanged(nameof(LtpQtumJpy));
        }
    }

    private string _ltpBatJpy = "...";
    public string LtpBatJpy
    {
        get => _ltpBatJpy;
        set
        {
            if (_ltpBatJpy == value)
            {
                return;
            }

            _ltpBatJpy = value;
            OnPropertyChanged(nameof(LtpBatJpy));
        }
    }

    private string _ltpOmgJpy = "...";
    public string LtpOmgJpy
    {
        get => _ltpOmgJpy;
        set
        {
            if (_ltpOmgJpy == value)
            {
                return;
            }

            _ltpOmgJpy = value;
            OnPropertyChanged(nameof(LtpOmgJpy));
        }
    }

    private string _ltpXymJpy = "...";
    public string LtpXymJpy
    {
        get => _ltpXymJpy;
        set
        {
            if (_ltpXymJpy == value)
            {
                return;
            }

            _ltpXymJpy = value;
            OnPropertyChanged(nameof(LtpXymJpy));
        }
    }

    private string _ltpLinkJpy = "...";
    public string LtpLinkJpy
    {
        get => _ltpLinkJpy;
        set
        {
            if (_ltpLinkJpy == value)
            {
                return;
            }

            _ltpLinkJpy = value;
            OnPropertyChanged(nameof(LtpLinkJpy));
        }
    }

    private string _ltpMkrJpy = "...";
    public string LtpMkrJpy
    {
        get => _ltpMkrJpy;
        set
        {
            if (_ltpMkrJpy == value)
            {
                return;
            }

            _ltpMkrJpy = value;
            OnPropertyChanged(nameof(LtpMkrJpy));
        }
    }

    private string _ltpBobaJpy = "...";
    public string LtpBobaJpy
    {
        get => _ltpBobaJpy;
        set
        {
            if (_ltpBobaJpy == value)
            {
                return;
            }

            _ltpBobaJpy = value;
            OnPropertyChanged(nameof(LtpBobaJpy));
        }
    }

    private string _ltpEnjJpy = "...";
    public string LtpEnjJpy
    {
        get => _ltpEnjJpy;
        set
        {
            if (_ltpEnjJpy == value)
            {
                return;
            }

            _ltpEnjJpy = value;
            OnPropertyChanged(nameof(LtpEnjJpy));
        }
    }

    private string _ltpMaticJpy = "...";
    public string LtpMaticJpy
    {
        get => _ltpMaticJpy;
        set
        {
            if (_ltpMaticJpy == value)
            {
                return;
            }

            _ltpMaticJpy = value;
            OnPropertyChanged(nameof(LtpMaticJpy));
        }
    }

    private string _ltpDotJpy = "...";
    public string LtpDotJpy
    {
        get => _ltpDotJpy;
        set
        {
            if (_ltpDotJpy == value)
            {
                return;
            }

            _ltpDotJpy = value;
            OnPropertyChanged(nameof(LtpDotJpy));
        }
    }

    private string _ltpDogeJpy = "...";
    public string LtpDogeJpy
    {
        get => _ltpDogeJpy;
        set
        {
            if (_ltpDogeJpy == value)
            {
                return;
            }

            _ltpDogeJpy = value;
            OnPropertyChanged(nameof(LtpDogeJpy));
        }
    }

    private string _ltpAstrJpy = "...";
    public string LtpAstrJpy
    {
        get => _ltpAstrJpy;
        set
        {
            if (_ltpAstrJpy == value)
            {
                return;
            }

            _ltpAstrJpy = value;
            OnPropertyChanged(nameof(LtpAstrJpy));
        }
    }

    private string _ltpAdaJpy = "...";
    public string LtpAdaJpy
    {
        get => _ltpAdaJpy;
        set
        {
            if (_ltpAdaJpy == value)
            {
                return;
            }

            _ltpAdaJpy = value;
            OnPropertyChanged(nameof(LtpAdaJpy));
        }
    }

    private string _ltpAvaxJpy = "...";
    public string LtpAvaxJpy
    {
        get => _ltpAvaxJpy;
        set
        {
            if (_ltpAvaxJpy == value)
            {
                return;
            }

            _ltpAvaxJpy = value;
            OnPropertyChanged(nameof(LtpAvaxJpy));
        }
    }

    private string _ltpAxsJpy = "...";
    public string LtpAxsJpy
    {
        get => _ltpAxsJpy;
        set
        {
            if (_ltpAxsJpy == value)
            {
                return;
            }

            _ltpAxsJpy = value;
            OnPropertyChanged(nameof(LtpAxsJpy));
        }
    }

    private string _ltpFlrJpy = "...";
    public string LtpFlrJpy
    {
        get => _ltpFlrJpy;
        set
        {
            if (_ltpFlrJpy == value)
            {
                return;
            }

            _ltpFlrJpy = value;
            OnPropertyChanged(nameof(LtpFlrJpy));
        }
    }

    private string _ltpSandJpy = "...";
    public string LtpSandJpy
    {
        get => _ltpSandJpy;
        set
        {
            if (_ltpSandJpy == value)
            {
                return;
            }

            _ltpSandJpy = value;
            OnPropertyChanged(nameof(LtpSandJpy));
        }
    }


    #endregion

    #endregion

    #region == Public Methods ==

    public async Task InitializeAndStartAsync()
    {
        try
        {
            _bitbankStreamService.Initialize(Pairs.Where(x => x.IsEnabled).Select(x => x.PairCode).ToList());

            if (_isWebSocketOn)
                await _bitbankStreamService.StartAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in InitializeAndStartAsync: {ex}");

            _dispatcherService.TryEnqueue(() => {
                (App.Current as App)?.AppendErrorLog("Error InitializeAndStartAsync", ex.ToString());
                (App.Current as App) ?.SaveErrorLog();
            });
        }
    }

    public async Task SetSelectedPairFromCodeAsync(PairCodes pairCode)
    {
        if (_selectedPair?.PairCode == pairCode)
        {
            return;
        }
        else
        {
            Debug.WriteLine($"SetSelectedPairFromCodeAsync() called for {pairCode} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        }

        await _asyncLock.WaitAsync();

        try
        {
            SelectedPair = EnabledPairs.FirstOrDefault(x => x.PairCode == pairCode);

            if (SelectedPair is not null)
            {
                if (_isLiveChartsOn)
                    await SelectedPair.InitializeAndLoadAsync();
                //_ = RunSafeAsync(() => SelectedPair.InitializeAndLoadAsync(), nameof(SelectedPair.InitializeAndLoadAsync));
            }
            else
            {
                Debug.WriteLine($"No PairViewModel found for {pairCode} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            }

        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in SetSelectedPairFromCodeAsync: {ex}");
        }
        finally
        {
            _asyncLock.Release();
        }
    }

    public async Task SetSelectedAllPairsFromCodeAsync()
    {
        await _asyncLock.WaitAsync();

        try
        {
            foreach (var pairVM in EnabledPairs)
            {
                pairVM.IsSelectedActive = true;

                if (_isLiveChartsOn)
                    await pairVM.InitializeAndLoadAsync();
                //_ = RunSafeAsync(() => pairVM.InitializeAndLoadAsync(), nameof(pairVM.InitializeAndLoadAsync));
            }
        }
        finally
        {
            _asyncLock.Release();
        }
    }

    public void UpdateCurrentSettings()
    {
        var config = _configurationService.CurrentConfiguration;
        
        config.Theme = Theme;
        config.SystemBackdrop = Material;

        config.IsSaveLog = IsSaveLog;

        config.IsNavigationViewMenuOpen = IsNavigationViewMenuOpen;

        config.IsOnBtcJpy = IsOnBtcJpy;
        config.IsOnEthJpy = IsOnEthJpy;
        config.IsOnXrpJpy = IsOnXrpJpy;

        config.IsOnLtcJpy = IsOnLtcJpy;
        config.IsOnMonaJpy = IsOnMonaJpy;
        config.IsOnBccJpy = IsOnBccJpy;
        config.IsOnXlmJpy = IsOnXlmJpy;
        config.IsOnQtumJpy = IsOnQtumJpy;
        config.IsOnBatJpy = IsOnBatJpy;
        config.IsOnOmgJpy = IsOnOmgJpy;
        config.IsOnXymJpy = IsOnXymJpy;
        config.IsOnLinkJpy = IsOnLinkJpy;
        config.IsOnMkrJpy = IsOnMkrJpy;
        config.IsOnBobaJpy = IsOnBobaJpy;
        config.IsOnEnjJpy = IsOnEnjJpy;
        config.IsOnMaticJpy = IsOnMaticJpy;
        config.IsOnDotJpy = IsOnDotJpy;
        config.IsOnDogeJpy = IsOnDogeJpy;
        config.IsOnAstrJpy = IsOnAstrJpy;
        config.IsOnAdaJpy = IsOnAdaJpy;
        config.IsOnAvaxJpy = IsOnAvaxJpy;
        config.IsOnAxsJpy = IsOnAxsJpy;
        config.IsOnFlrJpy = IsOnFlrJpy;
        config.IsOnSandJpy = IsOnSandJpy;

    }

    public void Dispose()
    {
        _bitbankStreamService.TickerUpdated -= OnBitbankStreamService_TickerUpdated;
        _bitbankStreamService.DepthUpdated -= OnBitbankStreamService_DepthUpdated;
        IsEnabledChanged -= OnIsEnabledChanged;
        ShowBalloon -= DoShowBalloon;

        try
        {
            foreach (var pair in Pairs)
            {
                pair.Dispose();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error disposing PairViewModels: {ex}");
        }

        if (_isNotificationManagerInitialized)
        {
            AppNotificationManager.Default.Unregister();
            _isNotificationManagerInitialized = false;
        }

        Debug.WriteLine($"MainViewModel disposed at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }

    #endregion

    #region == private methods ==

    private async void OnIsEnabledChanged(object? sender, IsEnabledState e)
    {
        // Update the EnabledPairs collection whenever a pair's IsEnabled state changes
        EnabledPairs = new ObservableCollection<PairViewModel>(Pairs.Where(x => x.IsEnabled));

        try
        {
            if (e.IsEnabled)
            {
                await _bitbankStreamService.JoinTickerRoomAsync(e.PairCode);
            }
            else
            {
                await _bitbankStreamService.LeaveTickerRoomAsync(e.PairCode);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in OnIsEnabledChanged: {ex}");
        }
    }

    private async void OnBitbankStreamService_TickerUpdated(object? sender, BitbankTickerData data)
    {
        /*
        PairCodes pair = data.TickerPair;
        DateTime timeStamp = DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).LocalDateTime;

        // Calculate a quick math metric (e.g., daily percentage change)
        decimal priceChange = data.Last - data.Open;
        decimal percentChange = data.Open != 0 ? (priceChange / data.Open) * 100 : 0;

        // Print out your typed metrics using standard number formatting
        Debug.WriteLine($"=== {pair} NUMERIC DATA [{timeStamp:yyyy-MM-dd HH:mm:ss}] ===");
        Debug.WriteLine($"  Last Price : {data.Last:N0} JPY ({percentChange:+0.00;-0.00;0.00}%)");
        Debug.WriteLine($"  Bid (Buy)  : {data.Buy:N0}   | Ask (Sell) : {data.Sell:N0}");
        Debug.WriteLine($"  24h High   : {data.High:N0}  | 24h Low    : {data.Low:N0}");
        Debug.WriteLine($"  24h Volume : {data.Vol:N4}");
        Debug.WriteLine(new string('=', 45));
        */

        var pairViewModel = Pairs.FirstOrDefault(p => p.PairCode == data.TickerPair);
        if (pairViewModel is not null)
        {
            if (pairViewModel.IsEnabled == false)
            {
                return;
            }

            await pairViewModel.UpdateTickerDataAsync(data);

            decimal priceChange = data.Last - data.Open;
            decimal percentChange = data.Open != 0 ? (priceChange / data.Open) * 100 : 0;

            _dispatcherService.TryEnqueue(() =>
            {
                switch (data.TickerPair)
                {
                    case PairCodes.btc_jpy:
                        LtpBtcJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpBtcJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.xrp_jpy:
                        LtpXrpJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpXrpJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.eth_jpy:
                        LtpEthJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpEthJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.ltc_jpy:
                        LtpLtcJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpLtcJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.mona_jpy:
                        LtpMonaJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpMonaJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.bcc_jpy:
                        LtpBccJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpBccJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.xlm_jpy:
                        LtpXlmJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpXlmJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.qtum_jpy:
                        LtpQtumJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpQtumJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.bat_jpy:
                        LtpBatJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpBatJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.omg_jpy:
                        LtpOmgJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpOmgJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.xym_jpy:
                        LtpXymJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpXymJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.link_jpy:
                        LtpLinkJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpLinkJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.mkr_jpy:
                        LtpMkrJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpMkrJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.boba_jpy:
                        LtpBobaJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpBobaJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.enj_jpy:
                        LtpEnjJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpEnjJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.matic_jpy:
                        LtpMaticJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpMaticJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.dot_jpy:
                        LtpDotJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpDotJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.doge_jpy:
                        LtpDogeJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpDogeJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.astr_jpy:
                        LtpAstrJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpAstrJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.ada_jpy:
                        LtpAdaJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpAdaJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.avax_jpy:
                        LtpAvaxJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpAvaxJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.axs_jpy:
                        LtpAxsJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpAxsJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.flr_jpy:
                        LtpFlrJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpFlrJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    case PairCodes.sand_jpy:
                        LtpSandJpy = string.Format(pairViewModel.LtpFormstString, data.Last);
                        LtpSandJpyPercentChange = string.Format("{0:+0.00;-0.00;0.00}%", percentChange);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            });


            #region == Alarm ==

            if (pairViewModel.AlarmPlus > 0)
            {
                if (pairViewModel.Ltp >= pairViewModel.AlarmPlus)
                {
                    pairViewModel.HighLowInfoTextColorFlag = true;
                    pairViewModel.HighLowInfoText = pairViewModel.PairString + "Alarm";

                    ShowBalloonEventArgs ag = new((pairViewModel.PairString + " High Price Alarm"), pairViewModel.AlarmPlusString);

                    pairViewModel.AlarmPlus = 0;

                    ShowBalloon?.Invoke(this, ag);
                }
            }

            if (pairViewModel.AlarmMinus > 0)
            {
                if (pairViewModel.Ltp <= pairViewModel.AlarmMinus)
                {
                    pairViewModel.HighLowInfoTextColorFlag = false;
                    pairViewModel.HighLowInfoText = pairViewModel.PairString + "Alarm";

                    ShowBalloonEventArgs ag = new(pairViewModel.PairString + " Low Price Alarm", pairViewModel.AlarmMinusString);

                    pairViewModel.AlarmMinus = 0;

                    ShowBalloon?.Invoke(this, ag);
                }
            }

            #endregion
        }
        else
        {
            Debug.WriteLine($"No PairViewModel found for {data.TickerPair} at {DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).LocalDateTime:yyyy-MM-dd HH:mm:ss}");
        }
    }

    private async void OnBitbankStreamService_DepthUpdated(object? sender, BitbankDepthData data)
    {
        var pairViewModel = Pairs.FirstOrDefault(p => p.PairCode == data.DepthPair);
        if (pairViewModel is not null)
        {
            if (pairViewModel.IsEnabled == false)
            {
                return;
            }

            await pairViewModel.UpdateDepthDataAsync(data);
        }
        else
        {
            Debug.WriteLine($"No PairViewModel found for {data.DepthPair} at {DateTimeOffset.FromUnixTimeMilliseconds(data.Timestamp).LocalDateTime:yyyy-MM-dd HH:mm:ss}");
        }
    }

    #endregion

    #region == Commands ==

    [DynamicWindowsRuntimeCast(typeof(FrameworkElement))]
    [RelayCommand]
    private void SwitchTheme(ElementTheme? param)
    {
        if (param is null)
        {
            return;
        }

        if (Theme == param)
        {
            return;
        }

        Theme = (ElementTheme)param;

        var main = App.GetService<MainWindow>();

        if (main.Content is FrameworkElement rootElement)
        {
            rootElement.RequestedTheme = Theme;

            main?.SetCapitionButtonColor();
        }
    }

    [RelayCommand]
    private void SwitchSystemBackdrop(string? backdrop)
    {
        if (backdrop == null)
        {
            return;
        }

        var main = App.GetService<MainWindow>();

        if (Enum.TryParse(backdrop, out SystemBackdropOption cacheBackdrop))
        {
            main?.SwitchBackdrop(cacheBackdrop);
        }
    }

    #endregion

    private void DoShowBalloon(object? sender, ShowBalloonEventArgs arg)
    {
        if (!_isNotificationManagerInitialized)
        {
            Debug.WriteLine("Notification Manager is not initialized. Cannot show notification.");
            return;
        }

        // https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/notifications/app-notifications/app-notifications-quickstart?tabs=cs
        var appNotification = new AppNotificationBuilder().AddText(arg.Title).AddText(arg.Text).SetTimeStamp(new DateTime(2017, 04, 15, 19, 45, 00, DateTimeKind.Utc)).BuildNotification();

        //.AddButton(new AppNotificationButton("OK")
        //.AddArgument("action", "dissmiss"))
        //.SetTimeStamp(new DateTime(2017, 04, 15, 19, 45, 00, DateTimeKind.Utc));

        //.SetScenario(AppNotificationScenario.Alarm)
        AppNotificationManager.Default.Show(appNotification);
    }

}