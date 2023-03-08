using BitWallpaper.Helpers;
using BitWallpaper.Helpers.UICommand1;
using BitWallpaper.Models;
using BitWallpaper.Models.APIClients;
using LiveChartsCore.Themes;
using Microsoft.UI.Xaml;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Storage;

namespace BitWallpaper.ViewModels;

public partial class MainViewModel : ViewModelBase
{

    #region == Application general ==

    private SystemBackdropOption _material = SystemBackdropOption.Mica;
    public SystemBackdropOption Material
    {
        get => _material;
        set
        {
            if (_material == value)
                return;

            _material = value;
            NotifyPropertyChanged(nameof(Material));
        }
    }

    private bool _isAcrylicSupported;
    public bool IsAcrylicSupported
    {
        get => _isAcrylicSupported;
        set
        {
            if (_isAcrylicSupported == value)
                return;

            _isAcrylicSupported = value;
            NotifyPropertyChanged(nameof(IsAcrylicSupported));
        }
    }

    private bool _isSystemBackdropSupported = true;
    public bool IsSystemBackdropSupported
    {
        get => _isSystemBackdropSupported;
        set
        {
            if (_isSystemBackdropSupported == value)
                return;

            _isSystemBackdropSupported = value;
            NotifyPropertyChanged(nameof(IsSystemBackdropSupported));
        }
    }

    private ElementTheme _elementTheme = ElementTheme.Default;
    public ElementTheme ElementTheme
    {
        get => _elementTheme;
        set
        {
            if (_elementTheme == value)
                return;

            _elementTheme = value;
            NotifyPropertyChanged(nameof(ElementTheme));
        }
    }

