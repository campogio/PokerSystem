using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WiredPlayers.Buildings.Houses;
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
    public class FastFood : Script
    {
        private static int FastFoodId;
        private static int OrderGenerationTime;
        private static Dictionary<int, Timer> FastFoodTimerList;
        private static List<FastfoodOrderModel> FastFoodOrderCollection;

        public FastFood()
        {
            // Initialize the class data
            FastFoodId = 1;
            FastFoodTimerList = new Dictionary<int, Timer>();
            FastFoodOrderCollection = new List<FastfoodOrderModel>();
            
            // Get orders generation time
            Random rnd = new Random();
            OrderGenerationTime = UtilityFunctions.GetTotalSeconds() + rnd.Next(0, 1) * 60;
        }

        public static void OnPlayerDisconnected(Player player)
        {
            // Check if the player is fastfood deliverer
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Job != PlayerJobs.Fastfood) return;

            if (!FastFoodTimerList.TryGetValue(player.Value, out Timer fastFoodTimer)) return;
            
            // Destroy the timer
            fastFoodTimer.Dispose();
            FastFoodTimerList.Remove(player.Value);

            // Respawn the job vehicle
            Vehicles.RespawnVehicle(player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).LastVehicle);
        }
        
        public static void RefreshOrders(int totalSeconds)
        {
            // Generate new fastfood orders
            if (OrderGenerationTime <= totalSeconds && House.HouseCollection.Count > 0)
            {
                Random rnd = new Random();
                int generatedOrders = rnd.Next(7, 20);
                
                for (int i = 0; i < generatedOrders; i++)
                {
                    FastfoodOrderModel order = new FastfoodOrderModel
                    {
                        id = FastFoodId,
                        pizzas = rnd.Next(0, 4),
                        hamburgers = rnd.Next(0, 4),
                        sandwitches = rnd.Next(0, 4),
                        position = GetPlayerFastFoodDeliveryDestination(),
                        limit = totalSeconds + 300,
                        taken = false
                    };

                    FastFoodOrderCollection.Add(order);
                    FastFoodId++;
                }

                // Update the new timer time
                OrderGenerationTime = totalSeconds + rnd.Next(2, 5) * 60;
            }

            // Remove old orders
            FastFoodOrderCollection.RemoveAll(order => !order.taken && order.limit <= totalSeconds);
        }

        public static void CheckFastfoodOrders(Player player)
        {
            // Get the deliverable orders
            FastfoodOrderModel[] fastFoodOrders = FastFoodOrderCollection.Where(o => !o.taken).ToArray();

            if (fastFoodOrders.Length == 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.order_none);
                return;
            }

            List<float> distancesList = new List<float>();

            foreach (FastfoodOrderModel order in fastFoodOrders)
            {
                float distance = player.Position.DistanceTo(order.position);
                distancesList.Add(distance);
            }

            player.TriggerEvent("showFastfoodOrders", NAPI.Util.ToJson(fastFoodOrders), NAPI.Util.ToJson(distancesList));
        }

        private int GetFastFoodOrderAmount(Player player)
        {
            int amount = 0;
            FastfoodOrderModel order = GetFastfoodOrderFromId(player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).DeliverOrder);
            
            if (order != null)
            {
                // Add the order amount
                amount += order.pizzas * (int)Prices.Pizza;
                amount += order.hamburgers * (int)Prices.Burger;
                amount += order.sandwitches * (int)Prices.Sandwich;
            }
            
            return amount;
        }

        private FastfoodOrderModel GetFastfoodOrderFromId(int orderId)
        {
            // Get the fastfood order from the specified identifier
            return FastFoodOrderCollection.FirstOrDefault(orderModel => orderModel.id == orderId);
        }

        private static Vector3 GetPlayerFastFoodDeliveryDestination()
        {
            Random random = new Random();
            int element = random.Next(House.HouseCollection.Count);
            return House.HouseCollection[House.HouseCollection.Keys.ToArray()[element]].Entrance;
        }

        private void OnFastFoodTimer(object playerObject)
        {
            Player player = (Player)playerObject;

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // Respawn the job vehicle
            Vehicles.RespawnVehicle(data.LastVehicle);

            // Cancel the order
            data.DeliverOrder = 0;
            data.LastVehicle = null;
            data.JobWon = 0;

            // Delete map blip
            player.TriggerEvent("fastFoodDeliverFinished");

            // Remove timer from the list
            Timer fastFoodTimer = FastFoodTimerList[player.Value];
            
            if (fastFoodTimer != null)
            {
                fastFoodTimer.Dispose();
                FastFoodTimerList.Remove(player.Value);
            }

            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.job_vehicle_abandoned);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void PlayerEnterVehicleServerEvent(Player player, Vehicle vehicle, sbyte _)
        {
            // Get the vehicle faction
            int vehicleFaction = Vehicles.GetVehicleById<VehicleModel>(player.Vehicle.GetData<int>(EntityData.VehicleId)).Faction;
            
            // Check if the player is driving the correct vehicle
            if (vehicleFaction != (int)PlayerJobs.Fastfood + Constants.MAX_FACTION_VEHICLES) return;

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (data.DeliverOrder == 0 && (data.LastVehicle == null || !data.LastVehicle.Exists))
            {
                player.WarpOutOfVehicle();
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_delivering_order);
                return;
            }
            
            if (data.LastVehicle != vehicle)
            {
                player.WarpOutOfVehicle();
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_your_job_vehicle);
                return;
            }
            
            if (FastFoodTimerList.TryGetValue(player.Value, out Timer fastFoodTimer))
            {
                fastFoodTimer.Dispose();
                FastFoodTimerList.Remove(player.Value);
            }
            
            // Get the order from the player
            FastfoodOrderModel order = GetFastfoodOrderFromId(player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).DeliverOrder);
            data.LastVehicle = vehicle;

            // Show the checkpoint
            player.TriggerEvent("fastFoodDestinationCheckPoint", order.position);
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void PlayerExitVehicleServerEvent(Player player, Vehicle vehicle)
        {
            // Check if the vehicle isn't destroyed
            if (vehicle == null || !vehicle.Exists) return;

            // Get the vehicle faction
            int vehicleFaction = Vehicles.GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId)).Faction;
            
            // Check if the player is driving the correct vehicle
            if (vehicleFaction != (int)PlayerJobs.Fastfood + Constants.MAX_FACTION_VEHICLES) return;
            
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Job != PlayerJobs.Fastfood) return;

            if (player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).LastVehicle != vehicle) return;
            
            // Timer with the time left to get into the vehicle
            Timer fastFoodTimer = new Timer(OnFastFoodTimer, player, 60000, Timeout.Infinite);
            FastFoodTimerList.Add(player.Value, fastFoodTimer);
                
            // Tell the player he left the vehicle
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.job_vehicle_left, 60));
        }

        [RemoteEvent("takeFastFoodOrder")]
        public void TakeFastFoodOrderRemoteEvent(Player player, int orderId)
        {
            // Get the order with the given identifier
            FastfoodOrderModel order = GetFastfoodOrderFromId(orderId);
            
            if (order == null)
            {
                // Order has been deleted
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.order_timeout);
                return;
            }
            
            if (order.taken)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.order_taken);
                return;
            }
        
            // Get the time to reach the destination
            int start = UtilityFunctions.GetTotalSeconds();
            int time = (int)Math.Round(player.Position.DistanceTo(order.position) / 9.5f);

            // We take the order
            order.taken = true;

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);
            data.DeliverOrder = orderId;
            data.DeliverStart = start;
            data.DeliverTime = time;

            // Information message sent to the player
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.deliver_order, time));
        }

        [RemoteEvent("fastfoodCheckpointReached")]
        public void FastfoodCheckpointReachedRemoteEvent(Player player)
        {
            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);
                
            if (data.DeliverStart > 0)
            {
                if (player.IsInVehicle)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.deliver_in_vehicle);
                    return;
                }
                    
                // Get the spawn position
                Vector3 vehiclePosition = Vehicles.GetVehicleById<VehicleModel>(data.LastVehicle.GetData<int>(EntityData.VehicleId)).Position;

                int elapsed = UtilityFunctions.GetTotalSeconds() - data.DeliverStart;
                int extra = (int)Math.Round((data.DeliverTime - elapsed) / 2.0f);
                int amount = GetFastFoodOrderAmount(player) + extra;

                data.DeliverStart = 0;
                data.JobWon = amount > 0 ? amount : Constants.MinimumFastfoodWon;

                player.TriggerEvent("fastFoodDeliverBack", vehiclePosition);

                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.deliver_completed);
            }
            else
            {                
                if (player.VehicleSeat != (int)VehicleSeat.Driver || player.Vehicle != data.LastVehicle)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_your_job_vehicle);
                    return;
                }

                // Remove the order from the list
                FastFoodOrderCollection.Remove(GetFastfoodOrderFromId(data.DeliverOrder));

                player.WarpOutOfVehicle();
                Money.GivePlayerMoney(player, data.JobWon, out string _);

                player.TriggerEvent("fastFoodDeliverFinished");
                
                // Warn the player about the earnings
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.job_won, data.JobWon));

                // Respawn the job vehicle
                Vehicles.RespawnVehicle(data.LastVehicle);

                // Reset the data from the order
                data.DeliverOrder = 0;
                data.LastVehicle = null;
                data.JobWon = 0;
            }
        }
    }
}
