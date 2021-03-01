using GTANetworkAPI;

namespace WiredPlayers.Data.Persistent
{
    public class PlantModel
    {
        public int Id { get; set; }
        public Vector3 Position { get; set; }
        public uint Dimension { get; set; }
        public int GrowTime { get; set; }
        public Object Object { get; set; }
        public TextLabel Progress { get; set; }
        public ColShape PlantColshape { get; set; }
    }
}
