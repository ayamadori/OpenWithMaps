using System;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace OpenWithMaps
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class SharePage : Page
    {
        public SharePage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string _bingquery = "";
            GoogleMapsConverter google = new GoogleMapsConverter();
            YahooMapsJpConverter yahoojp = new YahooMapsJpConverter();

            // refer to https://msdn.microsoft.com/en-us/library/windows/apps/mt243292.aspx
            if (e.Parameter is ShareOperation) // Share target
            {
                ShareOperation shareOperation = e.Parameter as ShareOperation;

                if (shareOperation.Data.Contains(StandardDataFormats.WebLink)) // URI
                {
                    Uri uri = await shareOperation.Data.GetWebLinkAsync();
                    string _link = uri.AbsoluteUri;
                    string _title = shareOperation.Data.Properties.Title;

                    Debug.WriteLine("WebLink: " + _link);

                    if (google.IsMapURI(_link))
                    {
                        Debug.WriteLine("This URI is in Google Maps");
                        _bingquery = google.GetQuery(_link, _title);
                    }
                    else if (yahoojp.IsMapURI(_link))
                    {
                        Debug.WriteLine("This URI is in Yahoo Maps(Japan)");
                        _bingquery = yahoojp.GetQuery(_link, _title);
                    }
                    else
                    {
                        Debug.WriteLine("This URI is Others");
                        _bingquery = "where=" + Uri.EscapeDataString(_title);
                    }
                }
                else if (shareOperation.Data.Contains(StandardDataFormats.Text)) // Text
                {
                    string _query = await shareOperation.Data.GetTextAsync();

                    Debug.WriteLine("Text: " + _query);
                    _bingquery = "where=" + Uri.EscapeDataString(_query);
                }
            }

            // Open Bing Maps website
            // refer to https://learn.microsoft.com/ja-jp/windows/apps/develop/launch/launch-maps-app
            var uriBingMaps = new Uri(@"https://www.bing.com/maps?" + _bingquery);
            var success = await Launcher.LaunchUriAsync(uriBingMaps);

            // Exit app
            Application.Current.Exit();
        }
    }
}
