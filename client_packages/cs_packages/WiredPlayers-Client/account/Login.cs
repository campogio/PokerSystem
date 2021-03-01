using RAGE;
using RAGE.Elements;
using WiredPlayers_Client.globals;

namespace WiredPlayers_Client.account
{
    class Login : Events.Script
    {
        public Login()
        {
            Events.Add("requestPlayerLogin", RequestPlayerLoginEvent);
            Events.Add("showLoginError", ShowLoginErrorEvent);
            Events.Add("clearLoginWindow", ClearLoginWindowEvent);
        }

        public static void ShowLoginForm()
        {
            // Create login window
            Browser.CreateBrowser("connection.html", null, "showLogin", AccountHandler.SocialName);
        }

        private void RequestPlayerLoginEvent(object[] args)
        {
            // Get the password from the array
            string password = args[0].ToString();

            // Check for the credentials
            Events.CallRemote("loginAccount", password);
        }

        private void ShowLoginErrorEvent(object[] args)
        {
            // Show the message on the panel
            Browser.ExecuteFunction("showLoginError");
        }

        private void ClearLoginWindowEvent(object[] args)
        {
            // Unfreeze the player
            Player.LocalPlayer.FreezePosition(false);

            // Show the message on the panel
            Browser.DestroyBrowserEvent(null);

            // Create the ColShapes
            AccountHandler.CreateLobbyColShapes(args[0].ToString(), args[1].ToString());
        }
    }
}

