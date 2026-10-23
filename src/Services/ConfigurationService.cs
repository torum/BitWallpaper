using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using BitWallpaper.Models;
using BitWallpaper.Services.Contracts;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable IDE0290 // Use primary constructor

namespace BitWallpaper.Services;

/*
 * WinUI3/AvaloniaUI does not provide a way to get the restore bounds of a window like WPF does when it is maximized or minimized.
 * So, we need to manually track the restore bounds of the window when it is in the restored state.
 * AppWindow_Changed(WinUI3)/Window_PositionChanged(AvaloniaUI) and Window_SizeChanged event handlers are called when the window state changes, such as when the window is maximized, minimized, or restored.
 * Make sure to update the restore bounds when the window is in the restored state.
 * Also, use Window_Activated event handler to update the restore bounds when the window is activated, because sometimes the window state change events are not fired when the window is activated.
 * This way, even if the window is closed while maximized or minimized, we can still save the restore bounds to the configuration file.
    _winRestoreHeight = sender.Size.Height;
    _winRestoreWidth = sender.Size.Width;
    _winRestoreTop = sender.Position.Y;
    _winRestoreLeft = sender.Position.X;
 */

public sealed class ConfigurationService : IConfigurationService
{
    private bool _isInitialized;
    private bool _isConfigLoaded;
    private string? _configFilePath = string.Empty;
    private XDocument? _xdoc;
    private readonly IDispatcherService _dispatcherService;

    public ConfigurationService(IDispatcherService dispatcherService)
    {
        _dispatcherService = dispatcherService;

        _isConfigLoaded = false;
        CurrentConfiguration = new Models.Configuration();
    }

    public Models.Configuration CurrentConfiguration { get; private set; }

