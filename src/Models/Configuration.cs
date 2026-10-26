
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Collections.Generic;

namespace BitWallpaper.Models;

public class Configuration
{
    public class WindowSettings
    {
        public int Width { get; set; } = 780;
        public int Height { get; set; } = 600;
        public int Left { get; set; } = 100;
        public int Top { get; set; } = 100;
        public OverlappedPresenterState WindowState { get; set; } = OverlappedPresenterState.Restored;

        public int RestoreWidth { get; set; } = 780;
        public int RestoreHeight { get; set; } = 600;
        public int RestoreLeft { get; set; } = 100;
        public int RestoreTop { get; set; } = 100;
    }

    public WindowSettings MainWindow { get; set; } = new WindowSettings();

    public bool IsNavigationViewMenuOpen { get; set; } = true;

    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    public SystemBackdropOption SystemBackdrop { get; set; } = SystemBackdropOption.Mica;


    public bool IsSaveLog { get; set; } = false;

    public bool IsOnBtcJpy { get; set; } = true;
    public bool IsOnXrpJpy { get; set; } = false;
    public bool IsOnEthJpy { get; set; } = false;


    public bool IsOnLtcJpy { get; set; } = false;
    public bool IsOnBccJpy { get; set; } = false;
    public bool IsOnMonaJpy { get; set; } = false;
    public bool IsOnXlmJpy { get; set; } = false;
    public bool IsOnQtumJpy { get; set; } = false;
    public bool IsOnBatJpy { get; set; } = false;
    public bool IsOnOmgJpy { get; set; } = false;
    public bool IsOnXymJpy { get; set; } = false;
    public bool IsOnLinkJpy { get; set; } = false;
    public bool IsOnMkrJpy { get; set; } = false;
    public bool IsOnBobaJpy { get; set; } = false;
    public bool IsOnEnjJpy { get; set; } = false;
    public bool IsOnMaticJpy { get; set; } = false;
    public bool IsOnDotJpy { get; set; } = false;
    public bool IsOnDogeJpy { get; set; } = false;
    public bool IsOnAstrJpy { get; set; } = false;
    public bool IsOnAdaJpy { get; set; } = false;
    public bool IsOnAvaxJpy { get; set; } = false;
    public bool IsOnAxsJpy { get; set; } = false;
    public bool IsOnFlrJpy { get; set; } = false;
    public bool IsOnSandJpy { get; set; } = false;

}
