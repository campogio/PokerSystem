using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using WiredPlayers.vehicles;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Buildings.Businesses
{
    public class CarShop : Script
    {
        private static Marker CarShopMarker;
        private static Marker BikeShopMarker;
        private static Marker BoatShopMarker;

        public static List<CarShopVehicleModel> DealerVehicles;

        public CarShop()
        {
            // Car dealer creation
            ColShape carShopColShape = NAPI.ColShape.CreateCylinderColShape(Coordinates.CarShop, 2.5f, 1.0f);
            carShopColShape.SetData(EntityData.ColShapeType, ColShapeTypes.VehicleDealer);
            carShopColShape.SetData(EntityData.InstructionalButton, HelpRes.show_catalog);

            Blip carShopBlip = NAPI.Blip.CreateBlip(Coordinates.CarShop);
            carShopBlip.Name = GenRes.car_dealer;
            carShopBlip.ShortRange = true;
            carShopBlip.Sprite = 225;

            CarShopMarker = NAPI.Marker.CreateMarker(29, Coordinates.CarShop, new Vector3(), new Vector3(), 0.75f, new Color(244, 126, 23));

            // Motorcycle dealer creation
            ColShape bikeShopColShape = NAPI.ColShape.CreateCylinderColShape(Coordinates.MotorbikeShop, 2.5f, 1.0f);
            bikeShopColShape.SetData(EntityData.ColShapeType, ColShapeTypes.VehicleDealer);
            bikeShopColShape.SetData(EntityData.InstructionalButton, HelpRes.show_catalog);

            Blip bikeShopBlip = NAPI.Blip.CreateBlip(Coordinates.MotorbikeShop);
            bikeShopBlip.Name = GenRes.motorcycle_dealer;
            bikeShopBlip.ShortRange = true;
            bikeShopBlip.Sprite = 226;

            BikeShopMarker = NAPI.Marker.CreateMarker(29, Coordinates.MotorbikeShop, new Vector3(), new Vector3(), 0.75f, new Color(244, 126, 23));

            // Boat dealer creation
            ColShape boatShopColShape = NAPI.ColShape.CreateCylinderColShape(Coordinates.BoatShop, 2.5f, 1.0f);
            boatShopColShape.SetData(EntityData.ColShapeType, ColShapeTypes.VehicleDealer);
            boatShopColShape.SetData(EntityData.InstructionalButton, HelpRes.show_catalog);

            Blip boatShopBlip = NAPI.Blip.CreateBlip(Coordinates.BoatShop);
            boatShopBlip.Name = GenRes.boat_dealer;
            boatShopBlip.ShortRange = true;
            boatShopBlip.Sprite = 455;

            BoatShopMarker = NAPI.Marker.CreateMarker(29, Coordinates.BoatShop, new Vector3(), new Vector3(), 0.75f, new Color(244, 126, 23));
        }

        public static int GetClosestCarShop(Player player, float distance = 2.0f)
        {
            int carShop = -1;

            if (player.Position.DistanceTo(CarShopMarker.Position) < distance)
            {
                carShop = 0;
            }
            else if (player.Position.DistanceTo(BikeShopMarker.Position) < distance)
            {
                carShop = 1;
            }
            else if (player.Position.DistanceTo(BoatShopMarker.Position) < distance)
            {
                carShop = 2;
            }

            return carShop;
        }
        
        public static void ShowCatalog(Player player)
        {
            // Get the player's dealer
            int dealer = GetClosestCarShop(player);

            // We get the vehicle list
            List<CarShopVehicleModel> carList = DealerVehicles.FindAll(vehicle => vehicle.carShop == dealer);

            if (carList == null || carList.Count == 0)
            {
                // There are no vehicles loaded on the vehicle dealer
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_dealer_empty);
                return;
            }

            // Getting the speed for each vehicle in the list
            foreach (CarShopVehicleModel carShopVehicle in carList)
            {
                carShopVehicle.model = carShopVehicle.hash.ToString();
                VehicleHash vehicleHash = NAPI.Util.VehicleNameToModel(carShopVehicle.model);
                carShopVehicle.speed = (int)Math.Round(NAPI.Vehicle.GetVehicleMaxSpeed(vehicleHash) * 3.6f);
            }
            
            // Get the money in the player's bank
            int bankMoney = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Bank;

            // We show the catalog
            player.TriggerEvent("ShowVehicleCatalog", carList, dealer, bankMoney);
        }

        public static void OnPlayerDisconnected(Player player)
        {
            // Check if the player created any testing vehicle
            if (!Vehicles.IngameVehicles.ContainsKey(-player.Value)) return;

            // Destroy the created vehicle
            Vehicles.GetVehicleById<Vehicle>(-player.Value).Delete();
            Vehicles.IngameVehicles.Remove(-player.Value);
        }

        private int GetVehiclePrice(uint vehicleHash)
        {
            // Get the price of the vehicle
            CarShopVehicleModel carDealerVehicle = DealerVehicles.Where(vehicle => NAPI.Util.GetHashKey(vehicle.hash) == vehicleHash).FirstOrDefault();

            return carDealerVehicle == null ? 0 : carDealerVehicle.price;
        }

        private async Task<bool> SpawnPurchasedVehicle(Player player, SpawnModel[] spawns, uint hash, int vehiclePrice, string firstColor, string secondColor)
        {
            // Get the first  spawn
            SpawnModel spawn = spawns.FirstOrDefault(s => NAPI.Pools.GetAllVehicles().Count(veh => s.Position.DistanceTo(veh.Position) < 2.5f) == 0);

            if (spawn == null)
            {
                // All the parking places are occupied
                return false;
            }

            // Basic data for vehicle creation
            VehicleModel vehicleModel = new VehicleModel()
            {
                Model = hash,
                Plate = string.Empty,
                Position = spawn.Position,
                Heading = spawn.Heading,
                Owner = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).RealName,
                ColorType = Constants.VEHICLE_COLOR_TYPE_CUSTOM,
                FirstColor = firstColor,
                SecondColor = secondColor,
                Pearlescent = 0,
                Price = vehiclePrice,
                Parking = 0,
                Parked = 0,
                Engine = 0,
                Locked = 0,
                Gas = 50.0f,
                Kms = 0.0f
        };

            // Creating the purchased vehicle
            await Vehicles.CreateVehicle(player, vehicleModel, false);

            return true;
        }

        [RemoteEvent("PurchaseVehicle")]
        public async Task PurchaseVehicleEvent(Player player, int carDealer, string hash, string firstColor, string secondColor)
        {
            uint model = uint.Parse(hash);
            int vehiclePrice = GetVehiclePrice(model);

            if (vehiclePrice == 0 || player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Bank < vehiclePrice)
            {
                // Player doesn't have enough money in the bank
                player.SendChatMessage(Constants.COLOR_ERROR + string.Format(ErrRes.carshop_no_money, vehiclePrice));
            }

            // Check if the vehicle can be spawned
            bool vehicleSpawned = false;

            switch (carDealer)
            {
                case 0:
                    // Create a new car
                    vehicleSpawned = await SpawnPurchasedVehicle(player, Coordinates.CarShopSpawns, model, vehiclePrice, firstColor, secondColor);
                    break;

                case 1:
                    // Create a new motorcycle
                    vehicleSpawned = await SpawnPurchasedVehicle(player, Coordinates.BikeShopSpawns, model, vehiclePrice, firstColor, secondColor);
                    break;

                case 2:
                    // Create a new ship
                    vehicleSpawned = await SpawnPurchasedVehicle(player, Coordinates.BoatShopSpawns, model, vehiclePrice, firstColor, secondColor);
                    break;
            }

            if (!vehicleSpawned)
            {
                // Parking places are occupied
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.carshop_spawn_occupied);
            }
        }

        [RemoteEvent("testVehicle")]
        public void TestVehicleEvent(Player player, int dealer, string hash)
        {
            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // Check if the player is already testing a vehicle
            if (data.TestingVehicle != null && data.TestingVehicle.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.already_testing_vehicle);
                return;
            }

            Vehicle vehicle = null;
            Vector3 testFinishCheckpoint = new Vector3();

            switch (dealer)
            {
                case 0:
                    vehicle = NAPI.Vehicle.CreateVehicle(uint.Parse(hash), Coordinates.TestCarSpawn, 128.0f, 0, 0);
                    testFinishCheckpoint = Coordinates.TestCarCheckpoint;
                    break;

                case 1:
                    vehicle = NAPI.Vehicle.CreateVehicle(uint.Parse(hash), Coordinates.TestMotorbikeSpawn, 180.0f, 0, 0);
                    testFinishCheckpoint = Coordinates.TestMotorbikeCheckpoint;
                    break;

                case 2:
                    vehicle = NAPI.Vehicle.CreateVehicle(uint.Parse(hash), Coordinates.TestBoatSpawn, 180.0f, 0, 0);
                    testFinishCheckpoint = Coordinates.TestBoatCheckpoint;
                    break;
            }

            vehicle.EngineStatus = true;
            vehicle.SetSharedData(EntityData.VehicleDoorsState, NAPI.Util.ToJson(new List<bool> { false, false, false, false, false, false }));

            // Vehicle variable initialization
            VehicleModel vehModel = new VehicleModel() { Kms = 0.0f, Gas = 50.0f, Testing = true };

            // Add the vehicle to the collection
            vehicle.SetData(EntityData.VehicleId, -player.Value);
            Vehicles.IngameVehicles.Add(-player.Value, vehModel);

            data.TestingVehicle = vehicle;
            player.SetIntoVehicle(vehicle, (int)VehicleSeat.Driver);

            // Adding the checkpoint
            player.TriggerEvent("showCarshopCheckpoint", testFinishCheckpoint);

            // Confirmation message sent to the player
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.player_test_vehicle);
        }

        [RemoteEvent("deliverTestVehicle")]
        public void DeliverTestVehicleEvent(Player player)
        {
            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (player.Vehicle == data.TestingVehicle)
            {
                // We destroy the vehicle
                player.Vehicle.Delete();
                
                // Delete the vehicle from the collection
                Vehicles.IngameVehicles.Remove(-player.Value);

                // Variable cleaning
                data.TestingVehicle = null;
            }
        }
    }
}
