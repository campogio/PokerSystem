using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WiredPlayers.Currency;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.error;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using WiredPlayers.vehicles;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.jobs
{
    public class Garbage : Script
    {
        public static Dictionary<int, Timer> garbageTimerList;

        public Garbage()
        {
            // Initialize the variables
            garbageTimerList = new Dictionary<int, Timer>();
        }

        public static void OnPlayerDisconnected(Player player)
        {
            if (garbageTimerList.TryGetValue(player.Value, out Timer garbageTimer) == true)
            {
                garbageTimer.Dispose();
                garbageTimerList.Remove(player.Value);
            }
        }

        private void OnGarbageTimer(object playerObject)
        {
            Player player = (Player)playerObject;

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // Respawn the job vehicle
            Vehicles.RespawnVehicle(data.LastVehicle);

            // Cancel the garbage route
            data.LastVehicle = null;
            data.JobCheckPoint = 0;
            data.JobPartner.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).JobCheckPoint = 0;

            if (garbageTimerList.TryGetValue(player.Value, out Timer garbageTimer) == true)
            {
                // Remove the timer
                garbageTimer.Dispose();
                garbageTimerList.Remove(player.Value);
            }

            // Send the message to both players
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.job_vehicle_abandoned);
            data.JobPartner.SendChatMessage(Constants.COLOR_ERROR + ErrRes.job_vehicle_abandoned);
        }

        public static void OnGarbageCollectedTimer(object playerObject)
        {
            Player player = (Player)playerObject;

            // Get the external data for both players
            PlayerTemporaryModel playerData = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);
            PlayerTemporaryModel driverData = playerData.JobPartner.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (garbageTimerList.TryGetValue(player.Value, out Timer garbageTimer) == true)
            {
                garbageTimer.Dispose();
                garbageTimerList.Remove(player.Value);
            }

            // Get garbage bag
            Task.Run(() => playerData.GarbageBag.Delete()).ConfigureAwait(false);
            player.StopAnimation();

            // Get the remaining checkpoints
            GarbageRoute route = driverData.JobRoute;
            int checkPoint = driverData.JobCheckPoint + 1;

            if (checkPoint < Constants.GarbagePoints[route].Length)
            {
                Vector3 currentGarbagePosition = GetGarbageCheckPointPosition(route, checkPoint);
                Vector3 nextGarbagePosition = GetGarbageCheckPointPosition(route, checkPoint + 1);

                driverData.JobCheckPoint = checkPoint;
                playerData.JobCheckPoint = checkPoint;

                Task.Run(() => 
                {
                    // Add the new garbage bag
                    playerData.GarbageBag = NAPI.Object.CreateObject(628215202, currentGarbagePosition, new Vector3());
                }).ConfigureAwait(false);

                // Create the checkpoints
                playerData.JobPartner.TriggerEvent("showGarbageCheckPoint", currentGarbagePosition, nextGarbagePosition, CheckpointType.CylinderSingleArrow);
                player.TriggerEvent("showGarbageCheckPoint", currentGarbagePosition, nextGarbagePosition, CheckpointType.CylinderSingleArrow);
            }
            else
            {
                playerData.JobPartner.SendChatMessage(Constants.COLOR_INFO + InfoRes.route_finished);

                playerData.JobPartner.TriggerEvent("showGarbageCheckPoint", Coordinates.Dumper, new Vector3(), CheckpointType.CylinderCheckerboard);
                player.TriggerEvent("deleteGarbageCheckPoint");
            }

            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.garbage_collected);
        }

        public static Vector3 GetGarbageCheckPointPosition(GarbageRoute route, int checkPoint)
        {
            // Get the garbage position
            return Constants.GarbagePoints[route][checkPoint];
        }

        private void FinishGarbageRoute(Player driver, bool canceled = false)
        {
            // Get the temporary data
            PlayerTemporaryModel driverData = driver.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);
            PlayerTemporaryModel partnerData = driverData.JobPartner.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // Respawn the job vehicle
            Vehicles.RespawnVehicle(driver.Vehicle);

            // Destroy the previous checkpoint
            driver.TriggerEvent("deleteGarbageCheckPoint");

            if (!canceled)
            {
                // Pay the earnings to both players
                Money.GivePlayerMoney(driver, Constants.MONEY_GARBAGE_ROUTE, out _);
                Money.GivePlayerMoney(driverData.JobPartner, Constants.MONEY_GARBAGE_ROUTE, out _);

                // Send the message with the earnings
                string message = string.Format(InfoRes.garbage_earnings, Constants.MONEY_GARBAGE_ROUTE);
                driver.SendChatMessage(Constants.COLOR_INFO + message);
                driverData.JobPartner.SendChatMessage(Constants.COLOR_INFO + message);
            }

            // Remove players from the vehicle
            driver.WarpOutOfVehicle();
            driverData.JobPartner.WarpOutOfVehicle();

            // Remove the route related data
            driverData.JobPartner = null;
            driverData.JobRoute = GarbageRoute.None;
            driverData.JobCheckPoint = 0;
            driverData.LastVehicle = null;

            partnerData.JobPartner = null;
            partnerData.GarbageBag = null;
            partnerData.JobCheckPoint = 0;
            partnerData.LastVehicle = null;
            partnerData.Animation = false;
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte seat)
        {
            // Get the vehicle faction
            int vehicleFaction = Vehicles.GetVehicleById<VehicleModel>(player.Vehicle.GetData<int>(EntityData.VehicleId)).Faction;
            
            // Check if the player is driving the correct vehicle
            if (vehicleFaction != (int)PlayerJobs.GarbageCollector + Constants.MAX_FACTION_VEHICLES) return;

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (seat == (sbyte)VehicleSeat.Driver)
            {
                if (data.JobRoute == GarbageRoute.None && (data.LastVehicle == null || !data.LastVehicle.Exists))
                {
                    player.WarpOutOfVehicle();
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_route);
                    return;
                }
                
                if (data.LastVehicle != vehicle)
                {
                    player.WarpOutOfVehicle();
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_your_job_vehicle);
                    return;
                }
                
                if (garbageTimerList.TryGetValue(player.Value, out Timer garbageTimer))
                {
                    garbageTimer.Dispose();
                    garbageTimerList.Remove(player.Value);
                }

                // Check whether route starts or he's returning to the truck
                if (data.JobPartner == player)
                {
                    data.JobPartner = player;
                    data.LastVehicle = vehicle;

                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.player_waiting_partner);

                    return;
                }
                
                // We continue with the previous route
                Vector3 garbagePosition = GetGarbageCheckPointPosition(data.JobRoute, data.JobCheckPoint);

                if (data.JobCheckPoint < Constants.GarbagePoints[data.JobRoute].Length)
                {
                    Vector3 nextPosition = GetGarbageCheckPointPosition(data.JobRoute, data.JobCheckPoint + 1);
                    player.TriggerEvent("showGarbageCheckPoint", garbagePosition, nextPosition, CheckpointType.CylinderSingleArrow);
                    data.JobPartner.TriggerEvent("showGarbageCheckPoint", garbagePosition, nextPosition, CheckpointType.CylinderSingleArrow);
                }
                else
                {
                    // Show the last checkpoint
                    player.TriggerEvent("showGarbageCheckPoint", garbagePosition, new Vector3(), CheckpointType.CylinderCheckerboard);
                }
            }
            else
            {
                // Get the vehicle's driver
                Player driver = vehicle.Occupants.Cast<Player>().ToList().FirstOrDefault(p => p.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).JobPartner != null && ((Player)p).VehicleSeat == (int)VehicleSeat.Driver);
                
                if (driver == null || !driver.Exists)
                {
                    // There's no player driving, kick the passenger
                    player.WarpOutOfVehicle();
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.wait_garbage_driver);
                    return;
                }

                // Check if the partner is driving
                if (data.JobPartner != driver) return;

                if (!player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty)
                {
                    player.WarpOutOfVehicle();
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                    return;
                }

                // Get the temporary data for the driver
                PlayerTemporaryModel driverData = driver.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

                // Link both players as partners
                data.JobPartner = driver;
                driverData.JobPartner = player;

                // Set the route to the passenger
                data.JobRoute = driverData.JobRoute;
                data.JobCheckPoint = 0;
                driverData.JobCheckPoint = 0;

                // Create the first checkpoint
                Vector3 currentGarbagePosition = GetGarbageCheckPointPosition(data.JobRoute, 0);
                Vector3 nextGarbagePosition = GetGarbageCheckPointPosition(data.JobRoute, 1);

                // Add garbage bag
                data.GarbageBag = NAPI.Object.CreateObject(628215202, currentGarbagePosition, new Vector3());

                driver.TriggerEvent("showGarbageCheckPoint", currentGarbagePosition, nextGarbagePosition, CheckpointType.CylinderSingleArrow);
                player.TriggerEvent("showGarbageCheckPoint", currentGarbagePosition, nextGarbagePosition, CheckpointType.CylinderSingleArrow);
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            // Check if the vehicle isn't destroyed
            if (vehicle == null || !vehicle.Exists) return;

            // Get the vehicle faction
            int vehicleFaction = Vehicles.GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId)).Faction;

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (data.LastVehicle == vehicle && player.VehicleSeat == (int)VehicleSeat.Driver && vehicleFaction == (int)PlayerJobs.GarbageCollector + Constants.MAX_FACTION_VEHICLES)
            {
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.job_vehicle_left, 45));
                    
                player.TriggerEvent("deleteGarbageCheckPoint");
                data.JobPartner.TriggerEvent("deleteGarbageCheckPoint");

                // Create the timer for driver to get into the vehicle
                Timer garbageTimer = new Timer(OnGarbageTimer, player, 45000, Timeout.Infinite);
                garbageTimerList.Add(player.Value, garbageTimer);
            }
        }

        [RemoteEvent("garbageCheckpointEntered")]
        public void GarbageCheckpointEnteredRemoteEvent(Player player)
        {
            // Get the temporary data for the player
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // Check if the driver is getting into the last checkpoint
            if (player.VehicleSeat != (int)VehicleSeat.Driver || data.JobCheckPoint != Constants.GarbagePoints[data.JobRoute].Length - 1) return;

            if (player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).LastVehicle == player.Vehicle)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle_job);
                return;
            }

            // Finish the route
            FinishGarbageRoute(player);
        }
    }
}
