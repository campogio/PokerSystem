using RAGE;
using RAGE.Elements;
using WiredPlayers_Client.account;
using WiredPlayers_Client.factions;
using WiredPlayers_Client.globals;
using System;
using System.Collections.Generic;
using System.Linq;
using WiredPlayers_Client.weapons;
using WiredPlayers_Client.chat;

namespace WiredPlayers_Client.Utility
{
    class KeyHandler : Events.Script
    {
        private static List<int> PressedKeys;
        private static List<int> ConsoleKeys;

        public KeyHandler()
        {
            // Initialize the key list
            PressedKeys = new List<int>();

            // Bind the required Keys
            BindConsoleKeys();
        }

        public static void CheckKeysToggled()
        {
            // Check if any key has been pressed
            CheckPressedKeys();

            if (PressedKeys != null && PressedKeys.Count > 0)
            {
                // Check if any key has been released
                CheckReleasedKeys();
            }
        }

        public static void HandleReleasedKeyAction(int key)
        {
            // Prevent actions when chat is opened
            if (ChatManager.Opened && key != (int)ConsoleKey.Escape) return;

            // Get the local player
            Player localPlayer = Player.LocalPlayer;

            switch (key)
            {
                case (int)ConsoleKey.Add:
                    if (!Police.handcuffed && !Emergency.dead)
                    {
                        // Reset the player's animation
                        Events.CallRemote("checkPlayerEventKeyStopAnim");
                    }
                    break;

                case (int)ConsoleKey.F:
                    if (!AccountHandler.PlayerLogged)
                    {
                        // Handle the lobby event
                        AccountHandler.HandleLobbyColShape();
                        return;
                    }

                    if (Globals.ExitMarkerList.Count > 0 && Globals.IsPlayerExiting())
                    {
                        // Make the player exit the interior
                        Events.CallRemote("PlayerExitBuilding");
                        return;
                    }

                    if (Globals.ActionMarker != null && localPlayer.Position.DistanceTo(Globals.ActionMarker.Position) <= 1.5f)
                    {
                        // Make the player exit the interior
                        Events.CallRemote("PlayerToggledAction");
                        return;
                    }

                    if (localPlayer.Vehicle == null && !Police.handcuffed)
                    {
                        // Check if player can enter any place
                        Events.CallRemote("checkPlayerEventKey");
                    }
                    break;

                case (int)ConsoleKey.K:
                    if (localPlayer.Vehicle != null && !Police.handcuffed)
                    {
                        if (!localPlayer.Vehicle.IsSeatFree(-1, 0) && localPlayer.Vehicle.GetPedInSeat(-1, 0) == localPlayer.Handle)
                        {
                            // Toggle vehicle's engine
                            Events.CallRemote("engineOnEventKey");
                        }
                    }
                    break;

                case (int)ConsoleKey.R:
                    if (localPlayer.Vehicle == null && !Police.handcuffed)
                    {
                        uint weapon = localPlayer.GetSelectedWeapon();

                        if (weapon > 0 && !localPlayer.IsReloading() && Weapons.IsValidWeapon(weapon))
                        {
                            int ammo = 0;
                            localPlayer.GetAmmoInClip(weapon, ref ammo);

                            // Reload the weapon
                            Events.CallRemote("reloadPlayerWeapon", ammo);
                        }
                    }
                    break;

                case (int)ConsoleKey.T:
                    if (!ChatManager.Locked)
                    {
                        // Toggle chat activation
                        ChatManager.ToggleChatOpenEvent(new object[] { !ChatManager.Opened });
                    }
                    break;

                case (int)ConsoleKey.F2:
                    if (!Globals.viewingPlayers)
                    {
                        // Change the flag
                        Globals.viewingPlayers = true;

                        // Create the player list browser
                        Browser.CreateBrowser("playerList.html", "destroyBrowser");
                    }
                    break;

                case (int)ConsoleKey.F4:
                    // Check if there's any browser opened
                    if (Browser.CustomBrowser != null) return;

                    // Show the animation categories
                    Events.CallRemote("ShowAnimationCategories");
                    break;

                case (int)ConsoleKey.Escape:
                    // Destroy the current browser if there's any opened
                    if (Browser.CustomBrowser != null) Browser.ForceBrowserClose();
                    break;
            }
        }

        private static void CheckPressedKeys()
        {
            // Check if the list has been initialized
            if (PressedKeys == null) return;

            foreach (int key in ConsoleKeys)
            {
                // Check if the key has been pressed
                if (PressedKeys.Contains(key) || Input.IsUp(key)) continue;

                // Add the key to the list
                PressedKeys.Add(key);
            }
        }

        private static void CheckReleasedKeys()
        {
            // Check if any of the keys was released
            if (PressedKeys.Count(k => Input.IsUp(k)) == 0) return;

            // Search for any key being released
            int key = PressedKeys.First(k => Input.IsUp(k));
            PressedKeys.Remove(key);

            // Take the action corresponding to the pressed key
            HandleReleasedKeyAction(key);
        }

        private void BindConsoleKeys()
        {
            // Initialize the list
            ConsoleKeys = new List<int>()
            {
                (int)ConsoleKey.Add,
                (int)ConsoleKey.F,
                (int)ConsoleKey.K,
                (int)ConsoleKey.R,
                (int)ConsoleKey.T,
                (int)ConsoleKey.F2,
                (int)ConsoleKey.F4,
                (int)ConsoleKey.Escape
            };
        }
    }
}