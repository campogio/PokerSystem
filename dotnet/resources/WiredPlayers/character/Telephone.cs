using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.character;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.factions;
using WiredPlayers.messages.general;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.character
{
    public class Telephone : Script
    {
        public static Dictionary<int, PhoneModel> phoneList;

        public static void AddPhoneContacts()
        {
            // Load the contact list
            Dictionary<int, ContactModel> contactList = DatabaseOperations.LoadAllContacts();

            foreach (PhoneModel phone in phoneList.Values)
            {
                // Get all the contacts from the phone
                phone.Contacts = contactList.Where(c => c.Value.Owner == phone.Number).ToDictionary(i => i.Key, i => i.Value);
            }
        }

        public static PhoneModel GetPlayerHoldingPhone(Player player)
        {
            // Get the item on the player's right hand
            string rightHand = player.GetSharedData<string>(EntityData.PlayerRightHand);

            if (rightHand != null)
            {
                // Check if the player has a phone on his hand
                int itemId = NAPI.Util.FromJson<AttachmentModel>(rightHand).itemId;
                return phoneList.Values.FirstOrDefault(p => p.ItemId == itemId);
            }

            return null;
        }

        public static Player SearchPhoneOwnerById(int phoneId)
        {
            ItemModel item = Inventory.ItemCollection.Values.FirstOrDefault(i => i.id == phoneId && (i.ownerEntity == Constants.ITEM_ENTITY_PLAYER || i.ownerEntity == Constants.ITEM_ENTITY_RIGHT_HAND));

            if (item != null)
            {
                // Get the player with the selected identifier
                return NAPI.Pools.GetAllPlayers().FirstOrDefault(p => Character.IsPlaying(p) && p.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id == item.ownerIdentifier);
            }

            return null;
        }

        public static Player SearchPhoneOwnerByNumber(int number)
        {
            PhoneModel phone = phoneList.ContainsKey(number) ? phoneList[number] : null;

            if (phone != null)
            {
                ItemModel item = Inventory.ItemCollection.ContainsKey(phone.ItemId) ? Inventory.ItemCollection[phone.ItemId] : null;

                if (item != null && (item.ownerEntity == Constants.ITEM_ENTITY_PLAYER || item.ownerEntity == Constants.ITEM_ENTITY_RIGHT_HAND))
                {
                    // Get the player with the selected identifier
                    return NAPI.Pools.GetAllPlayers().FirstOrDefault(p => Character.IsPlaying(p) && p.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id == item.ownerIdentifier);
                }
            }

            return null;
        }

        public static void OnPlayerDisconnected(Player player)
        {
            // Get the player's temporary model
            PlayerTemporaryModel temporaryModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (temporaryModel.PhoneTalking == null || !temporaryModel.PhoneTalking.Exists) return;

            // Hang up the call
            PlayerTemporaryModel targetTemporaryModel = temporaryModel.PhoneTalking.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);
            targetTemporaryModel.PhoneTalking = null;
            targetTemporaryModel.PhoneCallStarted = 0;

            // Send the confirmation message
            temporaryModel.PhoneTalking.SendChatMessage(Constants.COLOR_INFO + InfoRes.finished_call);
        }

        private ContactModel GetContactFromId(int number, int contactId)
        {
            // Get the phone corresponding to the number
            PhoneModel phone = phoneList.ContainsKey(number) ? phoneList[number] : null;

            if (phone == null) return null;

            // Get the contact matching the selected identifier
            return phone.Contacts.ContainsKey(contactId) ? phone.Contacts[contactId] : null;
        }

        public static int GetNumberFromContactName(string contactName, int playerPhone)
        {
            // Get the phone corresponding to the number
            PhoneModel phone = phoneList.ContainsKey(playerPhone) ? phoneList[playerPhone] : null;

            if (phone != null)
            {
                // Get the contact matching the name
                ContactModel contactModel = phone.Contacts.Values.FirstOrDefault(c => c.ContactName == contactName);

                return contactModel?.ContactNumber ?? 0;
            }

            return 0;
        }

        public static string GetContactInTelephone(int phone, int number)
        {
            // Get the phone corresponding to the number
            PhoneModel phoneModel = phoneList.ContainsKey(phone) ? phoneList[phone] : null;

            if (phoneModel != null)
            {
                // Get the contact matching the name
                return phoneModel.Contacts.ContainsKey(number) ? phoneModel.Contacts[number].ContactName : string.Empty;
            }

            return string.Empty;
        }

        [RemoteEvent("addNewContact")]
        public async void AddNewContactRemoteEvent(Player player, int contactNumber, string contactName)
        {
            // Get the current player's phone
            PhoneModel phone = GetPlayerHoldingPhone(player);

            // Create the model for the new contact
            ContactModel contact = new ContactModel
            {
                Owner = phone.Number,
                ContactNumber = contactNumber,
                ContactName = contactName
            };

            // Add contact to database
            contact.Id = await DatabaseOperations.AddNewContact(contact).ConfigureAwait(false);
            phone.Contacts.Add(contact.Id, contact);

            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.contact_created, contactName, contactNumber));
        }

        [RemoteEvent("modifyContact")]
        public void ModifyContactEvent(Player player, int contactIndex, int contactNumber, string contactName)
        {
            // Get the player's phone
            PhoneModel phone = GetPlayerHoldingPhone(player);

            // Modify contact data
            ContactModel contact = GetContactFromId(phone.Number, contactIndex);
            contact.ContactNumber = contactNumber;
            contact.ContactName = contactName;

            // Modify the contact's data
            Task.Run(() => DatabaseOperations.ModifyContact(contact)).ConfigureAwait(false);

            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.contact_modified);
        }

        [RemoteEvent("deleteContact")]
        public void DeleteContactEvent(Player player, int contactIndex)
        {
            // Get the player's phone
            PhoneModel phone = GetPlayerHoldingPhone(player);

            ContactModel contact = GetContactFromId(phone.Number, contactIndex);
            string contactName = contact.ContactName;
            int contactNumber = contact.ContactNumber;

            // Delete the contact
            Task.Run(() => DatabaseOperations.DeleteSingleRow("contacts", "id", contactIndex)).ConfigureAwait(false);
            phone.Contacts.Remove(contact.Id);

            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.contact_deleted, contactName, contactNumber));
        }

        [RemoteEvent("sendPhoneMessage")]
        public void SendPhoneMessageEvent(Player player, int contactIndex, string textMessage)
        {
            // Get the player's phone
            PhoneModel phoneModel = GetPlayerHoldingPhone(player);

            ContactModel contact = GetContactFromId(phoneModel.Number, contactIndex);

            // Get the phone owner
            Player target = SearchPhoneOwnerByNumber(contact.ContactNumber);

            if (target == null || !target.Exists)
            {
                // There's no player matching the contact
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.phone_disconnected);
                return;
            }

            string contactName = GetContactInTelephone(phoneModel.Number, contact.ContactNumber);

            if (contactName.Length == 0)
            {
                contactName = contact.ContactNumber.ToString();
            }

            // Send the message to the target
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.sms_sent);
            target.SendChatMessage(Constants.COLOR_INFO + "[" + GenRes.sms_from + contactName + "] " + textMessage);

            // Add the SMS to the database
            Task.Run(() => DatabaseOperations.AddPhoneLog(phoneModel.Number, contact.ContactNumber, textMessage)).ConfigureAwait(false);
        }
        
        [RemoteEvent("CallPhoneNumber")]
        public void CallPhoneNumberRemoteEvent(Player player, string called)
        {
            // Get the player's phone on the hand
            PhoneModel phone = GetPlayerHoldingPhone(player);

            // Get the player's temporary model
            PlayerTemporaryModel temporaryModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (int.TryParse(called, out int number))
            {
                string playerMessage = string.Empty;
                Player[] connectedPlayers = null;

                switch (number)
                {
                    case (int)ServiceNumbers.Police:
                        // Get all the police members online
                        connectedPlayers = NAPI.Pools.GetAllPlayers().Where(t => Faction.IsPoliceMember(t)).ToArray();
                        playerMessage = string.Format(InfoRes.calling, ServiceNumbers.Police);
                        temporaryModel.Calling = PlayerFactions.Police;
                        break;
                    case (int)ServiceNumbers.Emergency:
                        // Get all the emergency members online
                        connectedPlayers = NAPI.Pools.GetAllPlayers().Where(t => Character.IsPlaying(t) && t.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty && t.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction == PlayerFactions.Emergency).ToArray();
                        playerMessage = string.Format(InfoRes.calling, ServiceNumbers.Emergency);
                        temporaryModel.Calling = PlayerFactions.Emergency;
                        break;
                    case (int)ServiceNumbers.News:
                        // Get all the news members online
                        connectedPlayers = NAPI.Pools.GetAllPlayers().Where(t => Character.IsPlaying(t) && t.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction == PlayerFactions.News).ToArray();
                        playerMessage = string.Format(InfoRes.calling, ServiceNumbers.News);
                        temporaryModel.Calling = PlayerFactions.News;
                        break;
                    case (int)ServiceNumbers.Taxi:
                        // Get all the taxi members online
                        connectedPlayers = NAPI.Pools.GetAllPlayers().Where(t => Character.IsPlaying(t) && t.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction == PlayerFactions.Taxi).ToArray();
                        playerMessage = string.Format(InfoRes.calling, ServiceNumbers.Taxi);
                        temporaryModel.Calling = PlayerFactions.Taxi;
                        break;
                    case (int)ServiceNumbers.Fastfood:
                        // Get all the fastfood deliverers online
                        connectedPlayers = NAPI.Pools.GetAllPlayers().Where(t => Character.IsPlaying(t) && t.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty && t.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Job == PlayerJobs.Fastfood).ToArray();
                        playerMessage = string.Format(InfoRes.calling, ServiceNumbers.Fastfood);
                        temporaryModel.Calling = (int)PlayerJobs.Fastfood + 100;
                        break;
                    case (int)ServiceNumbers.Mechanic:
                        // Get all the mechanics online
                        connectedPlayers = NAPI.Pools.GetAllPlayers().Where(t => Character.IsPlaying(t) && t.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty && t.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Job == PlayerJobs.Mechanic).ToArray();
                        playerMessage = string.Format(InfoRes.calling, ServiceNumbers.Mechanic);
                        temporaryModel.Calling = (int)PlayerJobs.Mechanic + 100;
                        break;
                    default:
                        // Get the target with the selected phone number
                        Player target = SearchPhoneOwnerByNumber(number);

                        if (target == null || !target.Exists)
                        {
                            // The phone number doesn't exist
                            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.phone_disconnected);
                            return;
                        }

                        // Check if the player has a contact name
                        string contact = GetContactInTelephone(number, phone.Number);

                        if (contact.Length == 0)
                        {
                            contact = phone.Number.ToString();
                        }

                        // Make the player call
                        temporaryModel.Calling = target;

                        // Check if the player calling is a contact into target's contact list
                        target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.call_from, contact.Length > 0 ? contact : contact));

                        return;
                }

                if (connectedPlayers.Length == 0)
                {
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.line_occupied);
                    temporaryModel.Calling = null;
                    return;
                }
                
                // Send the message to each of the players
                foreach (Player target in connectedPlayers) target.SendChatMessage(Constants.COLOR_INFO + InfoRes.central_call);

                // Tell the player he's calling the number
                player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
            }
            else
            {
                // Get the contacts phone number
                int targetPhone = GetNumberFromContactName(called, phone.Number);

                // Get the player with the phone
                Player target = SearchPhoneOwnerByNumber(targetPhone);

                // Get the target's temporary model
                PlayerTemporaryModel targetTemporaryModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

                if (target == null || !target.Exists || targetTemporaryModel.Calling != null || targetTemporaryModel.PhoneTalking != null)
                {
                    // The contact player isn't online
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.phone_disconnected);
                    return;
                }

                // Store the call
                temporaryModel.Calling = target;

                // Check if the player is in target's contact list
                string contact = GetContactInTelephone(targetPhone, phone.Number);

                // Send the messages to both players
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.calling, called));
                target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.call_from, contact.Length > 0 ? contact : phone.Number.ToString()));
            }
        }

        [RemoteEvent("LoadTextMessages")]
        public void LoadTextMessagesRemoteEvent(Player player, string contact)
        {
            // Get the phone given the contact
            ContactModel contactModel = GetPlayerHoldingPhone(player).Contacts.Values.FirstOrDefault(c => c.ContactName == contact);
            int number = contactModel == null ? int.Parse(contact) : contactModel.ContactNumber;
        }
    }
}
