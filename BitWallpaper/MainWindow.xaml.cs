using BitWallpaper.Helpers;
using System;
using System.IO;

namespace BitWallpaper
{
    public sealed partial class MainWindow : WinUIEx.WindowEx
    {
        public MainWindow()
        {
            InitializeComponent();

            AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "App_Icon.ico"));
            Content = null;
            //Title = "AppDisplayName".GetLocalized();
            
            this.ExtendsContentIntoTitleBar = true;

            // dummy
            //this.SetTitleBar(AppTitleBar);
        }
    }
}
