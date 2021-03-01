using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WiredPlayers.Buildings.Businesses;
using WiredPlayers.Buildings.Houses;
using WiredPlayers.character;
using WiredPlayers.chat;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.drugs;
using WiredPlayers.jobs;
using WiredPlayers.messages.general;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using WiredPlayers.vehicles;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Server
{
    public class ServerHandler : Script
    {
        private Timer MinuteTimer;
        private Timer PlayerUpdateTimer;

        [ServerEvent(Event.ResourceStart)]
        public void ResourceStartEvent()
        {
            // Set the encoding
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            // Check if the voice chat is enabled
            using StreamReader reader = new StreamReader("conf.json");
            Dictionary<string, object> config = NAPI.Util.FromJson<Dictionary<string, object>>(reader.ReadToEnd());
            Chat.VoiceChatEnabled = config.ContainsKey("voice-chat") && config["voice-chat"] is bool && (bool)config["voice-chat"];

            // Set the culture
            string culture = NAPI.Resource.GetSetting<string>(this, "culture");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            NAPI.Util.ConsoleOutput("Server Language: " + Thread.CurrentThread.CurrentUICulture.Name);

            // Get the application type
            ConnectionHandler.ApplicationType = (ApplicationForm)NAPI.Resource.GetSetting<int>(this, "application");

            // Avoid player's respawn
            NAPI.Server.SetAutoRespawnAfterDeath(false);
            NAPI.Server.SetAutoSpawnOnConnect(false);

            // Disable global server chat
            NAPI.Server.SetGlobalServerChat(false);

            // Fix and build the interiors needed
            InitializeInteriors();

            // Register the commands into the server
            CommandHandler.RegisterServerCommands();

            // Initialize the database connection and load the basic data
            DatabaseManager.InitializeDatabaseConnection(this);

            // Add cycled timers
            MinuteTimer = new Timer(OnMinuteSpent, null, 60000, 60000);
            PlayerUpdateTimer = new Timer(UpdatePlayerList, null, 750, 750);
        }

        [ServerEvent(Event.PlayerEnterColshape)]
        public void PlayerEnterColshapeServerEvent(ColShape colShape, Player player)
        {
			// Check if the character has logged into the world
			if (!Character.IsPlaying(player)) return;
			
            // Set the current ColShape as entered
            player.SetData(EntityData.PlayerEnteredColShape, colShape);
            
            if (colShape.HasData(EntityData.InstructionalButton))
            {
                // Show the instructional button
                player.TriggerEvent("ShowInstructionalButton", colShape.GetData<string>(EntityData.InstructionalButton), "F");
            }

            // Handle the special ColShapes
            if (Business.HandleEnterColshape(player)) return;
        }

        [ServerEvent(Event.PlayerExitColshape)]
        public void PlayerExitColshapeServerEvent(ColShape _, Player player)
        {
			// Check if the character has logged into the world
			if (!Character.IsPlaying(player)) return;
			
            // Remove the player entered ColShape
            player.ResetData(EntityData.PlayerEnteredColShape);

            // Handle the special ColShapes
            if (Business.HandleExitColshape(player)) return;
        }

        private void InitializeInteriors()
        {
            // Add car dealer's interior
            NAPI.World.RequestIpl("shr_int");
            NAPI.World.RequestIpl("shr_int_lod");
            NAPI.World.RemoveIpl("fakeint");
            NAPI.World.RemoveIpl("fakeint_lod");
            NAPI.World.RemoveIpl("fakeint_boards");
            NAPI.World.RemoveIpl("fakeint_boards_lod");
            NAPI.World.RemoveIpl("shutter_closed");

            // Add clubhouse's door
            NAPI.World.RequestIpl("hei_bi_hw1_13_door");

            // Close the doors from the lobby
            NAPI.Object.CreateObject(NAPI.Util.GetHashKey("bkr_prop_biker_door_entry"), new Vector3(404.5286f, -996.5656f, -98.80404f), new Vector3(0.0f, 0.0f, 90.0f));
            NAPI.Object.CreateObject(NAPI.Util.GetHashKey("bkr_prop_biker_door_entry"), new Vector3(401.2006f, -997.8686f, -98.80404f), new Vector3(0.0f, 0.0f, 270.0f));
        }
        
        private void UpdatePlayerList(object unused)
        {
            // Initialize the score list
            List<ScoreModel> scoreList = new List<ScoreModel>();

            // Get all the players ingame
            List<Player> playingPlayers = NAPI.Pools.GetAllPlayers().FindAll(p => Character.IsPlaying(p));

            // Update the score list
            foreach (Player player in playingPlayers)
            {
                ScoreModel score = new ScoreModel(player.Value, player.Name, player.Ping);
                scoreList.Add(score);
            }

            // Update the list for all the players
            foreach (var p in playingPlayers) p.TriggerEvent("updatePlayerList", scoreList);
        }

        private void OnMinuteSpent(object unused)
        {
            // Adjust server's time
            TimeSpan currentTime = TimeSpan.FromTicks(DateTime.Now.Ticks);
            NAPI.World.SetTime(currentTime.Hours, currentTime.Minutes, currentTime.Seconds);

            // Synchronize the weather
            NAPI.World.SetWeather(NAPI.World.GetWeather());

            int totalSeconds = UtilityFunctions.GetTotalSeconds();
            Player[] onlinePlayers = NAPI.Pools.GetAllPlayers().Where(pl => Character.IsPlaying(pl)).ToArray();

            foreach (Player player in onlinePlayers)
            {
                // Get the character and player models
                CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);
                PlayerTemporaryModel playerModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

                if (characterModel.Played > 0 && characterModel.Played % 60 == 0)
                {
                    if (characterModel.EmployeeCooldown > 0)
                    {
                        // Reduce job cooldown
                        characterModel.EmployeeCooldown--;
                    }

                    // Generate the payday
                    GeneratePlayerPayday(player);
                }

                // Increase the played time
                characterModel.Played++;

                // Check if the player is injured waiting for the hospital respawn
                if (player.HasData(EntityData.TimeHospitalRespawn) && player.GetData<int>(EntityData.TimeHospitalRespawn) <= totalSeconds)
                {
                    // Send the death warning
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.player_can_die);
                }

                if (characterModel.JobCooldown > 0)
                {
                    // Decrease the job cooldown
                    characterModel.JobCooldown--;
                }

                if (playerModel.Jailed > 0)
                {
                    // Decrease the jailed time
                    playerModel.Jailed--;
                }
                else if (playerModel.Jailed == 0)
                {
                    // Set the player position
                    player.Position = Coordinates.JailSpawns[playerModel.JailType == JailTypes.Ic ? 3 : 4];

                    // Remove player from jail
                    playerModel.JailType = JailTypes.None;
                    playerModel.Jailed = -1;

                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.player_unjailed);
                }

                if (playerModel.DrunkLevel > 0.0f)
                {
                    // Lower alcohol level
                    float drunkLevel = playerModel.DrunkLevel - 0.05f;

                    if (drunkLevel <= 0.0f)
                    {
                        playerModel.DrunkLevel = 0.0f;
                    }
                    else
                    {
                        if (drunkLevel < Constants.WASTED_LEVEL)
                        {
                            player.ResetSharedData(EntityData.PlayerWalkingStyle);
                            NAPI.ClientEvent.TriggerClientEventForAll("resetPlayerWalkingStyle", player.Handle);
                        }

                        playerModel.DrunkLevel -= drunkLevel;
                    }
                }

                // Store the new values
                characterModel.Position = player.Position;
                characterModel.Rotation = player.Rotation;
                characterModel.Health = player.Health;
                characterModel.Armor = player.Armor;

                // Save the character's data
                Character.SaveCharacterData(characterModel);
            }

            // Refresh the orders
            FastFood.RefreshOrders(totalSeconds);

            // Update plants' growth
            Drugs.UpdateGrowth();

            // Save all the vehicles
            Vehicles.SaveAllVehicles();
        }

        private void GeneratePlayerPayday(Player player)
        {
            int total = 0;

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.payday_title);

            // Generate the salary
            if (characterModel.Faction != PlayerFactions.None && (int)characterModel.Faction <= Constants.LAST_STATE_FACTION)
            {
                // Add the faction money to the salary
                total += Constants.FACTION_RANK_LIST.FirstOrDefault(f => f.Faction == characterModel.Faction && f.Rank == characterModel.Rank)?.Salary ?? 0;
            }
            else
            {
                // Add the job money to the salary
                total += Constants.JobArray.FirstOrDefault(j => j.Job == characterModel.Job)?.Salary ?? 0;
            }

            // Send the salary message
            player.SendChatMessage(Constants.COLOR_HELP + GenRes.salary + total + "$");

            // Extra income from the level
            int levelEarnings = UtilityFunctions.GetPlayerLevel(player) * Constants.PAID_PER_LEVEL;
            total += levelEarnings;
            if (levelEarnings > 0)
            {
                player.SendChatMessage(Constants.COLOR_HELP + GenRes.extra_income + levelEarnings + "$");
            }

            // Bank interest
            int bankInterest = (int)Math.Round(characterModel.Bank * 0.001);
            total += bankInterest;
            if (bankInterest > 0)
            {
                player.SendChatMessage(Constants.COLOR_HELP + GenRes.bank_interest + bankInterest + "$");
            }

            // Vehicle taxes
            foreach (VehicleModel vehicle in Vehicles.IngameVehicles.Values)
            {
                VehicleHash vehicleHass = (VehicleHash)vehicle.Model;
                if (vehicle.Owner == player.Name && NAPI.Vehicle.GetVehicleClass(vehicleHass) != (int)VehicleClass.Cycle)
                {
                    int vehicleTaxes = (int)Math.Round(vehicle.Price * Constants.TAXES_VEHICLE);
                    string vehiclePlate = vehicle.Plate == string.Empty ? "LS " + (1000 + vehicle.Id) : vehicle.Plate;
                    player.SendChatMessage(Constants.COLOR_HELP + GenRes.vehicle_taxes_from + NAPI.Vehicle.GetVehicleDisplayName(vehicleHass) + " (" + vehiclePlate + "): -" + vehicleTaxes + "$");
                    total -= vehicleTaxes;
                }
            }

            // House taxes
            foreach (HouseModel house in House.HouseCollection.Values)
            {
                if (house.Owner == player.Name)
                {
                    int houseTaxes = (int)Math.Round(house.Price * Constants.TAXES_HOUSE);
                    player.SendChatMessage(Constants.COLOR_HELP + GenRes.house_taxes_from + house.Caption + ": -" + houseTaxes + "$");
                    total -= houseTaxes;
                }
            }

            // Calculate the total balance
            player.SendChatMessage(Constants.COLOR_HELP + "=====================");
            player.SendChatMessage(Constants.COLOR_HELP + GenRes.total + total + "$");
            characterModel.Bank += total;

            // Add the payment log
            Task.Run(() => DatabaseOperations.LogPayment("Payday", player.Name, "Payday", total)).ConfigureAwait(false);
        }
    }
}
