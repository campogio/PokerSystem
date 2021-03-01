using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WiredPlayers.Buildings;
using WiredPlayers.Buildings.Businesses;
using WiredPlayers.Currency;
using WiredPlayers.character;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.factions;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.information;
using WiredPlayers.messages.success;
using WiredPlayers.Utility;
using WiredPlayers.vehicles;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.jobs
{
    public class Thief : Script
    {
        public static Dictionary<int, Timer> RobberyTimerList;

        private const decimal ItemsRobbedPerTime = 1.5m;
        private const int MaxTheftsInRow = 4;

        public Thief()
        {
            foreach (Vector3 pawnShop in Coordinates.PawnShops)
            {
                // Create pawn shops
                NAPI.TextLabel.CreateTextLabel(GenRes.pawn_shop, pawnShop, 10.0f, 0.5f, 4, new Color(255, 255, 255), false, 0);
            }

            // Initialize the variables
            RobberyTimerList = new Dictionary<int, Timer>();
        }

        public static void OnPlayerDisconnected(Player player)
        {
            if (RobberyTimerList.TryGetValue(player.Value, out Timer robberyTimer))
            {
                robberyTimer.Dispose();
                RobberyTimerList.Remove(player.Value);
            }
        }

        public static void OnLockpickTimer(object playerObject)
        {
            Player player = (Player)playerObject;

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // Unlock the vehicle
            data.Lockpicking.Locked = false;

            player.StopAnimation();
            data.Lockpicking = null;
            data.Animation = false;

            if (RobberyTimerList.TryGetValue(player.Value, out Timer robberyTimer))
            {
                robberyTimer.Dispose();
                RobberyTimerList.Remove(player.Value);
            }

            player.SendChatMessage(Constants.COLOR_SUCCESS + SuccRes.lockpicked);
        }

        public static void OnHotwireTimer(object playerObject)
        {
            Player player = (Player)playerObject;

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // Turn the engine on
            data.Hotwiring.EngineStatus = true;

            // Stop the player animation
            player.TriggerEvent("StopSecondaryAnimation");
            data.Hotwiring = null;
            data.Animation = false;

            if (RobberyTimerList.TryGetValue(player.Value, out Timer robberyTimer))
            {
                robberyTimer.Dispose();
                RobberyTimerList.Remove(player.Value);
            }

            // Get all the members from any police faction
            List<Player> members = NAPI.Pools.GetAllPlayers().FindAll(m => Faction.IsPoliceMember(m) && m.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty);

            foreach (Player target in members)
            {
                // Send the warning to all the police players
                target.SendChatMessage(Constants.COLOR_INFO + InfoRes.police_warning);
            }

            // Mark the place where the thief has to deliver the vehicle
            SelectStolenVehicleDelivery(player);
        }

        public static async void OnPlayerRob(object playerObject)
        {
            Player player = (Player)playerObject;

            // Get the character and player models
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);
            PlayerTemporaryModel playerModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            int timeElapsed = UtilityFunctions.GetTotalSeconds() - playerModel.RobberyStart;
            decimal stolenItemsDecimal = timeElapsed / ItemsRobbedPerTime;
            int totalStolenItems = (int)Math.Round(stolenItemsDecimal);

            // Check if the player has stolen items
            ItemModel stolenItemModel = Inventory.GetPlayerItemModelFromHash(characterModel.Id, Constants.ITEM_HASH_STOLEN_OBJECTS);

            if (stolenItemModel == null)
            {
                stolenItemModel = new ItemModel()
                {
                    amount = totalStolenItems,
                    hash = Constants.ITEM_HASH_STOLEN_OBJECTS,
                    ownerEntity = Constants.ITEM_ENTITY_PLAYER,
                    ownerIdentifier = characterModel.Id,
                    position = new Vector3(),
                    dimension = 0
                };

                stolenItemModel.id = await DatabaseOperations.AddNewItem(stolenItemModel).ConfigureAwait(false);
                Inventory.ItemCollection.Add(stolenItemModel.id, stolenItemModel);
            }
            else
            {
                stolenItemModel.amount += totalStolenItems;

                // Update the amount into the database
                await Task.Run(() => DatabaseOperations.UpdateItem(stolenItemModel)).ConfigureAwait(false);
            }

            // Allow player movement
            player.StopAnimation();
            playerModel.RobberyStart = 0;
            playerModel.Animation = false;

            if (RobberyTimerList.TryGetValue(player.Value, out Timer robberyTimer))
            {
                robberyTimer.Dispose();
                RobberyTimerList.Remove(player.Value);
            }

            // Warn about the stolen items
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_robbed, totalStolenItems));

            // Check if the player commited the maximum thefts allowed
            if (characterModel.JobDeliver == MaxTheftsInRow)
            {
                // Apply a cooldown to the player
                characterModel.JobDeliver = 0;
                characterModel.JobCooldown = 60;
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.player_rob_pressure);
            }
            else
            {
                // Add another theft to the count
                characterModel.JobDeliver++;
            }
        }

        public static void GeneratePoliceRobberyWarning(Player player)
        {
            string robberyPlace = string.Empty;
            Vector3 robberyPosition = new Vector3();
            string robberyHour = DateTime.Now.ToString("h:mm:ss tt");

            if (BuildingHandler.IsIntoBuilding(player))
            {
                uint dimension = 0;

                // Get the interior information
                BuildingHandler.GetBuildingInformation(player, ref robberyPosition, ref dimension, ref robberyPlace);
            }
            else
            {
                robberyPosition = player.Position;
            }

            // Create the police report
            FactionWarningModel policeWarning = new FactionWarningModel(PlayerFactions.Police, player.Value, robberyPlace, robberyPosition, -1, robberyHour);
            FactionWarningModel sheriffWarning = new FactionWarningModel(PlayerFactions.Sheriff, player.Value, robberyPlace, robberyPosition, -1, robberyHour);
            Faction.factionWarningList.Add(policeWarning);
            Faction.factionWarningList.Add(sheriffWarning); 

            string warnMessage = string.Format(InfoRes.emergency_warning, Faction.factionWarningList.Count - 1);

            // Get all the members from any police faction
            List<Player> members = NAPI.Pools.GetAllPlayers().FindAll(m => Faction.IsPoliceMember(m) && m.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty);

            foreach (Player target in members)
            {
                // Send the warning
                target.SendChatMessage(Constants.COLOR_INFO + warnMessage);
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void PlayerExitVehicleServerEvent(Player player, Vehicle vehicle)
        {
            // Check if the vehicle isn't destroyed
            if (vehicle == null || !vehicle.Exists) return;

            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Job != PlayerJobs.Thief) return;

            // Get the temporary model for the player
            PlayerTemporaryModel playerModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (playerModel.Hotwiring != null && playerModel.Hotwiring.Exists)
            {
                // Remove player's hotwire
                player.StopAnimation();
                playerModel.Hotwiring = null;
                playerModel.Animation = false;

                if (RobberyTimerList.TryGetValue(player.Value, out Timer robberyTimer))
                {
                    robberyTimer.Dispose();
                    RobberyTimerList.Remove(player.Value);
                }

                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_stopped_hotwire);
            }
            else if (playerModel.RobberyStart > 0)
            {
                OnPlayerRob(player);
            }
        }

        [RemoteEvent("DeliverStoleVehicle")]
        public static void DeliverStoleVehicleEvent(Player player)
        {
            // Get the vehicle the player is driving
            Vehicle vehicle = player.Vehicle;

            // Check if the player has the vehicle keys
            if (Vehicles.HasPlayerVehicleKeys(player, vehicle, true))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_stolen);
                return;
            }

            // Get the vehicle price
            int vehiclePrice = CarShop.DealerVehicles.First(v => NAPI.Util.GetHashKey(v.hash) == vehicle.Model).price;

            // Receive a payment based on the vehicle's health amount
            int moneyWon = (int)Math.Round(vehiclePrice * player.Vehicle.Health * Constants.StolenVehicleMultiplier);

            // Pay the money to the player
            Money.GivePlayerMoney(player, moneyWon, out string _);

            // Place the car correctly
            vehicle.Locked = true;
            vehicle.EngineStatus = false;
            player.WarpOutOfVehicle();

            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.job_won, moneyWon));

            // Remove the checkpoint
            player.TriggerEvent("RemoveStolenVehicleDeliverPoint");
        }

        private static void SelectStolenVehicleDelivery(Player player)
        {
            // Get a random delivery point
            Random random = new Random();
            Vector3 deliveryPoint = Coordinates.VehicleDeliveryPlaces[random.Next(0, Coordinates.VehicleDeliveryPlaces.Length)];

            // Mark the delivery place
            player.TriggerEvent("ShowVehicleDeliveryPoint", deliveryPoint);
        }
    }
}
