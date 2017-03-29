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

        public override string GetQuery(string uri, string title)
        {
            string _query = "";
            string[] _param = uri.Substring(uri.IndexOf("maps/") + 5).Split('/', '?');

            if (_param[0].StartsWith("@")) // Centroid
                _query = "collection=" + getCollection(_param[0], title) + getCentroid(_param[0]);
            else if (_param[0].StartsWith("place", StringComparison.OrdinalIgnoreCase)) // Place -> place/(place)/(centroid)
                _query = "where=" + _param[1] + getCentroid(_param[2]);
            else if (_param[0].StartsWith("search", StringComparison.OrdinalIgnoreCase)) // Search -> search/(query)/(centroid)
                _query = "q=" + _param[1] + getCentroid(_param[2]);
            else if (_param[0].StartsWith("dir", StringComparison.OrdinalIgnoreCase)) // Dir(=Route) -> dir/(start)/(point)/---/(goal)/(centroid)
            {
                _query = "rtp=";
                for (int i = 1; i < _param.Length - 2; i++)
                    _query += getWaypoint(_param[i]) + "~";
                _query.Remove(_query.Length - 1);
                _query += getCentroid(_param[_param.Length - 2]);
            }

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

        // Get Collection
        // ->  @(latitude),(longitude),(zoomlevel)z
        private string getCollection(string param, string title)
        {
            string _center = "";
            string[] _point = param.Split(new char[] { '@', ',', 'z' }, StringSplitOptions.RemoveEmptyEntries);
            _center = "point." + _point[0] + "_" + _point[1] + "_" + Uri.EscapeDataString(title);
            return _center;
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
