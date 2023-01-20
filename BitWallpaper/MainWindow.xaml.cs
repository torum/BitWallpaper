using BitWallpaper.Helpers;
using Microsoft.UI.Xaml.Markup;
using System;
using System.Diagnostics;
using System.IO;
using WinUIEx;

namespace BitWallpaper
{
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
                foreach (var key in parseException.Data.Keys)
                {
                    Debug.WriteLine("{Key}:{Value}", key.ToString(), parseException.Data[key]?.ToString());
                }
                throw;
            }

            AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "App_Icon.ico"));
            //Content = null;
            Title = "AppDisplayName/Text".GetLocalized();
            
            // Need to be here in the code bihind.
            this.ExtendsContentIntoTitleBar = true;

        }
    }
}
