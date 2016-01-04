using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenWithMaps
{
    class GoogleMapsConverter : MapConverter
    {
        // refer to https://moz.com/blog/new-google-maps-url-parameters

        private const string uriRegExp = @"^https://www\.google\..*/maps/";
        private double zoomAdjust = 0.3;

        public override bool IsMapURI(string uri)
        {            
            return Regex.IsMatch(uri, uriRegExp, RegexOptions.IgnoreCase);
        }

        public override string GetQuery(string uri)
        {
            string _query = "";
            string[] _param = uri.Substring(uri.IndexOf("maps/") + 5).Split('/', '?');

            if (_param[0].StartsWith("@")) // Centroid
                _query = getCentroid(_param[0]);
            else if (_param[0].StartsWith("place", StringComparison.OrdinalIgnoreCase)) // Place -> place/(place)/(centroid)
                _query = "where=" + _param[1] + getCentroid(_param[2]);
            else if (_param[0].StartsWith("search", StringComparison.OrdinalIgnoreCase)) // Search -> search/(query)/(centroid)
                _query = "q=" + _param[1] + getCentroid(_param[2]);
            else if (_param[0].StartsWith("dir", StringComparison.OrdinalIgnoreCase)) // Dir(=Route) -> dir/(start)/(goal)/(centroid)
                _query = "rtp=" + getWaypoint(_param[1]) + "~" + getWaypoint(_param[2]) + getCentroid(_param[3]);

            return _query;
        }

        // Get Centroid
        // ->  @(latitude),(longitude),(zoomlevel)z
        private string getCentroid(string param)
        {
            string _centroid = "";
            string[] _point = param.Split(new char[] { '@', ',', 'z' }, StringSplitOptions.RemoveEmptyEntries);
            _centroid = "&cp=" + _point[0] + "~" + _point[1] + "&lvl=" + (double.Parse(_point[2]) + zoomAdjust);
            return _centroid;
        }

        // Get Waypoint
        // -> '(latitude),(longitude)' or (address)
        private string getWaypoint(string param)
        {
            string _waypoint = "";
            if (param.StartsWith("'"))
            {
                string[] _temp = param.Split(new char[] { '\'', ',' }, StringSplitOptions.RemoveEmptyEntries);
                _waypoint = "pos." + _temp[0] + "_" + _temp[1];
            }
            else if (param.Length > 0)
            {
                _waypoint = "adr." + param;
            }
            return _waypoint;
        }
    }
}
