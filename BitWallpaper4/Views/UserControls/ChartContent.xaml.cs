using BitWallpaper4.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using System.Diagnostics;

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

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(MainViewModel),
          //typeof(MainViewModel),
          typeof(object),
          typeof(ChartContent),
          new PropertyMetadata(null, ValueChanged));

        public MainViewModel vm
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        private static void ValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ChartContent)sender).ValueChanged();
        }

        private void ValueChanged()
        {
            
        }

        private void ListBoxDepth_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Task.Run(() => SetDepthListboxScrollPosition());
            SetDepthListboxScrollPosition();
        }

        private void SetDepthListboxScrollPosition()
        {
            //await Task.Delay(100);
            /*
            try
            {
                (App.Current as App)?.CurrentDispatcherQueue?.TryEnqueue(() =>
                {

                });
            }
            catch (Exception e)
            {
                Debug.WriteLine("■■■■■ SetDepthListboxScrollPosition Exception: " + e);
            }
            */
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
                                Debug.WriteLine("■■■■■ SetDepthListboxScrollPosition scrollProvider null Error");
                            }
                        }
                    }
                }
                catch
                {
                    Debug.WriteLine("■■■■■ SetDepthListboxScrollPosition SetScrollPercent Error");
                }
            }
        }

    }
}
