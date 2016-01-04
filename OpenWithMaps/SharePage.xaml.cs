using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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

            // refer to https://msdn.microsoft.com/en-us/library/windows/apps/mt243292.aspx
            ShareOperation shareOperation = (ShareOperation)e.Parameter;
            string _bingquery = "";

            if (shareOperation.Data.Contains(StandardDataFormats.Uri)) // URI
            {
                string _link = "";
                Uri uri = await shareOperation.Data.GetUriAsync();
                _link = uri.AbsoluteUri;

                Debug.WriteLine("Uri: " + _link);

                GoogleMapsConverter google = new GoogleMapsConverter();
                BingMapsConverter bing = new BingMapsConverter();
                YahooMapsJpConverter yahoojp = new YahooMapsJpConverter();
                if (google.IsMapURI(_link))
                {
                    Debug.WriteLine("This URI is in Google Maps");
                    _bingquery = google.GetQuery(_link);
                }
                else if (yahoojp.IsMapURI(_link))
                {
                    Debug.WriteLine("This URI is in Yahoo Maps(Japan)");
                    _bingquery = yahoojp.GetQuery(_link);
                }
                else if (bing.IsMapURI(_link))
                {
                    Debug.WriteLine("This URI is in Bing Maps");
                    _bingquery = bing.GetQuery(_link);
                }
            }
            else if (shareOperation.Data.Contains(StandardDataFormats.Text)) // Text
            {
                string _query = await shareOperation.Data.GetTextAsync();

                Debug.WriteLine("Text: " + _query);
                _bingquery = "where=" + WebUtility.UrlEncode(_query);
            }

            // Delay before opening map (I can't understand necessity...)
            // http://blog.okazuki.jp/entry/20120302/1330643881
            await Task.Delay(TimeSpan.FromMilliseconds(1000));

            OpenMaps(_bingquery);
        }

        private async void OpenMaps(string query)
        {
            // refer to https://msdn.microsoft.com/library/windows/apps/mt228341

            var uriNewYork = new Uri(@"bingmaps:?" + query);

            // Launch the Windows Maps app
            var launcherOptions = new Windows.System.LauncherOptions();
            launcherOptions.TargetApplicationPackageFamilyName = "Microsoft.WindowsMaps_8wekyb3d8bbwe";
            var success = await Windows.System.Launcher.LaunchUriAsync(uriNewYork, launcherOptions);
        }
    }
}