    public void Initialize(string configFilePath)
    {
        _configFilePath = configFilePath;
        _isConfigLoaded = false;
        CurrentConfiguration = new Models.Configuration();
        _xdoc = null;
        _isInitialized = true;

        var folderPath = Path.GetDirectoryName(configFilePath);
        if (folderPath is not null)
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }
        else
        {
            throw new InvalidOperationException("Invalid configuration file path.");
        }
    }

    public async Task<Models.Configuration> LoadConfigurationAsync()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("ConfigurationService is not initialized.");
        }

        var config = new Models.Configuration();

        if (_isConfigLoaded)
        {
            Debug.WriteLine("LoadConfigurationAsync() - Already loaded, returning current configuration.");
            return CurrentConfiguration ?? config;
        }

        if (string.IsNullOrEmpty(_configFilePath))
        {
            throw new InvalidOperationException("Configuration file path is not set.");
        }
        if (!System.IO.File.Exists(_configFilePath))
        {
            Debug.WriteLine($"Configuration file not found: {_configFilePath}");
            // Set _isConfigLoaded to true so that SaveConfigurationAsync() can create the file with default settings.
            _isConfigLoaded = true;
            return config;
        }

        Debug.WriteLine("LoadConfigurationAsync()");

        try
        {
            _xdoc = XDocument.Load(_configFilePath);
            if (_xdoc.Root is null)
            {
                return config;
            }

            #region == Window setting ==
            
            // Main Window element
            var mainWindow = _xdoc.Root.Element("MainWindow");
            if (mainWindow is not null)
            {
                int wY = 24;
                int wX = 24;

                // Window sizes
                var hoge = mainWindow.Attribute("top");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.MainWindow.Top = wY;
                    }
                }

                hoge = mainWindow.Attribute("left");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.MainWindow.Left = wX;
                    }
                }

                hoge = mainWindow.Attribute("height");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.MainWindow.Height = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = mainWindow.Attribute("width");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.MainWindow.Width = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Restore window sizes
                hoge = mainWindow.Attribute("restoreTop");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wY))
                    {
                        config.MainWindow.RestoreTop = wY;
                    }
                }

                hoge = mainWindow.Attribute("restoreLeft");
                if (hoge is not null)
                {
                    if (Int32.TryParse(hoge.Value, out wX))
                    {
                        config.MainWindow.RestoreLeft = wX;
                    }
                }

                hoge = mainWindow.Attribute("restoreHeight");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.MainWindow.RestoreHeight = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                hoge = mainWindow.Attribute("restoreWidth");
                if (hoge is not null)
                {
                    if (!string.IsNullOrEmpty(hoge.Value))
                    {
                        config.MainWindow.RestoreWidth = Int32.Parse(hoge.Value, CultureInfo.InvariantCulture);
                    }
                }

                // Window state
                hoge = mainWindow.Attribute("state");
                if (hoge is not null)
                {
                    if (hoge.Value == "Maximized")
                    {
                        // TODO: Since there is no restorebounds in AvaloniaUI or WinUI3.....
                        config.MainWindow.WindowState = OverlappedPresenterState.Maximized;
                    }
                    else if (hoge.Value == "Normal")
                    {
                        config.MainWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                    else if (hoge.Value == "Minimized")
                    {
                        // Ignore minimized state, and restore to normal state.
                        config.MainWindow.WindowState = OverlappedPresenterState.Restored;
                    }
                }

                #region == Layout ==

                var lay = mainWindow.Element("Layout");
                if (lay is not null)
                {
                    // old name. Leave it for backward compatibility.
                    var xbool = lay.Attribute("navigationViewMenuOpen")?.Value;
                    if (!string.IsNullOrEmpty(xbool))
                    {
                        if (xbool.Equals("True", StringComparison.OrdinalIgnoreCase))
                        {
                            config.IsNavigationViewMenuOpen = true;
                        }
                        else
                        {
                            config.IsNavigationViewMenuOpen = false;
                        }
                    }

                    xbool = lay.Attribute("isNavigationViewMenuOpen")?.Value;
                    if (!string.IsNullOrEmpty(xbool))
                    {
                        if (xbool.Equals("True", StringComparison.OrdinalIgnoreCase))
                        {
                            config.IsNavigationViewMenuOpen = true;
                        }
                        else
                        {
                            config.IsNavigationViewMenuOpen = false;
                        }
                    }

                }

                #endregion

            }

            #endregion

            #region == Theme ==

            var opts = _xdoc.Root.Element("Theme");
            if (opts != null)
            {
                // Old name. Leave it for backward compatibility.
                var xvalue = opts.Attribute("current");
                if (xvalue != null)
                {
                    if (!string.IsNullOrEmpty(xvalue.Value))
                    {
                        if (Enum.TryParse(xvalue.Value, out ElementTheme cacheTheme))
                        {
                            config.Theme = cacheTheme;
                            //isFoundNewThemeSetting = true;
                        }
                    }
                }
                // Old name. Leave it for backward compatibility.
                xvalue = opts.Attribute("backdrop");
                if (xvalue != null)
                {
                    if (!string.IsNullOrEmpty(xvalue.Value))
                    {
                        if (Enum.TryParse(xvalue.Value, out SystemBackdropOption cacheBackdrop))
                        {
                            config.SystemBackdrop = cacheBackdrop;
                            //isFoundNewThemeSetting = true;
                        }
                    }
                }
                // New name. 
                xvalue = opts.Attribute("ThemeName");
                if (xvalue != null)
                {
                    if (!string.IsNullOrEmpty(xvalue.Value))
                    {
                        if (Enum.TryParse(xvalue.Value, out ElementTheme cacheTheme))
                        {
                            config.Theme = cacheTheme;
                            //isFoundNewThemeSetting = true;
                        }
                    }
                }
                // New name. 
                xvalue = opts.Attribute("SystemBackdrop");
                if (xvalue != null)
                {
                    if (!string.IsNullOrEmpty(xvalue.Value))
                    {
                        if (Enum.TryParse(xvalue.Value, out SystemBackdropOption cacheBackdrop))
                        {
                            config.SystemBackdrop = cacheBackdrop;
                            //isFoundNewThemeSetting = true;
                        }
                    }
                }
            }

            #endregion

            #region == Options ==

            // TODO: Change to "Options" in the future, but keep "Opts" for backward compatibility.
            opts = _xdoc.Root.Element("Opts");
            if (opts is not null)
            {
                var attrs = opts.Attribute("IsSaveLog");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsSaveLog = true;
                        }
                        else
                        {
                            config.IsSaveLog = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsNavigationViewMenuOpen");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsNavigationViewMenuOpen = true;
                        }
                        else
                        {
                            config.IsNavigationViewMenuOpen = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnBtcJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnBtcJpy = true;
                        }
                        else
                        {
                            config.IsOnBtcJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnXrpJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnXrpJpy = true;
                        }
                        else
                        {
                            config.IsOnXrpJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnEthJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnEthJpy = true;
                        }
                        else
                        {
                            config.IsOnEthJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnLtcJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnLtcJpy = true;
                        }
                        else
                        {
                            config.IsOnLtcJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnBccJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnBccJpy = true;
                        }
                        else
                        {
                            config.IsOnBccJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnMonaJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnMonaJpy = true;
                        }
                        else
                        {
                            config.IsOnMonaJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnXlmJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnXlmJpy = true;
                        }
                        else
                        {
                            config.IsOnXlmJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnQtumJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnQtumJpy = true;
                        }
                        else
                        {
                            config.IsOnQtumJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnBatJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnBatJpy = true;
                        }
                        else
                        {
                            config.IsOnBatJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnOmgJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnOmgJpy = true;
                        }
                        else
                        {
                            config.IsOnOmgJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnXymJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnXymJpy = true;
                        }
                        else
                        {
                            config.IsOnXymJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnLinkJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnLinkJpy = true;
                        }
                        else
                        {
                            config.IsOnLinkJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnMkrJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnMkrJpy = true;
                        }
                        else
                        {
                            config.IsOnMkrJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnBobaJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnBobaJpy = true;
                        }
                        else
                        {
                            config.IsOnBobaJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnEnjJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnEnjJpy = true;
                        }
                        else
                        {
                            config.IsOnEnjJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnMaticJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnMaticJpy = true;
                        }
                        else
                        {
                            config.IsOnMaticJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnDotJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnDotJpy = true;
                        }
                        else
                        {
                            config.IsOnDotJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnDogeJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnDogeJpy = true;
                        }
                        else
                        {
                            config.IsOnDogeJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnAstrJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnAstrJpy = true;
                        }
                        else
                        {
                            config.IsOnAstrJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnAdaJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnAdaJpy = true;
                        }
                        else
                        {
                            config.IsOnAdaJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnAvaxJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnAvaxJpy = true;
                        }
                        else
                        {
                            config.IsOnAvaxJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnAxsJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnAxsJpy = true;
                        }
                        else
                        {
                            config.IsOnAxsJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnFlrJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnFlrJpy = true;
                        }
                        else
                        {
                            config.IsOnFlrJpy = false;
                        }
                    }
                }

                attrs = opts.Attribute("IsOnSandJpy");
                if (attrs is not null)
                {
                    if (!string.IsNullOrEmpty(attrs.Value))
                    {
                        if (attrs.Value == "True")
                        {
                            config.IsOnSandJpy = true;
                        }
                        else
                        {
                            config.IsOnSandJpy = false;
                        }
                    }
                }

            }
            
            #endregion

            _isConfigLoaded = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading configuration: {ex}");
            _dispatcherService.TryEnqueue(() => { (App.Current as App)?.AppendErrorLog("Error loading configuration", ex.ToString()); });
        }

        CurrentConfiguration = config;

        return CurrentConfiguration;
    }

    public async Task SaveConfigurationAsync()
    {
        if (!_isInitialized)
        {
            throw new InvalidOperationException("ConfigurationService is not initialized.");
        }

        // Make sure Window and settings have been fully loaded and not overriding with empty data.
        if (!_isConfigLoaded)
        {
            // Should be fine now. -> Don't return, because we want to save the configuration even if it hasn't been loaded yet, like the first time.
            //return;
        }
        if (string.IsNullOrEmpty(_configFilePath))
        {
            throw new InvalidOperationException("Configuration file path is not set.");
        }

        Debug.WriteLine("SaveConfigurationAsync()");

        // TODO: Load xdoc from global variable if exists, and apply changes to it, then save it back to file.
        //_xdoc

        var config = CurrentConfiguration;

        XmlDocument doc = new();
        XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.InsertBefore(xmlDeclaration, doc.DocumentElement);

        // Root Document Element
        XmlElement root = doc.CreateElement(string.Empty, "App", string.Empty);
        doc.AppendChild(root);

        XmlAttribute attrs;

        #region == Window settings ==

        var w = config.MainWindow;
        //double windowWidth = 780;

        // Main Window element
        XmlElement mainWindow = doc.CreateElement(string.Empty, "MainWindow", string.Empty);

        // Main Window attributes

        // Window sizes
        attrs = doc.CreateAttribute("height");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Height.ToString(CultureInfo.InvariantCulture) : w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("width");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Width.ToString(CultureInfo.InvariantCulture) : w.RestoreWidth.ToString(CultureInfo.InvariantCulture);

        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("top");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Top.ToString(CultureInfo.InvariantCulture) : w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("left");
        attrs.Value = w.WindowState == OverlappedPresenterState.Restored ? w.Left.ToString(CultureInfo.InvariantCulture) : w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        // Restore window sizes
        attrs = doc.CreateAttribute("restoreHeight");
        attrs.Value = w.RestoreHeight.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreWidth");
        attrs.Value = w.RestoreWidth.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreTop");
        attrs.Value = w.RestoreTop.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("restoreLeft");
        attrs.Value = w.RestoreLeft.ToString(CultureInfo.InvariantCulture);
        mainWindow.SetAttributeNode(attrs);


        attrs = doc.CreateAttribute("state");
        attrs.Value = w.WindowState switch
        {
            OverlappedPresenterState.Maximized => "Maximized",
            OverlappedPresenterState.Restored => "Normal",
            OverlappedPresenterState.Minimized => "Minimized",
            _ => attrs.Value
        };
        mainWindow.SetAttributeNode(attrs);

        #region == Layout ==

        var xLay = doc.CreateElement(string.Empty, "Layout", string.Empty);

        attrs = doc.CreateAttribute("isNavigationViewMenuOpen");
        attrs.Value = config.IsNavigationViewMenuOpen ? "True" : "False";
        xLay.SetAttributeNode(attrs);

        mainWindow.AppendChild(xLay);
        
        #endregion

        // set MainWindow element to root.
        root.AppendChild(mainWindow);

        #endregion

        #region == Theme ==

        XmlElement thm = doc.CreateElement(string.Empty, "Theme", string.Empty);

        attrs = doc.CreateAttribute("ThemeName");
        attrs.Value = config.Theme.ToString();
        thm.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("SystemBackdrop");
        attrs.Value = config.SystemBackdrop.ToString();
        thm.SetAttributeNode(attrs);

        /// 
        root.AppendChild(thm);

        #endregion

        #region == Options ==

        // TODO: Change to "Options" in the future, but keep "Opts" for backward compatibility.
        var opts = doc.CreateElement(string.Empty, "Opts", string.Empty);

        attrs = doc.CreateAttribute("IsSaveLog");
        attrs.Value = config.IsSaveLog ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsNavigationViewMenuOpen");
        attrs.Value = config.IsNavigationViewMenuOpen ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnBtcJpy");
        attrs.Value = config.IsOnBtcJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnXrpJpy");
        attrs.Value = config.IsOnXrpJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnEthJpy");
        attrs.Value = config.IsOnEthJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnLtcJpy");
        attrs.Value = config.IsOnLtcJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnBccJpy");
        attrs.Value = config.IsOnBccJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnMonaJpy");
        attrs.Value = config.IsOnMonaJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnXlmJpy");
        attrs.Value = config.IsOnXlmJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnQtumJpy");
        attrs.Value = config.IsOnQtumJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnBatJpy");
        attrs.Value = config.IsOnBatJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnOmgJpy");
        attrs.Value = config.IsOnOmgJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnXymJpy");
        attrs.Value = config.IsOnXymJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnLinkJpy");
        attrs.Value = config.IsOnLinkJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnMkrJpy");
        attrs.Value = config.IsOnMkrJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnBobaJpy");
        attrs.Value = config.IsOnBobaJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnEnjJpy");
        attrs.Value = config.IsOnEnjJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnMaticJpy");
        attrs.Value = config.IsOnMaticJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnDotJpy");
        attrs.Value = config.IsOnDotJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnDogeJpy");
        attrs.Value = config.IsOnDogeJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnAstrJpy");
        attrs.Value = config.IsOnAstrJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnAdaJpy");
        attrs.Value = config.IsOnAdaJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnAvaxJpy");
        attrs.Value = config.IsOnAvaxJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnAxsJpy");
        attrs.Value = config.IsOnAxsJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnFlrJpy");
        attrs.Value = config.IsOnFlrJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        attrs = doc.CreateAttribute("IsOnSandJpy");
        attrs.Value = config.IsOnSandJpy ? "True" : "False";
        opts.SetAttributeNode(attrs);

        /// 
        root.AppendChild(opts);

        #endregion

        try
        {
            // Now this is redundant, because we already create the directory in Initialize() method.
            if (!Directory.Exists(App.AppDataFolder))
            {
                Directory.CreateDirectory(App.AppDataFolder);
            }

            //doc.Save(App.AppConfigFilePath);
            var temporaryPath = _configFilePath + ".tmp";
            doc.Save(temporaryPath);
            File.Move(temporaryPath, _configFilePath, overwrite: true);
        }
        //catch (System.IO.FileNotFoundException) { }
        catch (Exception ex)
        {
            _dispatcherService.TryEnqueue(() =>
            {
                (App.Current as App)?.AppendErrorLog("Exception@SaveConfigurationAsync", ex.ToString());
            });
        }
    }
}
