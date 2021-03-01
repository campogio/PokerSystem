using RAGE;
using RAGE.Elements;
using Newtonsoft.Json;
using WiredPlayers_Client.globals;
using WiredPlayers_Client.model;
using WiredPlayers_Client.chat;
using System.Collections.Generic;
using System;
using System.Linq;

namespace WiredPlayers_Client.CarDealer
{
    class CarDealer : Events.Script
    {
        private Vector3 PlayerPosition;
        private int PlayerBankMoney;
        private string carShopVehiclesJson = null;
        private Blip carShopTestBlip = null;
        private Checkpoint carShopTestCheckpoint = null;
        private Vehicle previewVehicle = null;
        private int previewCamera;
        private int dealership;

        public CarDealer()
        {
            Events.Add("ShowVehicleCatalog", ShowVehicleCatalogEvent);
            Events.Add("previewCarShopVehicle", PreviewCarShopVehicleEvent);
            Events.Add("rotatePreviewVehicle", RotatePreviewVehicleEvent);
            Events.Add("previewVehicleChangeColor", PreviewVehicleChangeColorEvent);
            Events.Add("closeCatalog", CloseCatalogEvent);
            Events.Add("purchaseVehicle", PurchaseVehicleEvent);
            Events.Add("testVehicle", TestVehicleEvent);
            Events.Add("showCarshopCheckpoint", ShowCarshopCheckpointEvent);

            Events.OnPlayerEnterCheckpoint += OnPlayerEnterCheckpoint;
        }

        private void ShowVehicleCatalogEvent(object[] args)
        {
            // Get the variables from the arguments
            carShopVehiclesJson = args[0].ToString();
            dealership = Convert.ToInt32(args[1]);
            PlayerBankMoney = Convert.ToInt32(args[2]);

            // Disable the chat
            ChatManager.SetVisible(false);

            // Show the catalog
            Browser.CreateBrowser("catalog.html", "closeCatalog", "initializeCatalog", dealership, carShopVehiclesJson);
        }

        private void PreviewCarShopVehicleEvent(object[] args)
        {
            // Get the variables from the arguments
            uint model = RAGE.Game.Misc.GetHashKey(args[0].ToString());

            // Make the player invisible 
            Player.LocalPlayer.SetAlpha(0, false);

            // Disable the HUD
            RAGE.Game.Ui.DisplayHud(false);
            RAGE.Game.Ui.DisplayRadar(false);

            // Get the ground position and current player position
            PlayerPosition = Player.LocalPlayer.Position;
            float ground = 0.0f;
            SpawnModel spawn;

            switch (dealership)
            {
                case 2:
                    // Get the vehicle spawn
                    spawn = new SpawnModel(new Vector3(-878.5726f, -1353.408f, 0.1741f), 90.0f);

                    // Move the player to the position
                    RAGE.Game.Misc.GetGroundZFor3dCoord(-882.3361f, -1342.628f, 5.0783f, ref ground, false);
                    Player.LocalPlayer.Position = new Vector3(-882.3361f, -1342.628f, ground + 1.0f);

                    // Create the camera and spawn the player
                    previewCamera = RAGE.Game.Cam.CreateCameraWithParams(RAGE.Game.Misc.GetHashKey("DEFAULT_SCRIPTED_CAMERA"), -882.3361f, -1342.628f, 5.0783f, -20.0f, 0.0f, 200.0f, 90.0f, true, 2);
                    
                    break;

                default:
                    // Get the vehicle spawn
                    spawn = new SpawnModel(new Vector3(-31.98111f, -1090.434f, 26.42225f), 180.0f);

                    // Move the player to the position
                    RAGE.Game.Misc.GetGroundZFor3dCoord(-37.83527f, -1088.096f, 27.92234f, ref ground, false);
                    Player.LocalPlayer.Position = new Vector3(-37.83527f, -1088.096f, ground + 1.0f);

                    // Create the camera and spawn the player
                    previewCamera = RAGE.Game.Cam.CreateCameraWithParams(RAGE.Game.Misc.GetHashKey("DEFAULT_SCRIPTED_CAMERA"), -37.83527f, -1088.096f, 27.92234f, -20.0f, 0.0f, 250, 90.0f, true, 2);
                    
                    break;
            }

            // Make the camera point the vehicle
            RAGE.Game.Cam.SetCamActive(previewCamera, true);
            RAGE.Game.Cam.RenderScriptCams(true, false, 0, true, false, 0);

            // Vehicle preview menu
            Browser.ExecuteFunction("showVehiclePreview", CanPlayerPayVehicle(model));

            // Create the preview vehicle
            RAGE.Game.Invoker.Wait(750);
            previewVehicle = new Vehicle(model, spawn.Position, spawn.Heading);
        }

