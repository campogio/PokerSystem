using WiredPlayers.Data.Base;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Persistent
{
    public class HouseModel : PropertyModel
    {
        public int Price { get; set; }
        public string Owner { get; set; }
        public HouseState State { get; set; }
        public int Tenants { get; set; }
        public int Rental { get; set; }
        public bool Locked { get; set; }
    }
}
