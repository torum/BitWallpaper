using BitWallpaper.ViewModels;
using BitWallpaper.Views.Dialogs;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Controls;

namespace BitWallpaper.Views;

public sealed partial class ChartContent : UserControl
{
    public ChartContent()
    {
        InitializeComponent();

        this.DataContext = PairVM;

    }

    public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(PairViewModel),
      //typeof(PairViewModel),
      typeof(PairViewModel),
      typeof(ChartContent),
      new PropertyMetadata(null, ValueChanged));

    public PairViewModel PairVM
    {
        get => (PairViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private static void ValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ((ChartContent)sender).ValueChanged();
    }

    private void ValueChanged()
    {
        if (PairVM == null)
        {
            return;
        }

        this.DataContext = PairVM;

        PairVM.DepthScrollCenter += OnDepthScrollCenter;
    }

    private void OnDepthScrollCenter(object? sender, bool e)
    {
        //Task.Run(() => SetDepthListboxScrollPosition());
        SetDepthListboxScrollPosition();
    }

    private void ListBoxDepth_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        //Task.Run(() => SetDepthListboxScrollPosition());
        SetDepthListboxScrollPosition();
    }

    private async void SetDepthListboxScrollPosition()
    {
        await Task.Delay(300);

        var uithread = App.DispatcherQueue?.HasThreadAccess;
        if (uithread != null)
        {
            if (uithread == true)
            {
                DoScroll();
            }
            else
            {
                App.DispatcherQueue?.TryEnqueue(() =>
                {
                    DoScroll();
                });
            }
        }
    }

    private void DoScroll()
    {
        if (this.DepthListBox.Items.Count <= 0)
        {
            return;
        }

        try
        {
            // ListBoxからAutomationPeerを取得
            var peer = ItemsControlAutomationPeer.CreatePeerForElement(this.DepthListBox);
            // GetPatternでIScrollProviderを取得

            if (peer.GetPattern(Microsoft.UI.Xaml.Automation.Peers.PatternInterface.Scroll) is not IScrollProvider scrollProvider)
            {
                return;
            }
            if (!scrollProvider.VerticallyScrollable)
            {
                return;
            }
            // パーセントで位置を指定してスクロール
            scrollProvider.SetScrollPercent(
                // 水平スクロールは今の位置
                scrollProvider.HorizontalScrollPercent,
                // 垂直方向50%
                50.0);
        }
        catch
        {
            Debug.WriteLine("■■■■■ SetDepthListboxScrollPosition SetScrollPercent Error");
        }
    }

    private async void AppBarButton_Click(object sender, RoutedEventArgs e)
    {
        if (PairVM == null)
        {
            return;
        }
        ContentDialogContent dialog = new();

        if (PairVM.AlarmPlus > 0)
        {
            dialog.HighPrice = PairVM.AlarmPlus;
        }

        if (PairVM.AlarmMinus > 0)
        {
            dialog.LowPrice = PairVM.AlarmMinus;
        }

        // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        dialog.XamlRoot = XamlRoot;
        //dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
        dialog.Title = "Notification";
        dialog.PrimaryButtonText = "Set";
        dialog.CloseButtonText = "Cancel";
        dialog.DefaultButton = ContentDialogButton.Primary;
        //dialog.Content = new ContentDialogContent();

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            PairVM.AlarmPlus = dialog.HighPrice;
            PairVM.AlarmMinus = dialog.LowPrice;
        }
    }

    private void DepthListBox_DoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
    {
        SetDepthListboxScrollPosition();
    }
}
