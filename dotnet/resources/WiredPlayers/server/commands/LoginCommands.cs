using GTANetworkAPI;

namespace WiredPlayers.Server.Commands
{
    public static class LoginCommands
    {
        [Command]
        public static void DisconnectCommand(Player player)
        {
            // Show the disconnect window
            player.TriggerEvent("showLogoutWindow");
        }
    }
}