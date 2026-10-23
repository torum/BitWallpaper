using BitWallpaper.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BitWallpaper.Views;

public sealed partial class GridViewPage : Page
{
    public GridViewPage()
    {
        ViewModel = App.GetService<MainViewModel>();

        InitializeComponent();
    }

    public MainViewModel? ViewModel
    {
        get; private set;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        //base.OnNavigatedTo(e);

        if (e.Parameter is MainViewModel mvm)
        {
            ViewModel = mvm;

            Bindings.Update();
        }

        base.OnNavigatedTo(e);
    }
}
