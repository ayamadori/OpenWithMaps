using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenWithMaps
{
    abstract class MapConverter
    {
        public abstract bool IsMapURI(string uri);
        public abstract string GetQuery(string uri, string title);
    }
}
