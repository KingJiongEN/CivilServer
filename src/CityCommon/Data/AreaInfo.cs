using System;

namespace Plugins.CityCommon.Data
{
    [Serializable]
    public class AreaInfo : IKey
    {
        public int area_id;
        public string area_name;
        public int x;
        public int y;
        public string img;

        public int Key => area_id;
    }
}