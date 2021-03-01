using GTANetworkAPI;
using System;
using WiredPlayers.chat;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Utility;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.help;
using WiredPlayers.character;
using WiredPlayers.factions;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class ChatCommands
    {
        [Command]
        public static void SayCommand(Player player, string message)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                Chat.SendMessageToNearbyPlayers(player, message, ChatTypes.Talk, player.Dimension > 0 ? 7.5f : 10.0f);
            }
        }
        
        [Command]
        public static void YellCommand(Player player, string message)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                Chat.SendMessageToNearbyPlayers(player, message, ChatTypes.Yell, 45.0f);
            }
        }

        [Command]
        public static void WhisperCommand(Player player, string message)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                Chat.SendMessageToNearbyPlayers(player, message, ChatTypes.Whisper, 3.0f);
            }
        }

        [Command]
        public static void SilenceCommand(Player player)
        {
            player.TriggerEvent("TogglePlayerSilence");
        }

        [Command]
        public static void MeCommand(Player player, string message)
        {
            Chat.SendMessageToNearbyPlayers(player, message, ChatTypes.Me, player.Dimension > 0 ? 7.5f : 20.0f);
        }

        [Command]
        public static void DoCommand(Player player, string message)
        {
            Chat.SendMessageToNearbyPlayers(player, message, ChatTypes.Do, player.Dimension > 0 ? 7.5f : 20.0f);
        }

        [Command]
        public static void OocCommand(Player player, string message)
        {
            Chat.SendMessageToNearbyPlayers(player, message, ChatTypes.Ooc, player.Dimension > 0 ? 5.0f : 10.0f);
        }

        [Command]
        public static void LuckCommand(Player player)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
            }
            else
            {
                Random random = new Random();
                ChatTypes messageType = random.Next(0, 2) > 0 ? ChatTypes.Lucky : ChatTypes.Unlucky;
                Chat.SendMessageToNearbyPlayers(player, string.Empty, messageType, 20.0f);
            }
        }

        [Command]
        public static void AmeCommand(Player player, string message = "")
        {
            // Get the temporary data from the player
            PlayerTemporaryModel temporaryData = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (temporaryData.Ame == null || !temporaryData.Ame.Exists)
            {
                // Create a new textlabel with the data received
                temporaryData.Ame = NAPI.TextLabel.CreateTextLabel("*" + message + "*", new Vector3(), 50.0f, 0.5f, 4, new Color(201, 90, 0, 255));

                //ameLabel.AttachTo(player, "SKEL_Head", new Vector3(0.0f, 0.0f, 1.0f), new Vector3());
            }
            else
            {
                if (message.Length > 0)
                {
                    // We update label's text
                    temporaryData.Ame.Text = "*" + message + "*";
                }
                else
                {
                    // Deleting TextLabel
                    temporaryData.Ame.Delete();
                    temporaryData.Ame = null;
                }
            }
        }

        [Command]
        public static void MegaphoneCommand(Player player, string message)
        {
            if (!player.IsInVehicle)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle);
                return;
            }

            if (player.Vehicle.Class != (int)VehicleClass.Emergency)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_megaphone);
                return;
            }

            // Send the message with the megaphone
            Chat.SendMessageToNearbyPlayers(player, message, ChatTypes.Megaphone, 45.0f);
        }

        [Command]
        public static void PmCommand(Player player, string args)
        {
            // Get the arguments from the input string
            string[] arguments = args.Trim().Split(' ');
            
            if (arguments.Length < 2)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.pm);
                return;
            }
            
            // Get the target player            
            Player target = UtilityFunctions.GetPlayer(ref arguments);
            
            if (!Character.IsPlaying(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }
            
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank == StaffRank.None && target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank == StaffRank.None)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.mps_only_admin);
                return;
            }
            
            // Get the sent message
            string message = string.Join(' ', arguments);

            // Sending the message to both players
            player.SendChatMessage(Constants.COLOR_ADMIN_MP + string.Format("(({0}[ID:{1}] {2}: {3}))", GenRes.pm_to, target.Value, target.Name, message));
            target.SendChatMessage(Constants.COLOR_ADMIN_MP + string.Format("(({0}[ID:{1}] {2}: {3}))", GenRes.pm_from, player.Value, player.Name, message));
        }
    }
}
