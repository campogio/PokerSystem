using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.character;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.administration;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Buildings.Houses
{
    public class House : Script
    {
        public static Dictionary<int, HouseModel> HouseCollection;

        public static void GenerateEntrancePoints()
        {            
            foreach (HouseModel houseModel in HouseCollection.Values)
            {
                string houseLabelText = GetHouseLabelText(houseModel);
                houseModel.Label = NAPI.TextLabel.CreateTextLabel(houseLabelText, houseModel.Entrance, 20.0f, 0.75f, 4, new Color(255, 255, 255), false, houseModel.Dimension);
                
                // Create the ColShape
                houseModel.ColShape = NAPI.ColShape.CreateCylinderColShape(houseModel.Entrance, 2.5f, 1.0f);
                houseModel.ColShape.SetData(EntityData.ColShapeId, houseModel.Id);
                houseModel.ColShape.SetData(EntityData.ColShapeType, ColShapeTypes.HouseEntrance);
                houseModel.ColShape.SetData(EntityData.InstructionalButton, HelpRes.enter_house);
            }
        }

        public static HouseModel GetHouseById(int id)
        {
            // Get the house with the specified identifier
            return HouseCollection.ContainsKey(id) ? HouseCollection[id] : null;
        }

        public static HouseModel GetClosestHouse(Player player, float distance = 1.5f)
        {
            // Check if the collection is empty
            if (HouseCollection.Count == 0) return null;

            // Get the closest house given the distance
            return HouseCollection.Values.OrderBy(h => player.Position.DistanceTo2D(h.Entrance)).FirstOrDefault(h => player.Position.DistanceTo2D(h.Entrance) <= distance);
        }

        public static bool HasPlayerHouseKeys(Player player, HouseModel house)
        {
            return player.Name == house.Owner || player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).HouseRent == house.Id;
        }

        public static string GetHouseLabelText(HouseModel house)
        {
            string label = string.Empty;

            switch (house.State)
            {
                case HouseState.None:
                    label = house.Caption + "\n" + GenRes.state_occupied;
                    break;
                case HouseState.Rentable:
                    label = house.Caption + "\n" + GenRes.state_rent + "\n" + house.Rental + "$";
                    break;
                case HouseState.Buyable:
                    label = house.Caption + "\n" + GenRes.state_sale + "\n" + house.Price + "$";
                    break;
            }
            
            return label;
        }

        public static void BuyHouse(Player player, HouseModel house)
        {
            if (house.State != HouseState.Buyable)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.house_not_buyable);
                return;
            }

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Bank < house.Price)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.house_not_money);
                return;
            }

            // Update the bank money
            characterModel.Bank -= house.Price;
            
            house.State = HouseState.None;
            house.Label.Text = GetHouseLabelText(house);
            house.Owner = player.Name;
            house.Locked = true;
            
            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.house_buy, house.Caption, house.Price));

            // Update the house
            Task.Run(() => DatabaseOperations.UpdateHouse(house)).ConfigureAwait(false);
        }
        
        public static void SetHouseRentable(Player player, HouseModel house, int amount, bool isAdmin)
        {
            house.Rental = amount;
            house.State = HouseState.Rentable;
            house.Tenants = 2;

            // Update house's textlabel
            house.Label.Text = GetHouseLabelText(house);

            // Update the house
            Task.Run(() => DatabaseOperations.UpdateHouse(house)).ConfigureAwait(false);

            if (isAdmin)
            {
                // Confirmation message sent to the staff
                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.house_rental_modified, amount));
            }
            else 
            {
                // Message sent to the player
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.house_state_rent, amount));
            }
        }

        [RemoteEvent("GetPlayerStoredClothes")]
        public void GetPlayerStoredClothesRemoteEvent(Player player, int type, int slot)
        {
            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            ClothesModel[] clothesArray = Customization.GetPlayerClothes(characterModel.Id).Where(c => c.type == type && c.slot == slot && c.Stored).ToArray();

            if(clothesArray.Length > 0)
            {
                List<string> clothesNames = Customization.GetClothesNames(clothesArray);

                // Show player's clothes
                player.TriggerEvent("ShowPlayerClothes", NAPI.Util.ToJson(clothesArray), NAPI.Util.ToJson(clothesNames));
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_clothes_in_wardrobe);
            }
        }

        [RemoteEvent("WardrobeClothesItemSelected")]
        public void WardrobeClothesItemSelectedRemoteEvent(Player player, int type, int slot, int index)
        {
            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // Get the previously dressed clothes
            ClothesModel dressedClothes = Customization.GetDressedClothesInSlot(characterModel.Id, 0, slot);

            if (index > 0)
            {
                // Get the clothes from the player in the slot
                ClothesModel wardrobeClothes = Customization.clothesList.Where(c => c.slot == slot && c.player == characterModel.Id && c.Stored).ToArray()[index - 1];

                // Dress the new clothes
                wardrobeClothes.dressed = true;
                wardrobeClothes.Stored = false;

                if (wardrobeClothes.type == type)
                {
                    player.SetClothes(wardrobeClothes.slot, wardrobeClothes.drawable, wardrobeClothes.texture);
                }
                else
                {
                    player.SetAccessories(wardrobeClothes.slot, wardrobeClothes.drawable, wardrobeClothes.texture);
                }

                // Store the clothes
                Task.Run(() => DatabaseOperations.UpdateClothes(wardrobeClothes)).ConfigureAwait(false);
            }

            if (dressedClothes != null)
            {
                // Undress the current clothes
                dressedClothes.dressed = false;
                dressedClothes.Stored = true;

                // Save the dressed clothes
                Task.Run(() => DatabaseOperations.UpdateClothes(dressedClothes)).ConfigureAwait(false);
            }

            if (index == 0)
            {
                // Load the equiped clothes
                Customization.LoadCharacterClothesEvent(player);
            }

            // Reload the list
            ClothesModel[] clothesArray = Customization.GetPlayerClothes(characterModel.Id).Where(c => c.type == type && c.slot == slot && c.Stored).ToArray();
            List<string> clothesNames = Customization.GetClothesNames(clothesArray);

            // Show player's clothes
            player.TriggerEvent("ShowPlayerClothes", NAPI.Util.ToJson(clothesArray), NAPI.Util.ToJson(clothesNames));

        }
    }
}
