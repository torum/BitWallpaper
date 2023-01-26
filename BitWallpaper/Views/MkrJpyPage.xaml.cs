using BitWallpaper.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;

namespace BitWallpaper.Views
{
    public sealed partial class MkrJpyPage : Page
    {
        private PairViewModel _viewModel;

        public PairViewModel ViewModel
        {
            get=> _viewModel;
        }

        public MkrJpyPage()
        {
            try
            {
                InitializeComponent();
            }
            catch (XamlParseException parseException)
            {
                Debug.WriteLine($"Unhandled XamlParseException in MkrJpyPage: {parseException.Message}");
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

            if (e.Parameter is PairViewModel model)
            {
                _viewModel = model;
            }

            base.OnNavigatedTo(e);
        }
    }
}
