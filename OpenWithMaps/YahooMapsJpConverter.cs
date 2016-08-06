using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenWithMaps
{
    class YahooMapsJpConverter : MapConverter
    {
        // refer to http://user.numazu-ct.ac.jp/~tsato/tsato/geoweb/yahoo_olp/djws/url_arg.html

        private const string uriRegExp = @"^http://map(s\.loco)?\.yahoo\.co\.jp/(maps)|(mobile)";
        private const string intentRegExp = @"^intent:.*http:%2F%2Floco.yahoo.co.jp%2Fplace";
        private double zoomAdjust = -0.5;

        public override bool IsMapURI(string uri)
        {
            return Regex.IsMatch(uri, uriRegExp, RegexOptions.IgnoreCase)
                | Regex.IsMatch(uri, intentRegExp, RegexOptions.IgnoreCase);
        }

        public override string GetQuery(string uri, string title)
        {
            string _query = "";
            string lat = "", lon = "";
            string[] _param;
            _param = uri.Split('?')[1].Split('&');

            foreach (string _p in _param)
            {
                if (_p.StartsWith("p=")) // Place?
                    _query += "&where=" + (_p.Split('='))[1];
                else if (_p.StartsWith("lat=")) // Latitude
                {
                    lat = (_p.Split('='))[1];
                    _query += "&cp=" + lat;
                }
                else if (_p.StartsWith("lon=")) // Longitude
                {
                    lon = (_p.Split('='))[1];
                    _query += "~" + lon;
                }
                else if (_p.StartsWith("z=")) // Zoom Level
                    _query += "&lvl=" + (double.Parse((_p.Split('='))[1]) + zoomAdjust);
            }

            // Set collection if not include specific place
            if (uri.IndexOf("p=") < 0)
                _query += "&collection=point." + lat + "_" + lon + "_" + Uri.EscapeDataString(title);

                return _query;
        }
    }
}
