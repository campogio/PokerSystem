using GTANetworkAPI;
using System.Threading.Tasks;
using WiredPlayers.character;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Utility;
using WiredPlayers.messages.error;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class FishingCommands
    {
        [Command]
        public static void FishCommand(Player player)
        {
            if (player.HasData(EntityData.PlayerFishing))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_fishing);
                return;
            }
            
            if (player.VehicleSeat == (int)VehicleSeat.Driver)
            {
                VehicleHash vehicleModel = (VehicleHash)player.Vehicle.Model;

                if (vehicleModel != VehicleHash.Marquis && vehicleModel != VehicleHash.Tug)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_fishing_boat);
                    return;
                }
                
                if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Job != PlayerJobs.Fisherman)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_fisherman);
                    return;
                }
            }
            else if (player.HasSharedData(EntityData.PlayerRightHand))
            {
                string rightHand = player.GetSharedData<string>(EntityData.PlayerRightHand);
                int fishingRodId = NAPI.Util.FromJson<AttachmentModel>(rightHand).itemId;
                ItemModel fishingRod = Inventory.GetItemModelFromId(fishingRodId);

                if (fishingRod == null || fishingRod.hash != Constants.ITEM_HASH_FISHING_ROD)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_fishing_rod);
                    return;
                }
                    
                int playerId = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
                ItemModel bait = Inventory.GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_BAIT);
                
                if (bait == null || bait.amount == 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_baits);
                    return;
                }
                
                foreach (Vector3 fishingPosition in Coordinates.FishingPoints)
                {
                    // Check if the player is close to the area
                    if (player.Position.DistanceTo(fishingPosition) > 2.5f) continue;

                    // Set player looking to the sea
                    player.Rotation = Coordinates.SeaLookingRotation;
                    
                    if (bait.amount == 1)
                    {
                        // Remove the baits from the inventory
                        Task.Run(() => DatabaseOperations.DeleteSingleRow("items", "id", bait.id)).ConfigureAwait(false);
                        Inventory.ItemCollection.Remove(bait.id);
                    }
                    else
                    {
                        bait.amount--;

                        // Update the amount
                        Task.Run(() => DatabaseOperations.UpdateItem(bait)).ConfigureAwait(false);
                    }

                    // Start fishing minigame
                    player.SetData(EntityData.PlayerFishing, true);
                    player.PlayAnimation("amb@world_human_stand_fishing@base", "base", (int)AnimationFlags.Loop);
                    player.TriggerEvent("startPlayerFishing");
                    return;
                }

                // Player's not in the fishing zone
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_fishing_zone);
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_rod_boat);
            }
        }
    }
}
