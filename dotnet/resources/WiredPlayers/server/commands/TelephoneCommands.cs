using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WiredPlayers.character;
using WiredPlayers.chat;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.factions;
using WiredPlayers.Utility;
using WiredPlayers.messages.arguments;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Server.Commands
{
    public static class TelephoneCommands
    {
        [Command]
        public static void AnswerCommand(Player player)
        {
            // Get the player's temporary model
            PlayerTemporaryModel temporaryModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (temporaryModel.Calling != null || temporaryModel.PhoneTalking != null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.already_phone_talking);
                return;
            }

            // Get the players calling
            List<Player> callingPlayers = NAPI.Pools.GetAllPlayers().FindAll(p => p.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).Calling != null);

            foreach (Player target in callingPlayers)
            {
                // Get the target's temporary model
                PlayerTemporaryModel targetTemporaryModel = target.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

                if (targetTemporaryModel.Calling is int)
                {
                    // Get the data for the player
                    CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

                    if ((int)targetTemporaryModel.Calling == (int)characterModel.Faction || (Faction.IsPoliceMember(player) && (int)targetTemporaryModel.Calling == (int)PlayerFactions.Police) || (int)targetTemporaryModel.Calling == (int)characterModel.Job + 100)
                    {
                        // Link both players in the same call
                        temporaryModel.PhoneTalking = target;
                        targetTemporaryModel.Calling = null;
                        targetTemporaryModel.PhoneTalking = player;

                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.call_received);
                        target.SendChatMessage(Constants.COLOR_INFO + InfoRes.call_taken);

                        return;
                    }
                }
                else if ((Player)targetTemporaryModel.Calling == player)
                {
                    // Check if the player has a phone on his hand
                    PhoneModel phone = Telephone.GetPlayerHoldingPhone(player);

                    if (phone == null)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_telephone_hand);
                        return;
                    }

                    // Link both players in the same call
                    temporaryModel.PhoneTalking = target;
                    targetTemporaryModel.Calling = null;
                    targetTemporaryModel.PhoneTalking = player;

                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.call_received);
                    target.SendChatMessage(Constants.COLOR_INFO + InfoRes.call_taken);

                    // Store call starting time
                    target.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).PhoneCallStarted = UtilityFunctions.GetTotalSeconds();

                    return;
                }
            }

            // Nobody's calling the player
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_called);
        }

        [Command]
        public static void HangCommand(Player player)
        {
            // Get the player's temporary model
            PlayerTemporaryModel temporaryModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (temporaryModel != null)
            {
                // Hang up the call
                temporaryModel = null;
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.finished_call);
            }
            else if (temporaryModel.PhoneTalking != null)
            {
                // Get the target's temporary model
                PlayerTemporaryModel targetTemporaryModel = temporaryModel.PhoneTalking.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

                if (temporaryModel.PhoneCallStarted > 0 || temporaryModel.PhoneCallStarted > 0)
                {
                    // Get the phones from the players
                    int playerPhone = Telephone.GetPlayerHoldingPhone(player).Number;
                    int targetPhone = Telephone.GetPlayerHoldingPhone(temporaryModel.PhoneTalking).Number;

                    // Get when the call started
                    int started = temporaryModel.PhoneCallStarted > 0 ? temporaryModel.PhoneCallStarted : targetTemporaryModel.PhoneCallStarted;
                    int elapsed = UtilityFunctions.GetTotalSeconds() - started;

                    // Update the elapsed time into the database
                    Task.Run(() => DatabaseOperations.AddPhoneLog(playerPhone, targetPhone, elapsed)).ConfigureAwait(false);
                }

                // Hang up the call for both players
                temporaryModel.PhoneTalking = null;
                temporaryModel.PhoneCallStarted = 0;
                targetTemporaryModel.PhoneTalking = null;
                targetTemporaryModel.PhoneCallStarted = 0;

                // Send the message to both players
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.finished_call);
                temporaryModel.PhoneTalking.SendChatMessage(Constants.COLOR_INFO + InfoRes.finished_call);
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_phone_talking);
            }
        }

        [Command]
        public static void SmsCommand(Player player, int number, string message)
        {
            // Get the player's phone in hand
            PhoneModel phone = Telephone.GetPlayerHoldingPhone(player);

            if (phone == null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_telephone_hand);
                return;
            }

            // Get the target player who has the phone
            Player target = Telephone.SearchPhoneOwnerByNumber(number);

            if (target == null || !target.Exists)
            {
                // The phone doesn't exist
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.phone_disconnected);
                return;
            }

            // Check if the player's in the contact list
            string contact = Telephone.GetContactInTelephone(number, phone.Number);

            if (contact.Length == 0)
            {
                contact = phone.Number.ToString();
            }

            // Send the SMS warning to the player
            target.SendChatMessage(Constants.COLOR_INFO + "[" + GenRes.sms_from + contact + "] " + message);

            // Send the message that the player is texting
            Chat.SendMessageToNearbyPlayers(player, string.Format(InfoRes.player_texting, player.Name), ChatTypes.Me, player.Dimension > 0 ? 7.5f : 20.0f);
            
            // Add the SMS into the database
            Task.Run(() => DatabaseOperations.AddPhoneLog(phone.Number, number, message)).ConfigureAwait(false);
        }

        [Command]
        public static void ContactsCommand(Player player, string action)
        {
            // Get the phone the player is holding
            PhoneModel phone = Telephone.GetPlayerHoldingPhone(player);

            if (phone == null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_telephone_hand);
                return;
            }

            if(action.Equals(ArgRes.number, StringComparison.InvariantCultureIgnoreCase))
            {
                // Send the phone number to the player
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.phone_number, phone.Number));
                return;
            }

            if(action.Equals(ArgRes.view, StringComparison.InvariantCultureIgnoreCase))
            {
                if (phone.Contacts.Count > 0)
                {
                    player.TriggerEvent("showPhoneContacts", NAPI.Util.ToJson(phone.Contacts), Actions.Load);
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.contact_list_empty);
                }

                return;
            }

            if (action.Equals(ArgRes.add, StringComparison.InvariantCultureIgnoreCase))
            {
                player.TriggerEvent("addContactWindow", Actions.Add);
                return;
            }

            if (action.Equals(ArgRes.modify, StringComparison.InvariantCultureIgnoreCase))
            {
                if (phone.Contacts.Count > 0)
                {
                    player.TriggerEvent("showPhoneContacts", NAPI.Util.ToJson(phone.Contacts), Actions.Rename);
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.contact_list_empty);
                }

                return;
            }

            if (action.Equals(ArgRes.remove, StringComparison.InvariantCultureIgnoreCase))
            {
                if (phone.Contacts.Count > 0)
                {
                    player.TriggerEvent("showPhoneContacts", NAPI.Util.ToJson(phone.Contacts), Actions.Delete);
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.contact_list_empty);
                }

                return;
            }

            if (action.Equals(ArgRes.sms, StringComparison.InvariantCultureIgnoreCase))
            {
                if (phone.Contacts.Count > 0)
                {
                    player.TriggerEvent("showPhoneContacts", NAPI.Util.ToJson(phone.Contacts), Actions.Sms);
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.contact_list_empty);
                }

                return;
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.contacts);
        }
    }
}
