using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.character;
using WiredPlayers.Currency;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.error;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Buildings.Businesses
{
    public class Business : Script
    {
        public static Dictionary<int, BusinessModel> BusinessCollection;

        public static void GenerateEntrancePoints()
        {
            // Generate business elements for each business
            foreach (BusinessModel businessModel in BusinessCollection.Values) GenerateBusinessElements(businessModel);
        }
        
        public static void GenerateBusinessElements(BusinessModel businessModel)
        {
            // Create the colshape for the business
            businessModel.ColShape = NAPI.ColShape.CreateCylinderColShape(businessModel.Entrance, 2.5f, 1.0f, businessModel.Dimension);
            businessModel.ColShape.SetData(EntityData.ColShapeId, businessModel.Id);

            if (businessModel.Ipl.Name.Length == 0)
            {
                // We create the mark for each business
                businessModel.BusinessMarker = NAPI.Marker.CreateMarker(MarkerType.HorizontalSplitArrowCircle, businessModel.Entrance, new Vector3(), new Vector3(), 2.5f, new Color(244, 126, 23));
                businessModel.BusinessMarker.Dimension = businessModel.Dimension;

                // Add the message to pop the instructional button up
                businessModel.ColShape.SetData(EntityData.ColShapeType, ColShapeTypes.BusinessPurchase);
                businessModel.ColShape.SetData(EntityData.InstructionalButton, HelpRes.action_business);
            }
            else
            {
                // Add the message to pop the instructional button up
                businessModel.ColShape.SetData(EntityData.ColShapeType, ColShapeTypes.BusinessEntrance);
                businessModel.ColShape.SetData(EntityData.InstructionalButton, HelpRes.enter_business);

                // We create the entrance TextLabel for each business
                businessModel.Label = NAPI.TextLabel.CreateTextLabel(businessModel.Caption, businessModel.Entrance, 30.0f, 0.75f, 4, new Color(255, 255, 255));
                businessModel.Label.Dimension = businessModel.Dimension;

                // Create the NPC attending the shop
                IplModel interior = BuildingHandler.GetBusinessIpl(Convert.ToInt32(businessModel.Ipl.Type));
                NAPI.Ped.CreatePed(PedHash.Cletus, interior.PedPoint.Position, interior.PedPoint.Heading, (uint)businessModel.Id);
            }

            // Check if there's already a blip
            if (!Constants.BusinessBlipCollection.TryGetValue((BusinessTypes)businessModel.Ipl.Type, out uint sprite)) return;

            // Check if there's a business with that blip added already
            Blip businessBlip = NAPI.Pools.GetAllBlips().FirstOrDefault(b => b.Sprite == sprite);

            if (businessBlip == null || !businessBlip.Exists)
            {
                // Create the business blip
                businessBlip = NAPI.Blip.CreateBlip(businessModel.Entrance);
                businessBlip.Name = businessModel.Caption;
                businessBlip.Sprite = sprite;
                businessBlip.ShortRange = true;
            }
        }
        
        public static bool HandleEnterColshape(Player player)
        {
            // Check if the colshape corresponds to a business
            BusinessModel business = BusinessCollection.Values.FirstOrDefault(b => player.Position.DistanceTo(b.Entrance) <= 3.5f);

            if (business != null && business.Ipl.Name.Length == 0)
            {
                // Set the business entered
                player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered = new BuildingModel()
                {
                    Id = business.Id,
                    Type = BuildingTypes.Business
                };

                return true;
            }
            
            return false;
        }

        public static bool HandleExitColshape(Player player)
        {
            // Get the building
			BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

            if (building.Type != BuildingTypes.Business || GetBusinessById(building.Id).Ipl.Name.Length != 0) return false;

            // Set the business entered
            BuildingHandler.SetDefaultBuilding(player);

            return true;
        }

        public static BusinessModel GetBusinessById(int businessId)
        {
            // Get the business given an specific identifier
            return BusinessCollection.ContainsKey(businessId) ? BusinessCollection[businessId] : null;
        }

        public static BusinessModel GetClosestBusiness(Player player, float distance = 2.0f)
        {
            // Check if the collection is empty
            if (BusinessCollection.Count == 0) return null;

            // Get the building where the player is
			BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

            BusinessModel business = null;

            if (building.Type == BuildingTypes.Business && player.Dimension == 0)
            {
                // He's standing over a checkpoint
                business = GetBusinessById(building.Id);
            }
            else
            {
                // Get the closest business given the distance
                business = BusinessCollection.Values.OrderBy(b => player.Position.DistanceTo2D(b.Entrance)).FirstOrDefault(b => player.Position.DistanceTo2D(b.Entrance) <= distance);
            }

            return business;
        }

        public static List<BusinessItemModel> GetBusinessSoldItems(int business)
        {
            // Get the items sold in a business
            return Constants.BUSINESS_ITEM_LIST.Where(businessItem => (int)businessItem.Business == business).ToList();
        }

        public static BusinessItemModel GetBusinessItemFromName(string itemName)
        {
            // Get the item from its name
            return Constants.BUSINESS_ITEM_LIST.FirstOrDefault(businessItem => businessItem.description == itemName);
        }

        public static BusinessItemModel GetBusinessItemFromHash(string itemHash)
        {
            // Get the item from its hash
            return Constants.BUSINESS_ITEM_LIST.FirstOrDefault(businessItem => businessItem.hash == itemHash);
        }

        public static List<BusinessClothesModel> GetBusinessClothesFromSlotType(Sex sex, int type, int slot)
        {
            // Get the clothes for a sex from their slot and type
            return Constants.BUSINESS_CLOTHES_LIST.Where(clothes => clothes.type == type && (clothes.sex == sex || Sex.None == clothes.sex) && (int)Convert.ChangeType(clothes.BodyPart, clothes.BodyPart.GetTypeCode()) == slot).ToList();
        }

        public static int GetClothesProductsPrice(int id, Sex sex, int type, int slot)
        {
            // Get the products needed for the given clothes
            BusinessClothesModel clothesModel = Constants.BUSINESS_CLOTHES_LIST.FirstOrDefault(c => c.type == type && (c.sex == sex || Sex.None == c.sex) && (int)Convert.ChangeType(c.BodyPart, c.BodyPart.GetTypeCode()) == slot && c.clothesId == id);

            return clothesModel?.products ?? 0;
        }

        public static bool HasPlayerBusinessKeys(Player player, BusinessModel business)
        {
            return player.Name == business.Owner;
        }

        private List<BusinessTattooModel> GetBusinessZoneTattoos(Sex sex, int zone)
        {
            // Get the tattoos matching a body part
            return Constants.TATTOO_LIST.Where(tattoo => (int)tattoo.slot == zone && ((tattoo.maleHash.Length > 0 && sex == Sex.Male) || (tattoo.femaleHash.Length > 0 && sex == Sex.Female))).ToList();
        }

        [RemoteEvent("businessPurchaseMade")]
        public async void BusinessPurchaseMadeRemoveEventAsync(Player player, string itemHash, int amount)
        {
            // Check if the player is in any business
			BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

            if (building.Type != BuildingTypes.Business) return;

            BusinessModel business = GetBusinessById(building.Id);
            BusinessItemModel businessItem = GetBusinessItemFromHash(itemHash);

            // Get the player's character model
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if ((BusinessTypes)business.Ipl.Type == BusinessTypes.Ammunation && businessItem.type == ItemTypes.Weapon && characterModel.WeaponLicense < UtilityFunctions.GetTotalSeconds())
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.weapon_license_expired);
                return;
            }

            int hash = 0;
            int price = (int)Math.Round(businessItem.products * business.Multiplier) * amount;

            if(!Money.SubstractPlayerMoney(player, price, out string error))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + error);
                return;
            }

            string purchaseMessage = string.Format(InfoRes.business_item_purchased, price);

            // We look for the item in the inventory
            ItemModel itemModel = Inventory.GetPlayerItemModelFromHash(characterModel.Id, businessItem.hash);
            if (itemModel == null || businessItem.hash == Constants.ITEM_HASH_TELEPHONE)
            {
                // We create the purchased item
                itemModel = new ItemModel();
                {
                    itemModel.hash = businessItem.hash;
                    itemModel.ownerIdentifier = characterModel.Id;
                    itemModel.amount = businessItem.uses * amount;
                    itemModel.position = new Vector3();
                    itemModel.dimension = 0;
                }

                if (businessItem.type == ItemTypes.Weapon)
                {
                    itemModel.ownerEntity = Constants.ITEM_ENTITY_WHEEL;
                }
                else
                {
                    itemModel.ownerEntity = int.TryParse(itemModel.hash, out hash) ? Constants.ITEM_ENTITY_RIGHT_HAND : Constants.ITEM_ENTITY_PLAYER;
                }

                // Adding the item to the list and database
                itemModel.id = await DatabaseOperations.AddNewItem(itemModel);
                Inventory.ItemCollection.Add(itemModel.id, itemModel);
            }
            else
            {
                itemModel.amount += businessItem.uses * amount;

                if (int.TryParse(itemModel.hash, out hash))
                {
                    itemModel.ownerEntity = Constants.ITEM_ENTITY_RIGHT_HAND;
                }

                // Update the item's amount
                await Task.Run(() => DatabaseOperations.UpdateItem(itemModel)).ConfigureAwait(false);
            }

            // If the item has a valid hash, we give it in hand
            if (itemModel.ownerEntity == Constants.ITEM_ENTITY_RIGHT_HAND)
            {
                // Remove the previous item if there was any
                UtilityFunctions.RemoveItemOnHands(player);

                // Give the new item to the player
                UtilityFunctions.AttachItemToPlayer(player, itemModel.id, itemModel.hash, "IK_R_Hand", businessItem.position, businessItem.rotation, EntityData.PlayerRightHand);
            }
            else if (businessItem.type == ItemTypes.Weapon)
            {
                // Remove the previous item if there was any
                UtilityFunctions.RemoveItemOnHands(player);

                // We give the weapon to the player
                player.GiveWeapon(NAPI.Util.WeaponNameToModel(itemModel.hash), itemModel.amount);

                // Checking if it's been bought in the Ammu-Nation
                if ((BusinessTypes)business.Ipl.Type == BusinessTypes.Ammunation)
                {
                    // Add a registered weapon
                    DatabaseOperations.AddLicensedWeapon(itemModel.id, player.Name);
                }
            }

            if (itemModel.hash == Constants.ITEM_HASH_TELEPHONE)
            {
                PhoneModel phone = new PhoneModel();
                {
                    phone.ItemId = itemModel.id;
                    phone.Contacts = new Dictionary<int, ContactModel>();
                }

                // Generate a random number
                phone.Number = RandomPhoneNumber(100000, 999999, Telephone.phoneList.Keys);

                // Add the phone to the database
                Telephone.phoneList.Add(phone.Number, phone);
                await Task.Run(() => DatabaseOperations.AddPhoneNumber(phone, player.Name)).ConfigureAwait(false);

                // Sending the message with the new number to the player
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_phone, phone.Number));
            }

            // We substract the product and add funds to the business
            if (business.Owner != string.Empty)
            {
                business.Funds += price;
                business.Products -= businessItem.products;

                // Update the business
                Dictionary<int, BusinessModel> businessCollection = new Dictionary<int, BusinessModel>() { { business.Id, business } };
                await Task.Run(() => DatabaseOperations.UpdateBusinesses(businessCollection)).ConfigureAwait(false);
            }

            player.SendChatMessage(Constants.COLOR_INFO + purchaseMessage);
        }

        [RemoteEvent("getClothesByType")]
        public void GetClothesByTypeEvent(Player player, int type, int slot)
        {
            Sex sex = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Sex;
            List<BusinessClothesModel> clothesList = GetBusinessClothesFromSlotType(sex, type, slot);

            if (clothesList.Count > 0)
            {
                player.TriggerEvent("showTypeClothes", NAPI.Util.ToJson(clothesList));
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.business_clothes_not_available);
            }
        }

        [RemoteEvent("dressEquipedClothes")]
        public void DressEquipedClothesEvent(Player player, int type, int slot)
        {
            int playerId = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
            ClothesModel clothes = Customization.GetDressedClothesInSlot(playerId, type, slot);
            
            if (type == 0)
            {
                player.SetClothes(slot, clothes?.drawable ?? 0, clothes?.texture ?? 0);
            }
            else
            {
                player.SetAccessories(slot, clothes?.drawable ?? 255, clothes?.texture ?? 255);
            }
        }

        [RemoteEvent("ClothesItemSelected")]
        public async void ClothesItemSelectedEvent(Player player, string clothesJson, int slot)
        {
            // Check if the player is in any business
			BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

            if (building.Type != BuildingTypes.Business) return;

            BusinessClothesModel clothesModel = NAPI.Util.FromJson<BusinessClothesModel>(clothesJson);
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // Get the player's clothes
            List<ClothesModel> ownedClothes = Customization.GetPlayerClothes(characterModel.Id);

            if (ownedClothes.Any(c => c.slot == slot && c.type == clothesModel.type && c.drawable == clothesModel.clothesId && c.texture == clothesModel.texture))
            {
                // The player already has those clothes
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_owns_clothes);
                return;
            }

            // Store the data from the purchase
            ClothesModel clothes = new ClothesModel();
            {
                clothes.type = clothesModel.type;
                clothes.slot = slot;
                clothes.drawable = clothesModel.clothesId;
                clothes.texture = clothesModel.texture;
                clothes.player = characterModel.Id;
                clothes.dressed = true;
                clothes.Stored = false;
            };

            int products = GetClothesProductsPrice(clothes.drawable, characterModel.Sex, clothes.type, clothes.slot);
            BusinessModel business = GetBusinessById(building.Id);
            int price = (int)Math.Round(products * business.Multiplier);

            if (!Money.SubstractPlayerMoney(player, price, out string error))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + error);
                return;
            }

            // We substract the product and add funds to the business
            if (business.Owner != string.Empty)
            {
                business.Funds += price;
                business.Products -= products;

                // Update the business
                Dictionary<int, BusinessModel> businessCollection = new Dictionary<int, BusinessModel>() { { business.Id, business } };
                await Task.Run(() => DatabaseOperations.UpdateBusinesses(businessCollection)).ConfigureAwait(false);
            }

            // Undress previous clothes
            Customization.UndressClothes(characterModel.Id, clothes.type, clothes.slot, true);

            // Storing the clothes into database
            clothes.id = await DatabaseOperations.AddClothes(clothes);
            Customization.clothesList.Add(clothes);

            // Confirmation message sent to the player
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.business_item_purchased, price));
        }

        [RemoteEvent("changeHairStyle")]
        public void ChangeHairStyleEvent(Player player, string skinJson)
        {
            // Check if the player is in any business
			BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

            if (building.Type != BuildingTypes.Business) return;

            BusinessModel business = GetBusinessById(building.Id);
            int price = (int)Math.Round(business.Multiplier * (int)Prices.BarberShop);

            if(!Money.SubstractPlayerMoney(player, price, out string error))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + error);
                return;
            }

            // Get the character data
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // Saving the new hairstyle from the JSON
            SkinModel skinModel = NAPI.Util.FromJson<SkinModel>(skinJson);
            characterModel.Skin = skinModel;

            // We update values in the database
            Task.Run(() => DatabaseOperations.UpdateCharacterHair(characterModel.Id, skinModel)).ConfigureAwait(false);

            // We substract the product and add funds to the business
            if (business.Owner != string.Empty)
            {
                business.Funds += price;
                business.Products -= (int)Prices.BarberShop;
                
                // Update the business in the database
                Dictionary<int, BusinessModel> businessCollection = new Dictionary<int, BusinessModel>() { { business.Id, business } };
                Task.Run(() => DatabaseOperations.UpdateBusinesses(businessCollection)).ConfigureAwait(false);
            }

            // Delete the browser
            player.TriggerEvent("destroyBrowser");

            // Confirmation message sent to the player
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.haircut_purchased, price));
        }

        [RemoteEvent("loadZoneTattoos")]
        public void LoadZoneTattoosEvent(Player player, int zone)
        {
            Sex sex = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Sex;
            List<BusinessTattooModel> tattooList = GetBusinessZoneTattoos(sex, zone);

            // We update the menu with the tattoos
            player.TriggerEvent("showZoneTattoos", NAPI.Util.ToJson(tattooList));
        }

        [RemoteEvent("purchaseTattoo")]
        public void PurchaseTattooEvent(Player player, int tattooZone, int tattooIndex)
        {
            // Check if the player is in any business
			BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

            if (building.Type != BuildingTypes.Business) return;

            // Get the character model
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            BusinessModel business = GetBusinessById(building.Id);

            // Getting the tattoo and its price
            BusinessTattooModel businessTattoo = GetBusinessZoneTattoos(characterModel.Sex, tattooZone).ElementAt(tattooIndex);
            int price = (int)Math.Round(business.Multiplier * businessTattoo.price);

            if(!Money.SubstractPlayerMoney(player, price, out string error))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + error);
                return;
            }

            TattooModel tattoo = new TattooModel();
            {
                tattoo.player = characterModel.Id;
                tattoo.slot = tattooZone;
                tattoo.library = businessTattoo.library;
                tattoo.hash = characterModel.Sex == Sex.Male ? businessTattoo.maleHash : businessTattoo.femaleHash;
            }
            
            if (DatabaseOperations.AddTattoo(tattoo))
            {
                // We add the tattoo to the list
                Customization.tattooList.Add(tattoo);

                // We substract the product and add funds to the business
                if (business.Owner != string.Empty)
                {
                    business.Funds += price;
                    business.Products -= businessTattoo.price;
                    
                    // Update the business in the database
                    Dictionary<int, BusinessModel> businessCollection = new Dictionary<int, BusinessModel>() { { business.Id, business } };
                    Task.Run(() => DatabaseOperations.UpdateBusinesses(businessCollection)).ConfigureAwait(false);
                }

                // Confirmation message sent to the player
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.tattoo_purchased, price));

                // Reload client tattoo list
                player.TriggerEvent("addPurchasedTattoo", NAPI.Util.ToJson(tattoo));
            }
            else
            {
                // Player already had that tattoo
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.tattoo_duplicated);

                // Give the money back to the player
                Money.GivePlayerMoney(player, price, out _);
            }
        }
        
        private int RandomPhoneNumber(int minN, int maxN, ICollection<int> excludedNumbers)
        {
            int result = 0;
            Random random = new Random();

            // Generate a random unique number
            while (result == 0 || excludedNumbers.Contains(result)) result = random.Next(minN, maxN + 1);

            return result;
        }
    }
}
