using RAGE;
using RAGE.Elements;
using WiredPlayers_Client.globals;
using System;

namespace WiredPlayers_Client.account
{
    public class Logout : Events.Script
    {
        public Logout()
        {
            Events.Add("showLogoutWindow", ShowLogoutWindowEvent);
            Events.Add("handlePlayerLogout", HandlePlayerLogoutEvent);
        }

        private void ShowLogoutWindowEvent(object[] args)
        {
            // Disable the chat and freeze the player
            Chat.Activate(false);
            Player.LocalPlayer.FreezePosition(true);

            // Show the logout timer
            Browser.CreateBrowser("logout.html", null, "logPlayerOut");
        }

        private void HandlePlayerLogoutEvent(object[] args)
        {
            // Enable the chat and unfreeze the player
            Chat.Activate(true);
            Player.LocalPlayer.FreezePosition(false);

            // Destroy the CEF browser
            Browser.DestroyBrowserEvent(null);

            if (Convert.ToBoolean(args[0]))
            {
                // Send the player to the lobby
                Events.CallRemote("forcePlayerLogout");
            }
        }
    }
}
