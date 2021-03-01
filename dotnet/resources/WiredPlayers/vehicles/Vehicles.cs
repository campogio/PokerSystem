using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WiredPlayers.chat;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.jobs;
using WiredPlayers.messages.error;
using WiredPlayers.messages.information;
using WiredPlayers.messages.success;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.vehicles
{
    public class Vehicles : Script
    {
        public static Dictionary<int, Timer> gasTimerList;
        public static Dictionary<int, VehicleModel> IngameVehicles;

        public static void GenerateGameVehicles()
        {
            // Create the timer dictionaries
            gasTimerList = new Dictionary<int, Timer>();

            foreach (VehicleModel vehModel in IngameVehicles.Values)
            {
                if (vehModel.Parking == 0)
                {
                    // Create the vehicle ingame
                    CreateIngameVehicle(vehModel);
                }
            }
        }

        public static async Task CreateVehicle(Player player, VehicleModel vehModel, bool adminCreated)
        {
            // Add the vehicle to the database
            vehModel.Id = await DatabaseOperations.AddNewVehicle(vehModel);
            IngameVehicles.Add(vehModel.Id, vehModel);

            // Create the vehicle ingame
            CreateIngameVehicle(vehModel);

            if (!adminCreated)
            {
                player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Bank -= vehModel.Price;
                player.SendChatMessage(Constants.COLOR_SUCCESS + string.Format(SuccRes.vehicle_purchased, vehModel.Model, vehModel.Price));
            }
        }

        public static void CreateIngameVehicle(VehicleModel vehModel)
        {
            Vehicle vehicle = NAPI.Vehicle.CreateVehicle(vehModel.Model, vehModel.Position, vehModel.Heading, 0, 0);
            vehicle.NumberPlate = vehModel.Plate == string.Empty ? "LS " + (1000 + vehModel.Id) : vehModel.Plate;
            vehicle.EngineStatus = vehModel.Engine != 0;
            vehicle.Locked = vehModel.Locked != 0;
            vehicle.Dimension = vehModel.Dimension;

            // Paint the vehicle
            Mechanic.RepaintVehicle(vehicle, vehModel);

            if (vehicle.Model == (uint)VehicleHash.Ambulance)
            {
                // Set the livery for the Fire Department
                vehicle.Livery = 1;
            }

            // Add the vehicle data
            vehicle.SetData(EntityData.VehicleId, vehModel.Id);

            vehicle.SetSharedData(EntityData.VehicleSirenSound, true);
            vehicle.SetSharedData(EntityData.VehicleDoorsState, NAPI.Util.ToJson(new List<bool> { false, false, false, false, false, false }));

            // Set vehicle's tunning
            Mechanic.AddTunningToVehicle(vehModel.Id, vehicle);
        }

        public static void OnPlayerDisconnected(Player player)
        {
            if (gasTimerList.TryGetValue(player.Value, out Timer gasTimer))
            {
                gasTimer.Dispose();
                gasTimerList.Remove(player.Value);
            }
        }
        
        public static Vehicle GetClosestVehicle(Player player, float distance)
        {
            // Get the closest vehicle in the same dimension as the player
            VehicleModel closest = IngameVehicles.Values.OrderBy(v => player.Position.DistanceTo(v.Position)).FirstOrDefault(v => v.Dimension == player.Dimension && player.Position.DistanceTo(v.Position) <= distance);

            return closest == null ? null : GetVehicleById<Vehicle>(closest.Id);
        }

        public static bool HasPlayerVehicleKeys(Player player, object veh, bool onlyCitizen)
        {            
            // Convert the vehicle
            VehicleModel vehicle = veh is Vehicle ? GetVehicleById<VehicleModel>(((Vehicle)veh).GetData<int>(EntityData.VehicleId)) : (VehicleModel)veh;

            // We don't need to check testing vehicles
            if (vehicle.Testing) return false;

            bool hasKeys = false;

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (vehicle.Owner == player.Name)
            {
                hasKeys = true;
            }
            else if (onlyCitizen)
            {
                hasKeys = characterModel.VehicleKeys.Split(',').Any(key => int.Parse(key) == vehicle.Id);
            }
            else if (vehicle.Faction == (int)PlayerFactions.DrivingSchool)
            {
                // Check if the player is on a driving exam
                hasKeys = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).DrivingExam != DrivingExams.None;
            }
            else if (vehicle.Faction == (int)PlayerFactions.Admin)
            {
                // Check if the player has admin rank
                hasKeys = characterModel.AdminRank > StaffRank.None;
            }
            else if (vehicle.Faction != (int)PlayerFactions.None)
            {
                hasKeys = (vehicle.Faction == (int)characterModel.Faction || (int)characterModel.Job + Constants.MAX_FACTION_VEHICLES == vehicle.Faction);
            }

            return hasKeys;
        }

        public static T GetVehicleById<T>(int vehicleId)
        {
            if (typeof(VehicleModel).IsAssignableFrom(typeof(T)))
            {
                return IngameVehicles.ContainsKey(vehicleId) ? (T)Convert.ChangeType(IngameVehicles[vehicleId], typeof(T)) : default;
            }
            
            if (typeof(Vehicle).IsAssignableFrom(typeof(T)))
            {
                return (T)Convert.ChangeType(NAPI.Pools.GetAllVehicles().FirstOrDefault(veh => veh.GetData<int>(EntityData.VehicleId) == vehicleId), typeof(T));
            }   

            return default;
        }

        public static void SaveAllVehicles()
        {
            Dictionary<int, VehicleModel> savedVehicles = IngameVehicles.Where(v => !v.Value.Testing && v.Value.Faction != (int)PlayerFactions.None)
                                                                        .ToDictionary(pair => pair.Key, pair => pair.Value);
                                                                        
            // Save all the vehicles
            Task.Run(() => DatabaseOperations.SaveVehicles(savedVehicles)).ConfigureAwait(false);
        }

        public static bool IsVehicleTrunkInUse(Vehicle vehicle)
        {
            // Check if the trunk is in use
            return NAPI.Pools.GetAllPlayers().Any(p => p.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).OpenedTrunk == vehicle);
        }
        
        public static void RespawnVehicle(Vehicle vehicle)
        {
            if (vehicle == null || !vehicle.Exists) return;
            
            // Repair the vehicle
            vehicle.Repair();
            
            // Get the vehicle from the model
            VehicleModel vehModel = GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId));
            vehicle.Position = vehModel.Position;
            vehicle.Rotation = new Vector3(0.0f, 0.0f, vehModel.Heading);
        }

        private void OnVehicleDeathTimer(Vehicle vehicle)
        {
            // Get the data from the game vehicle
            VehicleModel vehicleModel = GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId));

            // Delete the vehicle
            vehicle.Delete();

            if (vehicleModel.Faction == (int)PlayerFactions.None)
            {
                ParkingModel scrapyard = Parking.ParkingList.Values.FirstOrDefault(p => p.Type == ParkingTypes.Scrapyard);

                if (scrapyard != null)
                {
                    // Add the vehicle to the parking
                    vehicleModel.Position = scrapyard.Position;
                    vehicleModel.Parking = scrapyard.Id;
                    vehicleModel.Parked = 0;
                }

                // Save vehicle data
                Dictionary<int, VehicleModel> vehicleCollection = new Dictionary<int, VehicleModel>() { { vehicleModel.Id, vehicleModel } };
                Task.Run(() => DatabaseOperations.SaveVehicles(vehicleCollection)).ConfigureAwait(false);
            }
            else
            {
                // Recreate the vehicle in the same position
                CreateIngameVehicle(vehicleModel);
            }
        }

        public static void OnVehicleRefueled(object vehicleObject)
        {
            Vehicle vehicle = (Vehicle)vehicleObject;
            Player player = vehicle.GetData<Player>(EntityData.VehicleRefueling);

            vehicle.ResetData(EntityData.VehicleRefueling);
            player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).Refueling = null;

            if (gasTimerList.TryGetValue(player.Value, out Timer gasTimer))
            {
                gasTimer.Dispose();
                gasTimerList.Remove(player.Value);
            }

            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.vehicle_refueled);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seat)
        {
            if (Convert.ToInt32(seat) != (int)VehicleSeat.Driver) return;
            
            // Get the vehicle data
            VehicleModel vehModel = GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId));
                        
            if (vehModel.Testing)
            {
                // Get the testing vehicle from the player
                Vehicle testingVehicle = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).TestingVehicle;
                
                if (vehicle != testingVehicle)
                {
                    player.WarpOutOfVehicle();
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_testing_vehicle);
                    return;
                }
            }
            else if (vehModel.Faction != (int)PlayerFactions.None)
            {
                // Get the character model for the player
                CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);
                
                if (characterModel.AdminRank == StaffRank.None && vehModel.Faction == (int)PlayerFactions.Admin)
                {
                    player.WarpOutOfVehicle();
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.admin_vehicle);
                    return;
                }
                
                if (vehModel.Faction < Constants.MAX_FACTION_VEHICLES && (int)characterModel.Faction != vehModel.Faction && vehModel.Faction != (int)PlayerFactions.DrivingSchool && vehModel.Faction != (int)PlayerFactions.Admin)
                {
                    player.WarpOutOfVehicle();
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle_faction);
                    return;
                }
                
                if (vehModel.Faction > Constants.MAX_FACTION_VEHICLES && (int)characterModel.Job + Constants.MAX_FACTION_VEHICLES != vehModel.Faction)
                {
                    player.WarpOutOfVehicle();
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle_job);
                    return;
                }
            }

            if (vehicle.Class != (int)VehicleClass.Cycle)
            {
                // Engine toggle message
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.how_to_start_engine);

                // Initialize speedometer and engine status
                player.TriggerEvent("initializeSpeedometer", vehModel.Kms, vehModel.Gas, vehicle.EngineStatus);
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void PlayerExitVehicletEvent(Player player, Vehicle vehicle)
        {
            // Set the vehicle engine state
            NAPI.Task.Run(() => player.TriggerEvent("KeepVehicleEngineState", vehicle.Id, vehicle.EngineStatus), 500);
        }

        [ServerEvent(Event.VehicleDeath)]
        public void OnVehicleDeath(Vehicle vehicle)
        {
            // Kick all the players from the vehicle if any
            vehicle.Occupants.Cast<Player>().ToList().ForEach(p => p.WarpOutOfVehicle());

            // Create the timer to delete the vehicle
            NAPI.Task.Run(() => OnVehicleDeathTimer(vehicle), 5000);
        }

        [RemoteEvent("stopPlayerCar")]
        public void StopPlayerCarEvent(Player player)
        {
            // Turn the engine off if the player is driving the vehicle
            if (player.VehicleSeat == (int)VehicleSeat.Driver) player.Vehicle.EngineStatus = false;
        }

        [RemoteEvent("saveVehicleConsumes")]
        public void SaveVehicleConsumesEvent(Player _, ushort vehicleId, bool inWater, float kms, float gas)
        {
            // Get the vehicle with the given handle
            Vehicle vehicle = NAPI.Pools.GetAllVehicles().FirstOrDefault(veh => Convert.ToInt32(vehicleId) == veh.Value);
            int id = vehicle.GetData<int>(EntityData.VehicleId);

            // Get the vehicle from the id
            if (IngameVehicles.ContainsKey(id))
            {
                // Update kms and gas
                IngameVehicles[id].Kms = kms;
                IngameVehicles[id].Gas = gas;
            }

            if (inWater)
            {
                // Destroy the vehicle
                OnVehicleDeathTimer(vehicle);
            }
        }

        [RemoteEvent("toggleSeatbelt")]
        public void ToggleSeatbeltEvent(Player player, bool seatbelt)
        {
            // Send the message to the nearby players
            Chat.SendMessageToNearbyPlayers(player, seatbelt ? InfoRes.seatbelt_fasten : InfoRes.seatbelt_unfasten, ChatTypes.Me, 20.0f);
        }
    }
}
