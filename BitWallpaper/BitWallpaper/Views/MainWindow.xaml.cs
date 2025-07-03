using BitApps.Core.Helpers;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using System.Diagnostics;
using Windows.Storage;

namespace BitWallpaper.Views;

public sealed partial class MainWindow : WinUIEx.WindowEx
{
    public MainWindow()
    {
        try
        {
            InitializeComponent();
        }
        catch (XamlParseException parseException)
        {
            Debug.WriteLine($"Unhandled XamlParseException in MainWindow: {parseException.Message}");
            /*
            foreach (var key in parseException.Data.Keys)
            {
                Debug.WriteLine("{Key}:{Value}", key.ToString(), parseException.Data[key]?.ToString());
            }
            throw;
            */
        }

        AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "App_Icon.ico"));
        //Content = null;
        Title = "AppDisplayName/Text".GetLocalized();
        
        // Need to be here in the code bihind.
        ExtendsContentIntoTitleBar = true;

        // SystemBackdrop
        if (Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController.IsSupported())
        {
            //manager.Backdrop = new WinUIEx.AcrylicSystemBackdrop();
            if (RuntimeHelper.IsMSIX)
            {
                // Load preference from localsetting.
                if (ApplicationData.Current.LocalSettings.Values.TryGetValue(App.BackdropSettingsKey, out var obj))
                {
                    var s = (string)obj;
                    if (s == SystemBackdropOption.Acrylic.ToString())
                    {
                        SystemBackdrop = new DesktopAcrylicBackdrop();
                    }
                    else if (s == SystemBackdropOption.Mica.ToString())
                    {
                        SystemBackdrop = new MicaBackdrop()
                        {
                            Kind = MicaKind.Base
                        };
                    }
                    else
                    {
                        SystemBackdrop = new DesktopAcrylicBackdrop();
                    }
                }
                else
                {
                    // default acrylic.
                    SystemBackdrop = new DesktopAcrylicBackdrop();
                }
            }
            else
            {
                // just for me.
                SystemBackdrop = new DesktopAcrylicBackdrop();
            }

        }
        else if (Microsoft.UI.Composition.SystemBackdrops.MicaController.IsSupported())
        {
            SystemBackdrop = new MicaBackdrop()
            {
                Kind = MicaKind.Base
            };
        }
        else
        {
            // Memo: Without Backdrop, theme setting's theme is not gonna have any effect( "system default" will be used). So the setting is disabled.
        }

    }
}
