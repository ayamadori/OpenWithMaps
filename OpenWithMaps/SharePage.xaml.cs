using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Storage;
using Windows.System.Profile;
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
            BingMapsConverter bing = new BingMapsConverter();
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
                    else if (bing.IsMapURI(_link))
                    {
                        Debug.WriteLine("This URI is in Bing Maps");
                        _bingquery = bing.GetQuery(_link, _title);
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
            else if (e.Parameter is StorageFile) // KML/KMZ file
            {
                KmlConverter kmlfile = new KmlConverter(e.Parameter as StorageFile);
                _bingquery = await kmlfile.ParseAsync();
            }

            // Get OS Version
            // https://social.msdn.microsoft.com/Forums/vstudio/en-US/2d8a7dab-1bad-4405-b70d-768e4cb2af96/uwp-get-os-version-in-an-uwp-app
            string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong version = ulong.Parse(deviceFamilyVersion);
            ulong major = (version & 0xFFFF000000000000L) >> 48;
            ulong minor = (version & 0x0000FFFF00000000L) >> 32;
            ulong build = (version & 0x00000000FFFF0000L) >> 16;
            ulong revision = (version & 0x000000000000FFFFL);
            var osVersion = $"{major}.{minor}.{build}.{revision}";
            Debug.WriteLine("OS Version: " + osVersion);

            if (build < 15063) // Older build than Creators Update(=15063)
            {
                // Delay before opening browser (I can't understand necessity...)
                // http://blog.okazuki.jp/entry/20120302/1330643881
                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }

            // Open Maps app
            // refer to https://msdn.microsoft.com/library/windows/apps/mt228341
            var uriBingMaps = new Uri(@"bingmaps:?" + _bingquery);

            // Launch the Windows Maps app
            var launcherOptions = new Windows.System.LauncherOptions();
            launcherOptions.TargetApplicationPackageFamilyName = "Microsoft.WindowsMaps_8wekyb3d8bbwe";
            var success = await Windows.System.Launcher.LaunchUriAsync(uriBingMaps, launcherOptions);

            // Exit app
            Application.Current.Exit();
        }
    }
}
