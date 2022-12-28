using BitWallpaper4.Helpers;
using BitWallpaper4.ViewModels;
using Microsoft.UI;
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
using System.Diagnostics;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BitWallpaper4.Views
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
