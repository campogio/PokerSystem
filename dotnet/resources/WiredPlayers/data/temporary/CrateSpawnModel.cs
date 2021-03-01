using GTANetworkAPI;

namespace WiredPlayers.Data.Temporary
{
    public class CrateSpawnModel
    {
        public int SpawnPoint { get; set; }
        public Vector3 Position { get; set; }

        public CrateSpawnModel(int spawnPoint, Vector3 position)
        {
            SpawnPoint = spawnPoint;
            Position = position;
        }
    }
}
