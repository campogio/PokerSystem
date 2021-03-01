using GTANetworkAPI;
using WiredPlayers.character;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Utility;
using WiredPlayers.jobs;
using WiredPlayers.messages.error;
using WiredPlayers.messages.information;
using WiredPlayers.vehicles;
using WiredPlayers.Buildings.Businesses;
using WiredPlayers.factions;
using System.Linq;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class MechanicCommands
    {
        [Command]
        public static void RepairCommand(Player player, int vehicleId, string type, int price = 0)
        {
            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Job != PlayerJobs.Mechanic)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_mechanic);
                return;
            }

            if (!characterModel.OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (!Mechanic.PlayerInValidRepairPlace(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_valid_repair_place);
                return;
            }
            
            Vehicle vehicle = Vehicles.GetVehicleById<Vehicle>(vehicleId);
            
            if (vehicle == null || !vehicle.Exists || vehicle.Position.DistanceTo(player.Position) > 5.0f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.wanted_vehicle_far);
                return;
            }
            
            int spentProducts = 0;

            // TODO: Fixed with new Vehicle API
            /*
            if (type.Equals(ArgRes.chassis, StringComparison.InvariantCultureIgnoreCase))
            {
                spentProducts = Constants.PRICE_VEHICLE_CHASSIS;
            }
            else if (type.Equals(ArgRes.doors, StringComparison.InvariantCultureIgnoreCase))
            {
                for (int i = 0; i < 6; i++)
                {
                    if (vehicle.IsDoorBroken(i) == true)
                    {
                        spentProducts += Constants.PRICE_VEHICLE_DOORS;
                    }
                }
            }
            else if (type.Equals(ArgRes.tyres, StringComparison.InvariantCultureIgnoreCase))
            {
                for (int i = 0; i < 4; i++)
                {
                    if (vehicle.IsTyrePopped(i) == true)
                    {
                        spentProducts += Constants.PRICE_VEHICLE_TYRES;
                    }
                }
            }
            else if (type.Equals(ArgRes.windows, StringComparison.InvariantCultureIgnoreCase))
            {
                for (int i = 0; i < 4; i++)
                {
                    if (vehicle.IsWindowBroken(i) == true)
                    {
                        spentProducts += Constants.PRICE_VEHICLE_WINDOWS;
                    }
                }
            }
            else 
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.repair);
                return;
            }*/
            
            if (price == 0)
            {
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.repair_price, spentProducts));
                return;
            }
            
            // Get player's products
            ItemModel item = Inventory.GetPlayerItemModelFromHash(characterModel.Id, Constants.ITEM_HASH_BUSINESS_PRODUCTS);

            if (item == null || item.amount < spentProducts)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + string.Format(ErrRes.not_required_products, spentProducts));
                return;
            }
            
            // Get the players who have the keys for the vehicle
            Player[] vehicleOwners = NAPI.Pools.GetAllPlayers().Where(p => Vehicles.HasPlayerVehicleKeys(p, vehicle, false)).ToArray();

            foreach (Player target in vehicleOwners)
            {
                if (target.Position.DistanceTo(player.Position) < 4.0f)
                {
                    // Get the temporary data
                    PlayerTemporaryModel data = target.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

                    // Fill repair entity data
                    data.JobPartner = player;
                    data.RepairVehicle = vehicle;
                    data.RepairType = type;
                    data.SellingProducts = spentProducts;
                    data.SellingPrice = price;

                    player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.mechanic_repair_offer, target.Name, price));
                    target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.mechanic_repair_accept, player.Name, price));

                    return;
                }
            }

            // There's no player with the vehicle's keys near
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
        }

        [Command]
        public static void RepaintCommand(Player player, int vehicleId)
        {
            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Job != PlayerJobs.Mechanic)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_mechanic);
                return;
            }

            if (!characterModel.OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            foreach (BusinessModel business in Business.BusinessCollection.Values)
            {
                if ((BusinessTypes)business.Ipl.Type == BusinessTypes.Mechanic && player.Position.DistanceTo(business.Entrance) < 25.0f)
                {
                    // Get the vehicle by id
                    Vehicle vehicle = Vehicles.GetVehicleById<Vehicle>(vehicleId);
                    
                    if (vehicle == null || !vehicle.Exists)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_exists);
                        return;
                    }

                    player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).LastVehicle = vehicle;
                    player.TriggerEvent("showRepaintMenu");
                        
                    return;
                }
            }

            // Player is not in any workshop
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_mechanic_workshop);
        }

        [Command]
        public static void TunningCommand(Player player)
        {
            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Job != PlayerJobs.Mechanic)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_mechanic);
                return;
            }

            if (!characterModel.OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }
            
            foreach (BusinessModel business in Business.BusinessCollection.Values)
            {
                if ((BusinessTypes)business.Ipl.Type == BusinessTypes.Mechanic && player.Position.DistanceTo(business.Entrance) < 25.0f)
                {
                    if (player.IsInVehicle)
                    {
                        player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).LastVehicle = player.Vehicle;
                        player.TriggerEvent("showTunningMenu");
                    }
                    else
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle);
                    }
                    return;
                }
            }

            // Player is not in any workshop
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_mechanic_workshop);
        }
    }
}
