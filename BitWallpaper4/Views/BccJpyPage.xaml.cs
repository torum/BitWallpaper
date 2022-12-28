using BitWallpaper4.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace BitWallpaper4.Views
{
    public sealed partial class BccJpyPage : Page
    {
        private MainViewModel _viewModel;

        public MainViewModel ViewModel
        {
            get=> _viewModel;
        }

        public BccJpyPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (XamlParseException parseException)
            {
                Debug.WriteLine($"Unhandled XamlParseException in ChartUserControl: {parseException.Message}");
                foreach (var key in parseException.Data.Keys)
                {
                    Debug.WriteLine("{Key}:{Value}", key.ToString(), parseException.Data[key]?.ToString());
                }
                throw;
            }
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
