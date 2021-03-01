using System;
using RAGE;
using RAGE.Ui;
using WiredPlayers_Client.account;
using WiredPlayers_Client.Utility;

namespace WiredPlayers_Client.chat
{
    class ChatManager : Events.Script
    {
        public static bool Visible { get; private set; }
        public static bool Opened { get; set; }
        public static bool Locked { get; set; }
        public static HtmlWindow ChatBrowser { get; private set; }

        public ChatManager()
        {
            // Deactivate the default chat
            Chat.Activate(false);

            // Create the custom chat
            ChatBrowser = new HtmlWindow("package://statics/html/chat.html");
            ChatBrowser.MarkAsChat();
            Chat.SafeMode = true;

            // Lock the chat
            Locked = true;

            // Register custom events
            Events.Add("toggleChatOpen", ToggleChatOpenEvent);

            // Register RAGE's events
            Events.OnPlayerCommand += OnPlayerCommandEvent;
        }

        public static void ToggleChatOpenEvent(object[] args)
        {
            // Get the locked state
            Opened = Convert.ToBoolean(args[0]);

            // Change the cursor state
            Cursor.Visible = Opened;

            // Toggle the chat open
            ChatBrowser.Call("chat:show", Opened);
        }

        private void OnPlayerCommandEvent(string cmd, Events.CancelEventArgs cancel)
        {
            // Temporary fix for Windows 7 users
            if (!AccountHandler.PlayerLogged && cmd != "keyf")
            {
                // Send the message to the player
                Events.CallRemote("playerNotLoggedCommand");

                // Cancel the command
                cancel.Cancel = true;
            }
            else
            {
                int commandKey = -1;

                switch(cmd)
                {
                    case "key+":
                        commandKey = (int)ConsoleKey.Add;
                        break;
                    case "keyf":
                        commandKey = (int)ConsoleKey.F;
                        break;
                    case "keyk":
                        commandKey = (int)ConsoleKey.K;
                        break;
                    case "keyr":
                        commandKey = (int)ConsoleKey.R;
                        break;
                    case "keyf2":
                        commandKey = (int)ConsoleKey.F2;
                        break;
                }

                if(commandKey >= 0)
                {
                    // Send the key command
                    KeyHandler.HandleReleasedKeyAction(commandKey);
                }

                // Log the command
                Events.CallRemote("logPlayerCommand", cmd);
            }
        }

        public static void SetVisible(bool visible)
        {
            // Show the chat
            ChatBrowser.Active = visible;

            // Toggle the visible state
            Visible = visible;
        }
    }
}
