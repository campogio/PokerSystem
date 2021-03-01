using GTANetworkAPI;
using System;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Utility;
using WiredPlayers.messages.arguments;
using WiredPlayers.messages.error;
using WiredPlayers.messages.help;
using WiredPlayers.Buildings.Houses;
using WiredPlayers.Buildings;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Server.Commands
{
    public static class FurnitureCommands
    {
        [Command]
        public static void FurnitureCommand(Player player, string action)
        {
            // Get the building
			BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;
            
            if (building.Type != BuildingTypes.House)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_in_house);
                return;
            }
            
            // Get the house where the player is
            HouseModel house = House.GetHouseById(building.Id);

            if (house == null || house.Owner != player.Name)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_house_owner);
                return;
            }
            
            if (action.Equals(ArgRes.place, StringComparison.InvariantCultureIgnoreCase))
            {
                FurnitureModel furniture = new FurnitureModel();
                {
                    furniture.Hash = NAPI.Util.GetHashKey("bkr_prop_weed_pallet");
                    furniture.House = Convert.ToUInt32(house.Id);
                    furniture.Position = player.Position;
                    furniture.Rotation = player.Rotation;
                    furniture.Handle = NAPI.Object.CreateObject(furniture.Hash, furniture.Position, furniture.Rotation, 255, furniture.House);
                }

                Furniture.FurnitureCollection.Add(furniture.Id, furniture);

                return;
            }
            
            if (action.Equals(ArgRes.move, StringComparison.InvariantCultureIgnoreCase))
            {
                // Manage furniture movement
                player.TriggerEvent("MoveFurniture", Furniture.GetFurnitureInHouse(house.Id));
                
                return;
            }
            
            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.furniture);
        }
    }
}