    public string VersionText {
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

            return $"{"AppDisplayName/Text".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
    public ICommand SwitchThemeCommand
    {
        get; private set;
    }

    public ICommand SwitchSystemBackdropCommand
    {
        get; private set;
    }

    #endregion

    #region == Pairs ==

    public Dictionary<string, PairCodes> GetPairs { get; set; } = new Dictionary<string, PairCodes>()
        {
            {"btc_jpy", PairCodes.btc_jpy},
            {"xrp_jpy", PairCodes.xrp_jpy},
            //{"eth_btc", Pairs.eth_btc},
            {"eth_jpy", PairCodes.eth_jpy},
            //{"ltc_btc", Pairs.ltc_btc},
            {"ltc_jpy", PairCodes.ltc_jpy},
            {"mona_jpy", PairCodes.mona_jpy},
            //{"mona_btc", Pairs.mona_btc},
            {"bcc_jpy", PairCodes.bcc_jpy},
            //{"bcc_btc", Pairs.bcc_btc},
            {"xlm_jpy", PairCodes.xlm_jpy},
            {"qtum_jpy", PairCodes.qtum_jpy},
            {"bat_jpy", PairCodes.bat_jpy},
            {"omg_jpy", PairCodes.omg_jpy},
            {"xym_jpy", PairCodes.xym_jpy},
            {"link_jpy", PairCodes.link_jpy},
            {"mkr_jpy", PairCodes.mkr_jpy},
            {"boba_jpy", PairCodes.boba_jpy},
            {"enj_jpy", PairCodes.enj_jpy},
            {"matic_jpy", PairCodes.matic_jpy},
            {"dot_jpy", PairCodes.dot_jpy},
            {"doge_jpy", PairCodes.doge_jpy},
            {"astr_jpy", PairCodes.astr_jpy},
            {"ada_jpy", PairCodes.ada_jpy},
            {"avax_jpy", PairCodes.avax_jpy},
            {"axs_jpy", PairCodes.axs_jpy},
            {"flr_jpy", PairCodes.flr_jpy},
            {"sand_jpy", PairCodes.sand_jpy},
        };

    private ObservableCollection<PairViewModel> _pairs = new() 
    {
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
        new PairViewModel(PairCodes.mkr_jpy, 24, "{0:#,0}", "C", 100M, 1000M),
        new PairViewModel(PairCodes.boba_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.enj_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.matic_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.dot_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.doge_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.astr_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.ada_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.avax_jpy, 24, "{0:#,0.000}", "C3", 100M, 1000M),
        new PairViewModel(PairCodes.axs_jpy, 24, "{0:#,0.000}", "C3", 100M, 1000M),
        new PairViewModel(PairCodes.flr_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
        new PairViewModel(PairCodes.sand_jpy, 24, "{0:#,0.000}", "C3", 0.1M, 0.01M),
    };
    public ObservableCollection<PairViewModel> Pairs
    {
        get => _pairs;
        set
        {
            if (_pairs == value) return;

            _pairs = value;
            NotifyPropertyChanged(nameof(Pairs));
        }
    }

    private PairViewModel _selectedPair;
    public PairViewModel SelectedPair
    {
        get => _selectedPair;
        set
        {
            if (_selectedPair == value) return;

            _selectedPair = value;

            if (_selectedPair != null)
            {
                foreach (var hoge in _pairs)
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

            NotifyPropertyChanged(nameof(SelectedPair));
        }
    }

    #endregion

    #region == Each Pairs ==

    #region == LTP ==

    private string _ltpBtcJpy = "";
    public string LtpBtcJpy
    {
        get => _ltpBtcJpy;
        set
        {
            if (_ltpBtcJpy == value)
                return;

            _ltpBtcJpy = value;
            NotifyPropertyChanged(nameof(LtpBtcJpy));
        }
    }

    private string _ltpXrpJpy = "";
    public string LtpXrpJpy
    {
        get => _ltpXrpJpy;
        set
        {
            if (_ltpXrpJpy == value)
                return;

            _ltpXrpJpy = value;
            NotifyPropertyChanged(nameof(LtpXrpJpy));
        }
    }

    private string _ltpEthJpy = "";
    public string LtpEthJpy
    {
        get => _ltpEthJpy;
        set
        {
            if (_ltpEthJpy == value)
                return;

            _ltpEthJpy = value;
            NotifyPropertyChanged(nameof(LtpEthJpy));
        }
    }

    private string _ltpLtcJpy = "";
    public string LtpLtcJpy
    {
        get => _ltpLtcJpy;
        set
        {
            if (_ltpLtcJpy == value)
                return;

            _ltpLtcJpy = value;
            NotifyPropertyChanged(nameof(LtpLtcJpy));
        }
    }

    private string _ltpMonaJpy = "";
    public string LtpMonaJpy
    {
        get => _ltpMonaJpy;
        set
        {
            if (_ltpMonaJpy == value)
                return;

            _ltpMonaJpy = value;
            NotifyPropertyChanged(nameof(LtpMonaJpy));
        }
    }

    private string _ltpBccJpy = "";
    public string LtpBccJpy
    {
        get => _ltpBccJpy;
        set
        {
            if (_ltpBccJpy == value)
                return;

            _ltpBccJpy = value;
            NotifyPropertyChanged(nameof(LtpBccJpy));
        }
    }

    private string _ltpXlmJpy = "";
    public string LtpXlmJpy
    {
        get => _ltpXlmJpy;
        set
        {
            if (_ltpXlmJpy == value)
                return;

            _ltpXlmJpy = value;
            NotifyPropertyChanged(nameof(LtpXlmJpy));
        }
    }

    private string _ltpQtumJpy = "";
    public string LtpQtumJpy
    {
        get => _ltpQtumJpy;
        set
        {
            if (_ltpQtumJpy == value)
                return;

            _ltpQtumJpy = value;
            NotifyPropertyChanged(nameof(LtpQtumJpy));
        }
    }

    private string _ltpBatJpy = "";
    public string LtpBatJpy
    {
        get => _ltpBatJpy;
        set
        {
            if (_ltpBatJpy == value)
                return;

            _ltpBatJpy = value;
            NotifyPropertyChanged(nameof(LtpBatJpy));
        }
    }

    private string _ltpOmgJpy = "";
    public string LtpOmgJpy
    {
        get => _ltpOmgJpy;
        set
        {
            if (_ltpOmgJpy == value)
                return;

            _ltpOmgJpy = value;
            NotifyPropertyChanged(nameof(LtpOmgJpy));
        }
    }

    private string _ltpXymJpy = "";
    public string LtpXymJpy
    {
        get => _ltpXymJpy;
        set
        {
            if (_ltpXymJpy == value)
                return;

            _ltpXymJpy = value;
            NotifyPropertyChanged(nameof(LtpXymJpy));
        }
    }

    private string _ltpLinkJpy = "";
    public string LtpLinkJpy
    {
        get => _ltpLinkJpy;
        set
        {
            if (_ltpLinkJpy == value)
                return;

            _ltpLinkJpy = value;
            NotifyPropertyChanged(nameof(LtpLinkJpy));
        }
    }

    private string _ltpMkrJpy = "";
    public string LtpMkrJpy
    {
        get => _ltpMkrJpy;
        set
        {
            if (_ltpMkrJpy == value)
                return;

            _ltpMkrJpy = value;
            NotifyPropertyChanged(nameof(LtpMkrJpy));
        }
    }

    private string _ltpBobaJpy = "";
    public string LtpBobaJpy
    {
        get => _ltpBobaJpy;
        set
        {
            if (_ltpBobaJpy == value)
                return;

            _ltpBobaJpy = value;
            NotifyPropertyChanged(nameof(LtpBobaJpy));
        }
    }

    private string _ltpEnjJpy = "";
    public string LtpEnjJpy
    {
        get => _ltpEnjJpy;
        set
        {
            if (_ltpEnjJpy == value)
                return;

            _ltpEnjJpy = value;
            NotifyPropertyChanged(nameof(LtpEnjJpy));
        }
    }

    private string _ltpMaticJpy = "";
    public string LtpMaticJpy
    {
        get => _ltpMaticJpy;
        set
        {
            if (_ltpMaticJpy == value)
                return;

            _ltpMaticJpy = value;
            NotifyPropertyChanged(nameof(LtpMaticJpy));
        }
    }

    private string _ltpDotJpy = "";
    public string LtpDotJpy
    {
        get => _ltpDotJpy;
        set
        {
            if (_ltpDotJpy == value)
                return;

            _ltpDotJpy = value;
            NotifyPropertyChanged(nameof(LtpDotJpy));
        }
    }

    private string _ltpDogeJpy = "";
    public string LtpDogeJpy
    {
        get => _ltpDogeJpy;
        set
        {
            if (_ltpDogeJpy == value)
                return;

            _ltpDogeJpy = value;
            NotifyPropertyChanged(nameof(LtpDogeJpy));
        }
    }

    private string _ltpAstrJpy = "";
    public string LtpAstrJpy
    {
        get => _ltpAstrJpy;
        set
        {
            if (_ltpAstrJpy == value)
                return;

            _ltpAstrJpy = value;
            NotifyPropertyChanged(nameof(LtpAstrJpy));
        }
    }

    private string _ltpAdaJpy = "";
    public string LtpAdaJpy
    {
        get => _ltpAdaJpy;
        set
        {
            if (_ltpAdaJpy == value)
                return;

            _ltpAdaJpy = value;
            NotifyPropertyChanged(nameof(LtpAdaJpy));
        }
    }

    private string _ltpAvaxJpy = "";
    public string LtpAvaxJpy
    {
        get => _ltpAvaxJpy;
        set
        {
            if (_ltpAvaxJpy == value)
                return;

            _ltpAvaxJpy = value;
            NotifyPropertyChanged(nameof(LtpAvaxJpy));
        }
    }

    private string _ltpAxsJpy = "";
    public string LtpAxsJpy
    {
        get => _ltpAxsJpy;
        set
        {
            if (_ltpAxsJpy == value)
                return;

            _ltpAxsJpy = value;
            NotifyPropertyChanged(nameof(LtpAxsJpy));
        }
    }

    private string _ltpFlrJpy = "";
    public string LtpFlrJpy
    {
        get => _ltpFlrJpy;
        set
        {
            if (_ltpFlrJpy == value)
                return;

            _ltpFlrJpy = value;
            NotifyPropertyChanged(nameof(LtpFlrJpy));
        }
    }

    private string _ltpSandJpy = "";
    public string LtpSandJpy
    {
        get => _ltpSandJpy;
        set
        {
            if (_ltpSandJpy == value)
                return;

            _ltpSandJpy = value;
            NotifyPropertyChanged(nameof(LtpSandJpy));
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
            if (_isOnBtcJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.btc_jpy);
            hoge.IsEnabled = value;

            _isOnBtcJpy = value;
            NotifyPropertyChanged(nameof(IsOnBtcJpy));
        }
    }

    private bool _isOnXrpJpy = true;
    public bool IsOnXrpJpy
    {
        get => _isOnXrpJpy;
        set
        {
            if (_isOnXrpJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.xrp_jpy);
            hoge.IsEnabled = value;

            _isOnXrpJpy = value;
            NotifyPropertyChanged(nameof(IsOnXrpJpy));
        }
    }

    private bool _isOnEthJpy = true;
    public bool IsOnEthJpy
    {
        get => _isOnEthJpy;
        set
        {
            if (_isOnEthJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.eth_jpy);
            hoge.IsEnabled = value;

            _isOnEthJpy = value;
            NotifyPropertyChanged(nameof(IsOnEthJpy));
        }
    }

    private bool _isOnLtcJpy = true;
    public bool IsOnLtcJpy
    {
        get => _isOnLtcJpy;
        set
        {
            if (_isOnLtcJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.ltc_jpy);
            hoge.IsEnabled = value;

            _isOnLtcJpy = value;
            NotifyPropertyChanged(nameof(IsOnLtcJpy));
        }
    }

    private bool _isOnBccJpy = true;
    public bool IsOnBccJpy
    {
        get => _isOnBccJpy;
        set
        {
            if (_isOnBccJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.bcc_jpy);
            hoge.IsEnabled = value;

            _isOnBccJpy = value;
            NotifyPropertyChanged(nameof(IsOnBccJpy));
        }
    }

    private bool _isOnMonaJpy = true;
    public bool IsOnMonaJpy
    {
        get => _isOnMonaJpy;
        set
        {
            if (_isOnMonaJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.mona_jpy);
            hoge.IsEnabled = value;

            _isOnMonaJpy = value;
            NotifyPropertyChanged(nameof(IsOnMonaJpy));
        }
    }

    private bool _isOnXlmJpy = true;
    public bool IsOnXlmJpy
    {
        get => _isOnXlmJpy;
        set
        {
            if (_isOnXlmJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.xlm_jpy);
            hoge.IsEnabled = value;

            _isOnXlmJpy = value;
            NotifyPropertyChanged(nameof(IsOnXlmJpy));
        }
    }

    private bool _isOnQtumJpy = true;
    public bool IsOnQtumJpy
    {
        get => _isOnQtumJpy;
        set
        {
            if (_isOnQtumJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.qtum_jpy);
            hoge.IsEnabled = value;

            _isOnQtumJpy = value;
            NotifyPropertyChanged(nameof(IsOnQtumJpy));
        }
    }

    private bool _isOnBatJpy = true;
    public bool IsOnBatJpy
    {
        get => _isOnBatJpy;
        set
        {
            if (_isOnBatJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.bat_jpy);
            hoge.IsEnabled = value;

            _isOnBatJpy = value;
            NotifyPropertyChanged(nameof(IsOnBatJpy));
        }
    }

    private bool _isOnOmgJpy = false;
    public bool IsOnOmgJpy
    {
        get => _isOnOmgJpy;
        set
        {
            if (_isOnOmgJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.omg_jpy);
            hoge.IsEnabled = value;

            _isOnOmgJpy = value;
            NotifyPropertyChanged(nameof(IsOnOmgJpy));
        }
    }

    private bool _isOnXymJpy = false;
    public bool IsOnXymJpy
    {
        get => _isOnXymJpy;
        set
        {
            if (_isOnXymJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.xym_jpy);
            hoge.IsEnabled = value;

            _isOnXymJpy = value;
            NotifyPropertyChanged(nameof(IsOnXymJpy));
        }
    }

    private bool _isOnLinkJpy = false;
    public bool IsOnLinkJpy
    {
        get => _isOnLinkJpy;
        set
        {
            if (_isOnLinkJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.link_jpy);
            hoge.IsEnabled = value;

            _isOnLinkJpy = value;
            NotifyPropertyChanged(nameof(IsOnLinkJpy));
        }
    }

    private bool _isOnMkrJpy = false;
    public bool IsOnMkrJpy
    {
        get => _isOnMkrJpy;
        set
        {
            if (_isOnMkrJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.mkr_jpy);
            hoge.IsEnabled = value;

            _isOnMkrJpy = value;
            NotifyPropertyChanged(nameof(IsOnMkrJpy));
        }
    }

    private bool _isOnBobaJpy = false;
    public bool IsOnBobaJpy
    {
        get => _isOnBobaJpy;
        set
        {
            if (_isOnBobaJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.boba_jpy);
            hoge.IsEnabled = value;

            _isOnBobaJpy = value;
            NotifyPropertyChanged(nameof(IsOnBobaJpy));
        }
    }

    private bool _isOnEnjJpy = false;
    public bool IsOnEnjJpy
    {
        get => _isOnEnjJpy;
        set
        {
            if (_isOnEnjJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.enj_jpy);
            hoge.IsEnabled = value;

            _isOnEnjJpy = value;
            NotifyPropertyChanged(nameof(IsOnEnjJpy));
        }
    }

    private bool _isOnMaticJpy = false;
    public bool IsOnMaticJpy
    {
        get => _isOnMaticJpy;
        set
        {
            if (_isOnMaticJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.matic_jpy);
            hoge.IsEnabled = value;

            _isOnMaticJpy = value;
            NotifyPropertyChanged(nameof(IsOnMaticJpy));
        }
    }

    private bool _isOnDotJpy = false;
    public bool IsOnDotJpy
    {
        get => _isOnDotJpy;
        set
        {
            if (_isOnDotJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.dot_jpy);
            hoge.IsEnabled = value;

            _isOnDotJpy = value;
            NotifyPropertyChanged(nameof(IsOnDotJpy));
        }
    }

    private bool _isOnDogeJpy = false;
    public bool IsOnDogeJpy
    {
        get => _isOnDogeJpy;
        set
        {
            if (_isOnDogeJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.doge_jpy);
            hoge.IsEnabled = value;

            _isOnDogeJpy = value;
            NotifyPropertyChanged(nameof(IsOnDogeJpy));
        }
    }

    private bool _isOnAstrJpy = false;
    public bool IsOnAstrJpy
    {
        get => _isOnAstrJpy;
        set
        {
            if (_isOnAstrJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.astr_jpy);
            hoge.IsEnabled = value;

            _isOnAstrJpy = value;
            NotifyPropertyChanged(nameof(IsOnAstrJpy));
        }
    }

    private bool _isOnAdaJpy = false;
    public bool IsOnAdaJpy
    {
        get => _isOnAdaJpy;
        set
        {
            if (_isOnAdaJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.ada_jpy);
            hoge.IsEnabled = value;

            _isOnAdaJpy = value;
            NotifyPropertyChanged(nameof(IsOnAdaJpy));
        }
    }

    private bool _isOnAvaxJpy = false;
    public bool IsOnAvaxJpy
    {
        get => _isOnAvaxJpy;
        set
        {
            if (_isOnAvaxJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.avax_jpy);
            hoge.IsEnabled = value;

            _isOnAvaxJpy = value;
            NotifyPropertyChanged(nameof(IsOnAvaxJpy));
        }
    }

    private bool _isOnAxsJpy = false;
    public bool IsOnAxsJpy
    {
        get => _isOnAxsJpy;
        set
        {
            if (_isOnAxsJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.axs_jpy);
            hoge.IsEnabled = value;

            _isOnAxsJpy = value;
            NotifyPropertyChanged(nameof(IsOnAxsJpy));
        }
    }

    private bool _isOnFlrJpy = false;
    public bool IsOnFlrJpy
    {
        get => _isOnFlrJpy;
        set
        {
            if (_isOnFlrJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.flr_jpy);
            hoge.IsEnabled = value;

            _isOnFlrJpy = value;
            NotifyPropertyChanged(nameof(IsOnFlrJpy));
        }
    }

    private bool _isOnSandJpy = false;
    public bool IsOnSandJpy
    {
        get => _isOnSandJpy;
        set
        {
            if (_isOnSandJpy == value)
                return;

            var hoge = Pairs.FirstOrDefault(x => x.PairCode == PairCodes.sand_jpy);
            hoge.IsEnabled = value;

            _isOnSandJpy = value;
            NotifyPropertyChanged(nameof(IsOnSandJpy));
        }
    }

    #endregion

    #endregion

    #region == Options ==

    private bool _navigationViewControl_IsPaneOpen = true;
    public bool NavigationViewControl_IsPaneOpen
    {
        get => _navigationViewControl_IsPaneOpen;
        set
        {
            if (_navigationViewControl_IsPaneOpen == value)
                return;

            _navigationViewControl_IsPaneOpen = value;
            NotifyPropertyChanged(nameof(NavigationViewControl_IsPaneOpen));
        }
    }

    private bool _isChartTooltipVisible = true;
    public bool IsChartTooltipVisible
    {
        get
        {
            return _isChartTooltipVisible;
        }
        set
        {
            if (_isChartTooltipVisible == value)
                return;

            foreach (var hoge in _pairs)
            {
                hoge.IsChartTooltipVisible = value;
            }

            _isChartTooltipVisible = value;
            NotifyPropertyChanged(nameof(IsChartTooltipVisible));
        }
    }

    private bool _isDebugSaveLog = true;
    public bool IsDebugSaveLog
    {
        get
        {
            return _isDebugSaveLog;
        }
        set
        {
            if (_isDebugSaveLog == value)
                return;

            _isDebugSaveLog = value;
            NotifyPropertyChanged(nameof(IsDebugSaveLog));
        }
    }

    #endregion

    // HTTP Clients
    readonly PublicAPIClient _pubTickerApi = new();
    //_pubTickerApi.ErrorOccured += new PrivateAPIClient.ClinetErrorEvent(OnError);

    // Timer
    readonly DispatcherTimer _dispatcherTimerTickAllPairs = new();

    // Event
    public event EventHandler<ShowBalloonEventArgs> ShowBalloon;

    public MainViewModel()
    {
        GetTickers();

        // Ticker update timer
        _dispatcherTimerTickAllPairs.Tick += TickerTimerAllPairs;
        _dispatcherTimerTickAllPairs.Interval = new TimeSpan(0, 0, 2);
        _dispatcherTimerTickAllPairs.Start();

        var manager = WinUIEx.WindowManager.Get(App.MainWindow);
        if (manager.Backdrop is WinUIEx.AcrylicSystemBackdrop)
        {
            Material = SystemBackdropOption.Acrylic;
        }
        else if (manager.Backdrop is WinUIEx.MicaSystemBackdrop)
        {
            Material = SystemBackdropOption.Mica;
        }

        if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
        {
            IsAcrylicSupported = true;
        }
        else
        {
            IsAcrylicSupported = false;

            if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
            {
                //
            }
            else
            {
                IsSystemBackdropSupported = false;
            }
        }

        SwitchThemeCommand = new GenericRelayCommand<ElementTheme>(param => OnSwitchTheme(param), param => CanSwitchThemeExecute());
        SwitchSystemBackdropCommand = new GenericRelayCommand<string>(param => OnSwitchSystemBackdrop(param), param => CanSwitchSystemBackdropExecute());

        //ChangeCandleTypeCommand = new GenericRelayCommand<CandleTypes>(param => ChangeCandleTypeCommand_Execute(param), param => ChangeCandleTypeCommand_CanExecute());

    }

    public void SetSelectedPairFromCode(PairCodes pairCode)
    {
        if (_selectedPair?.PairCode == pairCode) return;

        var fuga = Pairs.FirstOrDefault(x => x.PairCode == pairCode);

        // little hack.
        _selectedPair = fuga;

        if (_selectedPair != null)
        {
            foreach (var hoge in _pairs)
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

        NotifyPropertyChanged(nameof(SelectedPair));

        /*
        Task.Run(() =>
        {
            _selectedPair.InitializeAndStart();
        });
        */
        _selectedPair.InitializeAndLoad();
    }

    public void CleanUp()
    {
        try
        {
            _dispatcherTimerTickAllPairs.Stop();

            _pubTickerApi.Dispose();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error while Shutdown() : " + ex);
        }
    }

    private void TickerTimerAllPairs(object source, object e)
    {
        try
        {
            GetTickers();
        }
        catch(Exception ex)
        {
            Debug.WriteLine("Error while GetTickers() : " + ex);
        }
    }

    private async void GetTickers()
    {
        // 起動直後アラームを鳴らさない秒数
        //int waitTime = 60;

        foreach (var hoge in Pairs)
        {
            if (!hoge.IsEnabled)
                continue;

            try
            {
                Ticker tick = await _pubTickerApi?.GetTicker(hoge.PairCode.ToString());

                if (tick != null)
                {
                    if (hoge.PairCode == PairCodes.btc_jpy)
                    {
                        LtpBtcJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.xrp_jpy)
                    {
                        LtpXrpJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.eth_jpy)
                    {
                        LtpEthJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.ltc_jpy)
                    {
                        LtpLtcJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.mona_jpy)
                    {
                        LtpMonaJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.bcc_jpy)
                    {
                        LtpBccJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.xlm_jpy)
                    {
                        LtpXlmJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.qtum_jpy)
                    {
                        LtpQtumJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.bat_jpy)
                    {
                        LtpBatJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.omg_jpy)
                    {
                        LtpOmgJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.xym_jpy)
                    {
                        LtpXymJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.link_jpy)
                    {
                        LtpLinkJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.mkr_jpy)
                    {
                        LtpMkrJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.boba_jpy)
                    {
                        LtpBobaJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.enj_jpy)
                    {
                        LtpEnjJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.matic_jpy)
                    {
                        LtpMaticJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.dot_jpy)
                    {
                        LtpDotJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.doge_jpy)
                    {
                        LtpDogeJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.astr_jpy)
                    {
                        LtpAstrJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.ada_jpy)
                    {
                        LtpAdaJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.avax_jpy)
                    {
                        LtpAvaxJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.axs_jpy)
                    {
                        LtpAxsJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.flr_jpy)
                    {
                        LtpFlrJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else if (hoge.PairCode == PairCodes.sand_jpy)
                    {
                        LtpSandJpy = String.Format(hoge.LtpFormstString, tick.LTP);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    // TODO: more


                    /*
                    // Up/down flag. 一旦前の値を保存
                    var prevLtp = hoge.Ltp;

                    if (tick.LTP > prevLtp)
                    {
                        hoge.LtpUpFlag = true;
                    }
                    else if (tick.LTP < prevLtp)
                    {
                        hoge.LtpUpFlag = false;
                    }
                    */


                    // 最新の価格をセット
                    hoge.Ltp = tick.LTP;
                    hoge.Bid = tick.Bid;
                    hoge.Ask = tick.Ask;
                    hoge.TickTimeStamp = tick.TimeStamp;
                    hoge.LowestIn24Price = tick.Low;
                    hoge.HighestIn24Price = tick.High;
                    /*

                    // 起動時価格セット
                    if (hoge.BasePrice == 0) hoge.BasePrice = tick.LTP;

                    // 最安値登録
                    if (hoge.LowestPrice == 0)
                    {
                        hoge.LowestPrice = tick.LTP;
                    }
                    if (tick.LTP < hoge.LowestPrice)
                    {
                        hoge.LowestPrice = tick.LTP;
                    }

                    // 最高値登録
                    if (hoge.HighestPrice == 0)
                    {
                        hoge.HighestPrice = tick.LTP;
                    }
                    if (tick.LTP > hoge.HighestPrice)
                    {
                        hoge.HighestPrice = tick.LTP;
                    }
                    */

                    #region == チック履歴 ==
                    /*
                    TickHistory aym = new TickHistory();
                    aym.Price = tick.LTP;
                    aym.TimeAt = tick.TimeStamp;
                    if (hoge.TickHistories.Count > 0)
                    {
                        if (hoge.TickHistories[0].Price > aym.Price)
                        {
                            aym.TickHistoryPriceUp = true;
                            hoge.TickHistories.Insert(0, aym);

                        }
                        else if (hoge.TickHistories[0].Price < aym.Price)
                        {
                            aym.TickHistoryPriceUp = false;
                            hoge.TickHistories.Insert(0, aym);
                        }
                        else
                        {
                            //aym.TickHistoryPriceColor = Colors.Gainsboro;
                            hoge.TickHistories.Insert(0, aym);
                        }
                    }
                    else
                    {
                        //aym.TickHistoryPriceColor = Colors.Gainsboro;
                        hoge.TickHistories.Insert(0, aym);
                    }

                    // limit the number of the list.
                    if (hoge.TickHistories.Count > 60)
                    {
                        hoge.TickHistories.RemoveAt(60);
                    }

                    // 60(1分)の平均値を求める
                    decimal aSum = 0;
                    int c = 0;
                    if (hoge.TickHistories.Count > 0)
                    {

                        if (hoge.TickHistories.Count > 60)
                        {
                            c = 59;
                        }
                        else
                        {
                            c = hoge.TickHistories.Count - 1;
                        }

                        if (c == 0)
                        {
                            hoge.AveragePrice = hoge.TickHistories[0].Price;
                        }
                        else
                        {
                            for (int i = 0; i < c; i++)
                            {
                                aSum = aSum + hoge.TickHistories[i].Price;
                            }
                            hoge.AveragePrice = aSum / c;
                        }

                    }
                    else if (hoge.TickHistories.Count == 1)
                    {
                        hoge.AveragePrice = hoge.TickHistories[0].Price;
                    }
                    */
                    #endregion

                    #region == アラーム ==

                    if (hoge.AlarmPlus > 0)
                    {
                        if (hoge.Ltp >= hoge.AlarmPlus)
                        {
                            hoge.HighLowInfoTextColorFlag = true;
                            hoge.HighLowInfoText = hoge.PairString + "Alarm";

                            ShowBalloonEventArgs ag = new()
                            {
                                Title = hoge.PairString + " High Price Alarm",
                                Text = hoge.AlarmPlusString
                            };

                            // クリア
                            hoge.AlarmPlus = 0;

                            // バルーン表示
                            ShowBalloon?.Invoke(this, ag);
                        }
                    }

                    if (hoge.AlarmMinus > 0)
                    {
                        if (hoge.Ltp <= hoge.AlarmMinus)
                        {
                            hoge.HighLowInfoTextColorFlag = false;
                            hoge.HighLowInfoText = hoge.PairString + "Alarm";

                            ShowBalloonEventArgs ag = new()
                            {
                                Title = hoge.PairString + " Low Price Alarm",
                                Text = hoge.AlarmMinusString
                            };

                            // クリア
                            hoge.AlarmMinus = 0;

                            // バルーン表示
                            ShowBalloon?.Invoke(this, ag);
                        }
                    }

                    /*
                    bool isPlayed = false;

                    // カスタムアラーム
                    if (hoge.AlarmPlus > 0)
                    {
                        if (tick.LTP >= hoge.AlarmPlus)
                        {
                            hoge.HighLowInfoTextColorFlag = true;
                            hoge.HighLowInfoText = hoge.PairString + " ⇑⇑⇑　高値アラーム ";

                            //ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                            //{
                            //    Title = PairBtcJpy.PairString + " 高値アラーム",
                            //    Text = PairBtcJpy.AlarmPlus.ToString("#,0") + " に達しました。"
                            //};
                            // バルーン表示
                            //ShowBalloon?.Invoke(this, ag);

                            // クリア
                            hoge.AlarmPlus = 0;

                        }
                    }

                    if (hoge.AlarmMinus > 0)
                    {
                        if (tick.LTP <= hoge.AlarmMinus)
                        {
                            hoge.HighLowInfoTextColorFlag = false;
                            hoge.HighLowInfoText = hoge.PairString + " ⇓⇓⇓　安値アラーム ";

                            //ShowBalloonEventArgs ag = new ShowBalloonEventArgs
                            //{
                            //    Title = hoge.PairString + " 安値アラーム",
                            //    Text = hoge.AlarmMinus.ToString("#,0") + " に達しました。"
                            //};
                            // バルーン表示
                            //ShowBalloon?.Invoke(this, ag);

                            // クリア
                            hoge.AlarmMinus = 0;

                        }
                    }

                    // 起動後最高値
                    if ((tick.LTP >= hoge.HighestPrice) && (prevLtp != tick.LTP))
                    {
                        if ((hoge.TickHistories.Count > waitTime) && ((hoge.BasePrice + 100M) < tick.LTP))
                        {

                            // 値を点滅させるフラグ
                            if (hoge.PlaySoundHighest)
                                hoge.HighestPriceAlart = true;

                            // 音を鳴らす
                            if ((isPlayed == false) && (hoge.PlaySoundHighest == true))
                            {

                                // 色を変える
                                hoge.HighLowInfoTextColorFlag = true;
                                // テキストを点滅させる為、一旦クリア
                                hoge.HighLowInfoText = "";
                                hoge.HighLowInfoText = hoge.PairString + " ⇑⇑⇑　起動後最高値 ";


                                //if (PlaySound)
                                //{
                                //    SystemSounds.Hand.Play();
                                //    isPlayed = true;
                                //}

                            }
                        }
                    }
                    else
                    {
                        hoge.HighestPriceAlart = false;
                    }

                    // 起動後最安値
                    if ((tick.LTP <= hoge.LowestPrice) && (prevLtp != tick.LTP))
                    {
                        if ((hoge.TickHistories.Count > waitTime) && ((hoge.BasePrice - 100M) > tick.LTP))
                        {

                            if (hoge.PlaySoundLowest)
                                hoge.LowestPriceAlart = true;

                            if ((isPlayed == false) && (hoge.PlaySoundLowest == true))
                            {
                                hoge.HighLowInfoTextColorFlag = false;
                                hoge.HighLowInfoText = "";
                                hoge.HighLowInfoText = hoge.PairString + " ⇓⇓⇓　起動後最安値 ";

                                //if (PlaySound)
                                //{
                                //    SystemSounds.Beep.Play();
                                //    isPlayed = true;
                                //}

                            }

                        }
                    }
                    else
                    {
                        hoge.LowestPriceAlart = false;
                    }

                    // 過去24時間最高値
                    if ((tick.LTP >= hoge.HighestIn24Price) && (prevLtp != tick.LTP) && (hoge.TickHistories.Count > waitTime))
                    {
                        if (hoge.PlaySoundHighest24h)
                            hoge.HighestIn24PriceAlart = true;

                        if ((isPlayed == false) && (hoge.PlaySoundHighest24h == true))
                        {
                            hoge.HighLowInfoTextColorFlag = true;
                            hoge.HighLowInfoText = "";
                            hoge.HighLowInfoText = hoge.PairString + " ⇑⇑⇑⇑⇑⇑　24時間最高値 ";

                            //if (PlaySound)
                            //{
                            //    SystemSounds.Hand.Play();
                            //isPlayed = true;
                            //}

                        }
                    }
                    else
                    {
                        hoge.HighestIn24PriceAlart = false;
                    }

                    // 過去24時間最安値
                    if ((tick.LTP <= hoge.LowestIn24Price) && (prevLtp != tick.LTP) && (hoge.TickHistories.Count > waitTime))
                    {

                        if (hoge.PlaySoundLowest24h)
                            hoge.LowestIn24PriceAlart = true;

                        if ((isPlayed == false) && (hoge.PlaySoundLowest24h == true))
                        {
                            hoge.HighLowInfoTextColorFlag = false;
                            hoge.HighLowInfoText = "";
                            hoge.HighLowInfoText = hoge.PairString + " ⇓⇓⇓⇓⇓⇓　24時間最安値 ";

                            //if (PlaySound)
                            //{
                            //    SystemSounds.Hand.Play();
                            //    isPlayed = true;
                            //}

                        }
                    }
                    else
                    {
                        hoge.LowestIn24PriceAlart = false;
                    }
                    */
                    #endregion

                }
                else
                {
                    // TODO:
                    //APIResultTicker = "<<取得失敗>>";
                    Debug.WriteLine("■■■■■ TickerTimerAllPairs: GetTicker returned null");

                    //await Task.Delay(1000);
                    break;
                }
            }
            catch(TaskCanceledException) { return; }

            if (!hoge.IsEnabled)
                continue;


        }

    }

    private static bool CanSwitchThemeExecute()
    {
        return true;
    }

    private void OnSwitchTheme(ElementTheme theme)
    {
        if (ElementTheme != theme)
        {
            ElementTheme = theme;
            //await _themeSelectorService.SetThemeAsync(theme);
            if (App.MainWindow.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ElementTheme;

                TitleBarHelper.UpdateTitleBar(ElementTheme);

                if (RuntimeHelper.IsMSIX)
                {
                    ApplicationData.Current.LocalSettings.Values[App.ThemeSettingsKey] = theme.ToString();
                }
            }
        }
    }

    private static bool CanSwitchSystemBackdropExecute()
    {
        return true;
    }

    private void OnSwitchSystemBackdrop(string? backdrop)
    {
        if (backdrop != null)
        {
            var manager = WinUIEx.WindowManager.Get(App.MainWindow);

            if (backdrop == "Mica")
            {
                // Win10 says not supported but works. So if Acrylic is supported, we assume it's ok.
                if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported() || Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
                {
                    manager.Backdrop = new WinUIEx.MicaSystemBackdrop();
                    if (RuntimeHelper.IsMSIX)
                    {
                        ApplicationData.Current.LocalSettings.Values[App.BackdropSettingsKey] = SystemBackdropOption.Mica.ToString();
                    }
                    Material = SystemBackdropOption.Mica;
                }
            }
            else if (backdrop == "Acrylic")
            {
                if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
                {
                    manager.Backdrop = new WinUIEx.AcrylicSystemBackdrop();
                    if (RuntimeHelper.IsMSIX)
                    {
                        ApplicationData.Current.LocalSettings.Values[App.BackdropSettingsKey] = SystemBackdropOption.Acrylic.ToString();
                    }
                    Material = SystemBackdropOption.Acrylic;
                }
            }
        }
    }
}

