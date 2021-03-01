using GTANetworkAPI;
using System.Threading;
using System.Threading.Tasks;
using WiredPlayers.factions;
using WiredPlayers.character;
using WiredPlayers.Currency;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Utility;
using WiredPlayers.jobs;
using WiredPlayers.messages.error;
using WiredPlayers.messages.information;
using WiredPlayers.vehicles;
using WiredPlayers.Buildings.Houses;
using WiredPlayers.Buildings;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;
using System.Collections.Generic;

namespace WiredPlayers.Server.Commands
{
    public static class ThiefCommands
    {
        [Command]
        public static void ForceCommand(Player player)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }
            
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Job != PlayerJobs.Thief)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_thief);
                return;
            }

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (data.Lockpicking != null && data.Lockpicking.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.already_lockpicking);
                return;
            }
            
            // Get the closest vehicle
            Vehicle vehicle = Vehicles.GetClosestVehicle(player, 2.5f);
            
            if (vehicle == null || !vehicle.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_vehicles_near);
                return;
            }

            if (Vehicles.HasPlayerVehicleKeys(player, vehicle, false))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_cant_lockpick_own_vehicle);
                return;
            }

            if (!vehicle.Locked)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.veh_already_unlocked);
                return;
            }

            // Generate police report
            Thief.GeneratePoliceRobberyWarning(player);

            data.Lockpicking = vehicle;
            data.Animation = true;
            player.PlayAnimation("missheistfbisetup1", "hassle_intro_loop_f", (int)AnimationFlags.Loop);

            // Timer to finish forcing the door
            Timer robberyTimer = new Timer(Thief.OnLockpickTimer, player, 10000, Timeout.Infinite);
            Thief.RobberyTimerList.Add(player.Value, robberyTimer);
        }

        [Command]
        public static void StealCommand(Player player)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get the character and temporary models for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);
            PlayerTemporaryModel playerModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (characterModel.Job != PlayerJobs.Thief)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_thief);
                return;
            }

            if (playerModel.RobberyStart > 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_stealing);
                return;
            }

            if (characterModel.JobCooldown > 0)
            {
                int timeLeft = characterModel.JobCooldown - UtilityFunctions.GetTotalSeconds();
                player.SendChatMessage(Constants.COLOR_ERROR + string.Format(ErrRes.player_cooldown_thief, timeLeft));
                return;
            }

            if (BuildingHandler.IsIntoBuilding(player))
            {
                // Check if it's his own house
				BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;
                
                if (building.Type == BuildingTypes.House && House.HasPlayerHouseKeys(player, House.GetHouseById(building.Id)))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_cant_rob_own_house);
                    return;
                }

                // Generate the police report
                Thief.GeneratePoliceRobberyWarning(player);

                // Start stealing items
                player.PlayAnimation("misscarstealfinalecar_5_ig_3", "crouchloop", (int)AnimationFlags.Loop);

                playerModel.RobberyStart = UtilityFunctions.GetTotalSeconds();
                playerModel.Animation = true;

                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.searching_value_items);

                // Timer to finish the robbery
                Timer robberyTimer = new Timer(Thief.OnPlayerRob, player, 20000, Timeout.Infinite);
                Thief.RobberyTimerList.Add(player.Value, robberyTimer);
            }
            else if (player.VehicleSeat == (int)VehicleSeat.Driver)
            {
                if (Vehicles.HasPlayerVehicleKeys(player, player.Vehicle, false))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_cant_rob_own_vehicle);
                    return;
                }

                if (player.Vehicle.EngineStatus)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.engine_on);
                    return;
                }

                // Generate the police report
                Thief.GeneratePoliceRobberyWarning(player);

                // Start stealing items
                player.PlayAnimation("veh@plane@cuban@front@ds@base", "hotwire", (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl));

                playerModel.RobberyStart = UtilityFunctions.GetTotalSeconds();
                playerModel.Animation = true;

                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.searching_value_items);

                // Timer to finish the robbery
                Timer robberyTimer = new Timer(Thief.OnPlayerRob, player, 35000, Timeout.Infinite);
                Thief.RobberyTimerList.Add(player.Value, robberyTimer);
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_cant_rob);
            }
        }

        [Command]
        public static void HotwireCommand(Player player)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Job != PlayerJobs.Thief)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_thief);
                return;
            }

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (data.Hotwiring != null && data.Hotwiring.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_hotwiring);
                return;
            }

            if (player.VehicleSeat != (int)VehicleSeat.Driver)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_vehicle_driving);
                return;
            }
            
            if (Vehicles.HasPlayerVehicleKeys(player, player.Vehicle, false))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_cant_hotwire_own_vehicle);
                return;
            }
            
            if (player.Vehicle.EngineStatus)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.engine_already_started);
                return;
            }
            
            int vehicleId = player.Vehicle.GetData<int>(EntityData.VehicleId);
            Vector3 position = player.Vehicle.Position;

            player.PlayAnimation("veh@plane@cuban@front@ds@base", "hotwire", (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl));

            data.Animation = true;
            data.Hotwiring = player.Vehicle;

            // Create timer to finish the hotwire
            Timer robberyTimer = new Timer(Thief.OnHotwireTimer, player, 15000, Timeout.Infinite);
            Thief.RobberyTimerList.Add(player.Value, robberyTimer);
            
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.hotwire_started);

            // Add hotwire log to the database
            Task.Run(() => DatabaseOperations.LogHotwire(player.Name, vehicleId, position)).ConfigureAwait(false);
        }

        [Command]
        public static void PawnCommand(Player player)
        {
            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);
            
            foreach (Vector3 pawnShop in Coordinates.PawnShops)
            {
                if (player.Position.DistanceTo(pawnShop) < 1.5f)
                {
                    // Get player's inventory
                    List<InventoryModel> inventory = Inventory.GetEntityInventory(player, true);

                    if (inventory.Count == 0)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_items_inventory);
                        return;
                    }

                    // Show the inventory
                    player.TriggerEvent("showPlayerInventory", inventory, InventoryTarget.PawnShop);

                    return;
                }
            }
            
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_pawn_show);
        }
    }
}
