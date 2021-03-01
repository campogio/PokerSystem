using GTANetworkAPI;

namespace WiredPlayers.Data.Temporary
{
    public class DeathModel
    {
        public Player player { get; set; }
        public Player killer { get; set; }
        public uint weapon { get; set; }

        public DeathModel(Player player, Player killer, uint weapon)
        {
            this.player = player;
            this.killer = killer;
            this.weapon = weapon;
        }
    }
}
