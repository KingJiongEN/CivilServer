using System;

namespace Plugins.CityCommon.Msg
{
    [Serializable]
    public class AttrChange
    {
        public int attr;
        public int value;
        public int value_change;

        public AttrChange(int attr, int value, int value_change)
        {
            this.attr = attr;
            this.value = value;
            this.value_change = value_change;
        }
    }
}