using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using BitWallpaper.Helpers;
using BitWallpaper.Common;
using BitWallpaper.ViewModels;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using Windows.UI.Notifications;
using Windows.Data.Xml;
using Notifications.Wpf;

namespace BitWallpaper
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow
    {
        private System.Windows.Threading.DispatcherTimer _dispatcherMouseTimer = new System.Windows.Threading.DispatcherTimer();

        private System.Windows.Forms.NotifyIcon _notifyIcon;

        private string _appTitle;

        //private bool _isExit;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (this.DataContext as MainViewModel).OnWindowLoaded;

            Closing += (this.DataContext as MainViewModel).OnWindowClosing;

            Closed += OnWindowClosed;

            RestoreButton.Visibility = Visibility.Collapsed;
            MaxButton.Visibility = Visibility.Visible;

            _appTitle = (this.DataContext as MainViewModel).AppTitle;

            /*
            this.Top = 0;
            this.Left = 0;
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;

            this.WindowState = WindowState.Normal;
            */

            /*
            // 背景の指定
            var path = ("C:\\Users\\torum\\Pictures\\Wallpapers\\Desktop\\3840x2160\\dfe27e0ee314d6e8181a2c5c4166f12f.jpg");
            var uri = new Uri(path);
            var bitmap = new BitmapImage(uri);
            this.BackgroundImage.Source = bitmap;
            */

            // ウィンドウを最背面にセット
            //Helpers.WindowExtensions.SetAlwaysOnBottom(this, true);


            // マウス非表示のタイマー起動
            _dispatcherMouseTimer.Tick += new EventHandler(MouseTimer);
            _dispatcherMouseTimer.Interval = new TimeSpan(0, 0, 6);
            //dispatcherMouseTimer.Start();

            // システムトレイアイコン
            // https://www.thomasclaudiushuber.com/2015/08/22/creating-a-background-application-with-wpf/
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.DoubleClick += (s, args) => BringToForeground();
            _notifyIcon.Icon = BitWallpaper.Properties.Resources.AppIcon;//System.Drawing.SystemIcons.Information; //
            _notifyIcon.Text = _appTitle;
            _notifyIcon.Visible = true;
            _notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            _notifyIcon.ContextMenuStrip.Items.Add(_appTitle + " 表示").Click += (s, e) => BringToForeground();
            _notifyIcon.ContextMenuStrip.Items.Add("終了").Click += (s, e) => ExitApplication();




            /*
            //var message = "Sample message";
            //var xml = $"<?xml version=\"1.0\"?><toast><visual><binding template=\"ToastText01\"><text id=\"1\">{message}</text></binding></visual></toast>";

            var xml = @"<toast>
    <visual>
        <binding template=""ToastImageAndText04"">
            <image id=""1"" src=""file:///C:\meziantou.jpeg"" alt=""meziantou""/>
            <text id=""1"">Meziantou</text>
            <text id=""2"">Gérald Barré</text>
            <text id=""3"">asdf</text>
        </binding>
    </visual>
</toast>";

            var toastXml = new Windows.Data.Xml.Dom.XmlDocument();
            toastXml.LoadXml(xml);
            var toast = new ToastNotification(toastXml);
            ToastNotificationManager.CreateToastNotifier("Sample toast").Show(toast);
            */

            //var notificationManager = new NotificationManagerEx();
            //NotificationContentEx test = new NotificationContentEx();
            /*
            notificationManager.Show(new NotificationContent
            {
                Title = "Sample notification",
                Message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                Type = NotificationType.Information
            });
            */
            //notificationManager.Show(test, onClose: () => onNotificationsOverlayWindowClose(test));
           
        }

        private void ExitApplication()
        {
            this.Close();
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            _notifyIcon.Dispose();
            _notifyIcon = null;
            /*
            // clean up NotificationsOverlayWindows
            // https://github.com/Federerer/Notifications.Wpf/issues/10
            App app = App.Current as App;
            foreach (var w in app.Windows)
            {
                if (w is NotificationsOverlayWindow)
                {
                    // Close it.
                    (w as NotificationsOverlayWindow).Close();
                }
            }
            */
        }

        private void MouseTimer(object source, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.None;
                this.Cursor = System.Windows.Input.Cursors.None;
            }
        }

        public void BringToForeground()
        {
            if (this.WindowState == WindowState.Minimized || this.Visibility == Visibility.Hidden)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }

            this.Activate();
            //this.Topmost = true;
            //this.Topmost = false;
            this.Focus();


        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            LayoutChange();
        }

        private void LayoutChange()
        {

            // 幅
            double appWidth = this.ActualWidth;
            double boxWidth = (appWidth - 40) / 3;

            // WrapPanes's width
            coins_wrap.Width = appWidth;

            // WrapPanes's Margin
            coins_wrap.Margin = new Thickness(0, 0, 0, 0); ;

            btc_box.Width = boxWidth;
            ltc_box.Width = boxWidth;
            xrp_box.Width = boxWidth;
            eth_box.Width = boxWidth;
            mona_box.Width = boxWidth;
            bch_box.Width = boxWidth;

            // 高さ
            double appHeight = this.ActualHeight;
            double boxHeight = ((appHeight - 40) / 2);

            btc_box.Height = boxHeight;
            ltc_box.Height = boxHeight;
            xrp_box.Height = boxHeight;
            eth_box.Height = boxHeight;
            mona_box.Height = boxHeight;
            bch_box.Height = boxHeight;


            /*
             
            if (this.ActualWidth >= 480)
            {
                //this.Title = "xsmall 480dp 以上 (" + this.ActualWidth.ToString() + "x" + this.ActualHeight.ToString() + ")";

                if (this.ActualWidth >= 600)
                {
                    //this.Title = "small 600dp 以上 (" + this.ActualWidth.ToString() + "x" + this.ActualHeight.ToString() + ")";

                    if (this.ActualWidth >= 840)
                    {
                        //this.Title = "small 840dp 以上 (" + this.ActualWidth.ToString() + "x" + this.ActualHeight.ToString() + ")";

                        if (this.ActualWidth >= 960)
                        {
                            //this.Title = "medium 960dp 以上 (" + this.ActualWidth.ToString() + "x" + this.ActualHeight.ToString() + ")";

                            if (this.ActualWidth >= 1024)
                            {
                                //this.Title = "medium 1024dp 以上 (" + this.ActualWidth.ToString() + "x" + this.ActualHeight.ToString() + ")";

                                if (this.ActualWidth >= 1280)
                                {
                                    //this.Title = "large 1280dp 以上 (" + this.ActualWidth.ToString() + "x" + this.ActualHeight.ToString() + ")";

                                    if (this.ActualWidth >= 1440)
                                    {
                                        //this.Title = "large 1440dp 以上 (" + this.ActualWidth.ToString() + "x" + this.ActualHeight.ToString() + ")";

                                        if (this.ActualWidth >= 1600)
                                        {
                                            //this.Title = "large 1600dp 以上 (" + this.ActualWidth.ToString() + "x" + this.ActualHeight.ToString() + ")";

                                            //if (this.ActualWidth >= 1920)
                                            //{
                                            //this.Title = "xlarge 1920dp 以上 (" + this.ActualWidth.ToString() + "x" + this.ActualHeight.ToString() + ")";

                                            ResizeToXXXLarge();

                                            //System.Diagnostics.Debug.WriteLine(" LayoutChange (ResizeToXXXLarge) - width:" + this.ActualWidth.ToString() + " - height:" + this.ActualHeight.ToString());
                                            //}
                                        }
                                        else
                                        {
                                            // Width = 1536 x Height = 848

                                            if (this.ActualWidth > 1536)
                                            {
                                                //this.Title = "Big 1536 x 848  (" + this.ActualWidth.ToString() + "x" + this.ActualHeight.ToString() + ")";

                                                ResizeToXXXLarge();

                                                //System.Diagnostics.Debug.WriteLine(" LayoutChange (ResizeToXXXLarge) - width:" + this.ActualWidth.ToString() + " - height:" + this.ActualHeight.ToString());
                                            }
                                            else
                                            {
                                                ResizeToLarge();

                                                //System.Diagnostics.Debug.WriteLine(" LayoutChange (ResizeToLarge) - width:" + this.ActualWidth.ToString() + " - height:" + this.ActualHeight.ToString());
                                            }


                                        }
                                    }
                                    else
                                    {

                                        if (this.ActualWidth > 1356)
                                        {
                                            ResizeToLarge();

                                            //System.Diagnostics.Debug.WriteLine(" LayoutChange (ResizeToLarge) - width:" + this.ActualWidth.ToString() + " - height:" + this.ActualHeight.ToString());
                                        }
                                        else
                                        {
                                            ResizeToDefault();

                                            //System.Diagnostics.Debug.WriteLine(" LayoutChange (ResizeToDefault) - width:" + this.ActualWidth.ToString() + " - height:" + this.ActualHeight.ToString());
                                        }

                                        // default
                                        // Width = 1300 x Height = 760
                                        //this.Title = "Default 1300 x 760  (" + this.ActualWidth.ToString() + "x" + this.ActualHeight.ToString() + ")";

                                    }
                                }
                                else
                                {

                                    if (this.ActualWidth > 1124)
                                    {
                                        ResizeToDefault();

                                        //System.Diagnostics.Debug.WriteLine(" LayoutChange (ResizeToDefault) - width:" + this.ActualWidth.ToString() + " - height:" + this.ActualHeight.ToString());
                                    }
                                    else
                                    {
                                        ResizeToSmall();

                                        //System.Diagnostics.Debug.WriteLine(" LayoutChange (ResizeToSmall) - width:" + this.ActualWidth.ToString() + " - height:" + this.ActualHeight.ToString());
                                    }

                                }
                            }
                            else
                            {
                                ResizeToSmall();
                            }
                        }
                        else
                        {
                            ResizeToSmall();
                        }
                    }
                    else
                    {
                        // small
                        //this.Title = "Small  (" + this.ActualWidth.ToString() + "x" + this.ActualHeight.ToString() + ")";

                        ResizeToSmall();

                    }
                }
                else
                {
                    // smallest
                    //this.Title = "X Small  (" + this.ActualWidth.ToString() + "x" + this.ActualHeight.ToString() + ")";

                    ResizeToXXXSmall();

                }
            }
            else
            {
                //// 480dp 未満
                //this.Title = "480dp 未満 xsmall (" + this.ActualWidth.ToString() + ")";

                ResizeToXXXSmall();
            }

            */

        }

        private void ResizeToXXXSmall()
        {

        }

        private void ResizeToSmall()
        {


        }

        private void ResizeToDefault()
        {


        }

        private void ResizeToLarge()
        {


        }

        private void ResizeToXXXLarge()
        {



            /*
            // 左サイドメニュー
            LeftSideMenu.Visibility = Visibility.Visible;
            //LeftMenuColum.Width = new GridLength(42, GridUnitType.Pixel);

            TopMenu.Visibility = Visibility.Visible;

            TopMenu.SetValue(Grid.ColumnSpanProperty, 5);

            BottomContents.SetValue(Grid.ColumnSpanProperty, 4);
            Split.SetValue(Grid.ColumnSpanProperty, 4);

            Split.Visibility = Visibility.Visible;
            BottomContents.Visibility = Visibility.Visible;

            MainScroller.SetValue(Grid.ColumnSpanProperty, 4);
            // メインスクロールVのRowスパンを1に指定
            MainScroller.SetValue(Grid.RowSpanProperty, 1);

            Main1Grid.SetValue(Grid.ColumnSpanProperty, 3);
            Main2Grid.SetValue(Grid.ColumnSpanProperty, 1);

            Main3Grid.SetValue(Grid.ColumnProperty, 1);
            Main3Grid.SetValue(Grid.RowProperty, 0);
            Main3Grid.SetValue(Grid.ColumnSpanProperty, 2);

            // 歩み値を
            Transaction.Visibility = Visibility.Visible;
            Transaction.SetValue(Grid.ColumnProperty, 3);
            Transaction.SetValue(Grid.RowProperty, 0);

            //Asset.SetValue(Grid.ColumnSpanProperty, 2);
            //Order.SetValue(Grid.ColumnSpanProperty, 2);
            //Ifdoco.SetValue(Grid.ColumnSpanProperty, 2);
            //Chart.SetValue(Grid.ColumnSpanProperty, 2);
            Middle.SetValue(Grid.ColumnSpanProperty, 2);

            //Main1Grid.Height = 518;
            //MainContentsGrid.Height = 518;



            // ボトムを表示
            BottomContents.Visibility = Visibility.Visible;
            Split.Visibility = Visibility.Visible;

            Transaction.Visibility = Visibility.Visible;

            Depth.Visibility = Visibility.Visible;



            // メインコンテンツグリッドの高さ指定。＊スクロールが出るかでないかここのサイズ。
            MainContentsGrid.Height = 420;//1036;

            // メイン１の高さを指定　＊重要
            Main1Grid.Height = 420;

            Main2Grid.Height = 420;

            Main3Grid.Height = 420;


            ////
            //Asset.Width = 720;
            //Order.Width = 720;
            //Ifdoco.Width = 720;
            //Asset.Width = 720;
            //Order.Width = 720;
            //Ifdoco.Width = 720;
            //Chart.Width = 720;
            Middle.Width = 720;

            //Main2Colum.Width = new GridLength(360, GridUnitType.Pixel);
            //Main3Colum.Width = new GridLength(360, GridUnitType.Pixel);
            Main2Colum.Width = new GridLength(380, GridUnitType.Pixel);
            Main3Colum.Width = new GridLength(380, GridUnitType.Pixel);


            Transaction.Margin = new Thickness(0, 0, 20, 6);

            BottomContents.Margin = new Thickness(3, 0, 20, 0);

            //Chart.Margin = new Thickness(0, 0, 20, 6);
            Middle.Margin = new Thickness(0, 0, 20, 6);



            RightSide1Colum.Width = new GridLength(213, GridUnitType.Pixel);
            RightSide2Colum.Width = new GridLength(213, GridUnitType.Pixel);


            ChartSpanOneYearRadioButton.Visibility = Visibility.Visible;
            ChartSpanOneWeekRadioButton.Visibility = Visibility.Visible;
            ChartSpanThreeHourRadioButton.Visibility = Visibility.Visible;
            */
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {

            if (this.WindowState == WindowState.Normal)
            {
                RestoreButton.Visibility = Visibility.Collapsed;
                MaxButton.Visibility = Visibility.Visible;

                _dispatcherMouseTimer.Stop();
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                RestoreButton.Visibility = Visibility.Visible;
                MaxButton.Visibility = Visibility.Collapsed;

                _dispatcherMouseTimer.Start();

            }

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaxButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Normal;
        }

        private void MinButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Cursor = null;
            Mouse.OverrideCursor = null;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

    }

}
