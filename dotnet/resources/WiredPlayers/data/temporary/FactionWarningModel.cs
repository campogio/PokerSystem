using GTANetworkAPI;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Temporary
{
    public class FactionWarningModel
    {
        public PlayerFactions Faction { get; set; }
        public int PlayerId { get; set; }
        public string Place { get; set; }
        public Vector3 Position { get; set; }
        public int TakenBy { get; set; }
        public string Hour { get; set; }

        public FactionWarningModel(PlayerFactions faction, int playerId, string place, Vector3 position, int takenBy, string hour)
        {
            Faction = faction;
            PlayerId = playerId;
            Place = place;
            Position = position;
            TakenBy = takenBy;
            Hour = hour;
        }
    }
}
