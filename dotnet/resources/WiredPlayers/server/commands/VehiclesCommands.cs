using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WiredPlayers.character;
using WiredPlayers.chat;
using WiredPlayers.Currency;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.factions;
using WiredPlayers.Utility;
using WiredPlayers.messages.arguments;
using WiredPlayers.messages.error;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using WiredPlayers.messages.success;
using WiredPlayers.vehicles;
using WiredPlayers.weapons;
using WiredPlayers.Buildings.Businesses;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class VehiclesCommands
    {
        [Command]
        public static void SeatbeltCommand(Player player)
        {
            if (!player.IsInVehicle)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle);
                return;
            }

            // Change the seatbelt state
            player.TriggerEvent("toggleSeatbelt");
        }

        [Command]
        public static void LockCommand(Player player)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get the closest vehicle
            Vehicle vehicle = Vehicles.GetClosestVehicle(player, 2.5f);

            if (vehicle == null || !vehicle.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_vehicles_near);
                return;
            }

            if (!Vehicles.HasPlayerVehicleKeys(player, vehicle, false))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
                return;
            }

            if (player.Vehicle.Class == (int)VehicleClass.Cycle || player.Vehicle.Class == (int)VehicleClass.Motorcycle || player.Vehicle.Class == (int)VehicleClass.Boat)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_lockable);
                return;
            }

            vehicle.Locked = !vehicle.Locked;
            Chat.SendMessageToNearbyPlayers(player, vehicle.Locked ? SuccRes.veh_locked : SuccRes.veh_unlocked, ChatTypes.Me, 20.0f);
        }

        [Command]
        public static void HoodCommand(Player player)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get the closest vehicle
            Vehicle vehicle = Vehicles.GetClosestVehicle(player, 3.75f);

            if (vehicle == null || !vehicle.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_vehicles_near);
                return;
            }

            if (!Vehicles.HasPlayerVehicleKeys(player, vehicle, false) && player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Job != PlayerJobs.Mechanic)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
                return;
            }

            // Get the status of the doors
            int door = (int)Enumerators.VehicleDoor.Hood;
            List<bool> doorState = NAPI.Util.FromJson<List<bool>>(vehicle.GetSharedData<string>(EntityData.VehicleDoorsState));

            doorState[door] = !doorState[door];
            vehicle.SetSharedData(EntityData.VehicleDoorsState, NAPI.Util.ToJson(doorState));

            player.SendChatMessage(Constants.COLOR_INFO + (doorState[door] ? InfoRes.hood_opened : InfoRes.hood_closed));

            player.TriggerEvent("toggleVehicleDoor", vehicle.Value, door, doorState[door]);
        }

        [Command]
        public static async void TrunkCommand(Player player, string action)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get closest vehicle
            Vehicle vehicle = Vehicles.GetClosestVehicle(player, 3.75f);

            if (vehicle == null || !vehicle.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_vehicles_near);
                return;
            }

            // Get the state of the doors
            List<bool> doorState = NAPI.Util.FromJson<List<bool>>(vehicle.GetSharedData<string>(EntityData.VehicleDoorsState));

            if (action.Equals(ArgRes.open, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!Vehicles.HasPlayerVehicleKeys(player, vehicle, false))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
                    return;
                }

                // Get the selected
                int door = (int)VehicleDoor.Trunk;

                if (doorState[door])
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_trunk_opened);
                    return;
                }

                doorState[door] = !doorState[door];
                vehicle.SetSharedData(EntityData.VehicleDoorsState, NAPI.Util.ToJson(doorState));

                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.trunk_opened);

                player.TriggerEvent("toggleVehicleDoor", vehicle.Value, door, doorState[door]);

                return;
            }

            if (action.Equals(ArgRes.open, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!Vehicles.HasPlayerVehicleKeys(player, vehicle, false))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
                    return;
                }

                // Get the selected
                int door = (int)VehicleDoor.Trunk;

                if (!doorState[door])
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_trunk_closed);
                    return;
                }

                doorState[door] = !doorState[door];
                vehicle.SetSharedData(EntityData.VehicleDoorsState, NAPI.Util.ToJson(doorState));

                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.trunk_closed);

                player.TriggerEvent("toggleVehicleDoor", vehicle.Value, door, doorState[door]);

                return;
            }

            if (action.Equals(ArgRes.store, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!doorState[(int)VehicleDoor.Trunk])
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_trunk_closed);
                    return;
                }

                if (Vehicles.IsVehicleTrunkInUse(vehicle))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_trunk_in_use);
                    return;
                }

                if (player.HasData(EntityData.PlayerWeaponCrate))
                {
                    // Store the weapon crate into the trunk
                    Weapons.StoreCrateIntoTrunk(player, vehicle);
                }
                else if (Inventory.HasPlayerItemOnHand(player))
                {
                    // Initialize the item
                    ItemModel item;

                    // Get the identifier of the vehicle
                    int vehicleId = vehicle.GetData<int>(EntityData.VehicleId);

                    // Check if the item is a weapon or not
                    if (player.HasSharedData(EntityData.PlayerRightHand))
                    {
                        string rightHand = player.GetSharedData<string>(EntityData.PlayerRightHand);
                        item = Inventory.GetItemModelFromId(NAPI.Util.FromJson<AttachmentModel>(rightHand).itemId);
                    }
                    else
                    {
                        item = Weapons.GetWeaponItem(player, player.CurrentWeapon);
                    }

                    if (NAPI.Util.WeaponNameToModel(item.hash) != 0 || item.hash == Constants.ITEM_HASH_TELEPHONE)
                    {
                        // We store the item on the trunk
                        item.ownerIdentifier = vehicleId;
                        item.ownerEntity = Constants.ITEM_ENTITY_VEHICLE;

                        if (player.CurrentWeapon != WeaponHash.Unarmed)
                        {
                            // Remove the weapon
                            player.RemoveWeapon(player.CurrentWeapon);
                            player.GiveWeapon(WeaponHash.Unarmed, 0);
                        }
                        else
                        {
                            // Remove the item on the hand
                            NAPI.ClientEvent.TriggerClientEventInDimension(player.Dimension, "dettachItemFromPlayer", player.Value);

                            // Remove the item from the hand
                            player.ResetSharedData(EntityData.PlayerRightHand);
                        }
                    }
                    else
                    {
                        // Check for items on the trunk
                        ItemModel trunkItem = Inventory.ItemCollection.Values.FirstOrDefault(i => i.ownerEntity == Constants.ITEM_ENTITY_VEHICLE && i.ownerIdentifier == vehicleId);

                        if (trunkItem == null)
                        {
                            // Create the new item
                            trunkItem = new ItemModel()
                            {
                                hash = item.hash,
                                ownerIdentifier = vehicleId,
                                ownerEntity = Constants.ITEM_ENTITY_VEHICLE,
                                position = new Vector3(),
                                dimension = 0,
                                amount = 1
                            };

                            trunkItem.id = await DatabaseOperations.AddNewItem(trunkItem).ConfigureAwait(false);
                            Inventory.ItemCollection.Add(trunkItem.id, trunkItem);
                        }
                        else
                        {
                            // Change the amount for the item
                            trunkItem.amount++;

                            // Update item into database
                            await Task.Run(() => DatabaseOperations.UpdateItem(trunkItem)).ConfigureAwait(false);
                        }

                        // Substract one to the item
                        item.amount--;

                        if (item.amount == 0)
                        {
                            // Remove the item on the hand
                            NAPI.ClientEvent.TriggerClientEventInDimension(player.Dimension, "dettachItemFromPlayer", player.Value);

                            // Remove the item from the hand
                            player.ResetSharedData(EntityData.PlayerRightHand);

                            // Remove the item from the database and list
                            await Task.Run(() => DatabaseOperations.DeleteSingleRow("items", "id", item.id)).ConfigureAwait(false);
                            Inventory.ItemCollection.Remove(item.id);
                        }
                        else
                        {
                            await Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);
                        }
                    }

                    // Send the message to the player
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.trunk_stored_items);
                }
                else
                {
                    // Get player's inventory
                    List<InventoryModel> inventory = Inventory.GetEntityInventory(player, true);

                    if (inventory.Count == 0)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_items_inventory);
                        return;
                    }

                    player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).OpenedTrunk = vehicle;
                    player.TriggerEvent("showPlayerInventory", inventory, InventoryTarget.VehiclePlayer);
                }
                return;
            }

            if (action.Equals(ArgRes.withdraw, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!doorState[(int)VehicleDoor.Trunk])
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_trunk_closed);
                    return;
                }

                if (Vehicles.IsVehicleTrunkInUse(vehicle))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_trunk_in_use);
                    return;
                }

                // Load items into the trunk
                List<InventoryModel> inventory = Inventory.GetEntityInventory(vehicle, true);

                if (inventory.Count == 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_items_trunk);
                    return;
                }

                player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).OpenedTrunk = vehicle;
                player.TriggerEvent("showPlayerInventory", NAPI.Util.ToJson(inventory), InventoryTarget.Trunk);

                return;
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.trunk);
        }

        [Command]
        public static void KeysCommand(Player player, string action, int vehicleId, string targetString = "")
        {
            // Get the player's character model
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // Get lent keys
            string[] playerKeysArray = characterModel.VehicleKeys.Split(',');

            if (action.Equals(ArgRes.lend, StringComparison.InvariantCultureIgnoreCase))
            {
                VehicleModel vehicle = Vehicles.GetVehicleById<VehicleModel>(vehicleId);

                if (vehicle == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_exists);
                    return;
                }

                if (!Vehicles.HasPlayerVehicleKeys(player, vehicle, true))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
                    return;
                }

                if (targetString.Length == 0)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.keys);
                    return;
                }

                Player target = UtilityFunctions.GetPlayer(targetString);

                if (target == null || target.Position.DistanceTo(player.Position) < 5.0f)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                    return;
                }

                // Get the target's character model
                CharacterModel targetCharacterModel = target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

                // Get the vehicle keys
                string[] targetKeysArray = targetCharacterModel.VehicleKeys.Split(',');

                for (int i = 0; i < targetKeysArray.Length; i++)
                {
                    if (int.Parse(targetKeysArray[i]) == 0)
                    {
                        targetKeysArray[i] = vehicleId.ToString();
                        targetCharacterModel.VehicleKeys = string.Join(",", targetKeysArray);

                        player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.vehicle_keys_given, target.Name));
                        target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.vehicle_keys_received, player.Name));

                        return;
                    }
                }

                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_keys_full);

                return;
            }

            if (action.Equals(ArgRes.drop, StringComparison.InvariantCultureIgnoreCase))
            {
                for (int i = 0; i < playerKeysArray.Length; i++)
                {
                    if (playerKeysArray[i] == vehicleId.ToString())
                    {
                        playerKeysArray[i] = "0";

                        Array.Sort(playerKeysArray);
                        Array.Reverse(playerKeysArray);

                        characterModel.VehicleKeys = string.Join(',', playerKeysArray);

                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.vehicle_keys_thrown);
                        return;
                    }
                }

                // Send a message telling that keys are not found
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.keys);
        }

        [Command]
        public static void LocateCommand(Player player, int vehicleId)
        {
            VehicleModel vehicle = Vehicles.GetVehicleById<VehicleModel>(vehicleId);

            if (vehicle == null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_exists);
                return;
            }
            
            if (!Vehicles.HasPlayerVehicleKeys(player, vehicle, true))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
                return;
            }
            
            if (vehicle.Parking == 0)
            {
                // Set the checkpoint into the zone
                player.TriggerEvent("locateVehicle", vehicle.Position);
            }
            else
            {
                // Get the parking where the vehicle is located at
                ParkingModel parking = Parking.GetParkingById(vehicle.Parking);

                // Set the checkpoint into the zone
                player.TriggerEvent("locateVehicle", parking.Position);
            }
            
            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.vehicle_parked);
        }

        [Command]
        public static void RefuelCommand(Player player, int amount)
        {
            foreach (BusinessModel business in Business.BusinessCollection.Values)
            {
                if ((BusinessTypes)business.Ipl.Type == BusinessTypes.GasStation && player.Position.DistanceTo(business.Entrance) < 20.5f)
                {
                    Vehicle vehicle = Vehicles.GetClosestVehicle(player, 2.5f);

                    if (vehicle == null || !vehicle.Exists)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_vehicles_near);
                        return;
                    }

                    if (player.Vehicle != null)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_refuel_into_vehicle);
                        return;
                    }

                    if (!Vehicles.HasPlayerVehicleKeys(player, vehicle, false))
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
                        return;
                    }

                    if (vehicle.HasData(EntityData.VehicleRefueling))
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_refueling);
                        return;
                    }

                    // Get the player's external data
                    PlayerTemporaryModel playerModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

                    if (playerModel.Refueling != null && playerModel.Refueling.Exists)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_refueling);
                        return;
                    }

                    if (vehicle.EngineStatus)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.engine_on);
                        return;
                    }
                    
                    // Get the vehicle from the identifier
                    VehicleModel vehModel = Vehicles.GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId));
                    
                    if (vehModel.Gas == 50.0f)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_tank_full);
                        return;
                    }

                    float gasRefueled = 0.0f;
                    float gasLeft = 50.0f - vehModel.Gas;
                    int maxMoney = (int)Math.Ceiling(gasLeft * (int)Prices.Gas * business.Multiplier);

                    if (amount == 0 || amount > maxMoney)
                    {
                        amount = maxMoney;
                        gasRefueled = gasLeft;
                    }
                    else if (amount > 0)
                    {
                        gasRefueled = amount / ((int)Prices.Gas * business.Multiplier);
                    }

                    if (!Money.SubstractPlayerMoney(player, amount, out string error))
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + error);
                        return;
                    }

                    // Add gas to the vehicle
                    vehModel.Gas += gasRefueled;

                    playerModel.Refueling = vehicle;
                    vehicle.SetData(EntityData.VehicleRefueling, player);

                    // Timer called when vehicle's refueled
                    Timer gasTimer = new Timer(Vehicles.OnVehicleRefueled, vehicle, (int)Math.Round(gasLeft * 1000), Timeout.Infinite);
                    Vehicles.gasTimerList.Add(player.Value, gasTimer);

                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.vehicle_refueling);

                    return;
                }
            }

            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_fuel_station_near);
        }

        [Command]
        public static void FillCommand(Player player)
        {
            if (!player.HasSharedData(EntityData.PlayerRightHand))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.right_hand_empty);
                return;
            }

            string rightHand = player.GetSharedData<string>(EntityData.PlayerRightHand);
            int itemId = NAPI.Util.FromJson<AttachmentModel>(rightHand).itemId;
            ItemModel item = Inventory.GetItemModelFromId(itemId);

            if (item == null || item.hash != Constants.ITEM_HASH_JERRYCAN)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_jerrycan);
                return;
            }

            // Get the closest vehicle to the player
            Vehicle vehicle = Vehicles.GetClosestVehicle(player, 2.5f);

            if (vehicle == null || !vehicle.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_vehicles_near);
                return;
            }

            if (!Vehicles.HasPlayerVehicleKeys(player, vehicle, false) && player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Job != PlayerJobs.Mechanic)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
                return;
            }

            // Update the vehicle gasoline
            VehicleModel vehModel = Vehicles.GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId));
            vehModel.Gas = (vehModel.Gas + Constants.GAS_CAN_LITRES > 50.0f ? 50.0f : vehModel.Gas + Constants.GAS_CAN_LITRES);

            // Remove the item from the hand
            NAPI.ClientEvent.TriggerClientEventInDimension(player.Dimension, "dettachItemFromPlayer", player.Value);
            player.ResetSharedData(EntityData.PlayerRightHand);

            // Remove the item
            Task.Run(() => DatabaseOperations.DeleteSingleRow("items", "id", item.id)).ConfigureAwait(false);
            Inventory.ItemCollection.Remove(item.id);

            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.vehicle_refilled);
        }

        [Command]
        public static void ScrapCommand(Player player)
        {
            if (player.VehicleSeat != (int)VehicleSeat.Driver)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_vehicle_driving);
                return;
            }
            
            // Get the vehicle data
            VehicleModel vehicle = Vehicles.GetVehicleById<VehicleModel>(player.Vehicle.GetData<int>(EntityData.VehicleId));

            if (vehicle.Owner != player.Name)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_veh_owner);
                return;
            }

            // Get the closest parking to the player
            ParkingModel parking = Parking.GetClosestParking(player, 3.5f);

            if (parking == null && parking.Type != ParkingTypes.Scrapyard)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_scrapyard_near);
                return;
            }

            // Calculate amount won
            int vehicleMaxValue = (int)Math.Round(vehicle.Price * 0.5f);
            int vehicleMinValue = (int)Math.Round(vehicle.Price * 0.1f);
            float vehicleReduction = vehicle.Kms / Constants.REDUCTION_PER_KMS / 1000;
            int amountGiven = vehicleMaxValue - (int)Math.Round(vehicleReduction / 100 * vehicleMaxValue);

            if (amountGiven < vehicleMinValue)
            {
                amountGiven = vehicleMinValue;
            }

            // Payment to the player
            Money.GivePlayerMoney(player, amountGiven, out string error);


            // Delete the vehicle
            Task.Run(() => DatabaseOperations.DeleteSingleRow("vehicles", "id", vehicle.Id)).ConfigureAwait(false);
            Vehicles.IngameVehicles.Remove(vehicle.Id);
            player.Vehicle.Delete();

            player.SendChatMessage(Constants.COLOR_SUCCESS + string.Format(SuccRes.vehicle_scrapyard, amountGiven));
        }
    }
}
