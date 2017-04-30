using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.ApplicationModel;
using Windows.System;


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
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var uriReview = new Uri($"ms-windows-store:REVIEW?PFN={Package.Current.Id.FamilyName}");
            var success = await Launcher.LaunchUriAsync(uriReview);
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
