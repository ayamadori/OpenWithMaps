using System;
using System.Text.RegularExpressions;

namespace OpenWithMaps
{
    class GoogleMapsConverter : MapConverter
    {
        // refer to https://moz.com/blog/new-google-maps-url-parameters

        private const string uriRegExp = @"^https://www\.google\..*/maps/";
        private double zoomAdjust = 0;//0.3;

        public override bool IsMapURI(string uri)
        {            
            return Regex.IsMatch(uri, uriRegExp, RegexOptions.IgnoreCase);
        }

        public override string GetQuery(string uri, string title)
        {
            string _query = "";
            string[] param = uri.Substring(uri.IndexOf("maps/") + 5).Split('/');

            if (param[0].StartsWith("@")) // Centroid
                _query = $"collection={getCollection(param[0], title)}{getCentroid(param[0])}";
            else if (param[0].StartsWith("place", StringComparison.OrdinalIgnoreCase)) // Place -> place/(place)/(centroid)
                _query = $"where={param[1]}{getCentroid(param[2])}";
            else if (param[0].StartsWith("search", StringComparison.OrdinalIgnoreCase)) // Search -> search/(query)/(centroid)
                _query = $"q={param[1]}{getCentroid(param[2])}";
            else if (param[0].StartsWith("dir", StringComparison.OrdinalIgnoreCase)) // Dir(=Route) -> dir/(start)/(point)/---/(goal)/(centroid)
            {
                _query = "rtp=";
                for (int i = 1; i < param.Length - 2; i++)
                    _query += getWaypoint(param[i]) + "~";
                _query.Remove(_query.Length - 1);
                _query += getCentroid(param[param.Length - 2]);
            }

            return _query;
        }

        // Get Centroid
        // ->  @(latitude),(longitude),(zoomlevel)z
        private string getCentroid(string param)
        {
            string _centroid = "";
            string[] point = param.Split(new char[] { '@', ',', 'z' }, StringSplitOptions.RemoveEmptyEntries);
            _centroid = $"&cp={point[0]}~{point[1]}&lvl={double.Parse(point[2]) + zoomAdjust}";
            return _centroid;
        }

        // Get Collection
        // ->  @(latitude),(longitude),(zoomlevel)z
        private string getCollection(string param, string title)
        {
            string _center = "";
            string[] point = param.Split(new char[] { '@', ',', 'z' }, StringSplitOptions.RemoveEmptyEntries);
            _center = $"point.{point[0]}_{point[1]}_{Uri.EscapeDataString(title)}";
            return _center;
        }

        // Get Waypoint
        // -> '(latitude),(longitude)' or (address)
        private string getWaypoint(string param)
        {
            string _waypoint = "";
            if (param.StartsWith("'"))
            {
                string[] temp = param.Split(new char[] { '\'', ',' }, StringSplitOptions.RemoveEmptyEntries);
                _waypoint = $"pos.{temp[0]}_{temp[1]}";
            }
            else if (param.Length > 0)
            {
                _waypoint = $"adr.{param}";
            }
            return _waypoint;
        }
    }
}
