using BitWallpaper.Helpers;
using System;
using System.IO;
using WinUIEx;

namespace BitWallpaper
{
    public sealed partial class MainWindow : WinUIEx.WindowEx
    {
        public MainWindow()
        {
            InitializeComponent();

            AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "App_Icon.ico"));
            //Content = null;
            Title = "AppDisplayName/Text".GetLocalized();
            
            // Need to be here in the code bihind.
            this.ExtendsContentIntoTitleBar = true;
        }
    }
}
