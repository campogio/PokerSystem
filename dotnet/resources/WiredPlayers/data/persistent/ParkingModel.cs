using GTANetworkAPI;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Persistent
{
    public class ParkingModel
    {
        public int Id { get; set; }
        public ParkingTypes Type { get; set; }
        public int HouseId { get; set; }
        public Vector3 Position { get; set; }
        public int Capacity { get; set; }
        public TextLabel ParkingLabel { get; set; }
    }
}
