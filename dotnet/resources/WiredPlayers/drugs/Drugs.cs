using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WiredPlayers.character;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.drugs
{
    class Drugs : Script
    {
        public static List<PlantModel> Plants;
        public static Dictionary<int, Timer> PlantingTimer;

        private const int PlantGrowthTime = 120;
        private const int MaxWeedPerPlant = 10;

        public Drugs()
        {
            // Initialize the timer collection
            PlantingTimer = new Dictionary<int, Timer>();
        }

        public static void OnPlayerDisconnected(Player player)
        {
            if (PlantingTimer.TryGetValue(player.Value, out Timer plantTimer))
            {
                plantTimer.Dispose();
                PlantingTimer.Remove(player.Value);
            }
        }

        public static void UpdateGrowth()
        {
            foreach(PlantModel plant in Plants)
            {
                // Check if the plant has fully grown
                if (plant.GrowTime == PlantGrowthTime) continue;

                // Update the plant
                UpdatePlant(plant);
            }
        }

        public static void UpdatePlant(PlantModel plant)
        {
            // Get the growth percentage
            int growthTime = plant.Object == null ? plant.GrowTime : plant.GrowTime + 1;
            float growth = (float)Math.Round((decimal)(growthTime * 100 / PlantGrowthTime), 2);
            string growthText = string.Format(InfoRes.growth, Math.Floor(growth));

            // Create the corresponding object
            uint model = GetPlantModel(growth);

            if (plant.Object == null)
            {
                NAPI.Task.Run(() =>
                {
                    // Create the plant
                    plant.Object = NAPI.Object.CreateObject(model, plant.Position, new Vector3(), 255, plant.Dimension);

                    // Create the growth label
                    plant.Progress = NAPI.TextLabel.CreateTextLabel(growthText, new Vector3(0.0f, 0.0f, 4.0f).Add(plant.Position), 12.5f, 0.5f, 4, new Color(225, 200, 165), true, plant.Dimension);
                });
            }
            else
            {
                if (plant.Object.Model != model)
                {
                    NAPI.Task.Run(() =>
                    {
                        // Destroy the current object
                        plant.Object.Delete();

                        // Create the plant
                        plant.Object = NAPI.Object.CreateObject(model, plant.Position, new Vector3(), 255, plant.Dimension);
                    });
                }

                // Update the growth label
                plant.Progress.Text = growthText;
                plant.GrowTime++;

                // Update the plant's growth
                Task.Run(() => DatabaseOperations.ModifyPlant(plant.Id, plant.GrowTime)).ConfigureAwait(false);
            }

            if (plant.GrowTime == PlantGrowthTime)
            {
                NAPI.Task.Run(() =>
                {
                    // Create the colshape for the plant
                    plant.PlantColshape = NAPI.ColShape.CreateCylinderColShape(plant.Position, 3.5f, 1.0f, plant.Dimension);
                    plant.PlantColshape.SetData(EntityData.ColShapeId, plant.Id);

                    // Add the message to pop the instructional button up
                    plant.PlantColshape.SetData(EntityData.ColShapeType, ColShapeTypes.Plant);
                    plant.PlantColshape.SetData(EntityData.InstructionalButton, HelpRes.action_plant);
                });
            }
        }

        public static PlantModel GetClosestPlant(Player player)
        {
            // Get the closest plant to the player
            return Plants.FirstOrDefault(p => p.Dimension == player.Dimension && player.Position.DistanceTo(p.Position) < 2.0f);
        }

        public static void AnimatePlayerWeedManagement(Player player, bool isPlanting)
        {
            // Play the animation
            player.PlayAnimation("amb@world_human_gardener_plant@male@idle_a", "idle_a", (int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop));

            // Create the timer
            Timer plantManagementTimer = null;

            if (isPlanting)
            {
                // Create the plant timer
                plantManagementTimer = new Timer(PlantWeedSeedsAsync, player, 5000, Timeout.Infinite);
            }
            else
            {
                // Create the collect timer
                plantManagementTimer = new Timer(CollectWeedAsync, player, 5000, Timeout.Infinite);                
            }

            // Add the timer to the list
            PlantingTimer.Add(player.Value, plantManagementTimer);
        }

        public static async void PlantWeedSeedsAsync(object planter)
        {
            // Get the client planting
            Player player = (Player)planter;

            // Stop the plant animation
            player.StopAnimation();

            // Create the new plant object
            PlantModel plant = new PlantModel();
            {
                plant.Position = new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 1.0f);
                plant.Dimension = player.Dimension;
                plant.GrowTime = 0;
            }

            // Add the plant to the database
            plant.Id = await DatabaseOperations.AddPlant(plant).ConfigureAwait(false);
            Plants.Add(plant);

            // Remove the timer from the list
            PlantingTimer.Remove(player.Value);

            // Create the ingame object
            await Task.Run(() => UpdatePlant(plant)).ConfigureAwait(false);
        }

        public static async void CollectWeedAsync(object planter)
        {
            // Get the client planting
            Player player = (Player)planter;

            // Stop the plant animation
            player.StopAnimation();

            // Give the weed to the player
            Random random = new Random();
            int amount = random.Next(1, MaxWeedPerPlant);

            // Check if the player has any weed plant in the inventory
            int playerId = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
            ItemModel weedItem = Inventory.GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_WEED);

            if (weedItem == null)
            {
                // Create the object
                weedItem = new ItemModel()
                {
                    amount = amount,
                    dimension = 0,
                    position = new Vector3(),
                    hash = Constants.ITEM_HASH_WEED,
                    ownerEntity = Constants.ITEM_ENTITY_PLAYER,
                    ownerIdentifier = playerId,
                    objectHandle = null
                };

                weedItem.id = await DatabaseOperations.AddNewItem(weedItem).ConfigureAwait(false);
                Inventory.ItemCollection.Add(weedItem.id, weedItem);
            }
            else
            {
                // Add the amount
                weedItem.amount += amount;

                // Update the amount into the database
                await Task.Run(() => DatabaseOperations.UpdateItem(weedItem)).ConfigureAwait(false);
            }

            // Get the closest plant
            PlantModel plant = GetClosestPlant(player);

            await Task.Run(() =>
            {
                NAPI.Task.Run(() =>
                {
                    // Remove the plant with its description
                    plant.PlantColshape.Delete();
                    plant.Progress.Delete();
                    plant.Object.Delete();
                });

                // Delete the row into the database
                DatabaseOperations.DeleteSingleRow("plants", "id", plant.Id);
                Plants.Remove(plant);
            }).ConfigureAwait(false);

            // Remove the timer from the list
            PlantingTimer.Remove(player.Value);

            // Send the confirmation message
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.plant_collected, amount));
        }

        private static uint GetPlantModel(float growth)
        {
            string modelName;

            if (growth < 25.0f) modelName = "bkr_prop_weed_plantpot_stack_01b";
            else if (growth < 50.0f) modelName = "bkr_prop_weed_01_small_01b";
            else if (growth < 75.0f) modelName = "bkr_prop_weed_med_01b";
            else modelName = "bkr_prop_weed_lrg_01b";

            return NAPI.Util.GetHashKey(modelName);
        }
    }
}
