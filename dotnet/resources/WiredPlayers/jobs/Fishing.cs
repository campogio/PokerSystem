using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WiredPlayers.character;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.messages.error;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.jobs
{
    public class Fishing : Script
    {
        private static Dictionary<int, Timer> fishingTimerList;

        public Fishing()
        {
            // Initialize the variables
            fishingTimerList = new Dictionary<int, Timer>();
        }

        public static void OnPlayerDisconnected(Player player)
        {
            if (fishingTimerList.TryGetValue(player.Value, out Timer fishingTimer))
            {
                // Remove the timer
                fishingTimer.Dispose();
                fishingTimerList.Remove(player.Value);
            }
        }

        private void OnFishingPrewarnTimer(object playerObject)
        {
            Player player = (Player)playerObject;

            if (fishingTimerList.TryGetValue(player.Value, out Timer fishingTimer))
            {
                // Remove the timer
                fishingTimer.Dispose();
                fishingTimerList.Remove(player.Value);
            }

            // Start the minigame
            player.TriggerEvent("fishingBaitTaken");

            // Send the message and play fishing animation
            player.PlayAnimation("amb@world_human_stand_fishing@idle_a", "idle_c", (int)Enumerators.AnimationFlags.Loop);
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.something_baited);
        }

        private int GetPlayerFishingLevel(Player player)
        {
            // Get player points
            int fishingPoints = Job.GetJobPoints(player, PlayerJobs.Fisherman);

            // Calculamos el nivel
            if (fishingPoints > 600) return 5;
            if (fishingPoints > 300) return 4;
            if (fishingPoints > 150) return 3;
            if (fishingPoints > 50) return 2;

            return 1;
        }

        [RemoteEvent("startFishingTimer")]
        public void StartFishingTimerRemoteEvent(Player player)
        {
            Random random = new Random();

            // Timer for the game to start
            Timer fishingTimer = new Timer(OnFishingPrewarnTimer, player, random.Next(1250, 2500), Timeout.Infinite);
            fishingTimerList.Add(player.Value, fishingTimer);

            // Confirmation message
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.player_fishing_rod_thrown);
        }

        [RemoteEvent("fishingCanceled")]
        public void FishingCanceledRemoteEvent(Player player)
        {
            if (fishingTimerList.TryGetValue(player.Value, out Timer fishingTimer))
            {
                fishingTimer.Dispose();
                fishingTimerList.Remove(player.Value);
            }

            // Cancel the fishing
            player.StopAnimation();
            player.ResetData(EntityData.PlayerFishing);
            
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.fishing_canceled);
        }

        [RemoteEvent("fishingSuccess")]
        public async Task FishingSuccessRemoteEventSync(Player player)
        {
            // Calculate failure chance
            bool failed = false;
            Random random = new Random();
            int successChance = random.Next(100);

            // Getting player's level
            int fishingLevel = GetPlayerFishingLevel(player);

            switch (fishingLevel)
            {
                case 1:
                    failed = successChance >= 70;
                    break;
                case 2:
                    failed = successChance >= 80;
                    break;
                default:
                    failed = successChance >= 90;
                    fishingLevel = 3;
                    break;
            }

            if (!failed)
            {
                // Get player earnings
                int fishWeight = random.Next(fishingLevel * 100, fishingLevel * 750);
                int playerDatabaseId = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
                ItemModel fishItem = Inventory.GetPlayerItemModelFromHash(playerDatabaseId, Constants.ITEM_HASH_FISH);

                if (fishItem == null)
                {
                    fishItem = new ItemModel()
                    {
                        amount = fishWeight,
                        hash = Constants.ITEM_HASH_FISH,
                        ownerEntity = Constants.ITEM_ENTITY_PLAYER,
                        ownerIdentifier = playerDatabaseId,
                        position = new Vector3(),
                        dimension = 0
                    };

                    // Add the fish item to database
                    fishItem.id = await DatabaseOperations.AddNewItem(fishItem).ConfigureAwait(false);
                    Inventory.ItemCollection.Add(fishItem.id, fishItem);
                }
                else
                {
                    // Update the inventory
                    fishItem.amount += fishWeight;
                    await Task.Run(() => DatabaseOperations.UpdateItem(fishItem)).ConfigureAwait(false);

                }

                // Send the message to the player
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.fished_weight, fishWeight));
            }
            else
            {
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.garbage_fished);
            }

            // Add one skill point to the player
            Job.SetJobPoints(player, PlayerJobs.Fisherman, Job.GetJobPoints(player, PlayerJobs.Fisherman) + 1);

            // Cancel fishing
            player.StopAnimation();
            player.ResetData(EntityData.PlayerFishing);
        }

        [RemoteEvent("fishingFailed")]
        public void FishingFailedRemoteEvent(Player player)
        {
            // Cancel fishing
            player.StopAnimation();
            player.ResetData(EntityData.PlayerFishing);
            
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.fishing_failed);
        }
    }
}
