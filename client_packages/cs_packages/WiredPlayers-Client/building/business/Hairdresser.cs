using RAGE;
using RAGE.Elements;
using System.Collections.Generic;
using WiredPlayers_Client.globals;
using WiredPlayers_Client.model;
using Newtonsoft.Json;
using System;

using static WiredPlayers.Client.Utility.Enumerators;

namespace WiredPlayers_Client.Building.Business
{
    class Hairdresser : Events.Script
    {
        private int customCamera;
        private List<int> facialHair = null;
        private FacialHair initialHair = null;

        public Hairdresser()
        {
            Events.Add("showHairdresserMenu", ShowHairdresserMenuEvent);
            Events.Add("updateFacialHair", UpdateFacialHairEvent);
            Events.Add("applyHairdresserChanges", ApplyHairdresserChangesEvent);
            Events.Add("cancelHairdresserChanges", CancelHairdresserChangesEvent);
        }

        private void ShowHairdresserMenuEvent(object[] args)
        {
            // Get the variables from the arguments
            int sex = Convert.ToInt32(args[0]);
            string skinJson = args[1].ToString();
            string businessName = args[2].ToString();

            // Add the options
            string faceOption = JsonConvert.SerializeObject(sex == (int)PlayerSex.Male ? Constants.MaleFaceOptions : Constants.FemaleFaceOptions);

            // Initialize the face values
            initialHair = JsonConvert.DeserializeObject<FacialHair>(skinJson);

            facialHair = new List<int>()
            {
                initialHair.hairModel,
                initialHair.firstHairColor,
                initialHair.secondHairColor,
                initialHair.eyebrowsModel,
                initialHair.eyebrowsColor,
                initialHair.beardModel,
                initialHair.beardColor
            };

            // Create a custom camera
            float forwardX = Player.LocalPlayer.Position.X + (Player.LocalPlayer.GetForwardX() * 1.5f);
            float forwardY = Player.LocalPlayer.Position.Y + (Player.LocalPlayer.GetForwardY() * 1.5f);
            customCamera = RAGE.Game.Cam.CreateCamera(RAGE.Game.Misc.GetHashKey("DEFAULT_SCRIPTED_CAMERA"), true);
            RAGE.Game.Cam.SetCamCoord(customCamera, forwardX, forwardY, Player.LocalPlayer.Position.Z + 0.5f);
            RAGE.Game.Cam.PointCamAtCoord(customCamera, Player.LocalPlayer.Position.X, Player.LocalPlayer.Position.Y, Player.LocalPlayer.Position.Z);

            // Enable the camera
            RAGE.Game.Cam.SetCamActive(customCamera, true);
            RAGE.Game.Cam.RenderScriptCams(true, false, 0, true, false, 0);

            // Create hairdressers' menu
            Browser.CreateBrowser("sideMenu.html", "cancelHairdresserChanges", "populateHairdresserMenu", faceOption, JsonConvert.SerializeObject(facialHair), businessName);
        }

        private void UpdateFacialHairEvent(object[] args)
        {
            // Get the variables from the arguments
            int slot = Convert.ToInt32(args[0]);
            int value = Convert.ToInt32(args[1]);

            // Save the new value
            facialHair[slot] = value;

            // Check if the beard is out of range
            int eyebrowsModel = facialHair[3] < 0 || facialHair[3] > 255 ? 255 : facialHair[3];
            int beardModel = facialHair[5] < 0 || facialHair[5] > 255 ? 255 : facialHair[5];

            // Update the player's head
            Player.LocalPlayer.SetComponentVariation(2, facialHair[0], 0, 0);
            Player.LocalPlayer.SetHairColor(facialHair[1], facialHair[2]);
            Player.LocalPlayer.SetHeadOverlay(1, beardModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlay(2, eyebrowsModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlayColor(1, 1, facialHair[6], 0);
            Player.LocalPlayer.SetHeadOverlayColor(2, 1, facialHair[4], 0);
        }

        private void ApplyHairdresserChangesEvent(object[] args)
        {
            FacialHair generatedFace = new FacialHair()
            {

                hairModel = facialHair[0],
                firstHairColor = facialHair[1],
                secondHairColor = facialHair[2],
                eyebrowsModel = facialHair[3] < 0 || facialHair[3] > 255 ? 255 : facialHair[3],
                eyebrowsColor = facialHair[4],
                beardModel = facialHair[5] < 0 || facialHair[5] > 255 ? 255 : facialHair[5],
                beardColor = facialHair[6]
            };

            // Make the default camera active
            RAGE.Game.Cam.DestroyCam(customCamera, true);
            RAGE.Game.Cam.RenderScriptCams(false, false, 0, true, false, 0);

            Events.CallRemote("changeHairStyle", JsonConvert.SerializeObject(generatedFace));
        }

        private void CancelHairdresserChangesEvent(object[] args)
        {
            // Revert the changes
            Player.LocalPlayer.SetComponentVariation(2, initialHair.hairModel, 0, 0);
            Player.LocalPlayer.SetHairColor(initialHair.firstHairColor, initialHair.secondHairColor);
            Player.LocalPlayer.SetHeadOverlay(1, initialHair.beardModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlay(2, initialHair.eyebrowsModel, 1.0f);
            Player.LocalPlayer.SetHeadOverlayColor(1, 1, initialHair.beardColor, 0);
            Player.LocalPlayer.SetHeadOverlayColor(2, 1, initialHair.eyebrowsColor, 0);

            // Make the default camera active
            RAGE.Game.Cam.DestroyCam(customCamera, true);
            RAGE.Game.Cam.RenderScriptCams(false, false, 0, true, false, 0);

            // Destroy the current browser
            Browser.DestroyBrowserEvent(null);
        }
    }
}
