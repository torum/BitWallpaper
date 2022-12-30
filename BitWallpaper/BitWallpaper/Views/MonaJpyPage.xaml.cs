using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using BitWallpaper.ViewModels;
using System.Diagnostics;
using BitWallpaper.Models;
using Microsoft.UI.Xaml.Markup;

namespace BitWallpaper.Views
{
    public sealed partial class MonaJpyPage : Page
    {
        private PairViewModel _viewModel;

        public PairViewModel ViewModel
        {
            get=> _viewModel;
        }

        public MonaJpyPage()
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

            if (e.Parameter is PairViewModel)
            {
                _viewModel = (PairViewModel)e.Parameter;
            }

            base.OnNavigatedTo(e);
        }
    }
}
