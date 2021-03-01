using GTANetworkAPI;
using System;
using System.Linq;
using WiredPlayers.character;
using WiredPlayers.Data.Temporary;
using WiredPlayers.factions;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.success;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.chat
{
    public class Chat : Script
    {
        public static bool VoiceChatEnabled;

        public static void OnPlayerDisconnected(Player player)
        {
            // Get the player's attached label
            TextLabel playerTextLabel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).Ame;

            if (playerTextLabel != null && playerTextLabel.Exists)
            {
                // Delete the text label
                playerTextLabel.Delete();
            }
        }

        public static void SendMessageToNearbyPlayers(Player player, string message, ChatTypes type, float range, bool excludePlayer = false)
        {
            // Calculate the different gaps for the chat
            float distanceGap = range / Constants.CHAT_RANGES;

            // Get the list of the connected players
            Player[] targetList = NAPI.Pools.GetAllPlayers().Where(p => Character.IsPlaying(p) && p.Dimension == player.Dimension).ToArray();

            foreach (Player target in targetList)
            {
                if (player != target || (player == target && !excludePlayer))
                {
                    float distance = player.Position.DistanceTo(target.Position);

                    if (distance <= range)
                    {
                        // Getting message color
                        string chatMessageColor = GetChatMessageColor(distance, distanceGap, false);
                        string oocMessageColor = GetChatMessageColor(distance, distanceGap, true);

                        switch (type)
                        {
                            // We send the message
                            case ChatTypes.Talk:
                                target.SendChatMessage(chatMessageColor + player.Name + GenRes.chat_say + message);
                                break;

                            case ChatTypes.Yell:
                                target.SendChatMessage(chatMessageColor + player.Name + GenRes.chat_yell + message + "!");
                                break;

                            case ChatTypes.Whisper:
                                target.SendChatMessage(chatMessageColor + player.Name + GenRes.chat_whisper + message);
                                break;

                            case ChatTypes.Phone:
                                target.SendChatMessage(chatMessageColor + player.Name + GenRes.chat_phone + message);
                                break;

                            case ChatTypes.Radio:
                                target.SendChatMessage(chatMessageColor + player.Name + GenRes.chat_radio + message);
                                break;

                            case ChatTypes.Me:
                                target.SendChatMessage(Constants.COLOR_CHAT_ME + player.Name + " " + message);
                                break;

                            case ChatTypes.Do:
                                target.SendChatMessage(Constants.COLOR_CHAT_DO + message + "(( " + player.Name + " [ID: " + player.Value + "] ))");
                                break;

                            case ChatTypes.Ooc:
                                target.SendChatMessage(oocMessageColor + "(([ID: " + player.Value + "] " + player.Name + ": " + message + "))");
                                break;

                            case ChatTypes.Disconnect:
                                target.SendChatMessage(Constants.COLOR_HELP + "[ID: " + player.Value + "] " + player.Name + ": " + message);
                                break;

                            case ChatTypes.Megaphone:
                                target.SendChatMessage(Constants.COLOR_INFO + "[Megafono di " + player.Name + "]: " + message);
                                break;

                            case ChatTypes.Lucky:
                                target.SendChatMessage(Constants.COLOR_SU_POSITIVE + string.Format(SuccRes.possitive_result, player.Name));
                                break;

                            case ChatTypes.Unlucky:
                                target.SendChatMessage(Constants.COLOR_ERROR + string.Format(ErrRes.negative_result, player.Name));
                                break;
                        }
                    }
                }
            }
        }

        private static string GetChatMessageColor(float distance, float distanceGap, bool ooc)
        {
            string color;
            if (distance < distanceGap)
            {
                color = ooc ? Constants.COLOR_OOC_CLOSE : Constants.COLOR_CHAT_CLOSE;
            }
            else if (distance < distanceGap * 2)
            {
                color = ooc ? Constants.COLOR_OOC_NEAR : Constants.COLOR_CHAT_NEAR;
            }
            else if (distance < distanceGap * 3)
            {
                color = ooc ? Constants.COLOR_OOC_MEDIUM : Constants.COLOR_CHAT_MEDIUM;
            }
            else if (distance < distanceGap * 4)
            {
                color = ooc ? Constants.COLOR_OOC_FAR : Constants.COLOR_CHAT_FAR;
            }
            else
            {
                color = ooc ? Constants.COLOR_OOC_LIMIT : Constants.COLOR_CHAT_LIMIT;
            }
            return color;
        }

        [ServerEvent(Event.ChatMessage)]
        public void OnChatMessage(Player player, string message)
        {
            if (!Character.IsPlaying(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_cant_chat);
                return;
            }

            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get the player's external data
            PlayerTemporaryModel playerModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (playerModel.OnAir)
            {
                WeazelNews.SendNewsMessage(player, message);
            }
            else if (playerModel.PhoneTalking != null)
            {
                // We send the message to the player and target
                player.SendChatMessage(Constants.COLOR_CHAT_PHONE + GenRes.phone + player.Name + GenRes.chat_say + message);
                playerModel.PhoneTalking.SendChatMessage(Constants.COLOR_CHAT_PHONE + GenRes.phone + player.Name + GenRes.chat_say + message);

                // We send the message to nearby players
                SendMessageToNearbyPlayers(player, message, ChatTypes.Phone, player.Dimension > 0 ? 7.5f : 10.0f, true);
            }
            else
            {
                // We send the message to nearby players
                SendMessageToNearbyPlayers(player, message, ChatTypes.Talk, player.Dimension > 0 ? 7.5f : 10.0f);
            }

            // Log the message on the server
            string timeString = "[" + DateTime.Now.ToString("HH:mm:ss") + "." + DateTime.Now.Millisecond + "] ";
            NAPI.Util.ConsoleOutput(timeString + "[ID:" + player.Value + "] " + player.Name + GenRes.chat_say + message);
        }

        [RemoteEvent("playerNotLoggedCommand")]
        public void PlayerNotLoggedCommandEvent(Player player)
        {
            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_cant_command);
        }

        [RemoteEvent("logPlayerCommand")]
        public void LogPlayerCommandEvent(Player player, string command)
        {
            // Get the server time
            string timeString = "[" + DateTime.Now.ToString("HH:mm:ss") + "." + DateTime.Now.Millisecond + "] ";

            // Log the command used
            NAPI.Util.ConsoleOutput(timeString + string.Format(GenRes.command_used, player.Value, player.Name, command));
        }

        [RemoteEvent("StartVoiceForPlayer")]
        public void StartVoiceForPlayerEvent(Player player, int handle)
        {
            // Get the player given the handle
            Player target = NAPI.Pools.GetAllPlayers().FirstOrDefault(p => p.Value == handle);

            if (target != null && target.Exists)
            {
                // Enable the voice to the player
                player.EnableVoiceTo(target);
            }
        }

        [RemoteEvent("StopVoiceForPlayer")]
        public void StopVoiceForPlayerEvent(Player player, int handle)
        {
            // Get the player given the handle
            Player target = NAPI.Pools.GetAllPlayers().FirstOrDefault(p => p.Value == handle);

            if (target != null && target.Exists)
            {
                // Enable the voice to the player
                player.DisableVoiceTo(target);
            }
        }
    }
}