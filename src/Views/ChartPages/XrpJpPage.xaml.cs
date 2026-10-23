using BitWallpaper.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace BitWallpaper.Views;

public sealed partial class XrpJpyPage : Page
{
    public PairViewModel? ViewModel { get; private set; }

    public XrpJpyPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        //base.OnNavigatedTo(e);

        if (e.Parameter is PairViewModel pvm)
        {
            ViewModel = pvm;

            Bindings.Update();
        }
        else
        {
            Debug.WriteLine($"XrpJpyPage.OnNavigatedTo: Parameter is not of type PairViewModel. Actual type: {e.Parameter?.GetType().FullName ?? "null"}");
        }

        base.OnNavigatedTo(e);
    }
}
