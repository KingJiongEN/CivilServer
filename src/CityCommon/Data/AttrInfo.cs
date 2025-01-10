using System;
using System.Collections.Generic;

namespace Plugins.CityCommon.Data
{
    [Serializable]
    public class AttrInfo : IKey
    {
        public const int ENERGY = 101;
        public const int GOLD = 102;
        public const int PRIMITIVE = 103;
        public const int CHARACTER = 104;
        public const int CREATIVITY = 105;
        public const int CHARM = 106;
        public const int ART_STYLE	=	107	;
        public const int REBELLIOUSNESS = 108;
        public const int HEALTH = 109;
        public const int MOOD = 110;


        public int attr_id;
        public string attr_name;
        public int visible;
        public int variable;
        public float anim_duration;
        public string attr_icon;
        public int hide_on_zero;

        public bool IsVariable => variable == 1;
        public bool IsVisible => visible == 1;
        public bool IsHideOnZero => hide_on_zero == 1;
        public int Key => attr_id;
        
    }
}