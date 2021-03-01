using GTANetworkAPI;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Temporary
{
    public class JobPickModel
    {
        public PlayerJobs Job { get; set; }
        public uint Sprite { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public string Description { get; set; }

        public JobPickModel(PlayerJobs job, uint blip, string name, Vector3 position, string description)
        {
            Job = job;
            Sprite = blip;
            Name = name;
            Position = position;
            Description = description;
        }
    }
}
