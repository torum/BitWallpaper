// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using BitWallpaper4.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BitWallpaper4.Views.UserControls
{

    public sealed partial class ChartContent : UserControl
    {
        public MainViewModel ViewModel
        {
            get;
        }

        public ChartContent()
        {
            this.InitializeComponent();

        }



        public static readonly DependencyProperty BtcJpViewModelProperty = DependencyProperty.Register(
          nameof(MainViewModel),
          typeof(MainViewModel),
          typeof(ChartContent),
          null);
        
        public MainViewModel vm
        {
            get => (MainViewModel)GetValue(BtcJpViewModelProperty);
            set => SetValue(BtcJpViewModelProperty, value);
        }

        private void ListBoxDepth_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            Task.Run(() => SetDepthListboxScrollPosition());
        }

        private async void SetDepthListboxScrollPosition()
        {
            try
            {

                await Task.Delay(1000);

                _dispatcherQueue.TryEnqueue(() =>
                {
                    if (this.DepthListBox.Items.Count > 0)
                    {
                        try
                        {
                            // ListBoxからAutomationPeerを取得
                            var peer = ItemsControlAutomationPeer.CreatePeerForElement(this.DepthListBox);
                            // GetPatternでIScrollProviderを取得
                            var scrollProvider = peer.GetPattern(Microsoft.UI.Xaml.Automation.Peers.PatternInterface.Scroll) as IScrollProvider;

                            if (scrollProvider != null)
                            {
                                if (scrollProvider.VerticallyScrollable)
                                {
                                    try
                                    {
                                        // パーセントで位置を指定してスクロール
                                        scrollProvider.SetScrollPercent(
                                            // 水平スクロールは今の位置
                                            scrollProvider.HorizontalScrollPercent,
                                            // 垂直方向50%
                                            50.0);
                                    }
                                    catch
                                    {
                                        System.Diagnostics.Debug.WriteLine("■■■■■ SetDepthListboxScrollPosition scrollProvider null Error");
                                    }
                                }
                            }
                        }
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine("■■■■■ SetDepthListboxScrollPosition SetScrollPercent Error");
                        }
                    }
                });
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("■■■■■ SetDepthListboxScrollPosition Exception: " + e);
            }


        }
        readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

    }
}
