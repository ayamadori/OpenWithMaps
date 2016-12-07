using System;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;

namespace OpenWithMaps
{
    class KmlConverter
    {
        private StorageFile kmlfile;

        public KmlConverter(StorageFile kmlfile)
        {
            this.kmlfile = kmlfile;
        }

        public async Task<string> ParseAsync()
        {
            // https://msdn.microsoft.com/en-us/library/windows/apps/windows.data.xml.dom.xmldocument.aspx
            XmlDocument kml = await XmlDocument.LoadFromFileAsync(kmlfile);
            // http://stackoverflow.com/questions/13325541/what-format-is-expected-by-the-namespaces-parameter-in-selectsinglenodens
            string _ns = "xmlns:x='http://www.opengis.net/kml/2.2'";
            string _name = "name." + Uri.EscapeDataString(kml.DocumentElement.SelectSingleNodeNS("x:Document/x:name", _ns).InnerText);
            var elements = kml.DocumentElement.SelectNodesNS("//x:Placemark", _ns);
            string _points = "";
            foreach(var item in elements)
            {
                string _placename = Uri.EscapeDataString(item.SelectSingleNodeNS("x:name", _ns).InnerText);
                string _coordinate = item.SelectSingleNodeNS("x:Point/x:coordinates", _ns).InnerText;
                string _point = "~point." + _coordinate.Split(',')[1] + "_" + _coordinate.Split(',')[0] + "_" + _placename;
                _points += _point;
            }

            return "collection=" + _name + _points;
        }
    }
}
