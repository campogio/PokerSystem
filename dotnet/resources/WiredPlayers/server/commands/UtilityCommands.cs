using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WiredPlayers.Buildings.Businesses;
using WiredPlayers.character;
using WiredPlayers.chat;
using WiredPlayers.Currency;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.factions;
using WiredPlayers.Utility;
using WiredPlayers.Buildings.Houses;
using WiredPlayers.jobs;
using WiredPlayers.messages.arguments;
using WiredPlayers.messages.commands;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using WiredPlayers.messages.success;
using WiredPlayers.vehicles;
using WiredPlayers.weapons;
using WiredPlayers.Buildings;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class UtilityCommands
    {        
        [Command]
        public static void StoreCommand(Player player)
        {
            if (!player.HasSharedData(EntityData.PlayerRightHand))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.right_hand_empty);
                return;
            }

            // Store the item on right hand
            Inventory.StoreItemOnHand(player);
        }

        [Command]
        public static void ConsumeCommand(Player player)
        {
            if (!player.HasSharedData(EntityData.PlayerRightHand))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.right_hand_empty);
                return;
            }

            string rightHand = player.GetSharedData<string>(EntityData.PlayerRightHand);
            int itemId = NAPI.Util.FromJson<AttachmentModel>(rightHand).itemId;
            ItemModel item = Inventory.GetItemModelFromId(itemId);
            BusinessItemModel businessItem = Business.GetBusinessItemFromHash(item.hash);

            // Check if it's consumable
            if (businessItem.type == ItemTypes.Consumable)
            {
                // Consume the item on the hand
                Inventory.ConsumeItem(player, item, businessItem, true);
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.item_not_consumable);
            }
        }

        [Command]
        public static void InventoryCommand(Player player)
        {
            // Get the player's inventory
            List<InventoryModel> inventory = Inventory.GetEntityInventory(player);
            
            if (inventory.Count == 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_items_inventory);
                return;
            }
            
            // Show the inventory
            player.TriggerEvent("showPlayerInventory", inventory, InventoryTarget.Self);
        }

        [Command]
        public static async void PurchaseCommand(Player player, int amount = 0)
        {
            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // Check if the player is inside a business
            if (characterModel.BuildingEntered.Type == BuildingTypes.Business)
            {
                BusinessModel business = Business.GetBusinessById(characterModel.BuildingEntered.Id);

                switch (business.Ipl.Type)
                {
                    case BusinessTypes.Clothes:
                        // Temporary
                        if (!Customization.IsCustomCharacter(player))
                        {
                            player.SendChatMessage(Constants.COLOR_ERROR + "No queda ropa de tu talla.");
                            return;
                        }

                        // Get the clothes on the character
                        List<ClothesModel> clothes = Customization.GetPlayerClothes(characterModel.Id);

                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.about_complements);
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.for_avoid_clipping1);
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.for_avoid_clipping2);
                        player.TriggerEvent("showClothesBusinessPurchaseMenu", business.Caption, business.Multiplier, Faction.IsPoliceMember(player));
                        break;
						
                    case BusinessTypes.BarberShop:
                        // Temporary
                        if (!Customization.IsCustomCharacter(player))
                        {
                            player.SendChatMessage(Constants.COLOR_ERROR + "La peluquería está cerrada.");
                            return;
                        }

                        // Load the players skin model
                        player.TriggerEvent("showHairdresserMenu", characterModel.Sex, NAPI.Util.ToJson(characterModel.Skin), business.Caption);
                        break;
						
                    case BusinessTypes.TattooShop:
                        // Temporary
                        if (!Customization.IsCustomCharacter(player))
                        {
                            player.SendChatMessage(Constants.COLOR_ERROR + "La tienda de tatuajes está cerrada.");
                            return;
                        }

                        // Remove player's clothes
                        Customization.RemovePlayerClothes(player, characterModel.Sex, true);

                        // Load tattoo list
                        List<TattooModel> tattooList = Customization.tattooList.Where(t => t.player == characterModel.Id).ToList();
                        player.TriggerEvent("showTattooMenu", characterModel.Sex, NAPI.Util.ToJson(tattooList), NAPI.Util.ToJson(Constants.TATTOO_LIST), business.Caption, business.Multiplier);

                        break;
						
                    default:
                        List<BusinessItemModel> businessItems = Business.GetBusinessSoldItems(Convert.ToInt32(business.Ipl.Type));

                        if (businessItems.Count > 0)
                        {
                            // Show the purchase menu
                            player.TriggerEvent("showBusinessPurchaseMenu", NAPI.Util.ToJson(businessItems), business.Caption, business.Multiplier);
                        }

                        break;
                }
            }
            else
            {
                // Get all the houses
                foreach (HouseModel house in House.HouseCollection.Values)
                {
                    if (player.Position.DistanceTo(house.Entrance) <= 1.5f && player.Dimension == house.Dimension)
                    {
                        House.BuyHouse(player, house);
                        return;
                    }
                }

                // Check if the player's in the scrapyard
                foreach (ParkingModel parking in Parking.ParkingList.Values)
                {
                    if (player.Position.DistanceTo(parking.Position) < 2.5f && parking.Type == ParkingTypes.Scrapyard)
                    {
                        if (!Money.SubstractPlayerMoney(player, amount, out string error))
                        {
                            player.SendChatMessage(Constants.COLOR_ERROR + error);
                            return;
                        }

                        ItemModel item = Inventory.GetPlayerItemModelFromHash(characterModel.Id, Constants.ITEM_HASH_BUSINESS_PRODUCTS);

                        if (item == null)
                        {
                            item = new ItemModel()
                            {
                                amount = amount,
                                dimension = 0,
                                position = new Vector3(),
                                hash = Constants.ITEM_HASH_BUSINESS_PRODUCTS,
                                ownerEntity = Constants.ITEM_ENTITY_PLAYER,
                                ownerIdentifier = characterModel.Id,
                                objectHandle = null
                            };

                            // Add the item into the database
                            item.id = await DatabaseOperations.AddNewItem(item);
                            Inventory.ItemCollection.Add(item.id, item);
                        }
                        else
                        {
                            item.amount += amount;

                            // Update the amount into the database
                            await Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);
                        }

                        player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.products_bought, amount, amount));

                        return;
                    }
                }
            }

        }

        [Command]
        public static void SellCommand(Player player, string args)
        {
            // Get all the arguments
            string[] arguments = args.Split(' ');

            if (arguments == null || arguments.Length == 0)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.sell);
                return;
            }

            // Get the action
            string action = arguments[0];
            arguments = arguments.Where(w => w != arguments[0]).ToArray();

            if (action.Equals(ArgRes.vehicle, StringComparison.InvariantCultureIgnoreCase))
            {
                if (arguments.Length < 3 || !int.TryParse(arguments[0], out int objectId))
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.sell_vehicle);
                    return;
                }

                // Remove the object identifier
                arguments = arguments.Skip(1).ToArray();

                // Get the target player
                Player target = UtilityFunctions.GetPlayer(ref arguments);

                if (target == null || target == player || player.Position.DistanceTo(target.Position) > 5.0f)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                    return;
                }

                if (arguments.Length == 0 || !int.TryParse(arguments[0], out int price))
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.sell_vehicle);
                    return;
                }

                if (price <= 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.money_amount_positive);
                    return;
                }

                // Get the vehicle given the identifier
                VehicleModel vehModel = Vehicles.GetVehicleById<VehicleModel>(objectId);

                if (vehModel == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_exists);
                    return;
                }

                if (vehModel.Owner != player.Name)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_veh_owner);
                    return;
                }

                // Get the temporary data from the target
                PlayerTemporaryModel data = target.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

                // Send the selling offer
                data.JobPartner = player;
                data.SellingPrice = price;
                data.SellingHouse = objectId;

                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.vehicle_sell, vehModel.Model, target.Name, price));
                target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.vehicle_sold, player.Name, vehModel.Model, price));

                return;
            }

            if (action.Equals(ArgRes.house, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!int.TryParse(arguments[0], out int objectId))
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.sell_house);
                    return;
                }

                // Get the house given the id
                HouseModel house = House.GetHouseById(objectId);

                if (house == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.house_not_exists);
                    return;
                }

                if (house.Owner != player.Name)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_house_owner);
                    return;
                }

                // Get all the players on an interior
                Player[] playerArray = NAPI.Pools.GetAllPlayers().Where(p => Character.IsPlaying(p) && BuildingHandler.IsIntoBuilding(p)).ToArray();

                foreach (Player target in playerArray)
                {
                    // Get the house from the player
					BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

                    if (building.Type == BuildingTypes.House && building.Id == house.Id)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.house_occupied);
                        return;
                    }
                }

                if (arguments.Length == 1)
                {
                    // Set the selling house state
                    player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).SellingHouseState = objectId;

                    int sellValue = (int)Math.Round(house.Price * Constants.HOUSE_SALE_STATE);
                    player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.house_sell_state, sellValue));
                }
                else
                {
                    // Remove the object identifier
                    arguments = arguments.Skip(1).ToArray();

                    // Get the target player
                    Player target = UtilityFunctions.GetPlayer(ref arguments);

                    if (target == null || target == player || player.Position.DistanceTo(target.Position) > 5.0f)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                        return;
                    }

                    if (arguments.Length == 0 || !int.TryParse(arguments[0], out int price) || price <= 0)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.money_amount_positive);
                        return;
                    }

                    // Get the temporary data from the target
                    PlayerTemporaryModel data = target.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

                    data.JobPartner = player;
                    data.SellingPrice = price;
                    data.SellingHouse = objectId;

                    player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.house_sell, target.Name, price));
                    target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.house_sold, player.Name, price));
                }

                return;
            }

            if (action.Equals(ArgRes.fish, StringComparison.InvariantCultureIgnoreCase))
            {
                // Get the building where the player is
				BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

                if (building.Type == BuildingTypes.Business)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_fishing_business);
                    return;
                }

                BusinessModel business = Business.GetBusinessById(building.Id);

                if (business == null || (BusinessTypes)business.Ipl.Type != BusinessTypes.Fishing)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_fishing_business);
                    return;
                }

                ItemModel fishModel = Inventory.GetPlayerItemModelFromHash(player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id, Constants.ITEM_HASH_FISH);

                if (fishModel == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_fish_sellable);
                    return;
                }

                int amount = (int)Math.Round(fishModel.amount * (int)Prices.Fish / 1000.0d);

                // Remove the item from the database
                Task.Run(() => DatabaseOperations.DeleteSingleRow("items", "id", fishModel.id)).ConfigureAwait(false);
                Inventory.ItemCollection.Remove(fishModel.id);

                Money.GivePlayerMoney(player, amount, out string error);
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.fishing_won, amount));

                return;
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.sell);
        }

        [Command]
        public static void HelpCommand(Player player)
        {
            player.SendChatMessage(Constants.COLOR_ERROR + "Command not implemented.");
            //player.TriggerEvent("helptext");
        }

        [Command]
        public static void WelcomeCommand(Player player)
        {
            player.SendChatMessage(Constants.COLOR_ERROR + "Command not implemented.");
            //player.TriggerEvent("welcomeHelp");
        }

        [Command]
        public static void ShowCommand(Player player, string args)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get all the arguments
            string[] arguments = args.Trim().Split(' ');

            if (arguments.Length != 2 && arguments.Length != 3)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.show);
                return;
            }

            // Get the target player
            Player target = UtilityFunctions.GetPlayer(ref arguments);

            if (target == null || player.Position.DistanceTo(target.Position) > 2.5f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                return;
            }

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (arguments[0].Equals(ArgRes.licenses, StringComparison.InvariantCultureIgnoreCase))
            {
                // Send the message to the players near
                Chat.SendMessageToNearbyPlayers(player, string.Format(InfoRes.licenses_show, target.Name), ChatTypes.Me, 20.0f);
                
                // Show the license to the player
                DrivingSchool.ShowDrivingLicense(player, target);

                return;
            }

            if (arguments[0].Equals(ArgRes.insurance, StringComparison.InvariantCultureIgnoreCase))
            {
                if (characterModel.Insurance == 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_medical_insurance);
                    return;
                }

                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                dateTime = dateTime.AddSeconds(characterModel.Insurance);
                
                Chat.SendMessageToNearbyPlayers(player, string.Format(InfoRes.insurance_show, target.Name), ChatTypes.Me, 20.0f);

                target.SendChatMessage(Constants.COLOR_INFO + GenRes.name + characterModel.RealName);
                target.SendChatMessage(Constants.COLOR_INFO + GenRes.age + characterModel.Age);
                target.SendChatMessage(Constants.COLOR_INFO + GenRes.sex + (characterModel.Sex == Sex.Male ? GenRes.SexMale : GenRes.SexFemale));
                target.SendChatMessage(Constants.COLOR_INFO + GenRes.expiry + dateTime.ToShortDateString());

                return;
            }

            if (arguments[0].Equals(ArgRes.insurance, StringComparison.InvariantCultureIgnoreCase))
            {
                if (characterModel.Documentation == 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_undocumented);
                    return;
                }

                Chat.SendMessageToNearbyPlayers(player, string.Format(InfoRes.identification_show, target.Name), ChatTypes.Me, 20.0f);

                target.SendChatMessage(Constants.COLOR_INFO + GenRes.name + characterModel.RealName);
                target.SendChatMessage(Constants.COLOR_INFO + GenRes.age + characterModel.Age);
                target.SendChatMessage(Constants.COLOR_INFO + GenRes.sex + (characterModel.Sex == Sex.Male ? GenRes.SexMale : GenRes.SexFemale));

                return;
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.show);
        }

        [Command]
        public static void PayCommand(Player player, string args)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get the message splitted
            string[] arguments = args.Trim().Split(' ');

            if (arguments.Length != 2 && arguments.Length != 3)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.pay);
                return;
            }

            // Initialize the target player
            Player target = UtilityFunctions.GetPlayer(ref arguments);

            if (target == null || player.Position.DistanceTo(target.Position) > 2.5f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                return;
            }

            if (target == player)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.hooker_offered_himself);
                return;
            }

            if (arguments.Length == 0 || !int.TryParse(arguments[0], out int amount) || amount < 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.money_amount_positive);
                return;
            }
            
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Money < amount)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_enough_money);
                return;
            }

            // Get the temporary data
            PlayerTemporaryModel data = target.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // Send the payment
            data.Payment = player;
            data.SellingPrice = amount;

            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.payment_offer, amount, target.Name));
            target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.payment_received, player.Name, amount));
        }

        [Command]
        public static void GiveCommand(Player player, string targetString)
        {
            if (!Inventory.HasPlayerItemOnHand(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.right_hand_empty);
                return;
            }

            Player target = UtilityFunctions.GetPlayer(targetString);

            if (target == null || player.Position.DistanceTo(target.Position) > 2.0f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                return;
            }

            if (Inventory.HasPlayerItemOnHand(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.target_right_hand_not_empty);
                return;
            }

            ItemModel item = null;
            string playerMessage = string.Empty;
            string targetMessage = string.Empty;

            if (player.HasSharedData(EntityData.PlayerRightHand))
            {
                string rightHand = player.GetSharedData<string>(EntityData.PlayerRightHand);
                item = Inventory.GetItemModelFromId(NAPI.Util.FromJson<AttachmentModel>(rightHand).itemId);

                BusinessItemModel businessItem = Business.GetBusinessItemFromHash(item.hash);
                UtilityFunctions.AttachItemToPlayer(target, item.id, item.hash, "IK_R_Hand", businessItem.position, businessItem.rotation, EntityData.PlayerRightHand);

                player.ResetSharedData(EntityData.PlayerRightHand);

                playerMessage = string.Format(InfoRes.item_given, businessItem.description.ToLower(), target.Name);
                targetMessage = string.Format(InfoRes.item_received, player.Name, businessItem.description.ToLower());
            }
            else
            {
                // Get the player weapon on the hand
                WeaponHash weaponHash = player.CurrentWeapon;
                item = Weapons.GetWeaponItem(player, weaponHash);

                target.GiveWeapon(weaponHash, 0);
                target.SetWeaponAmmo(weaponHash, item.amount);

                // Unequip the weapon from the player
                player.RemoveWeapon(weaponHash);
                player.GiveWeapon(WeaponHash.Unarmed, 0);

                playerMessage = string.Format(InfoRes.item_given, item.hash.ToLower(), target.Name);
                targetMessage = string.Format(InfoRes.item_received, player.Name, item.hash.ToLower());
            }

            // Change item's owner
            item.ownerIdentifier = target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;

            player.SendChatMessage(Constants.COLOR_INFO + playerMessage);
            target.SendChatMessage(Constants.COLOR_INFO + targetMessage);

            // Update the amount into the database
            Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);
        }

        [Command]
        public static void CancelCommand(Player player, string cancel)
        {
            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (cancel.Equals(ArgRes.interview, StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.OnAir)
                {
                    data.OnAir = false;
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.on_air_canceled);
                }

                return;
            }

            if (cancel.Equals(ArgRes.service, StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.AlreadyFucking != null && data.AlreadyFucking.Exists)
                {
                    data.AlreadyFucking = null;
                    data.JobPartner = null;
                    data.HookerService = 0;
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.hooker_service_canceled);
                }

                return;
            }

            if (cancel.Equals(ArgRes.money, StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.Payment != null && data.Payment.Exists)
                {
                    data.Payment = null;
                    data.JobPartner = null;
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.payment_canceled);
                }

                return;
            }

            if (cancel.Equals(ArgRes.order, StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.DeliverOrder > 0)
                {
                    data.DeliverOrder = 0;
                    data.JobCheckPoint = 0;
                    data.LastVehicle = null;
                    data.JobWon = 0;

                    // Remove the checkpoints
                    player.TriggerEvent("fastFoodDeliverFinished");

                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.deliverer_order_canceled);
                }

                return;
            }

            if (cancel.Equals(ArgRes.repaint, StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.Repaint != null)
                {
                    // Get the mechanic and the vehicle
                    VehicleModel vehModel = Vehicles.GetVehicleById<VehicleModel>(data.Repaint.Vehicle.GetData<int>(EntityData.VehicleId));

                    // Repaint the vehicle
                    Mechanic.RepaintVehicle(data.Repaint.Vehicle, vehModel);

                    // Remove repaint window
                    data.JobPartner.TriggerEvent("closeRepaintWindow");

                    data.JobPartner = null;
                    data.Repaint.Vehicle = null;
                    data.Repaint.ColorType = 0;
                    data.Repaint.FirstColor = string.Empty;
                    data.Repaint.SecondColor = string.Empty;
                    data.SellingPrice = 0;

                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.repaint_canceled);
                }

                return;
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.cancel);
        }

        [Command]
        public static void AcceptCommand(Player player, string accept)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (accept.Equals(ArgRes.repair, StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.RepairVehicle == null || !data.RepairVehicle.Exists)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_repair_offered);
                    return;
                }

                if (data.JobPartner == null || player == data.JobPartner && data.JobPartner.Position.DistanceTo(player.Position) > 5.0f)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                    return;
                }

                if (!Money.ExchangePlayersMoney(player, data.JobPartner, data.SellingPrice, out string playerError, out string mechanicError))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + playerError);
                    data.JobPartner.SendChatMessage(Constants.COLOR_ERROR + mechanicError);
                    return;
                }

                // Get the vehicle to repair and the broken part
                string type = player.GetData<string>(EntityData.PlayerRepairType);
                Vehicle vehicle = player.GetData<Vehicle>(EntityData.PlayerRepairVehicle);

                int mechanicId = data.JobPartner.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
                ItemModel item = Inventory.GetPlayerItemModelFromHash(mechanicId, Constants.ITEM_HASH_BUSINESS_PRODUCTS);

                // TODO: FIxed with new Vehicle API
                /*
                if (type.Equals(ArgRes.chassis, StringComparison.InvariantCultureIgnoreCase))
                {
                    vehicle.Repair();
                }
                else if (type.Equals(ArgRes.doors, StringComparison.InvariantCultureIgnoreCase))
                {
                    for (int i = 0; i < 6; i++)
                    {
                        if (vehicle.IsDoorBroken(i))
                        {
                            vehicle.FixDoor(i);
                        }
                    }
                }
                else if (type.Equals(ArgRes.tyres, StringComparison.InvariantCultureIgnoreCase))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (vehicle.IsTyrePopped(i) == true)
                        {
                            vehicle.FixTyre(i);
                        }
                    }
                }
                else if (type.Equals(ArgRes.windows, StringComparison.InvariantCultureIgnoreCase))
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (vehicle.IsWindowBroken(i) == true)
                        {
                            vehicle.FixWindow(i);
                        }
                    }
                }*/

                item.amount -= data.SellingProducts;

                if (item.amount == 0)
                {
                    // Remove the item from the database
                    Task.Run(() => DatabaseOperations.DeleteSingleRow("items", "id", item.id)).ConfigureAwait(false);
                    Inventory.ItemCollection.Remove(item.id);
                }
                else
                {
                    // Update the amount into the database
                    Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);
                }

                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.vehicle_repaired_by, data.JobPartner.Name, data.SellingPrice));
                data.JobPartner.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.vehicle_repaired_by, player.Name, data.SellingPrice));

                // Save the log into the database
                Task.Run(() => DatabaseOperations.LogPayment(player.Name, data.JobPartner.Name, ComRes.repair, data.SellingPrice)).ConfigureAwait(false);

                data.JobPartner = null;
                data.RepairVehicle = null;
                data.RepairType = string.Empty;
                data.SellingProducts = 0;
                data.SellingPrice = 0;

                return;
            }

            if (accept.Equals(ArgRes.repaint, StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.Repaint == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_repaint_offered);
                    return;
                }

                if (data.JobPartner == null || data.JobPartner == player || data.JobPartner.Position.DistanceTo(player.Position) > 5.0f)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                    return;
                }

                if (!Money.ExchangePlayersMoney(player, data.JobPartner, data.SellingPrice, out string playerError, out string mechanicError))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + playerError);
                    data.JobPartner.SendChatMessage(Constants.COLOR_ERROR + mechanicError);
                    return;
                }

                VehicleModel vehModel = Vehicles.GetVehicleById<VehicleModel>(data.Repaint.Vehicle.GetData<int>(EntityData.VehicleId));
                
                // Store the new colors
                vehModel.ColorType = data.Repaint.ColorType;
                vehModel.FirstColor = data.Repaint.FirstColor;
                vehModel.SecondColor = data.Repaint.SecondColor;
                vehModel.Pearlescent = data.Repaint.Pearlescent;
                
                // Repaint the vehicle
                Mechanic.RepaintVehicle(data.Repaint.Vehicle, vehModel);
                
                int mechanicId = data.JobPartner.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
                ItemModel item = Inventory.GetPlayerItemModelFromHash(mechanicId, Constants.ITEM_HASH_BUSINESS_PRODUCTS);
            
                // Create the dictionary with the key and values
                Dictionary<string, object> keyValues = new Dictionary<string, object>()
                {
                    { "colorType", vehModel.ColorType },
                    { "firstColor", vehModel.FirstColor },
                    { "secondColor", vehModel.SecondColor },
                    { "pearlescent", vehModel.Pearlescent }
                };

                // Update the vehicle's colors into the database
                Task.Run(() => DatabaseOperations.UpdateVehicleValues(keyValues, vehModel.Id)).ConfigureAwait(false);

                item.amount -= data.SellingProducts;

                if (item.amount == 0)
                {
                    // Remove the item from the database
                    Task.Run(() => DatabaseOperations.DeleteSingleRow("items", "id", item.id)).ConfigureAwait(false);
                    Inventory.ItemCollection.Remove(item.id);
                }
                else
                {
                    // Update the amount into the database
                    Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);
                }

                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.vehicle_repainted_by, data.JobPartner.Name, data.SellingPrice));
                data.JobPartner.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.vehicle_repainted_to, player.Name, data.SellingPrice));

                // Remove repaint menu
                data.JobPartner.TriggerEvent("closeRepaintWindow");

                // Save the log into the database
                Task.Run(() => DatabaseOperations.LogPayment(player.Name, data.JobPartner.Name, ComRes.repaint, data.SellingPrice)).ConfigureAwait(false);

                data.Repaint = null;
                data.JobPartner = null;
                data.SellingProducts = 0;
                data.SellingPrice = 0;

                return;
            }

            if (accept.Equals(ArgRes.repair, StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.HookerService == 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_service_offered);
                    return;
                }

                if (data.AlreadyFucking != null && data.AlreadyFucking.Exists)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.already_fucking);
                    return;
                }

                if (player.VehicleSeat != (int)VehicleSeat.Driver)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_vehicle_driving);
                    return;
                }

                if (player.Vehicle.EngineStatus)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.engine_on);
                    return;
                }

                if (!Character.IsPlaying(data.JobPartner))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                    return;
                }

                if (!Money.ExchangePlayersMoney(player, data.JobPartner, data.SellingPrice, out string playerError, out string targetError))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + playerError);
                    data.JobPartner.SendChatMessage(Constants.COLOR_ERROR + targetError);
                    return;
                }

                data.Animation = true;
                data.AlreadyFucking = data.JobPartner;
                data.JobPartner.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).AlreadyFucking = player;

                // Check the type of the service
                if (data.HookerService == HookerService.Oral)
                {
                    player.PlayAnimation("mini@prostitutes@sexlow_veh", "low_car_bj_loop_player", (int)AnimationFlags.Loop);
                    data.JobPartner.PlayAnimation("mini@prostitutes@sexlow_veh", "low_car_bj_loop_female", (int)AnimationFlags.Loop);

                    // Timer to finish the service
                    Timer sexTimer = new Timer(Hooker.OnSexServiceTimer, player, 120000, Timeout.Infinite);
                    Hooker.sexTimerList.Add(player.Value, sexTimer);
                }
                else
                {
                    player.PlayAnimation("mini@prostitutes@sexlow_veh", "low_car_sex_loop_player", (int)AnimationFlags.Loop);
                    data.JobPartner.PlayAnimation("mini@prostitutes@sexlow_veh", "low_car_sex_loop_female", (int)AnimationFlags.Loop);

                    // Timer to finish the service
                    Timer sexTimer = new Timer(Hooker.OnSexServiceTimer, player, 180000, Timeout.Infinite);
                    Hooker.sexTimerList.Add(player.Value, sexTimer);
                }

                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.service_paid, data.SellingPrice));
                data.JobPartner.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.service_received, data.SellingPrice));

                // Save the log into the database
                Task.Run(() => DatabaseOperations.LogPayment(player.Name, data.JobPartner.Name, GenRes.hooker, data.SellingPrice)).ConfigureAwait(false);

                // Reset the entity data
                data.SellingPrice = 0;
                data.JobPartner = null;

                return;
            }

            if (accept.Equals(ArgRes.interview, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!player.IsInVehicle)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle);
                    return;
                }

                if (player.VehicleSeat != (int)VehicleSeat.RightRear)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_in_right_rear);
                    return;
                }

                data.OnAir = true;

                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.already_on_air);
                data.JobPartner.SendChatMessage(Constants.COLOR_SUCCESS + SuccRes.interview_accepted);

                return;
            }

            if (accept.Equals(ArgRes.money, StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.Payment == null || !data.Payment.Exists) return;
                
                if (!Character.IsPlaying(data.Payment) || player.Position.DistanceTo(data.Payment.Position) > 5.0f)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                    return;
                }

                if (!Money.ExchangePlayersMoney(data.Payment, player, data.SellingPrice, out string targetError, out string playerError))
                {
                    data.Payment.SendChatMessage(Constants.COLOR_ERROR + targetError);
                    player.SendChatMessage(Constants.COLOR_ERROR + playerError);
                    return;
                }

                // Send the messages to both players
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_paid, data.Payment.Name, data.SellingPrice));
                data.Payment.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.target_paid, data.SellingPrice, player.Name));

                // Save the logs into database
                Task.Run(() => DatabaseOperations.LogPayment(data.Payment.Name, player.Name, GenRes.payment_players, data.SellingPrice)).ConfigureAwait(false);

                // Reset the entity data
                data.Payment = null;
                data.SellingPrice = 0;

                return;
            }

            if (accept.Equals(ArgRes.vehicle, StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.SellingVehicle == 0) return;

                if (!Character.IsPlaying(data.JobPartner) || player.Position.DistanceTo(data.JobPartner.Position) > 5.0f)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                    return;
                }

                if (characterModel.Bank < data.SellingPrice)
                {
                    // Send the error message to the player
                    player.SendChatMessage(Constants.COLOR_ERROR + string.Format(ErrRes.carshop_no_money, data.SellingPrice));
                }

                string vehicleModel = string.Empty;

                // Get the vehicle given the identifier
                VehicleModel vehModel = Vehicles.GetVehicleById<VehicleModel>(data.SellingVehicle);

                if (vehModel == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_exists);
                    return;
                }

                vehModel.Owner = player.Name;
                vehicleModel = NAPI.Vehicle.GetVehicleDisplayName((VehicleHash)vehModel.Model);

                characterModel.Bank -= data.SellingPrice;
                data.JobPartner.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Bank += data.SellingPrice;

                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.vehicle_buy, data.JobPartner.Name, vehicleModel, data.SellingPrice));
                data.JobPartner.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.vehicle_bought, player.Name, vehicleModel, data.SellingPrice));

                // Save the logs into database
                Task.Run(() => DatabaseOperations.LogPayment(data.JobPartner.Name, player.Name, GenRes.vehicle_sale, data.SellingPrice)).ConfigureAwait(false);

                data.SellingVehicle = 0;
                data.SellingPrice = 0;

                return;
            }

            if (accept.Equals(ArgRes.house, StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.SellingHouse == 0) return;

                if (!Character.IsPlaying(data.JobPartner) || player.Position.DistanceTo(data.JobPartner.Position) > 5.0f)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                    return;
                }

                if (characterModel.Bank < data.SellingPrice)
                {
                    // Send the error message to the player
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.house_not_money);
                }

                HouseModel house = House.GetHouseById(data.SellingHouse);

                if (house.Owner == data.JobPartner.Name)
                {
                    house.Owner = player.Name;
                    house.Tenants = 2;
                    
                    house.State = HouseState.None;
                    house.Label.Text = House.GetHouseLabelText(house);

                    characterModel.Bank -= data.SellingPrice;
                    data.JobPartner.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Bank += data.SellingPrice;

                    player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.house_buyto, data.JobPartner.Name, data.SellingPrice));
                    data.JobPartner.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.house_bought, player.Name, data.SellingPrice));

                    Task.Run(() =>
                    {
                        // Update the house
                        DatabaseOperations.KickTenantsOut(house.Id);
                        DatabaseOperations.UpdateHouse(house);

                        // Log the payment into database
                        DatabaseOperations.LogPayment(data.JobPartner.Name, player.Name, GenRes.house_sale, data.SellingPrice);

                        // Reset the data
                        data.SellingHouse = 0;
                        data.SellingPrice = 0;
                    }).ConfigureAwait(false);
                }
                else
                {
                    player.SendChatMessage(ErrRes.house_sell_generic);
                    data.JobPartner.SendChatMessage(ErrRes.house_sell_generic);
                }

                return;
            }

            if (accept.Equals(ArgRes.statehouse, StringComparison.InvariantCultureIgnoreCase))
            {
                // Set the selling house state
                int sellingHouseState = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).SellingHouseState;

                if (sellingHouseState == 0) return;

                HouseModel house = House.GetHouseById(sellingHouseState);
                int amount = (int)Math.Round(house.Price * Constants.HOUSE_SALE_STATE);

                if (house.Owner == player.Name)
                {
                    house.Locked = true;
                    house.Owner = string.Empty;
                    house.State = HouseState.Buyable;
                    house.Label.Text = House.GetHouseLabelText(house);
                    house.Tenants = 2;

                    // Update the bank money
                    characterModel.Bank += amount;
                    player.SendChatMessage(Constants.COLOR_SUCCESS + string.Format(SuccRes.house_sold, amount));

                    Task.Run(() =>
                    {
                        // Update the house
                        DatabaseOperations.KickTenantsOut(house.Id);
                        DatabaseOperations.UpdateHouse(house);

                        // Log the payment into the database
                        DatabaseOperations.LogPayment(player.Name, GenRes.state, GenRes.house_sale, amount);
                    }).ConfigureAwait(false);
                }
                else
                {
                    player.SendChatMessage(ErrRes.house_sell_generic);
                }

                return;
            }

            player.SendChatMessage(Constants.COLOR_ERROR + HelpRes.accept);
        }

        [Command]
        public static void PickUpCommand(Player player)
        {
            if (Inventory.HasPlayerItemOnHand(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.right_hand_occupied);
            }
            else if (player.HasSharedData(EntityData.PlayerWeaponCrate))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.both_hand_occupied);
            }
            else
            {
                ItemModel item = Inventory.GetClosestItem(player);
                if (item != null)
                {
                    // Get the item on the ground
                    ItemModel playerItem = Inventory.GetPlayerItemModelFromHash(player.Value, item.hash);

                    if (playerItem != null)
                    {
                        // Add the amount to the player item
                        playerItem.amount += item.amount;

                        Task.Run(() => DatabaseOperations.DeleteSingleRow("items", "id", item.id)).ConfigureAwait(false);
                        Inventory.ItemCollection.Remove(item.id);
                    }
                    else
                    {
                        playerItem = item;
                    }

                    // Delete the item on the ground
                    item.objectHandle.Delete();

                    // Play the animation
                    player.PlayAnimation("random@domestic", "pickup_low", 0);

                    // Add the item to the player
                    BusinessItemModel businessItem = Business.GetBusinessItemFromHash(playerItem.hash);
                    uint hash = NAPI.Util.GetHashKey(playerItem.hash);

                    if (Enum.IsDefined(typeof(WeaponHash), hash))
                    {
                        // Get the weapon hash
                        WeaponHash weapon = (WeaponHash)hash;

                        // Give the weapon to the player
                        player.GiveWeapon(weapon, 0);
                        player.SetWeaponAmmo(weapon, playerItem.amount);

                        // Change the owner entity
                        playerItem.ownerEntity = Constants.ITEM_ENTITY_WHEEL;
                    }
                    else
                    {
                        // Change the owner entity
                        playerItem.ownerEntity = Constants.ITEM_ENTITY_RIGHT_HAND;

                        // Create the object on the player's right hand
                        UtilityFunctions.AttachItemToPlayer(player, playerItem.id, playerItem.hash, "IK_R_Hand", businessItem.position, businessItem.rotation, EntityData.PlayerRightHand);
                    }

                    // Get the new owner of the item
                    playerItem.ownerIdentifier = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;

                    // Update the item's owner
                    Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);

                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.player_picked_item);
                }
                else
                {
                    WeaponCrateModel weaponCrate = Weapons.GetClosestWeaponCrate(player);
                    
                    if (weaponCrate == null)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_items_near);
                        return;
                    }
                    
                    // Pick up the weapon crate
                    Weapons.PickUpCrate(player, weaponCrate);
                }
            }
        }

        [Command]
        public static void DropCommand(Player player)
        {
            if (Inventory.HasPlayerItemOnHand(player))
            {
                // Get the item on the right hand
                ItemModel item;

                if (player.HasSharedData(EntityData.PlayerRightHand))
                {
                    string rightHand = player.GetSharedData<string>(EntityData.PlayerRightHand);
                    item = Inventory.GetItemModelFromId(NAPI.Util.FromJson<AttachmentModel>(rightHand).itemId);
                }
                else
                {
                    item = Weapons.GetWeaponItem(player, player.CurrentWeapon);
                }

                // Get the business item from the hash
                BusinessItemModel businessItem = Business.GetBusinessItemFromHash(item.hash);

                // Drop the item on the hand
                Inventory.DropItem(player, item, businessItem, true);
            }
            else if (player.HasSharedData(EntityData.PlayerWeaponCrate))
            {
                WeaponCrateModel weaponCrate = Weapons.GetPlayerCarriedWeaponCrate(player.Value);

                if (weaponCrate != null)
                {
                    // Update the new position and carrier
                    weaponCrate.Position = new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 1.0f);
                    weaponCrate.CarriedEntity = string.Empty;
                    weaponCrate.CarriedIdentifier = 0;

                    // Create the object on the floor
                    weaponCrate.CrateObject = NAPI.Object.CreateObject(481432069, weaponCrate.Position, new Vector3(), 0);

                    // Dettach the object from the player
                    UtilityFunctions.RemoveItemOnHands(player);
                    player.StopAnimation();

                    // Send the message to the player
                    player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_inventory_drop, GenRes.weapon_crate));
                }
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.right_hand_empty);
            }
        }

        [Command]
        public static void DoorCommand(Player player)
        {
            // Get the player building
			BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

            // Check if the player's in his house
            foreach (HouseModel house in House.HouseCollection.Values)
            {
                if ((player.Position.DistanceTo(house.Entrance) <= 1.5f && player.Dimension == house.Dimension) || (building.Type == BuildingTypes.House && building.Id == house.Id))
                {
                    if (!House.HasPlayerHouseKeys(player, house))
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_house_owner);
                    }
                    else
                    {
                        house.Locked = !house.Locked;

                        // Update the house
                        Task.Run(() => DatabaseOperations.UpdateHouse(house)).ConfigureAwait(false);

                        player.SendChatMessage(Constants.COLOR_INFO + (house.Locked ? InfoRes.house_locked : InfoRes.house_opened));
                    }
                    return;
                }
            }

            // Check if the player's in his business
            foreach (BusinessModel business in Business.BusinessCollection.Values)
            {
                if ((player.Position.DistanceTo(business.Entrance) <= 1.5f && player.Dimension == business.Dimension) || (building.Type == BuildingTypes.Business && building.Id == business.Id))
                {
                    if (!Business.HasPlayerBusinessKeys(player, business))
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_business_owner);
                    }
                    else
                    {
                        business.Locked = !business.Locked;

                        // Update the business
                        Dictionary<int, BusinessModel> businessCollection = new Dictionary<int, BusinessModel>() { { business.Id, business } };
                        Task.Run(() => DatabaseOperations.UpdateBusinesses(businessCollection)).ConfigureAwait(false);

                        player.SendChatMessage(business.Locked ? Constants.COLOR_INFO + InfoRes.business_locked : Constants.COLOR_INFO + InfoRes.business_opened);
                    }
                    return;
                }
            }

            // He's not in any house or business
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_house_business);
        }
    }
}
