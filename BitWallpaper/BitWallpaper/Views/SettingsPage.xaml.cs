using BitWallpaper.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace BitWallpaper.Views
{
    public sealed partial class SettingsPage : Page
    {
        private MainViewModel _viewModel;

        public MainViewModel ViewModel
        {
            get => _viewModel;
        }

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //base.OnNavigatedTo(e);

            if (e.Parameter is MainViewModel)
            {
                _viewModel = (MainViewModel)e.Parameter;
            }

            base.OnNavigatedTo(e);
        }
 
    }
}