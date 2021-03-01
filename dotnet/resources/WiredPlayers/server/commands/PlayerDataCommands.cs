using GTANetworkAPI;
using WiredPlayers.character;

namespace WiredPlayers.Server.Commands
{
    public static class PlayerDataCommands
    {
        [Command]
        public static void PlayerCommand(Player player)
        {
            // Get players basic data
            Character.RetrieveBasicDataEvent(player, player.Value);
        }
    }
}
