using BitWallpaper.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace BitWallpaper.Views;

public sealed partial class BccJpyPage : Page
{
    public PairViewModel? ViewModel
    {
        get; private set;
    }

    public BccJpyPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is PairViewModel pvm)
        {
            ViewModel = pvm;

            ViewModel.IsSelectedActive = true;

            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel?.IsSelectedActive = false;

        base.OnNavigatedFrom(e);
    }
}
