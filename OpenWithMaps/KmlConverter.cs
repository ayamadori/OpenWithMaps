﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;

namespace OpenWithMaps
{
    class KmlConverter
    {
        private StorageFile kmlFile;
        private StorageFile kmzFile;

        public KmlConverter(StorageFile file)
        {
            if (file.Name.EndsWith(".kml", StringComparison.OrdinalIgnoreCase)) // KML
            {
                this.kmlFile = file;
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
                        entry.ExtractToFile(Path.Combine(folder.Path, entry.FullName));
                        // Set KML file
                        kmlFile = await folder.GetFileAsync(entry.FullName);
                    }
                }
            }
        }

        public async Task<string> ParseAsync()
        {
            if (kmzFile != null)
                await UnzipKmz(kmzFile);

            string _name = "name.Collection";
            string _points = "";
            try
            {
                // https://msdn.microsoft.com/en-us/library/windows/apps/windows.data.xml.dom.xmldocument.aspx
                XmlDocument kml = await XmlDocument.LoadFromFileAsync(kmlFile);
                // http://stackoverflow.com/questions/13325541/what-format-is-expected-by-the-namespaces-parameter-in-selectsinglenodens
                string _ns = "xmlns:x='http://www.opengis.net/kml/2.2'";
                _name = "name." + Uri.EscapeDataString(kml.DocumentElement.SelectSingleNodeNS("x:Document/x:name", _ns).InnerText);
                XmlNodeList elements = kml.DocumentElement.SelectNodesNS("//x:Placemark", _ns);               

                foreach (IXmlNode item in elements)
                {
                    string _placename = "";
                    string _longitude = "";
                    string _latitude = "";
                    //Debug.WriteLine("ITEM: " + item.GetXml());
                    try
                    {
                        _placename = Uri.EscapeDataString(item.SelectSingleNodeNS("x:name", _ns).InnerText);
                        string[] _coordinate = item.SelectSingleNodeNS("*//x:coordinates", _ns).InnerText.Split(',');
                        _longitude = _coordinate[0];
                        _latitude = _coordinate[1];
                        _points += "~point." + _latitude + "_" + _longitude + "_" + _placename;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("PARSE ERROR 1: " + e);
                    }        
                }          
            }
            catch (Exception e)
            {
                Debug.WriteLine("PARSE ERROR 2: " + e);
            }

            // Clear in temporary folder
            ClearTempFolder();

            return "collection=" + _name + _points;
        }
    }
}
