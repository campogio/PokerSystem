using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.messages.general;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.vehicles
{
    public static class Parking
    {
        public static Dictionary<int, ParkingModel> ParkingList;

        public static void GenerateIngameParkings()
        {
            foreach (ParkingModel parking in ParkingList.Values)
            {
                string parkingLabelText = GetParkingLabelText(parking.Type);
                parking.ParkingLabel = NAPI.TextLabel.CreateTextLabel(parkingLabelText, parking.Position, 30.0f, 0.75f, 4, new Color(255, 255, 255));
            }
        }

        public static ParkingModel GetClosestParking(Player player, float distance = 1.5f)
        {
            // Get the closest parking given the distance
            return ParkingList.Values.OrderBy(p => player.Position.DistanceTo2D(p.Position)).FirstOrDefault(p => player.Position.DistanceTo2D(p.Position) <= distance);
        }

        public static int GetParkedCarAmount(int parking)
        {
            // Get all the vehicles in a parking
            return Vehicles.IngameVehicles.Values.Count(v => v.Parking == parking);
        }

        public static string GetParkingLabelText(ParkingTypes type)
        {
            string labelText = string.Empty;
            
            switch (type)
            {
                case ParkingTypes.Public:
                    labelText = GenRes.public_parking;
                    break;
                case ParkingTypes.Garage:
                    labelText = GenRes.garage;
                    break;
                case ParkingTypes.Scrapyard:
                    labelText = GenRes.scrapyard;
                    break;
                case ParkingTypes.Deposit:
                    labelText = GenRes.police_depot;
                    break;
            }
            
            return labelText;
        }

        public static ParkingModel GetParkingById(int parkingId)
        {
            // Get the parking given an specific identifier
            return ParkingList.Values.FirstOrDefault(parkingModel => parkingModel.Id == parkingId);
        }

        public static void PlayerParkVehicle(Player player, ParkingModel parking)
        {            
            // Get vehicle data
            VehicleModel vehicleModel = Vehicles.GetVehicleById<VehicleModel>(player.Vehicle.GetData<int>(EntityData.VehicleId));

            // Update parking values
            vehicleModel.Position = parking.Position;
            vehicleModel.Dimension = Convert.ToUInt32(parking.Id);
            vehicleModel.Parking = parking.Id;
            vehicleModel.Parked = 0;

            // Save the vehicle and delete it from the game
            player.Vehicle.Delete();

            // Save the vehicle
            Dictionary<int, VehicleModel> vehicleCollection = new Dictionary<int, VehicleModel>() { { vehicleModel.Id, vehicleModel } };
            Task.Run(() => DatabaseOperations.SaveVehicles(vehicleCollection)).ConfigureAwait(false);
        }
    }
}
