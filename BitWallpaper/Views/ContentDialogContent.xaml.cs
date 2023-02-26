using Microsoft.UI.Xaml.Controls;

namespace BitWallpaper.Views
{
    public sealed partial class ContentDialogContent : ContentDialog
    {

        public decimal HighPrice { get; set; }
        public decimal LowPrice { get; set; }

        public ContentDialogContent()
        {
            this.InitializeComponent();
            this.Opened += ContentDialog_Opened;
            this.Closing += ContentDialog_Closing;
            this.PrimaryButtonClick+= ContentDialog_PrimaryButtonClick;
        }

        void ContentDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (HighPrice>0)
                Higher.Text = HighPrice.ToString();
            if (LowPrice>0)
                Lower.Text = LowPrice.ToString();
            /*
            this.Result = SignInResult.Nothing;

            // If the user name is saved, get it and populate the user name field.
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Values.ContainsKey("userName"))
            {
                userNameTextBox.Text = roamingSettings.Values["userName"].ToString();
                saveUserNameCheckBox.IsChecked = true;
            }
            */
        }

        void ContentDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            /*
            // If sign in was successful, save or clear the user name based on the user choice.
            if (this.Result == SignInResult.SignInOK)
            {
                if (saveUserNameCheckBox.IsChecked == true)
                {
                    SaveUserName();
                }
                else
                {
                    ClearUserName();
                }
            }

            // If the user entered a name and checked or cleared the 'save user name' checkbox, then clicked the back arrow,
            // confirm if it was their intention to save or clear the user name without signing in.
            if (this.Result == SignInResult.Nothing && !string.IsNullOrEmpty(userNameTextBox.Text))
            {
                if (saveUserNameCheckBox.IsChecked == false)
                {
                    args.Cancel = true;
                    FlyoutBase.SetAttachedFlyout(this, (FlyoutBase)this.Resources["DiscardNameFlyout"]);
                    FlyoutBase.ShowAttachedFlyout(this);
                }
                else if (saveUserNameCheckBox.IsChecked == true && !string.IsNullOrEmpty(userNameTextBox.Text))
                {
                    args.Cancel = true;
                    FlyoutBase.SetAttachedFlyout(this, (FlyoutBase)this.Resources["SaveNameFlyout"]);
                    FlyoutBase.ShowAttachedFlyout(this);
                }
            }
            */
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (!string.IsNullOrEmpty(Higher.Text))
            {
                try
                {
                    HighPrice = Decimal.Parse(Higher.Text);
                }
                catch
                {
                    args.Cancel = true;
                    HighPrice = 0;
                    return;
                }
            }

            if (!string.IsNullOrEmpty(Lower.Text))
            {
                try
                {
                    LowPrice = Decimal.Parse(Lower.Text);
                }
                catch
                {
                    args.Cancel = true;
                    LowPrice = 0;
                    return;
                }
            }

            if (string.IsNullOrEmpty(Higher.Text) && string.IsNullOrEmpty(Lower.Text))
            {
                args.Cancel = true; 
                return;
            }

            if ((LowPrice == 0) && (HighPrice == 0))
            {
                args.Cancel = true; 
                return;
            }

            args.Cancel = false;

            // How do I disable primary button?

            //this.Result = ContentDialogResult.Primary;

        }
    }
}
