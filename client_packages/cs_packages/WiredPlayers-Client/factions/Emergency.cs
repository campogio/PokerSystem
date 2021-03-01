using RAGE;
using RAGE.Elements;
using WiredPlayers_Client.globals;
using System;

namespace WiredPlayers_Client.factions
{
    class Emergency : Events.Script
    {
        public static bool dead;

        public Emergency()
        {
            Events.Add("togglePlayerDead", TogglePlayerDeadEvent);
        }

        private void TogglePlayerDeadEvent(object[] args)
        {
            // Change dead state
            dead = Convert.ToBoolean(args[0]);

            // Check if the player should be in God mode
            Player.LocalPlayer.SetInvincible(dead);
        }
    }
}
