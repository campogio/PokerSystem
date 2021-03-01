using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using WiredPlayers_Client.globals;

namespace WiredPlayers_Client.chat
{
    class VoiceChat : Events.Script
    {
        private static float voiceDistance;
        public static bool voiceEnabled;
        private static List<Player> listeningPlayers;

        public VoiceChat()
        {
            // Add default events
            Events.OnPlayerQuit += OnPlayerQuitEvent;

            // Add custom events
            Events.Add("TogglePlayerSilence", TogglePlayerSilenceEvent);

            // Initialize the variables
            voiceDistance = 25.0f;
            listeningPlayers = new List<Player>();

            // Disable the voice by default
            Player.LocalPlayer.Voice3d = false;
        }

        private void OnPlayerQuitEvent(Player player)
        {
            if (!listeningPlayers.Contains(player)) return;

            // Remove the player from the list
            RemovePlayerFromListeners(player, false);
        }

        private void TogglePlayerSilenceEvent(object[] args)
        {
            // Toggle the mute state
            Voice.Muted = !Voice.Muted;
        }

        public static void Initialize()
        {
            // Enable the voice
            Voice.Muted = false;
            voiceEnabled = (Player.LocalPlayer.Voice3d = true);

            // Adjust the volume
            Player.LocalPlayer.AutoVolume = false;
            Player.LocalPlayer.VoiceVolume = 1.0f;
        }

        public static void ProcessVoiceStream()
        {
            // Get the player's position
            Player localPlayer = Player.LocalPlayer;
            Vector3 position = localPlayer.Position;

            foreach (Player target in Entities.Players.All)
            {
                if (target == localPlayer) continue;

                // Get the distance between players
                float distance = target.Position.DistanceTo(position);

                if (distance > Constants.MAX_VOICE_RANGE)
                {
                    // Disable the voice for the player
                    RemovePlayerFromListeners(target, true);
                    continue;
                }

                if (!listeningPlayers.Contains(target))
                {
                    // Add the target to the player's listeners
                    listeningPlayers.Add(target);

                    // Enable the voice for the target
                    Events.CallRemote("StartVoiceForPlayer", target.RemoteId);
                }
                
                // Set the volume based on the distance
                target.VoiceVolume = 1.0f - distance / voiceDistance;
            }
        }

        private static void RemovePlayerFromListeners(Player player, bool callServer)
        {
            listeningPlayers.Remove(player);

            if (callServer)
            {
                // Prevent the target from listening the player
                Events.CallRemote("StopVoiceForPlayer", player.RemoteId);
            }
        }
    }
}
