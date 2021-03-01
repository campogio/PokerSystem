using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.Buildings;
using WiredPlayers.Buildings.Businesses;
using WiredPlayers.Buildings.Houses;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.general;
using WiredPlayers.Utility;
using WiredPlayers.vehicles;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.character
{
    public class Character : Script
    {
        public static bool IsPlaying(Player player)
        {
            if (player == null || !player.Exists) return false;

            // Check if the given player is already logged into the world
            CharacterModel characterData = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);
            return characterData != null && characterData.Playing;
        }
        
        public static void InitializePlayerData(Player player)
        {
            // Initialize the external data
            player.SetExternalData((int)ExternalDataSlot.Database, new CharacterModel());
            player.SetExternalData((int)ExternalDataSlot.Ingame, new PlayerTemporaryModel());

            NAPI.Player.SpawnPlayer(player, Coordinates.LobbySpawn, 180.0f);
            player.Dimension = Convert.ToUInt32(player.Value);

            player.Health = 100;
            player.Armor = 0;

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);
            data.JailType = JailTypes.None;
            data.Jailed = -1;

            // Clear weapons
            player.RemoveAllWeapons();
        }

        public static void SaveCharacterData(CharacterModel model)
        {
            if (model.BuildingEntered.Type == BuildingTypes.Business)
            {
                // Check if the bussiness has an IPL
                BusinessModel business = Business.GetBusinessById(model.BuildingEntered.Id);

                if (business.Ipl.Name.Length == 0)
                {
                    // The player is in an outer business, we don't store it
                    model.BuildingEntered = new BuildingModel() { Id = 0, Type = BuildingTypes.None };
                }
            }

            // Save the player into database
            Task.Run(() => DatabaseOperations.SaveCharacterInformation(model)).ConfigureAwait(false);
        }
        
        [RemoteEvent("retrieveBasicData")]
        public static void RetrieveBasicDataEvent(Player player, int targetId)
        {
            // Get the target player
            Player target = NAPI.Pools.GetAllPlayers().First(p => p.Value == targetId);

            // Get the data from the player
            CharacterModel model = target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // Get the basic data
            string age = model.Age + GenRes.years;
            string sex = model.Sex == Sex.Male ? GenRes.SexMale : GenRes.SexFemale;
            string money = string.Format("{0}$", model.Money);
            string bank = string.Format("{0}$", model.Bank);
            string job = GenRes.unemployed;
            string rank = string.Empty;

            // Get the job
            JobModel jobModel = Constants.JobArray.FirstOrDefault(j => model.Job == j.Job);

            if (jobModel != null && jobModel.Job == 0)
            {
                // Get the player's faction
                FactionModel factionModel = Constants.FACTION_RANK_LIST.FirstOrDefault(f => model.Faction == f.Faction && model.Rank == f.Rank);

                if (factionModel != null && factionModel.Faction > 0)
                {
                    switch (factionModel.Faction)
                    {
                        case PlayerFactions.Police:
                            job = GenRes.police_faction;
                            break;
                        case PlayerFactions.Emergency:
                            job = GenRes.emergency_faction;
                            break;
                        case PlayerFactions.News:
                            job = GenRes.news_faction;
                            break;
                        case PlayerFactions.TownHall:
                            job = GenRes.townhall_faction;
                            break;
                        case PlayerFactions.Taxi:
                            job = GenRes.transport_faction;
                            break;
                        case PlayerFactions.Sheriff:
                            job = GenRes.sheriff_faction;
                            break;
                    }

                    // Set player's rank
                    rank = model.Sex == Sex.Male ? factionModel.DescriptionMale : factionModel.DescriptionFemale;
                }
            }
            else if (jobModel != null)
            {
                // Set the player's job
                job = model.Sex == Sex.Male ? jobModel.DescriptionMale : jobModel.DescriptionFemale;
            }

            // Show the data for the player
            player.TriggerEvent("showPlayerData", target.Value, target.Name, age, sex, money, bank, job, rank, player == target || model.AdminRank != StaffRank.None);
        }

        [RemoteEvent("retrievePropertiesData")]
        public static void RetrievePropertiesDataEvent(Player player, int targetId)
        {
            // Initialize the variables
            List<string> houseAddresses = new List<string>();
            string rentedHouse = string.Empty;

            // Get the target player
            Player target = NAPI.Pools.GetAllPlayers().Find(p => p.Value == targetId);

            // Get the houses where the player is the owner
            HouseModel[] houseList = House.HouseCollection.Values.Where(h => h.Owner == target.Name).ToArray();

            foreach (HouseModel house in houseList)
            {
                // Add the name of the house to the list
                houseAddresses.Add(house.Caption);
            }

            // Get the rent house if any
            int rent = target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).HouseRent;
            
            if (rent > 0)
            {
                // Get the name of the rented house
                rentedHouse = House.HouseCollection.ContainsKey(rent) ? House.HouseCollection[rent].Caption : string.Empty;
            }

            // Show the data for the player
            player.TriggerEvent("showPropertiesData", NAPI.Util.ToJson(houseAddresses), rentedHouse);
        }

        [RemoteEvent("retrieveVehiclesData")]
        public static void RetrieveVehiclesDataEvent(Player player, int targetId)
        {
            // Initialize the variables
            List<string> ownedVehicles = new List<string>();
            List<string> lentVehicles = new List<string>();

            // Get the target player
            Player target = NAPI.Pools.GetAllPlayers().Find(p => p.Value == targetId);

            // Get the vehicles in the game
            VehicleModel[] vehicles = Vehicles.IngameVehicles.Values.Where(v => Vehicles.HasPlayerVehicleKeys(target, v, true)).ToArray();
            
            foreach (VehicleModel veh in vehicles)
            {
                // Get the vehicle name
                string vehicleName = ((VehicleHash)veh.Model) + " LS-" + (veh.Id + 1000);

                if (veh.Owner == target.Name)
                {
                    // Add the the owned vehicles
                    ownedVehicles.Add(vehicleName);
                }
                else
                {
                    // Add the the lent vehicles
                    lentVehicles.Add(vehicleName);
                }
            }

            // Show the data for the player
            player.TriggerEvent("showVehiclesData", NAPI.Util.ToJson(ownedVehicles), NAPI.Util.ToJson(lentVehicles));
        }

        [RemoteEvent("retrieveExtendedData")]
        public static void RetrieveExtendedDataEvent(Player player, int targetId)
        {
            // Get the target player
            Player target = NAPI.Pools.GetAllPlayers().Find(p => p.Value == targetId);

            // Get the played time
            TimeSpan played = TimeSpan.FromMinutes(player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Played);
            string playedTime = Convert.ToInt32(played.TotalHours) + "h " + Convert.ToInt32(played.Minutes) + "m";

            // Show the data for the player
            player.TriggerEvent("showExtendedData", playedTime);
        }
    }
}