        private void RotatePreviewVehicleEvent(object[] args)
        {
            // Get the variables from the arguments
            float rotation = (float)Convert.ToDouble(args[0]);

            // Set the vehicle's heading
            previewVehicle.SetHeading(rotation);
        }

        private void PreviewVehicleChangeColorEvent(object[] args)
        {
            // Get the variables from the arguments
            string colorHex = args[0].ToString().Substring(1);
            bool colorMain = (bool)args[1];

            // Get the RGB from HEX string
            int red = Convert.ToInt32(colorHex.Substring(0, 2), 16);
            int green = Convert.ToInt32(colorHex.Substring(2, 2), 16);
            int blue = Convert.ToInt32(colorHex.Substring(4, 2), 16);

            if (colorMain)
            {
                // Set the vehicle's primary color
                previewVehicle.SetCustomPrimaryColour(red, green, blue);
            }
            else
            {
                // Set the vehicle's secondary color
                previewVehicle.SetCustomSecondaryColour(red, green, blue);
            }
        }

        private void CloseCatalogEvent(object[] args)
        {
            // Destroy the browser
            Browser.DestroyBrowserEvent(null);

            if (previewVehicle != null && args == null)
            {
                // Go back to the main catalog
                Browser.CreateBrowser("catalog.html", "closeCatalog", "initializeCatalog", dealership, carShopVehiclesJson);
            }
            else
            {
                // Enable the HUD
                RAGE.Game.Ui.DisplayHud(true);
                RAGE.Game.Ui.DisplayRadar(true);

                // Enable the chat
                ChatManager.SetVisible(true);
            }

            if (PlayerPosition != null)
            {
                // Set the character's original position
                Player.LocalPlayer.Position = PlayerPosition;
                PlayerPosition = null;
            }

            if (previewVehicle != null)
            {
                // There's a vehicle already created
                previewVehicle.Destroy();
                previewVehicle = null;
            }

            // Make the player visible
            Player.LocalPlayer.SetAlpha(255, false);

            // Position the camera behind the character
            RAGE.Game.Cam.DestroyCam(previewCamera, true);
            RAGE.Game.Cam.RenderScriptCams(false, false, 0, true, false, 0);
        }

        private bool CanPlayerPayVehicle(uint model)
        {
            // Get the vehicles' list
            List<CarDealerVehicle> vehicleList = JsonConvert.DeserializeObject<List<CarDealerVehicle>>(carShopVehiclesJson);
            
            // Check if the player can pay the vehicle
            return PlayerBankMoney >= vehicleList.First(v => RAGE.Game.Misc.GetHashKey(v.model) == model).price;
        }

        private void PurchaseVehicleEvent(object[] args)
        {
            // Get the colors variables
            int primaryRed = 0, primaryGreen = 0, primaryBlue = 0;
            int secondaryRed = 0, secondaryGreen = 0, secondaryBlue = 0;

            // Get the vehicle's data
            uint model = previewVehicle.Model;
            previewVehicle.GetCustomPrimaryColour(ref primaryRed, ref primaryGreen, ref primaryBlue);
            previewVehicle.GetCustomSecondaryColour(ref secondaryRed, ref secondaryGreen, ref secondaryBlue);

            // Get color strings
            string firstColor = string.Format("{0},{1},{2}", primaryRed, primaryGreen, primaryBlue);
            string secondColor = string.Format("{0},{1},{2}", secondaryRed, secondaryGreen, secondaryBlue);

            // Destroy preview menu
            CloseCatalogEvent(new object[] { true });

            // Purchase the vehicle
            Events.CallRemote("PurchaseVehicle", dealership, model.ToString(), firstColor, secondColor);
        }

        private void TestVehicleEvent(object[] args)
        { 
            // Get the vehicle's data
            string model = previewVehicle.Model.ToString();

            // Destroy preview menu
            CloseCatalogEvent(new object[] { true });

            // Create the vehicle for testing
            Events.CallRemote("testVehicle", dealership, model);
        }

        private void ShowCarshopCheckpointEvent(object[] args)
        {
            // Get the variables from the arguments
            Vector3 position = (Vector3)args[0];

            // Add a blip with the delivery place
            carShopTestBlip = new Blip(1, position, string.Empty, 1f, 1);
            carShopTestCheckpoint = new Checkpoint(4, position, 2.5f, new Vector3(), new RGBA(198, 40, 40, 200));
        }

        private void OnPlayerEnterCheckpoint(Checkpoint checkpoint, Events.CancelEventArgs cancel)
        {
            if(checkpoint == carShopTestCheckpoint && Player.LocalPlayer.Vehicle != null)
            {
                // Destroy the checkpoint
                carShopTestCheckpoint.Destroy();
                carShopTestCheckpoint = null;

                // Delete the blip
                carShopTestBlip.Destroy();
                carShopTestBlip = null;

                // Deliver the test vehicle
                Events.CallRemote("deliverTestVehicle");
            }
        }
    }
}
