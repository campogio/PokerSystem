using GTANetworkAPI;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.character;
using WiredPlayers.Currency;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.factions;
using WiredPlayers.Utility;
using WiredPlayers.messages.error;
using WiredPlayers.messages.information;
using WiredPlayers.Buildings.Houses;
using WiredPlayers.Buildings;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Server.Commands
{
    public static class HouseCommands
    {
        [Command]
        public static void RentableCommand(Player player, int amount = 0)
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
            
            if (amount > 0)
            {
                // Set the house rentable
                House.SetHouseRentable(player, house, amount, false);
            }
            else if (house.State == HouseState.Rentable)
            {
                house.State = HouseState.None;
                house.Tenants = 2;

                // Update house's textlabel
                house.Label.Text = House.GetHouseLabelText(house);

                Task.Run(() =>
                {
                    // Update the house
                    DatabaseOperations.KickTenantsOut(house.Id);
                    DatabaseOperations.UpdateHouse(house);
                }).ConfigureAwait(false);

                // Message sent to the player
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.house_rent_cancel);
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.money_amount_positive);
            }
        }

        [Command]
        public static void RentCommand(Player player)
        {
            // Get the character model from the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // Check if the player has a rented house
            if (characterModel.HouseRent > 0) 
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_house_rented);
                return;
            }

            // Get the closest house
            HouseModel house = House.GetClosestHouse(player);

            if (house == null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_house_near);
                return;
            }

            if (house.State != HouseState.Rentable)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.house_not_rentable);
                return;
            }

            if(!Money.SubstractPlayerMoney(player, house.Rental, out string error))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + error);
                return;
            }

            // Rent the house to the player
            characterModel.HouseRent = house.Id;
            house.Tenants--;

            if (house.Tenants == 0)
            {
                house.State = HouseState.None;
                house.Label.Text = House.GetHouseLabelText(house);
            }

            // Update house's tenants
            Task.Run(() => DatabaseOperations.UpdateHouse(house)).ConfigureAwait(false);

            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.house_rent, house.Caption, house.Rental));
        }

        [Command]
        public static void UnrentCommand(Player player)
        {
            // Get the player's character model
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // Check if the house has any rented house
            if (characterModel.HouseRent == 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_rented_house);
                return;
            }

            // Get the house where the player is rented
            HouseModel house = House.GetHouseById(characterModel.HouseRent);

            if (player.Position.DistanceTo2D(house.Entrance) > 2.5f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_house_door);
                return;
            }

            // Remove player's rental
            characterModel.HouseRent = 0;
            house.Tenants++;
            
            // Change the state of the house
            house.State = HouseState.Rentable;
            house.Label.Text = House.GetHouseLabelText(house);

            // Update house's tenants
            Task.Run(() => DatabaseOperations.UpdateHouse(house)).ConfigureAwait(false);

            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.house_rent_stop, house.Caption));
        }

        [Command]
        public static void WardrobeCommand(Player player)
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
            
            if (!House.HasPlayerHouseKeys(player, house)) 
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_house_owner);
                return;
            }
            
            if (Customization.GetPlayerClothes(player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id).Count(c => c.Stored) > 0)
            {
                player.TriggerEvent("showPlayerWardrobe", Faction.IsPoliceMember(player));
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_clothes_in_wardrobe);
            }
        }
    }
}
