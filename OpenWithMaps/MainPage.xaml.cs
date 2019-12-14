using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.ApplicationModel;
using Windows.System;
using Windows.Services.Store;


// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace OpenWithMaps
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            // https://docs.microsoft.com/en-us/windows/uwp/monetize/launch-feedback-hub-from-your-app
            if (Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported())
            {
                this.FeedbackButton.Visibility = Visibility.Visible;
            }
        }

        private async void RateButton_Click(object sender, RoutedEventArgs e)
        {
            // https://docs.microsoft.com/en-us/windows/uwp/monetize/request-ratings-and-reviews
            var success = await StoreContext.GetDefault().RequestRateAndReviewAppAsync();
        }

        private async void FeedbackButton_Click(object sender, RoutedEventArgs e)
        {
            //// https://docs.microsoft.com/en-us/windows/uwp/monetize/launch-feedback-hub-from-your-app
            var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
            await launcher.LaunchAsync();
        }

        private async void OpenBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            var uriBrowser = new Uri("https://www.google.com/maps/");
            var success = await Launcher.LaunchUriAsync(uriBrowser);
        }

        private async void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            // FileOpenPicker
            // https://msdn.microsoft.com/ja-jp/library/windows/apps/mt186456.aspx

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".kml");
            picker.FileTypeFilter.Add(".kmz");
            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                Frame.Navigate(typeof(SharePage), file);
            }
        }
    }
}
