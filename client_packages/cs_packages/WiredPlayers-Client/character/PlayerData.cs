using RAGE;
using WiredPlayers_Client.globals;
using System;

namespace WiredPlayers_Client.character
{
    class PlayerData : Events.Script
    {
        private static ushort playerId;

        public PlayerData()
        {
            Events.Add("showPlayerData", ShowPlayerDataEvent);
            Events.Add("showPropertiesData", ShowPropertiesDataEvent);
            Events.Add("showVehiclesData", ShowVehiclesDataEvent);
            Events.Add("showExtendedData", ShowExtendedDataEvent);
            Events.Add("retrievePanelData", RetrievePanelDataEvent);
            Events.Add("closePlayerData", ClosePlayerDataEvent);

            // Initialize the player
            playerId = Constants.INVALID_VALUE;
        }

        private void ShowPlayerDataEvent(object[] args)
        {
            // Get the data from the input
            string name = args[1].ToString();
            string age = args[2].ToString();
            string sex = args[3].ToString();
            string money = args[4].ToString();
            string bank = args[5].ToString();
            string job = args[6].ToString();
            string rank = args[7].ToString();

            if (playerId == Constants.INVALID_VALUE)
            {
                // Get the player identifier
                playerId = Convert.ToUInt16(args[0]);
            }

            if (Browser.CustomBrowser == null)
            {
                // Create the window with the basic data
                Browser.CreateBrowser("playerData.html", "destroyBrowser", "initializePlayerData", name, age, sex, money, bank, job, rank, args[8].ToString());
            }
            else
            {
                // Update the window
                Browser.ExecuteFunction("populateBasicData", name, age, sex, money, bank, job, rank);
            }
        }

        private void ShowPropertiesDataEvent(object[] args)
        {
            // Update the window
            Browser.ExecuteFunction("populatePropertiesData", args[0].ToString(), args[1].ToString());
        }

        private void ShowVehiclesDataEvent(object[] args)
        {
            // Update the window
            Browser.ExecuteFunction("populateVehiclesData", args[0].ToString(), args[1].ToString());
        }

        private void ShowExtendedDataEvent(object[] args)
        {
            // Update the window
            Browser.ExecuteFunction("populateExtendedData", args[0].ToString());
        }

        private void RetrievePanelDataEvent(object[] args)
        {
            // Call the event from the parameters
            Events.CallRemote(args[0].ToString(), playerId);
        }

        private void ClosePlayerDataEvent(object[] args)
        {
            // Clear the player
            playerId = Constants.INVALID_VALUE;

            // Destroy the browser
            Browser.DestroyBrowserEvent(null);
        }
    }
}
