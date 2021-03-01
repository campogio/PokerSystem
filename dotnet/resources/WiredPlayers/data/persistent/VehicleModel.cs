using GTANetworkAPI;

namespace WiredPlayers.Data.Persistent
{
    public class VehicleModel
    {
        public int Id { get; set; }
        public uint Model { get; set; }
        public string Owner { get; set; }
        public string Plate { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public int ColorType { get; set; }
        public string FirstColor { get; set; }
        public string SecondColor { get; set; }
        public int Pearlescent { get; set; }
        public uint Dimension { get; set; }
        public int Faction { get; set; }
        public int Engine { get; set; }
        public int Locked { get; set; }
        public int Price { get; set; }
        public int Parking { get; set; }
        public int Parked { get; set; }
        public float Gas { get; set; }
        public float Kms { get; set; }
        public bool Testing { get; set; }
    }
}
