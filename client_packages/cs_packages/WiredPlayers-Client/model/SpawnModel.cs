using RAGE;

namespace WiredPlayers_Client.model
{
    public class SpawnModel
    {
        public Vector3 Position { get; private set; }
        public float Heading { get; private set; }

        public SpawnModel(Vector3 position, float heading)
        {
            Position = position;
            Heading = heading;
        }
    }
}
