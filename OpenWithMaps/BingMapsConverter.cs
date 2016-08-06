using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenWithMaps
{
    class BingMapsConverter : MapConverter
    {
        // refer to https://alastaira.wordpress.com/2012/06/19/url-parameters-for-the-bing-maps-website/

        private const string uriRegExp = @"^https*://www\.bing\.com/maps/";

        public override bool IsMapURI(string uri)
        {
            return Regex.IsMatch(uri, uriRegExp, RegexOptions.IgnoreCase);
        }

        public override string GetQuery(string uri, string title)
        {
            return uri.Split('?')[1].Replace("where1", "where");
        }
    }
}
