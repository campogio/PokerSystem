using GTANetworkAPI;

namespace WiredPlayers.Data.Temporary
{
    public class SpawnModel
    {
        public Vector3 Position { get; set; }
        public float Heading { get; set; }

        public SpawnModel() { }

        public SpawnModel(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
        }
    }
}
