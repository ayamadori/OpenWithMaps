using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.System.Profile;

namespace OpenWithMaps
{
    class KmlConverter
    {
        private StorageFile kmlFile;
        private StorageFile kmzFile;
        // https://support.microsoft.com/ja-jp/help/208427/maximum-url-length-is-2,083-characters-in-internet-explorer
        private const int MAX_URL_LENGTH = 2073; // =2083-"bingmaps:?".Length

        public KmlConverter(StorageFile file)
        {
            if (file.Name.EndsWith(".kml", StringComparison.OrdinalIgnoreCase)) // KML
            {
                kmlFile = file;
            }
            else if (file.Name.EndsWith(".kmz", StringComparison.OrdinalIgnoreCase)) // KMZ
            {
                kmzFile = file;
            }
        }

        // Clear in temporary folder
        private async void ClearTempFolder()
        {
            StorageFolder folder = ApplicationData.Current.TemporaryFolder;
            var items = await folder.GetFilesAsync();
            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    await item.DeleteAsync();
                }
            }
        }

        // http://stackoverflow.com/questions/36728529/extracting-specific-file-from-archive-in-uwp
        private async Task UnzipKmz(StorageFile kmzFile)
        {
            StorageFolder folder = ApplicationData.Current.TemporaryFolder;
            // Clear in temporary folder
            ClearTempFolder();
            // Need to copy file to temporary folder
            StorageFile temp = await kmzFile.CopyAsync(folder);
            using (ZipArchive archive = ZipFile.OpenRead(temp.Path))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".kml", StringComparison.OrdinalIgnoreCase))
                    {
                        string filename = kmzFile.DisplayName.Replace(".kmz", "") + ".kml";
                        entry.ExtractToFile(Path.Combine(folder.Path, filename));
                        // Set KML file
                        kmlFile = await folder.GetFileAsync(filename);
                    }
                }
            }
        }

        public async Task<string> ParseAsync()
        {
            if (kmzFile != null)
                await UnzipKmz(kmzFile);

            string name = "name." + Uri.EscapeDataString(kmlFile.DisplayName.Replace(".kml", ""));
            string points = "";
            try
            {
                // https://msdn.microsoft.com/en-us/library/windows/apps/windows.data.xml.dom.xmldocument.aspx
                XmlDocument kml = await XmlDocument.LoadFromFileAsync(kmlFile);
                // http://stackoverflow.com/questions/13325541/what-format-is-expected-by-the-namespaces-parameter-in-selectsinglenodens
                string _ns = $"xmlns:x='{kml.DocumentElement.NamespaceUri}'";
                XmlNodeList elements = kml.DocumentElement.SelectNodesNS("//x:Placemark", _ns);

                foreach (IXmlNode item in elements)
                {
                    string placename = "";
                    string longitude = "";
                    string latitude = "";

                    try
                    {
                        placename = Uri.EscapeDataString(item.SelectSingleNodeNS("x:name", _ns).InnerText.Trim());
                        string[] _coordinate = item.SelectSingleNodeNS("*//x:coordinates", _ns).InnerText.Split(',');
                        longitude = _coordinate[0].Trim();
                        latitude = _coordinate[1].Trim();
                        points += $"~point.{latitude}_{longitude}_{placename}";
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("PARSE ERROR 1: " + e);
                    }        
                }

                name = "name." + Uri.EscapeDataString(kml.DocumentElement.SelectSingleNodeNS("*/x:name", _ns).InnerText.Trim());
            }
            catch (Exception e)
            {
                Debug.WriteLine("PARSE ERROR 2: " + e);
            }

            // Clear in temporary folder
            ClearTempFolder();

            // https://gist.github.com/wagonli/40d8a31bd0d6f0dd7a5d
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop" && name.Length + points.Length > MAX_URL_LENGTH)
                points = points.Remove(points.LastIndexOf("~", MAX_URL_LENGTH - name.Length));

            int paramlength = name.Length + points.Length;
            Debug.WriteLine("Count= " + paramlength);

            return $"collection={name}{points}";
        }
    }
}
