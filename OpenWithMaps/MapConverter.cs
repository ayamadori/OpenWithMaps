namespace OpenWithMaps
{
    abstract class MapConverter
    {
        public abstract bool IsMapURI(string uri);
        public abstract string GetQuery(string uri, string title);
    }
}
