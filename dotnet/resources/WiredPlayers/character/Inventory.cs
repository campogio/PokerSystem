using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.Buildings.Businesses;
using WiredPlayers.chat;
using WiredPlayers.Currency;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.arguments;
using WiredPlayers.messages.error;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using WiredPlayers.weapons;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.character
{
    public class Inventory : Script
    {
        public static Dictionary<int, ItemModel> ItemCollection;
        
        public static void GenerateGroundItems()
        {
            // Get the objects on the ground
            ItemModel[] groundItems = ItemCollection.Values.Where(it => it.ownerEntity == Constants.ITEM_ENTITY_GROUND).ToArray();

            foreach (ItemModel item in groundItems)
            {
                // Get the numeric hash from the object
                uint hash = NAPI.Util.GetHashKey(item.hash);

                if (Enum.IsDefined(typeof(WeaponHash), hash))
                {
                    // It's a weapon, we need to get the prop's hash
                    hash = NAPI.Util.GetHashKey(Constants.WeaponItemModels[(WeaponHash)hash]);
                }

                // Create each of the items on the ground
                item.objectHandle = NAPI.Object.CreateObject(hash, item.position, new Vector3(), 255, item.dimension);
            }
        }

        public static ItemModel GetPlayerItemModelFromHash(int playerId, string hash)
        {
            // Get the item given the hash
            return ItemCollection.Values.FirstOrDefault(i => i.ownerEntity == Constants.ITEM_ENTITY_PLAYER && i.ownerIdentifier == playerId && i.hash == hash);
        }

        public static ItemModel GetClosestItem(Player player, string hash = "")
        {
            // Get the closest item to the player
            return ItemCollection.Values.FirstOrDefault(i => i.ownerEntity == Constants.ITEM_ENTITY_GROUND && (i.hash == hash || hash == string.Empty) && player.Position.DistanceTo(i.position) < 2.0f);
        }

        public static ItemModel GetItemInEntity(int entityId, string entity)
        {
            // Get the item in the identity given the id
            return ItemCollection.Values.FirstOrDefault(i => i.ownerEntity == entity && i.ownerIdentifier == entityId);
        }

        public static ItemModel GetItemModelFromId(int itemId)
        {
            return ItemCollection.ContainsKey(itemId) ? ItemCollection[itemId] : null;
        }

        public static bool HasPlayerItemOnHand(Player player)
        {
            // Check if the player has an item or weapon on the right hand
            return player.GetSharedData<string>(EntityData.PlayerRightHand) != null || player.CurrentWeapon != WeaponHash.Unarmed;
        }

        public static List<InventoryModel> GetEntityInventory(Entity entity, bool includeWeapons = false)
        {
            int entityId = 0;
            List<InventoryModel> inventory = new List<InventoryModel>();
            
            // Get the owner list for the items            
            List<string> owners = new List<string> { entity is Player ? Constants.ITEM_ENTITY_PLAYER : Constants.ITEM_ENTITY_VEHICLE };
            
            if (includeWeapons && entity is Player)
            {
                owners.Add(Constants.ITEM_ENTITY_WHEEL);
                owners.Add(Constants.ITEM_ENTITY_RIGHT_HAND);
            }
            
            if (entity is Player)
            {
                // Get the player identifier
                entityId = entity.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
            }
            else
            {
                // Get the vehicle identifier
                entityId = entity.GetData<int>(EntityData.VehicleId);
            }

            // Get the item array
            ItemModel[] itemArray = ItemCollection.Values.Where(i => owners.Contains(i.ownerEntity) && i.ownerIdentifier == entityId).ToArray();            

            foreach (ItemModel item in itemArray)
            {
                // Check whether is a common item or a weapon
                InventoryModel inventoryItem = new InventoryModel();
                BusinessItemModel businessItem = Business.GetBusinessItemFromHash(item.hash);

                if (businessItem != null)
                {
                    inventoryItem.description = businessItem.description;
                    inventoryItem.type = (int)businessItem.type;
                }
                else
                {
                    inventoryItem.description = item.hash;
                    inventoryItem.type = (int)ItemTypes.Weapon;
                }

                // Update the values
                inventoryItem.id = item.id;
                inventoryItem.hash = item.hash;
                inventoryItem.amount = item.amount;

                // Add the item to the inventory
                inventory.Add(inventoryItem);
            }

            return inventory;
        }

        public static void ConsumeItem(Player player, ItemModel item, BusinessItemModel businessItem, bool consumedFromHand)
        {
            item.amount--;

            // Check if it changes the health
            if (businessItem.health != 0 && player.Health < 100)
            {
                player.Health += businessItem.health;
                if (player.Health > 100) player.Health = 100;
            }

            if (businessItem.alcoholLevel > 0)
            {
                // Add alcohol level to the player
                PlayerTemporaryModel playerModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);
                playerModel.DrunkLevel += businessItem.alcoholLevel;

                if (playerModel.DrunkLevel > Constants.WASTED_LEVEL)
                {
                    player.SetSharedData(EntityData.PlayerWalkingStyle, "move_m@drunk@verydrunk");
                    NAPI.ClientEvent.TriggerClientEventForAll("changePlayerWalkingStyle", player.Handle, "move_m@drunk@verydrunk");
                }
            }

            if (item.amount == 0)
            {
                // Remove the item from the hand
                NAPI.ClientEvent.TriggerClientEventInDimension(player.Dimension, "dettachItemFromPlayer", player.Value);
                player.ResetSharedData(EntityData.PlayerRightHand);

                // Remove the item from the database
                Task.Run(() => DatabaseOperations.DeleteSingleRow("items", "id", item.id)).ConfigureAwait(false);
                ItemCollection.Remove(item.id);
            }
            else
            {
                // Update the amount into the database
                Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);
            }

            if (!consumedFromHand)
            {
                // Update the inventory
                UpdateInventory(player, item, businessItem, InventoryTarget.Self);
            }

            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_inventory_consume, businessItem.description.ToLower()));
        }

        public static async void DropItem(Player player, ItemModel item, BusinessItemModel businessItem, bool droppedFromHand)
        {
            // Check if there are items of the same type near
            ItemModel closestItem = GetClosestItem(player, item.hash);

            // Check if it's a weapon or not
            WeaponHash weaponHash = NAPI.Util.WeaponNameToModel(item.hash);

            // Get the dropped amount
            int amount = weaponHash != 0 ? item.amount : 1;
            item.amount -= amount;

            if (closestItem != null)
            {
                closestItem.amount += amount;

                // Update the closest item's amount
                await Task.Run(() => DatabaseOperations.UpdateItem(closestItem)).ConfigureAwait(false);
            }
            else
            {
                if (weaponHash != 0 || (weaponHash == 0 && uint.TryParse(item.hash, out uint hash)))
                {
                    // Get the hash from the item dropped
                    uint itemHash = weaponHash != 0 ? NAPI.Util.GetHashKey(Constants.WeaponItemModels[weaponHash]) : uint.Parse(item.hash);

                    closestItem = item.Copy();
                    closestItem.amount = amount;
                    closestItem.ownerEntity = Constants.ITEM_ENTITY_GROUND;
                    closestItem.dimension = player.Dimension;
                    closestItem.position = player.Position.Subtract(new Vector3(0.0f, 0.0f, 0.8f));
                    closestItem.objectHandle = NAPI.Object.CreateObject(itemHash, closestItem.position, new Vector3(), (byte)closestItem.dimension);

                    // Create the new item
                    closestItem.id = await DatabaseOperations.AddNewItem(closestItem).ConfigureAwait(false);
                    ItemCollection.Add(closestItem.id, closestItem);
                }
            }

            if (item.amount == 0)
            {
                if (droppedFromHand)
                {
                    // Remove the attachment information
                    player.ResetSharedData(EntityData.PlayerRightHand);

                    if (weaponHash != 0)
                    {
                        // Remove the weapon from the player
                        player.RemoveWeapon(weaponHash);
                    }
                    else
                    {
                        // Remove the item from the hand
                        NAPI.ClientEvent.TriggerClientEventInDimension(player.Dimension, "dettachItemFromPlayer", player.Value);
                    }
                }

                // There are no more items, we delete it
                await Task.Run(() => DatabaseOperations.DeleteSingleRow("items", "id", item.id)).ConfigureAwait(false);
                ItemCollection.Remove(item.id);
            }
            else
            {
                // Update the item's amount
                await Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);
            }

            if (!droppedFromHand)
            {
                // Update the inventory
                UpdateInventory(player, item, businessItem, InventoryTarget.Self);
            }

            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_inventory_drop, businessItem.description.ToLower()));
        }

        public static void StoreItemOnHand(Player player)
        {
            // Get the item
            ItemModel item = null;

            if (player.HasSharedData(EntityData.PlayerRightHand))
            {
                string rightHand = player.GetSharedData<string>(EntityData.PlayerRightHand);
                item = GetItemModelFromId(NAPI.Util.FromJson<AttachmentModel>(rightHand).itemId);

                // Remove the item from the hand
                NAPI.ClientEvent.TriggerClientEventInDimension(player.Dimension, "dettachItemFromPlayer", player.Value);

                // Reset the player data
                player.ResetSharedData(EntityData.PlayerRightHand);
            }
            else
            {
                item = Weapons.GetWeaponItem(player, player.CurrentWeapon);
                player.GiveWeapon(WeaponHash.Unarmed, 0);
            }

            // Search for items of the same type
            ItemModel inventoryItem = GetPlayerItemModelFromHash(player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id, item.hash);

            if (inventoryItem == null)
            {
                // Store the item on the floor
                item.ownerEntity = Constants.ITEM_ENTITY_PLAYER;

                // Update the amount into the database
                Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);
            }
            else
            {
                // Add the amount to the item in the inventory
                inventoryItem.amount += item.amount;

                // Delete the item on the hand
                Task.Run(() => DatabaseOperations.DeleteSingleRow("items", "id", item.id)).ConfigureAwait(false);
                ItemCollection.Remove(item.id);
            }
        }

        [RemoteEvent("processMenuAction")]
        public async void ProcessMenuActionRemoteEventAsync(Player player, int itemId, string action)
        {
            ItemModel item = GetItemModelFromId(itemId);
            BusinessItemModel businessItem = Business.GetBusinessItemFromHash(item.hash);

            if (action.Equals(ArgRes.consume, StringComparison.InvariantCultureIgnoreCase))
            {
                // Consume the selected item
                ConsumeItem(player, item, businessItem, false);

                return;
            }

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (action.Equals(ArgRes.open, StringComparison.InvariantCultureIgnoreCase))
            {
                switch (item.hash)
                {
                    case Constants.ITEM_HASH_PACK_BEER_AM:
                        ItemModel itemModel = GetPlayerItemModelFromHash(characterModel.Id, Constants.ITEM_HASH_BOTTLE_BEER_AM);
                        if (itemModel == null)
                        {
                            // Create the item
                            itemModel = new ItemModel()
                            {
                                hash = Constants.ITEM_HASH_BOTTLE_BEER_AM,
                                ownerEntity = Constants.ITEM_ENTITY_PLAYER,
                                ownerIdentifier = characterModel.Id,
                                amount = Constants.ITEM_OPEN_BEER_AMOUNT,
                                position = new Vector3(),
                                dimension = player.Dimension
                            };

                            // Create the new item
                            itemModel.id = await DatabaseOperations.AddNewItem(itemModel).ConfigureAwait(false);
                            ItemCollection.Add(itemModel.id, itemModel);
                        }
                        else
                        {
                            // Add the amount to the current item
                            itemModel.amount += Constants.ITEM_OPEN_BEER_AMOUNT;

                            // Update the amount into the database
                            await Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);
                        }
                        break;
                }

                // Substract container amount
                SubstractPlayerItems(item);

                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_inventory_open, businessItem.description.ToLower()));

                // Update the inventory
                UpdateInventory(player, item, businessItem, InventoryTarget.Self);

                return;
            }

            if (action.Equals(ArgRes.equip, StringComparison.InvariantCultureIgnoreCase))
            {
                if (HasPlayerItemOnHand(player))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.right_hand_occupied);
                    return;
                }

                // Set the item into the hand
                item.ownerEntity = Constants.ITEM_ENTITY_RIGHT_HAND;
                UtilityFunctions.AttachItemToPlayer(player, item.id, item.hash, "IK_R_Hand", businessItem.position, businessItem.rotation, EntityData.PlayerRightHand);

                // Store the player's current weapon
                player.GiveWeapon(WeaponHash.Unarmed, 0);

                // Update the inventory
                UpdateInventory(player, item, businessItem, InventoryTarget.Self);

                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_inventory_equip, businessItem.description.ToLower()));

                return;
            }

            if (action.Equals(ArgRes.drop, StringComparison.InvariantCultureIgnoreCase))
            {
                // Drop the item from the inventory
                DropItem(player, item, businessItem, false);

                return;
            }

            if (action.Equals(ArgRes.confiscate, StringComparison.InvariantCultureIgnoreCase))
            {
                Player target = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).SearchedTarget;

                // Transfer the item from the target to the player
                item.ownerEntity = Constants.ITEM_ENTITY_PLAYER;
                item.ownerIdentifier = characterModel.Id;

                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.police_retired_items_to, target.Name));
                target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.police_retired_items_from, player.Name));

                // Update the inventory
                UpdateInventory(player, item, businessItem, InventoryTarget.Player);

                // Update the amount into the database
                await Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);

                return;
            }

            if (action.Equals(ArgRes.store, StringComparison.InvariantCultureIgnoreCase))
            {
                Vehicle targetVehicle = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).OpenedTrunk;

                // Transfer the item from the player to the vehicle
                item.ownerEntity = Constants.ITEM_ENTITY_VEHICLE;
                item.ownerIdentifier = targetVehicle.GetData<int>(EntityData.VehicleId);

                // Remove the weapon if it's a weapon
                player.RemoveWeapon(NAPI.Util.WeaponNameToModel(item.hash));

                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.trunk_stored_items);

                // Update the inventory
                UpdateInventory(player, item, businessItem, InventoryTarget.VehiclePlayer);

                // Update the amount into the database
                await Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);

                return;
            }

            if (action.Equals(ArgRes.withdraw, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check if the player has any item on the hand
                if (HasPlayerItemOnHand(player))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.right_hand_occupied);
                    return;
                }

                Vehicle sourceVehicle = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).OpenedTrunk;

                WeaponHash weaponHash = NAPI.Util.WeaponNameToModel(item.hash);

                if (weaponHash != 0)
                {
                    // Give the weapon to the player
                    item.ownerEntity = Constants.ITEM_ENTITY_WHEEL;
                    player.GiveWeapon(weaponHash, 0);
                    player.SetWeaponAmmo(weaponHash, item.amount);
                }
                else
                {
                    // Place the item into the inventory
                    item.ownerEntity = Constants.ITEM_ENTITY_PLAYER;
                    player.GiveWeapon(WeaponHash.Unarmed, 0);
                }

                // Transfer the item from the vehicle to the player
                item.ownerIdentifier = characterModel.Id;

                Chat.SendMessageToNearbyPlayers(player, InfoRes.trunk_item_withdraw, ChatTypes.Me, 20.0f);
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.trunk_withdraw_items);

                // Update the inventory
                UpdateInventory(player, item, businessItem, InventoryTarget.Trunk);

                // Update the amount into the database
                await Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);

                return;
            }

            if (action.Equals(ArgRes.sell, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check if the item has any value
                if (businessItem.products == 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.item_not_sellable);
                    return;
                }

                // Calculate the earnings
                int wonAmount = (int)Math.Round(item.amount * businessItem.products * Constants.PawnMultiplier);
                Money.GivePlayerMoney(player, wonAmount, out string error);

                // Delete stolen items
                _ = Task.Run(() => DatabaseOperations.DeleteSingleRow("items", "id", item.id)).ConfigureAwait(false);
                ItemCollection.Remove(item.id);
                item.amount = 0;

                // Update the inventory
                UpdateInventory(player, item, businessItem, InventoryTarget.PawnShop);

                // Send the message to the player
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_pawned_items, wonAmount));

                return;
            }
        }

        [RemoteEvent("closeInventory")]
        public static void CloseInventoryRemoteEvent(Player player)
        {
            // Reset the variables related
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);
            data.OpenedTrunk = null;
            data.SearchedTarget = null;
        }

        private void SubstractPlayerItems(ItemModel item, int amount = 1)
        {
            item.amount -= amount;
            if (item.amount != 0) return;

            // Remove the item from the database
            Task.Run(() => DatabaseOperations.DeleteSingleRow("items", "id", item.id)).ConfigureAwait(false);
            ItemCollection.Remove(item.id);
        }

        private static void UpdateInventory(Player player, ItemModel item, BusinessItemModel businessItem, InventoryTarget target)
        {
            // Check if the item is a weapon
            WeaponHash weaponHash = NAPI.Util.WeaponNameToModel(item.hash);

            // Get the description and type
            ItemTypes type = weaponHash != 0 ? ItemTypes.Weapon : businessItem.type;
            string description = weaponHash != 0 ? weaponHash.ToString() : businessItem.description;

            // Create the inventory item to update
            InventoryModel inventoryItem = new InventoryModel()
            {
                id = item.id,
                hash = item.hash,
                description = description,
                type = (int)type,
                amount = item.amount
            };

            // Update the inventory
            player.TriggerEvent("updateInventory", inventoryItem, target);
        }
    }
}
