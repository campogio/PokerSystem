using GTANetworkAPI;
using WiredPlayers.Data.Base;

namespace WiredPlayers.Data.Persistent
{
    public class BusinessModel : PropertyModel
    {
        public string Owner { get; set; }
        public int Funds { get; set; }
        public int Products { get; set; }
        public float Multiplier { get; set; }
        public bool Locked { get; set; }
        public Marker BusinessMarker { get; set; }
    }
}
