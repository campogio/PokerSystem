using RAGE;
using RAGE.Elements;
using WiredPlayers_Client.globals;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace WiredPlayers_Client.factions
{
    class Police : Events.Script
    {
        public static bool handcuffed;
        private int controlAction;
        private string crimesJson = null;
        private string crimesList = null;
        private string selectedControl = null;
        private Dictionary<int, Blip> reinforces = null;

        public Police()
        {
            Events.Add("showCrimesMenu", ShowCrimesMenuEvent);
            Events.Add("applyCrimes", ApplyCrimesEvent);
            Events.Add("executePlayerCrimes", ExecutePlayerCrimesEvent);
            Events.Add("backCrimesMenu", BackCrimesMenuEvent);
            Events.Add("loadPoliceControlList", LoadPoliceControlListEvent);
            Events.Add("proccessPoliceControlAction", ProccessPoliceControlActionEvent);
            Events.Add("policeControlSelectedName", PoliceControlSelectedNameEvent);
            Events.Add("updatePoliceReinforces", UpdatePoliceReinforcesEvent);
            Events.Add("reinforcesRemove", ReinforcesRemoveEvent);

            Events.AddDataHandler("PLAYER_HANDCUFFED", PlayerHandcuffedStateChanged);

            Events.OnPlayerStartEnterVehicle += OnPlayerStartEnterVehicle;

            // Initialize the reinforces
            reinforces = new Dictionary<int, Blip>();
        }

        private void ShowCrimesMenuEvent(object[] args)
        {
            // Save crimes list
            crimesJson = args[0].ToString();

            // Show crimes menu
            Browser.CreateBrowser("sideMenu.html", "destroyBrowser", "populateCrimesMenu", crimesJson, string.Empty);
        }

        private void ApplyCrimesEvent(object[] args)
        {
            // Store crimes to be applied
            crimesList = args[0].ToString();

            // Destroy crimes menu
            Browser.DestroyBrowserEvent(null);

            // Show the confirmation window
            Browser.CreateBrowser("crimesConfirm.html", "backCrimesMenu", "populateCrimesConfirmMenu", crimesList);
        }

        private void ExecutePlayerCrimesEvent(object[] args)
        {
            // Destroy the confirmation menu
            Browser.DestroyBrowserEvent(null);

            // Apply crimes to the player
            Events.CallRemote("applyCrimesToPlayer", crimesList);
        }

        private void BackCrimesMenuEvent(object[] args)
        {
            // Destroy the confirmation menu
            Browser.DestroyBrowserEvent(null);

            // Show crimes menu
            Browser.CreateBrowser("sideMenu.html", "destroyBrowser", "populateCrimesMenu", crimesJson, crimesList);
        }

        private void LoadPoliceControlListEvent(object[] args)
        {
            // Get the action taken
            controlAction = Convert.ToInt32(args[0]);

            // Show the menu with the police control list
            Browser.CreateBrowser("sideMenu.html", "destroyBrowser", "populatePoliceControlMenu", args[0].ToString());
        }

        private void ProccessPoliceControlActionEvent(object[] args)
        {
            // Get the variables from the arguments
            string control = args[0] == null ? string.Empty : args[0].ToString();

            // Check the selected option
            int controlOption = (int)Player.LocalPlayer.GetSharedData("PLAYER_POLICE_CONTROL");

            switch (controlOption)
            {
                case 1:
                    if (control.Length == 0)
                    {
                        // Save the police control with a new name
                        Browser.CreateBrowser("policeControlName.html", null);
                    }
                    else
                    {
                        // Override the existing police control
                        Events.CallRemote("policeControlSelected", controlAction, control);
                    }
                    break;
                case 2:
                    // Show the window to change control's name
                    Browser.CreateBrowser("policeControlName.html", null);
                    selectedControl = control;
                    break;
                default:
                    // Execute the option over the police control
                    Events.CallRemote("policeControlSelected", controlAction, control);
                    break;
            }
        }

        private void PoliceControlSelectedNameEvent(object[] args)
        {
            // Save the police control with a new name
            Events.CallRemote("updatePoliceControlName", controlAction, selectedControl, args[0].ToString());
        }

        private void UpdatePoliceReinforcesEvent(object[] args)
        {
            Dictionary<int, Vector3> updatedReinforces = JsonConvert.DeserializeObject<Dictionary<int, Vector3>>(args[0].ToString());

            // Search for policemen asking for reinforces
            foreach (KeyValuePair<int, Vector3> entry in updatedReinforces)
            {
                if(reinforces.ContainsKey(entry.Key))
                {
                    // Update the blip's position
                    reinforces[entry.Key].SetCoords(entry.Value.X, entry.Value.Y, entry.Value.Z);
                }
                else
                {
                    // Create a blip on the map
                    Blip reinforcesBlip = new Blip(487, entry.Value, string.Empty, 1, 38);

                    // Add the new member to the array
                    reinforces[entry.Key] = reinforcesBlip;
                }
            }
        }

        private void ReinforcesRemoveEvent(object[] args)
        {
            // Get the variables from the arguments
            int officer = Convert.ToInt32(args[0]);

            // Delete officer's reinforces
            reinforces[officer].Destroy();
            reinforces.Remove(officer);
        }

        private void PlayerHandcuffedStateChanged(Entity entity, object arg, object oldArg)
        {
            if(entity != Player.LocalPlayer) return;
            
            // Toggle the handcuffed state
            handcuffed = arg != null;
        }

        private void OnPlayerStartEnterVehicle(Vehicle vehicle, int seatId, Events.CancelEventArgs cancel)
        {
            if (handcuffed && seatId == Constants.VEHICLE_SEAT_DRIVER)
            {
                // Prevent the player from driving the vehicle
                cancel.Cancel = true;
            }
        }
    }
}
