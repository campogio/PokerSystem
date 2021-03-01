using RAGE;
using RAGE.Elements;
using Newtonsoft.Json;
using WiredPlayers_Client.model;
using WiredPlayers_Client.globals;
using System.Collections.Generic;
using System;

namespace WiredPlayers_Client.character
{
    class Telephone : Events.Script
    {
        public static bool phoneActive;

        private int action;
        private int contact;
        private List<Contact> contactsList;

        public Telephone()
        {
            Events.Add("showPhoneContacts", ShowPhoneContactsEvent);
            Events.Add("addContactWindow", AddContactWindowEvent);
            Events.Add("preloadContactData", PreloadContactDataEvent);
            Events.Add("setContactData", SetContactDataEvent);
            Events.Add("executePhoneAction", ExecutePhoneActionEvent);
            Events.Add("sendPhoneMessage", SendPhoneMessageEvent);
            Events.Add("cancelPhoneMessage", CancelPhoneMessageEvent);
            
            Events.Add("CallPhoneNumber", CallPhoneNumberEvent);
            Events.Add("LoadConversation", LoadConversationEvent);
            Events.Add("HidePhone", HidePhoneEvent);
        }

        public static bool IsValidPhone(string itemHash)
        {
            // Check if the hash is a phone
            return itemHash == "xm_prop_x17_phone_01";            
        }

        public static void ShowPhoneHome()
        {
            // Get the current server date and hour
            int year = 0, month = 0, day = 0, hour = 0, minutes = 0, seconds = 0;
            RAGE.Game.Clock.GetLocalTime(ref year, ref month, ref day, ref hour, ref minutes, ref seconds);

            // Convert the date and time
            string time = string.Format("{0}:{1}", hour, minutes);
            string date = string.Format("{0}-{1}-{2}", year, month, day);

            // Mark the phone as active
            phoneActive = true;

            // Load the player phone
            Browser.CreateBrowser("phone.html", null, "updateTimeDate", time, date);
        }

        private void HidePhoneEvent(object[] args)
        {
            // Destroy the browser
            Browser.DestroyBrowserEvent(null);

            // Mark the phone as inactive
            phoneActive = false;
        }

        private void ShowPhoneContactsEvent(object[] args)
        {
            // Get the variables from the arguments
            string contactsJson = args[0].ToString();

            // Store the values
            action = Convert.ToInt32(args[1]);
            contactsList = JsonConvert.DeserializeObject<List<Contact>>(contactsJson);

            // Show the list
            Browser.CreateBrowser("sideMenu.html", null, "populateContactsMenu", contactsJson, action);
        }

        private void AddContactWindowEvent(object[] args)
        {
            // Store the action
            action = Convert.ToInt32(args[0]);

            // Show the menu to add a contact
            Browser.CreateBrowser("addPhoneContact.html", null);
        }

        private void PreloadContactDataEvent(object[] args)
        {
            if (contact > 0)
            {
                // Load contact's data
                int number = contactsList[contact].contactNumber;
                string name = contactsList[contact].contactName;

                // Show the data on the browser
                Browser.ExecuteFunction("populateContactData", number, name);
            }
        }

        private void SetContactDataEvent(object[] args)
        {
            // Get the variables from the arguments
            int number = Convert.ToInt32(args[0]);
            string name = args[1].ToString();

            // Destroy the web browser
            Browser.DestroyBrowserEvent(null);

            if (action == 4)
            {
                // Create new contact
                Events.CallRemote("addNewContact", number, name);
            }
            else
            {
                // Modify the contact data
                Events.CallRemote("modifyContact", contact, number, name);
            }
        }

        private void ExecutePhoneActionEvent(object[] args)
        {
            // Get the variables from the arguments
            int contactIndex = Convert.ToInt32(args[0]);

            // Get the selected contact
            contact = contactsList[contactIndex].id;

            // Destroy the web browser
            Browser.DestroyBrowserEvent(null);

            switch (action)
            {
                case 2:
                    // Load contact's data
                    int number = contactsList[contactIndex].contactNumber;
                    string name = contactsList[contactIndex].contactName;

                    // Modify a contact
                    Browser.CreateBrowser("addPhoneContact.html", null, "populateContactData", number, name);

                    break;
                case 3:
                    // Delete a contact
                    Events.CallRemote("deleteContact", contact);

                    break;
                case 5:
                    // Send SMS to a contact
                    Browser.CreateBrowser("sendContactMessage.html", null);

                    break;
            }
        }

        private void SendPhoneMessageEvent(object[] args)
        {
            // Destroy the web browser
            Browser.DestroyBrowserEvent(null);

            // Send the SMS to the target
            Events.CallRemote("sendPhoneMessage", contact, args[0].ToString());
        }

        private void CancelPhoneMessageEvent(object[] args)
        {
            // Destroy the web browser
            Browser.DestroyBrowserEvent(null);

            // Show the list
            Browser.CreateBrowser("sideMenu.html", null, "populateContactsMenu", JsonConvert.SerializeObject(contactsList), action);
        }

        private void CallPhoneNumberEvent(object[] args)
        {
            // Get the number
            int phoneNumber = Convert.ToInt32(args[0]);
            
            // Check if the number exists
            Events.CallRemote("CallPhoneNumber", phoneNumber);
        }

        private void LoadConversationEvent(object[] args)
        {
            // Load the text messages with the contact
            Events.CallRemote("LoadTextMessages", args[0].ToString());
        }
    }
}
