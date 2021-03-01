using GTANetworkAPI;

namespace WiredPlayers.Data.Persistent
{
    public class FurnitureModel
    {
        public int Id { get; set; }
        public uint Hash { get; set; }
        public uint House { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Object Handle { get; set; }
    }
}
