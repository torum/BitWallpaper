using BitWallpaper.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace BitWallpaper.Views;

public sealed partial class XymJpyPage : Page
{
    public PairViewModel? ViewModel { get; private set; }

    public XymJpyPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        //base.OnNavigatedTo(e);

        if (e.Parameter is PairViewModel model)
        {
            ViewModel = model;
        }

        base.OnNavigatedTo(e);
    }
}
