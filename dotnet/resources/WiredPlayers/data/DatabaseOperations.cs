using GTANetworkAPI;
using MySql.Data.MySqlClient;
using SouthValleyFive.data.persistent;
using SouthValleyFive.poker;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.Administration;
using WiredPlayers.Buildings;
using WiredPlayers.Buildings.Businesses;
using WiredPlayers.Buildings.Houses;
using WiredPlayers.character;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.drugs;
using WiredPlayers.factions;
using WiredPlayers.jobs;
using WiredPlayers.messages.general;
using WiredPlayers.Server;
using WiredPlayers.Utility;
using WiredPlayers.vehicles;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data
{
    public static class DatabaseOperations
    {
        public static async Task<AccountModel> GetAccount(string socialName)
        {
            // Initialize the account
            AccountModel account = new AccountModel();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "SELECT `forumName`, `status`, `lastCharacter` FROM `accounts` WHERE `socialName` = @socialName LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@socialName", socialName } };
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            if (reader.HasRows)
            {
                await reader.ReadAsync().ConfigureAwait(false);
                account.ForumName = reader.GetString(reader.GetOrdinal("forumName"));
                account.State = reader.GetInt16(reader.GetOrdinal("status"));
                account.LastCharacter = reader.GetInt16(reader.GetOrdinal("lastCharacter"));
                account.Registered = true;
            }

            return account;
        }

        public static async Task<int> LoginAccount(string socialName, string password)
        {
            int state = -1;

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "SELECT `status` FROM `accounts` WHERE `socialName` = @socialName AND `password` = SHA2(@password, '256') LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@socialName", socialName }, { "@password", password } };
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            if (reader.HasRows)
            {
                await reader.ReadAsync().ConfigureAwait(false);
                state = reader.GetInt32(reader.GetOrdinal("status"));
            }
            return state;
        }

        public static void RegisterAccount(string socialName, string password)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "INSERT INTO `accounts` (`socialName`, `password`, `status`) VALUES (@socialName, SHA2(@password, '256'), @state)";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@socialName", socialName }, { "@password", password },
                { "@state", ConnectionHandler.ApplicationType == ApplicationForm.None ? 1 : 0 }
            };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void ApproveAccount(string socialName)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `accounts` SET `status` = 1 WHERE `socialName`= @socialName LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@socialName", socialName } };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void RegisterApplication(string socialName, int mistakes)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "INSERT INTO `applications` (`account`, `mistakes`) VALUES (@socialName, @mistakes)";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@socialName", socialName }, { "@mistakes", mistakes } };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static int GetPlayerStatus(string name)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "SELECT `status` FROM `users` WHERE `name` = @name LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@name", name } };
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using MySqlDataReader reader = command.ExecuteReader();

            return reader.HasRows ? reader.GetInt16("status") : 0;
        }

        public static async Task<List<string>> GetAccountCharacters(string account)
        {
            List<string> characters = new List<string>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "SELECT `name` FROM `users` WHERE `socialName` = @account";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@account", account } };
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                // Add each character to the collection
                characters.Add(reader.GetString(reader.GetOrdinal("name")));
            }

            return characters;
        }

        public static async Task<int> CreateCharacter(Player player, CharacterModel character)
        {
            int playerId = 0;

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "SELECT COUNT(`id`) FROM `users` WHERE `name` = @name LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@name", character.RealName } };
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            if ((Int64)await command.ExecuteScalarAsync().ConfigureAwait(false) > 0)
            {
                // There is already a character registered with that name
                player.TriggerEvent("characterNameDuplicated", character.RealName);
                return playerId;
            }

            // Insert the newly created character into the database
            query = "INSERT INTO `users` (`name`, `model`, `age`, `sex`, `socialName`) VALUES (@playerName, @playerModel, @playerAge, @playerSex, @socialName)";
            parameters = new Dictionary<string, object>()
            {
                { "@playerName", character.RealName }, { "@playerModel", character.Model }, { "@playerAge", character.Age },
                { "@playerSex", character.Sex }, { "@socialName", player.SocialClubName }
            };
            command = DatabaseManager.GenerateCommand(connection, query, parameters);

            // Get the inserted identifier
            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            playerId = (int)command.LastInsertedId;

            if (character.Skin != null)
            {
                // Store player's skin
                query = "INSERT INTO `skins` VALUES (@playerId, @firstHeadShape, @secondHeadShape, @firstSkinTone, @secondSkinTone, @headMix, @skinMix, ";
                query += "@hairModel, @firstHairColor, @secondHairColor, @beardModel, @beardColor, @chestModel, @chestColor, @blemishesModel, @ageingModel, ";
                query += "@complexionModel, @sundamageModel, @frecklesModel, @noseWidth, @noseHeight, @noseLength, @noseBridge, @noseTip, @noseShift, @browHeight, ";
                query += "@browWidth, @cheekboneHeight, @cheekboneWidth, @cheeksWidth, @eyes, @lips, @jawWidth, @jawHeight, @chinLength, @chinPosition, @chinWidth, ";
                query += "@chinShape, @neckWidth, @eyesColor, @eyebrowsModel, @eyebrowsColor, @makeupModel, @blushModel, @blushColor, @lipstickModel, @lipstickColor)";

                parameters = new Dictionary<string, object>()
                {
                    { "@playerId", playerId }, { "@firstHeadShape", character.Skin.firstHeadShape }, { "@secondHeadShape", character.Skin.secondHeadShape },
                    { "@firstSkinTone", character.Skin.firstSkinTone }, { "@secondSkinTone", character.Skin.secondSkinTone }, { "@headMix", character.Skin.headMix },
                    { "@skinMix", character.Skin.skinMix }, { "@hairModel", character.Skin.hairModel }, { "@firstHairColor", character.Skin.firstHairColor },
                    { "@secondHairColor", character.Skin.secondHairColor }, { "@beardModel", character.Skin.beardModel }, { "@beardColor", character.Skin.beardColor },
                    { "@chestModel", character.Skin.chestModel }, { "@chestColor", character.Skin.chestColor }, { "@blemishesModel", character.Skin.blemishesModel },
                    { "@ageingModel", character.Skin.ageingModel }, { "@complexionModel", character.Skin.complexionModel }, { "@sundamageModel", character.Skin.sundamageModel },
                    { "@frecklesModel", character.Skin.frecklesModel }, { "@noseWidth", character.Skin.noseWidth }, { "@noseHeight", character.Skin.noseHeight },
                    { "@noseLength", character.Skin.noseLength }, { "@noseBridge", character.Skin.noseBridge }, { "@noseTip", character.Skin.noseTip },
                    { "@noseShift", character.Skin.noseShift }, { "@browHeight", character.Skin.browHeight }, { "@browWidth", character.Skin.browWidth },
                    { "@cheekboneHeight", character.Skin.cheekboneHeight }, { "@cheekboneWidth", character.Skin.cheekboneWidth }, { "@cheeksWidth", character.Skin.cheeksWidth },
                    { "@eyes", character.Skin.eyes }, { "@lips", character.Skin.lips }, { "@jawWidth", character.Skin.jawWidth }, { "@jawHeight", character.Skin.jawHeight },
                    { "@chinLength", character.Skin.chinLength }, { "@chinPosition", character.Skin.chinPosition }, { "@chinWidth", character.Skin.chinWidth },
                    { "@chinShape", character.Skin.chinShape }, { "@neckWidth", character.Skin.neckWidth }, { "@eyesColor", character.Skin.eyesColor },
                    { "@eyebrowsModel", character.Skin.eyebrowsModel }, { "@eyebrowsColor", character.Skin.eyebrowsColor }, { "@makeupModel", character.Skin.makeupModel },
                    { "@blushModel", character.Skin.blushModel }, { "@blushColor", character.Skin.blushColor }, { "@lipstickModel", character.Skin.lipstickModel },
                    { "@lipstickColor", character.Skin.lipstickColor }
                };

                await DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            // Update the last character into the database
            await Task.Run(() => UpdateLastCharacter(player.SocialClubName, playerId));

            return playerId;
        }

        public static async Task<SkinModel> GetCharacterSkin(int characterId)
        {
            SkinModel skin = new SkinModel();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "SELECT * FROM `skins` WHERE `characterId` = @characterId LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@characterId", characterId } };
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            if (reader.HasRows)
            {
                await reader.ReadAsync().ConfigureAwait(false);
                skin.firstHeadShape = reader.GetInt32(reader.GetOrdinal("firstHeadShape"));
                skin.secondHeadShape = reader.GetInt32(reader.GetOrdinal("secondHeadShape"));
                skin.firstSkinTone = reader.GetInt32(reader.GetOrdinal("firstSkinTone"));
                skin.secondSkinTone = reader.GetInt32(reader.GetOrdinal("secondSkinTone"));
                skin.headMix = reader.GetFloat(reader.GetOrdinal("headMix"));
                skin.skinMix = reader.GetFloat(reader.GetOrdinal("skinMix"));
                skin.hairModel = reader.GetInt32(reader.GetOrdinal("hairModel"));
                skin.firstHairColor = reader.GetInt32(reader.GetOrdinal("firstHairColor"));
                skin.secondHairColor = reader.GetInt32(reader.GetOrdinal("secondHairColor"));
                skin.beardModel = reader.GetInt32(reader.GetOrdinal("beardModel"));
                skin.beardColor = reader.GetInt32(reader.GetOrdinal("beardColor"));
                skin.chestModel = reader.GetInt32(reader.GetOrdinal("chestModel"));
                skin.chestColor = reader.GetInt32(reader.GetOrdinal("chestColor"));
                skin.blemishesModel = reader.GetInt32(reader.GetOrdinal("blemishesModel"));
                skin.ageingModel = reader.GetInt32(reader.GetOrdinal("ageingModel"));
                skin.complexionModel = reader.GetInt32(reader.GetOrdinal("complexionModel"));
                skin.sundamageModel = reader.GetInt32(reader.GetOrdinal("sundamageModel"));
                skin.frecklesModel = reader.GetInt32(reader.GetOrdinal("frecklesModel"));
                skin.noseWidth = reader.GetFloat(reader.GetOrdinal("noseWidth"));
                skin.noseHeight = reader.GetFloat(reader.GetOrdinal("noseHeight"));
                skin.noseLength = reader.GetFloat(reader.GetOrdinal("noseLength"));
                skin.noseBridge = reader.GetFloat(reader.GetOrdinal("noseBridge"));
                skin.noseTip = reader.GetFloat(reader.GetOrdinal("noseTip"));
                skin.noseShift = reader.GetFloat(reader.GetOrdinal("noseShift"));
                skin.browHeight = reader.GetFloat(reader.GetOrdinal("browHeight"));
                skin.browWidth = reader.GetFloat(reader.GetOrdinal("browWidth"));
                skin.cheekboneHeight = reader.GetFloat(reader.GetOrdinal("cheekboneHeight"));
                skin.cheekboneWidth = reader.GetFloat(reader.GetOrdinal("cheekboneWidth"));
                skin.cheeksWidth = reader.GetFloat(reader.GetOrdinal("cheeksWidth"));
                skin.eyes = reader.GetFloat(reader.GetOrdinal("eyes"));
                skin.lips = reader.GetFloat(reader.GetOrdinal("lips"));
                skin.jawWidth = reader.GetFloat(reader.GetOrdinal("jawWidth"));
                skin.jawHeight = reader.GetFloat(reader.GetOrdinal("jawHeight"));
                skin.chinLength = reader.GetFloat(reader.GetOrdinal("chinLength"));
                skin.chinPosition = reader.GetFloat(reader.GetOrdinal("chinPosition"));
                skin.chinWidth = reader.GetFloat(reader.GetOrdinal("chinWidth"));
                skin.chinShape = reader.GetFloat(reader.GetOrdinal("chinShape"));
                skin.neckWidth = reader.GetFloat(reader.GetOrdinal("neckWidth"));
                skin.eyesColor = reader.GetInt32(reader.GetOrdinal("eyesColor"));
                skin.eyebrowsModel = reader.GetInt32(reader.GetOrdinal("eyebrowsModel"));
                skin.eyebrowsColor = reader.GetInt32(reader.GetOrdinal("eyebrowsColor"));
                skin.makeupModel = reader.GetInt32(reader.GetOrdinal("makeupModel"));
                skin.blushModel = reader.GetInt32(reader.GetOrdinal("blushModel"));
                skin.blushColor = reader.GetInt32(reader.GetOrdinal("blushColor"));
                skin.lipstickModel = reader.GetInt32(reader.GetOrdinal("lipstickModel"));
                skin.lipstickColor = reader.GetInt32(reader.GetOrdinal("lipstickColor"));
            }

            return skin;
        }

        public static void UpdateCharacterHair(int playerId, SkinModel skin)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `skins` SET `hairModel` = @hairModel, `firstHairColor` = @firstHairColor, `secondHairColor` = @secondHairColor, `beardModel` = @beardModel, ";
            query += "`beardColor` = @beardColor, `eyebrowsModel` = @eyebrowsModel, `eyebrowsColor` = @eyebrowsColor WHERE `characterId` = @playerId LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@hairModel", skin.hairModel }, { "@firstHairColor", skin.firstHairColor }, { "@secondHairColor", skin.secondHairColor }, { "@beardModel", skin.beardModel },
                { "@beardColor", skin.beardColor }, { "@eyebrowsModel", skin.eyebrowsModel }, { "@eyebrowsColor", skin.eyebrowsColor }, { "@playerId", playerId }
            };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static async Task LoadCharacterInformation(Player player, object filterField)
        {
            CharacterModel character = new CharacterModel();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = (filterField is int) ? "SELECT * FROM `users` WHERE `id` = @filter LIMIT 1" : "SELECT * FROM `users` WHERE `name` = @filter LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@filter", filterField } };
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            if (reader.HasRows)
            {
                await reader.ReadAsync().ConfigureAwait(false);
                float posX = reader.GetFloat(reader.GetOrdinal("posX"));
                float posY = reader.GetFloat(reader.GetOrdinal("posY"));
                float posZ = reader.GetFloat(reader.GetOrdinal("posZ"));
                float rot = reader.GetFloat(reader.GetOrdinal("rotation"));
                string[] building = reader.GetString(reader.GetOrdinal("buildingEntered")).Split(",");

                character.Id = reader.GetInt32(reader.GetOrdinal("id"));
                character.RealName = reader.GetString(reader.GetOrdinal("name"));
                character.Model = reader.GetString(reader.GetOrdinal("model"));
                character.AdminRank = (StaffRank)reader.GetInt32(reader.GetOrdinal("adminRank"));
                character.AdminName = reader.GetString(reader.GetOrdinal("adminName"));
                character.Position = new Vector3(posX, posY, posZ);
                character.Rotation = new Vector3(0.0f, 0.0f, rot);
                character.Money = reader.GetInt32(reader.GetOrdinal("money"));
                character.Bank = reader.GetInt32(reader.GetOrdinal("bank"));
                character.Health = reader.GetInt32(reader.GetOrdinal("health"));
                character.Armor = reader.GetInt32(reader.GetOrdinal("armor"));
                character.Age = reader.GetInt32(reader.GetOrdinal("age"));
                character.Sex = (Sex)reader.GetInt32(reader.GetOrdinal("sex"));
                character.Faction = (PlayerFactions)reader.GetInt32(reader.GetOrdinal("faction"));
                character.Job = (PlayerJobs)reader.GetInt32(reader.GetOrdinal("job"));
                character.Rank = reader.GetInt32(reader.GetOrdinal("rank"));
                character.OnDuty = reader.GetBoolean(reader.GetOrdinal("duty"));
                character.Radio = reader.GetInt32(reader.GetOrdinal("radio"));
                character.KilledBy = reader.GetInt32(reader.GetOrdinal("killed"));
                character.Jailed = reader.GetString(reader.GetOrdinal("jailed"));
                character.VehicleKeys = reader.GetString(reader.GetOrdinal("carKeys"));
                character.Documentation = reader.GetInt32(reader.GetOrdinal("documentation"));
                character.Licenses = reader.GetString(reader.GetOrdinal("licenses"));
                character.Insurance = reader.GetInt32(reader.GetOrdinal("insurance"));
                character.WeaponLicense = reader.GetInt32(reader.GetOrdinal("weaponLicense"));
                character.HouseRent = reader.GetInt32(reader.GetOrdinal("houseRent"));
                character.BuildingEntered = new BuildingModel() { Id = Convert.ToInt32(building[1]), Type = (BuildingTypes)Convert.ToInt32(building[0]) };
                character.EmployeeCooldown = reader.GetInt32(reader.GetOrdinal("employeeCooldown"));
                character.JobCooldown = reader.GetInt32(reader.GetOrdinal("jobCooldown"));
                character.JobDeliver = reader.GetInt32(reader.GetOrdinal("jobDeliver"));
                character.JobPoints = reader.GetString(reader.GetOrdinal("jobPoints"));
                character.RolePoints = reader.GetInt32(reader.GetOrdinal("rolePoints"));
                character.Status = reader.GetInt32(reader.GetOrdinal("status"));
                character.Played = reader.GetInt32(reader.GetOrdinal("played"));
            }

            // Save the player external data
            player.SetExternalData((int)ExternalDataSlot.Database, character);
        }

        public static void SaveCharacterInformation(CharacterModel character)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `users` SET `posX` = @posX, `posY` = @posY, `posZ` = @posZ, `rotation` = @rotation, `money` = @money, `bank` = @bank, `health` = @health, ";
            query += "`armor` = @armor, `radio` = @radio, `killed` = @killed, `jailed` = @jailed, `faction` = @faction, `job` = @job, `rank` = @rank, `duty` = @duty, ";
            query += "`carKeys` = @carKeys, `documentation` = @documentation, `licenses` = @licenses, `insurance` = @insurance, `weaponLicense` = @weaponLicense, ";
            query += "`houseRent` = @houseRent, `buildingEntered` = @buildingEntered, `employeeCooldown` = @employeeCooldown, `jobCooldown` = @jobCooldown, ";
            query += "`jobDeliver` = @jobDeliver, `jobPoints` = @jobPoints, `rolePoints` = @rolePoints, `played` = @played WHERE `id` = @playerId LIMIT 1";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@posX", character.Position.X }, { "@posY", character.Position.Y }, { "@posZ", character.Position.Z }, { "@rotation", character.Rotation.Z },
                { "@money", character.Money }, { "@bank", character.Bank }, { "@health", character.Health }, { "@armor", character.Armor }, { "@radio", character.Radio },
                { "@killed", character.KilledBy }, { "@jailed", character.Jailed }, { "@faction", character.Faction }, { "@job", character.Job }, { "@rank", character.Rank },
                { "@duty", character.OnDuty }, { "@carKeys", character.VehicleKeys }, { "@documentation", character.Documentation }, { "@licenses", character.Licenses },
                { "@insurance", character.Insurance }, { "@weaponLicense", character.WeaponLicense }, { "@houseRent", character.HouseRent }, { "@buildingEntered", character.BuildingEntered.ToString() },
                { "@employeeCooldown", character.EmployeeCooldown }, { "@jobCooldown", character.JobCooldown }, { "@jobDeliver", character.JobDeliver },
                { "@jobPoints", character.JobPoints }, { "@rolePoints", character.RolePoints }, { "@played", character.Played }, { "@playerId", character.Id }
            };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void UpdateLastCharacter(string socialName, int playerId)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `accounts` SET `lastCharacter` = @playerId WHERE `socialName` = @socialName LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@playerId", playerId }, { "@socialName", socialName } };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static bool FindCharacter(string name)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "SELECT `id` FROM `users` WHERE `name` = @name LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@name", name } };
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using MySqlDataReader reader = command.ExecuteReader();

            // Check if the character was found
            return reader.HasRows;
        }

        public static async Task<List<BankOperationModel>> GetBankOperations(string playerName, int start, int count)
        {
            List<BankOperationModel> operations = new List<BankOperationModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "SELECT * FROM `money` WHERE (`source` = @playerName OR `receiver` = @playerName) AND (`type` = @opTransfer ";
            query += "OR `type` = @opDeposit OR `type` = @opWithdraw) ORDER BY `date` DESC, `hour` DESC LIMIT @start, @count";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@playerName", playerName }, { "@opTransfer", GenRes.bank_op_transfer }, { "@opDeposit", GenRes.bank_op_deposit },
                { "@opWithdraw", GenRes.bank_op_withdraw }, { "@start", start }, { "@count", count }
            };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                BankOperationModel bankOperation = new BankOperationModel
                {
                    Source = reader.GetString(reader.GetOrdinal("source")),
                    Receiver = reader.GetString(reader.GetOrdinal("receiver")),
                    Type = reader.GetString(reader.GetOrdinal("type")),
                    Amount = reader.GetInt32(reader.GetOrdinal("amount")),
                    Day = reader.GetString(reader.GetOrdinal("date")).Split(' ')[0],
                    Time = reader.GetString(reader.GetOrdinal("hour"))
                };

                operations.Add(bankOperation);
            }

            return operations;
        }

        public static void LoadAllDealerVehicles()
        {
            CarShop.DealerVehicles = new List<CarShopVehicleModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `dealers`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                CarShopVehicleModel vehicle = new CarShopVehicleModel
                {
                    hash = reader.GetString("vehicleHash"),
                    carShop = reader.GetInt32("dealerId"),
                    type = reader.GetInt32("vehicleType"),
                    price = reader.GetInt32("price")
                };

                CarShop.DealerVehicles.Add(vehicle);
            }
        }

        public static void LoadAllVehicles()
        {
            Vehicles.IngameVehicles = new Dictionary<int, VehicleModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `vehicles`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                float posX = reader.GetFloat("posX");
                float posY = reader.GetFloat("posY");
                float posZ = reader.GetFloat("posZ");
                string hash = reader.GetString("model");

                VehicleModel vehicle = new VehicleModel
                {
                    Id = reader.GetInt32("id"),
                    Model = NAPI.Util.GetHashKey(hash),
                    Position = new Vector3(posX, posY, posZ),
                    Heading = reader.GetFloat("rotation"),
                    ColorType = reader.GetInt32("colorType"),
                    FirstColor = reader.GetString("firstColor"),
                    SecondColor = reader.GetString("secondColor"),
                    Pearlescent = reader.GetInt32("pearlescent"),
                    Owner = reader.GetString("owner"),
                    Plate = reader.GetString("plate"),
                    Dimension = reader.GetUInt32("dimension"),
                    Faction = reader.GetInt32("faction"),
                    Engine = reader.GetInt32("engine"),
                    Locked = reader.GetInt32("locked"),
                    Price = reader.GetInt32("price"),
                    Parking = reader.GetInt32("parking"),
                    Parked = reader.GetInt32("parkedTime"),
                    Gas = reader.GetFloat("gas"),
                    Kms = reader.GetFloat("kms")
                };

                Vehicles.IngameVehicles.Add(vehicle.Id, vehicle);
            }

            // Generate the vehicles into the game
            Vehicles.GenerateGameVehicles();
        }

        public static async Task<int> AddNewVehicle(VehicleModel vehicle)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `vehicles` (`model`, `posX`, `posY`, `posZ`, `rotation`, `firstColor`, `secondColor`, ";
            query += "`dimension`, `faction`, `owner`, `plate`, `gas`) VALUES (@model, @posX, @posY, @posZ, @rotation, ";
            query += "@firstColor, @secondColor, @dimension, @faction, @owner, @plate, @gas)";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@model", ((VehicleHash)vehicle.Model).ToString() }, { "@posX", vehicle.Position.X }, { "@posY", vehicle.Position.Y }, { "@posZ", vehicle.Position.Z },
                { "@rotation", vehicle.Heading }, { "@firstColor", vehicle.FirstColor }, { "@secondColor", vehicle.SecondColor }, { "@dimension", vehicle.Dimension },
                { "@faction", vehicle.Faction }, { "@owner", vehicle.Owner }, { "@plate", vehicle.Plate }, { "@gas", vehicle.Gas }
            };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return (int)command.LastInsertedId;
        }

        public static void UpdateVehicleValues(Dictionary<string, object> columnValues, int vehicleId)
        {
            bool first = true;

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `vehicles` ";
            Dictionary<string, object> parameters = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> entry in columnValues)
            {
                if (!first) query += ", ";

                query += string.Format("SET `{0}` = @{1}", entry.Key, entry.Key);
                parameters.Add(string.Format("@{0}", entry.Key), entry.Value);

                // Reset the first checker
                first = false;
            }

            // Add the WHERE clause
            query += " WHERE `id` = @vehicle LIMIT 1";
            parameters.Add("@vehicle", vehicleId);

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void SaveVehicles(Dictionary<int, VehicleModel> vehicleList)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `vehicles` SET `posX` = @posX, `posY` = @posY, `posZ` = @posZ, `rotation` = @rotation, ";
            query += "`colorType` = @colorType, `firstColor` = @firstColor, `secondColor` = @secondColor, ";
            query += "`pearlescent` = @pearlescent, `dimension` = @dimension, `engine` = @engine, `locked` = @locked, ";
            query += "`faction` = @faction, `owner` = @owner, `plate` = @plate, `price` = @price, `parking` = @parking, ";
            query += "`parkedTime` = @parkedTime, `gas` = @gas, `kms` = @kms WHERE `id` = @vehId LIMIT 1";

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, new Dictionary<string, object>());

            foreach (VehicleModel vehicle in vehicleList.Values)
            {
                command.Parameters.Clear();

                command.Parameters.AddWithValue("@posX", vehicle.Position.X);
                command.Parameters.AddWithValue("@posY", vehicle.Position.Y);
                command.Parameters.AddWithValue("@posZ", vehicle.Position.Z);
                command.Parameters.AddWithValue("@rotation", vehicle.Heading);
                command.Parameters.AddWithValue("@colorType", vehicle.ColorType);
                command.Parameters.AddWithValue("@firstColor", vehicle.FirstColor);
                command.Parameters.AddWithValue("@secondColor", vehicle.SecondColor);
                command.Parameters.AddWithValue("@pearlescent", vehicle.Pearlescent);
                command.Parameters.AddWithValue("@dimension", vehicle.Dimension);
                command.Parameters.AddWithValue("@engine", vehicle.Engine);
                command.Parameters.AddWithValue("@locked", vehicle.Locked);
                command.Parameters.AddWithValue("@faction", vehicle.Faction);
                command.Parameters.AddWithValue("@owner", vehicle.Owner);
                command.Parameters.AddWithValue("@plate", vehicle.Plate);
                command.Parameters.AddWithValue("@price", vehicle.Price);
                command.Parameters.AddWithValue("@parking", vehicle.Parking);
                command.Parameters.AddWithValue("@parkedTime", vehicle.Parked);
                command.Parameters.AddWithValue("@gas", vehicle.Gas);
                command.Parameters.AddWithValue("@kms", vehicle.Kms);
                command.Parameters.AddWithValue("@vehId", vehicle.Id);

                command.ExecuteNonQuery();
            }
        }

        public static void LoadAllTunning()
        {
            Mechanic.tunningList = new List<TunningModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `tunning`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                TunningModel tunning = new TunningModel
                {
                    id = reader.GetInt32("id"),
                    vehicle = reader.GetInt32("vehicle"),
                    slot = reader.GetInt32("slot"),
                    component = reader.GetInt32("component")
                };

                Mechanic.tunningList.Add(tunning);
            }
        }

        public static async Task<int> AddTunning(TunningModel tunning)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `tunning` (`vehicle`, `slot`, `component`) VALUES (@vehicle, @slot, @component)";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@vehicle", tunning.vehicle }, { "@slot", tunning.slot }, { "@component", tunning.component }
            };
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return (int)command.LastInsertedId;
        }

        public static void TransferMoneyToPlayer(string name, int amount)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `users` SET `bank` = `bank` + @amount WHERE `name` = @name LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@name", name }, { "@amount", amount } };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void LogPayment(string source, string receiver, string type, int amount)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "INSERT INTO `money` VALUES (@source, @receiver, @type, @amount, CURDATE(), CURTIME())";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@source", source }, { "@receiver", receiver }, { "@type", type }, { "@amount", amount }
            };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void LogHotwire(string playerName, int vehicleId, Vector3 position)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "INSERT INTO `hotwires` VALUES (@vehicleId, @playerName, @posX, @posY, @posZ, NOW())";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@playerName", playerName }, { "@vehicleId", vehicleId }, { "@posX", position.X }, { "@posY", position.Y }, { "@posZ", position.Z }
            };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void LoadAllItems()
        {
            Inventory.ItemCollection = new Dictionary<int, ItemModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `items`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                float posX = reader.GetFloat("posX");
                float posY = reader.GetFloat("posY");
                float posZ = reader.GetFloat("posZ");

                ItemModel item = new ItemModel
                {
                    id = reader.GetInt32("id"),
                    hash = reader.GetString("hash"),
                    ownerEntity = reader.GetString("ownerEntity"),
                    ownerIdentifier = reader.GetInt32("ownerIdentifier"),
                    amount = reader.GetInt32("amount"),
                    position = new Vector3(posX, posY, posZ),
                    dimension = reader.GetUInt32("dimension")
                };

                Inventory.ItemCollection.Add(item.id, item);
            }

            // Add the items on the ground
            Inventory.GenerateGroundItems();
        }

        public static async Task<int> AddNewItem(ItemModel item)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `items` (`hash`, `ownerEntity`, `ownerIdentifier`, `amount`, `posX`, `posY`, `posZ`)";
            query += " VALUES (@hash, @ownerEntity, @ownerIdentifier, @amount, @posX, @posY, @posZ)";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@hash", item.hash }, { "@ownerEntity", item.ownerEntity }, { "@ownerIdentifier", item.ownerIdentifier }, { "@amount", item.amount },
                { "@posX", item.position.X }, { "@posY", item.position.Y }, { "@posZ", item.position.Z }
            };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return (int)command.LastInsertedId;
        }

        public static void UpdateItem(ItemModel item)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `items` SET `ownerEntity` = @ownerEntity, `ownerIdentifier` = @ownerIdentifier, `amount` = @amount, ";
            query += "`posX` = @posX, `posY` = @posY, `posZ` = @posZ, `dimension` = @dimension WHERE `id` = @id LIMIT 1";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@ownerEntity", item.ownerEntity }, { "@ownerIdentifier", item.ownerIdentifier }, { "@amount", item.amount }, { "@posX", item.position.X },
                { "@posY", item.position.Y }, { "@posZ", item.position.Z }, { "@dimension", item.dimension }, { "@id", item.id }
            };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void LoadAllAnimationCategories()
        {
            Animations.AnimationGroup = new Dictionary<int, CategoryModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `categories`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                CategoryModel category = new CategoryModel
                {
                    Id = reader.GetInt32("id"),
                    Description = reader.GetString("name"),
                    Animations = new Dictionary<int, AnimationModel>()
                };

                // Add the category to the dictionary
                Animations.AnimationGroup.Add(category.Id, category);
            }

            if (Animations.AnimationGroup.Count > 0)
            {
                // Load the animations for each category
                Animations.AddAnimations();
            }
        }

        public static Dictionary<int, AnimationModel> LoadAnimationList()
        {
            Dictionary<int, AnimationModel> animations = new Dictionary<int, AnimationModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `animations`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                AnimationModel animation = new AnimationModel
                {
                    Id = reader.GetInt32("id"),
                    Category = reader.GetInt32("category"),
                    Description = reader.GetString("description"),
                    Library = reader.GetString("library"),
                    Name = reader.GetString("name"),
                    Flag = reader.GetInt32("flag")
                };

                // Add the category to the dictionary
                animations.Add(animation.Id, animation);
            }

            return animations;
        }

        public static void LoadAllBusinesses()
        {
            Business.BusinessCollection = new Dictionary<int, BusinessModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `business`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                float posX = reader.GetFloat("posX");
                float posY = reader.GetFloat("posY");
                float posZ = reader.GetFloat("posZ");
                int type = reader.GetInt32("type");

                BusinessModel business = new BusinessModel
                {
                    Id = reader.GetInt32("id"),
                    Ipl = BuildingHandler.GetBusinessIpl(type),
                    Caption = reader.GetString("name"),
                    Entrance = new Vector3(posX, posY, posZ),
                    Dimension = reader.GetUInt32("dimension"),
                    Owner = reader.GetString("owner"),
                    Multiplier = reader.GetFloat("multiplier"),
                    Locked = reader.GetBoolean("locked")
                };

                if (reader.GetBoolean("inner"))
                {
                    // Remove the IPL from the business
                    business.Ipl.Name = string.Empty;
                }

                Business.BusinessCollection.Add(business.Id, business);
            }

            // Generate the Colshapes, Checkpoints and TextLabels
            Business.GenerateEntrancePoints();
        }

        public static void UpdateBusinesses(Dictionary<int, BusinessModel> businessDictionary)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `business` SET `type` = @type, `posX` = @posX, `posY` = @posY, `posZ` = @posZ, ";
            query += "`dimension` = @dimension, `name` = @name, `owner` = @owner, `funds` = @funds, `products` = @products, ";
            query += "`multiplier` = @multiplier, `locked` = @locked WHERE `id` = @id LIMIT 1";
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, new Dictionary<string, object>());

            foreach (BusinessModel business in businessDictionary.Values)
            {
                command.Parameters.Clear();

                command.Parameters.AddWithValue("@type", business.Ipl.Type);
                command.Parameters.AddWithValue("@posX", business.Entrance.X);
                command.Parameters.AddWithValue("@posY", business.Entrance.Y);
                command.Parameters.AddWithValue("@posZ", business.Entrance.Z);
                command.Parameters.AddWithValue("@dimension", business.Dimension);
                command.Parameters.AddWithValue("@name", business.Caption);
                command.Parameters.AddWithValue("@owner", business.Owner);
                command.Parameters.AddWithValue("@funds", business.Funds);
                command.Parameters.AddWithValue("@products", business.Products);
                command.Parameters.AddWithValue("@multiplier", business.Multiplier);
                command.Parameters.AddWithValue("@locked", business.Locked);
                command.Parameters.AddWithValue("@id", business.Id);

                command.ExecuteNonQuery();
            }
        }

        public static async Task<int> AddNewBusiness(BusinessModel business)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `business` (`type`, `inner`, `posX`, `posY`, `posZ`, `dimension`) ";
            query += "VALUES (@type, @inner, @posX, @posY, @posZ, @dimension)";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@type", business.Ipl.Type }, { "@inner", business.Ipl.Name.Length == 0 }, { "@posX", business.Entrance.X },
                { "@posY", business.Entrance.Y }, { "@posZ", business.Entrance.Z }, { "@dimension", business.Dimension }
            };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync();
            return (int)command.LastInsertedId;
        }

        public static void LoadAllHouses()
        {
            House.HouseCollection = new Dictionary<int, HouseModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `houses`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                float posX = reader.GetFloat("posX");
                float posY = reader.GetFloat("posY");
                float posZ = reader.GetFloat("posZ");

                HouseModel house = new HouseModel
                {
                    Id = reader.GetInt32("id"),
                    Ipl = BuildingHandler.GetHouseIpl(reader.GetInt32("type")),
                    Caption = reader.GetString("name"),
                    Entrance = new Vector3(posX, posY, posZ),
                    Dimension = reader.GetUInt32("dimension"),
                    Price = reader.GetInt32("price"),
                    Owner = reader.GetString("owner"),
                    State = (HouseState)reader.GetInt32("status"),
                    Tenants = reader.GetInt32("tenants"),
                    Rental = reader.GetInt32("rental"),
                    Locked = reader.GetBoolean("locked")
                };

                House.HouseCollection.Add(house.Id, house);
            }

            // Generate the entrances
            House.GenerateEntrancePoints();
        }

        public static async Task<int> AddHouse(HouseModel house)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `houses` (`type`, `posX`, `posY`, `posZ`, `dimension`) VALUES (@type, @posX, @posY, @posZ, @dimension)";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@type", house.Ipl.Type }, { "@posX", house.Entrance.X }, { "@posY", house.Entrance.Y }, { "@posZ", house.Entrance.Z }, { "@dimension", house.Dimension }
            };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return (int)command.LastInsertedId;
        }

        public static void UpdateHouse(HouseModel house)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `houses` SET `type` = @type, `posX` = @posX, `posY` = @posY, `posZ` = @posZ, ";
            query += "`dimension` = @dimension, `name` = @name, `price` = @price, `owner` = @owner, `status` = @state, ";
            query += "`tenants` = @tenants, `rental` = @rental, `locked` = @locked WHERE `id` = @id LIMIT 1";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@type", house.Ipl.Type }, { "@posX", house.Entrance.X }, { "@posY", house.Entrance.Y }, { "@posZ", house.Entrance.Z },
                { "@dimension", house.Dimension }, { "@name", house.Caption }, { "@price", house.Price }, { "@owner", house.Owner },
                { "@state", house.State }, { "@tenants", house.Tenants }, { "@rental", house.Rental }, { "@locked", house.Locked },
                { "@id", house.Id }
            };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void KickTenantsOut(int houseId)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `users` SET `houseRent` = 0 WHERE `houseRent` = @houseRent";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@houseRent", houseId } };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void LoadAllFurniture()
        {
            Furniture.FurnitureCollection = new Dictionary<int, FurnitureModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `furniture`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                float posX = reader.GetFloat("posX");
                float posY = reader.GetFloat("posY");
                float posZ = reader.GetFloat("posZ");
                float rot = reader.GetFloat("rotation");

                FurnitureModel furniture = new FurnitureModel
                {
                    Id = reader.GetInt32("id"),
                    Hash = reader.GetUInt32("hash"),
                    House = reader.GetUInt32("house"),
                    Position = new Vector3(posX, posY, posZ),
                    Rotation = new Vector3(0.0f, 0.0f, rot)
                };

                Furniture.FurnitureCollection.Add(furniture.Id, furniture);
            }

            // Add the furniture ingame
            Furniture.GenerateIngameFurniture();
        }

        public static void LoadAllInteriors()
        {
            GenericInterior.InteriorCollection = new Dictionary<int, InteriorModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `interiors`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                float posX = reader.GetFloat("posX");
                float posY = reader.GetFloat("posY");
                float posZ = reader.GetFloat("posZ");

                InteriorModel interior = new InteriorModel
                {
                    Id = reader.GetInt32("id"),
                    Caption = reader.GetString("name"),
                    Entrance = new Vector3(posX, posY, posZ),
                    Dimension = reader.GetUInt32("dimension"),
                    Ipl = BuildingHandler.GetInteriorIpl(reader.GetInt32("type")),
                    BlipSprite = reader.GetInt32("blip")
                };

                GenericInterior.InteriorCollection.Add(interior.Id, interior);
            }

            // Generate the entrance
            GenericInterior.GenerateEntrancePoints();
        }

        public static async Task<int> AddInteriorAsync(InteriorModel interior)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `interiors` (`name`, `type`, `posX`, `posY`, `posZ`, `dimension`, `blip`) VALUES (@name, @type, @posX, @posY, @posZ, @dimension, @blip)";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@name", interior.Caption }, { "@type", interior.Ipl.Type }, { "@posX", interior.Entrance.X }, { "@posY", interior.Entrance.Y },
                { "@posZ", interior.Entrance.Z }, { "@dimension", interior.Dimension }, { "@blip", interior.BlipSprite }
            };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return (int)command.LastInsertedId;
        }

        public static void UpdateInterior(InteriorModel interior)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `interiors` SET `name` = @name, `type` = @type, `posX` = @posX, `posY` = @posY, `posZ` = @posZ, ";
            query += "`dimension` = @dimension, `blip` = @blip WHERE `id` = @id LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@name", interior.Caption }, { "@type", interior.Ipl.Type }, { "@posX", interior.Entrance.X }, { "@posY", interior.Entrance.Y },
                { "@posZ", interior.Entrance.Z }, { "@dimension", interior.Dimension }, { "@blip", interior.BlipSprite }, { "@id", interior.Id }
            };

            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void LoadCrimes()
        {
            // Initialize the list
            Police.crimeList = new List<CrimeModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `crimes`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                CrimeModel crime = new CrimeModel
                {
                    crime = reader.GetString("description"),
                    jail = reader.GetInt32("jail"),
                    fine = reader.GetInt32("fine"),
                    reminder = reader.GetString("reminder")
                };

                Police.crimeList.Add(crime);
            }
        }

        public static void LoadAllPoliceControls()
        {
            Police.policeControlList = new List<PoliceControlModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `controls`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                float posX = reader.GetFloat("posX");
                float posY = reader.GetFloat("posY");
                float posZ = reader.GetFloat("posZ");
                float rot = reader.GetFloat("rotation");

                PoliceControlModel policeControl = new PoliceControlModel
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Item = (PoliceControlItems)reader.GetInt32("item"),
                    Position = new Vector3(posX, posY, posZ),
                    Rotation = new Vector3(0.0f, 0.0f, rot)
                };

                Police.policeControlList.Add(policeControl);
            }
        }

        public static void LoadAllParkings()
        {
            // Initialize the parking list
            Parking.ParkingList = new Dictionary<int, ParkingModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `parkings`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                float posX = reader.GetFloat("posX");
                float posY = reader.GetFloat("posY");
                float posZ = reader.GetFloat("posZ");

                ParkingModel parking = new ParkingModel
                {
                    Id = reader.GetInt32("id"),
                    Type = (ParkingTypes)reader.GetInt32("type"),
                    HouseId = reader.GetInt32("house"),
                    Capacity = reader.GetInt32("capacity"),
                    Position = new Vector3(posX, posY, posZ)
                };

                Parking.ParkingList.Add(parking.Id, parking);
            }

            // Create the parkings ingame
            Parking.GenerateIngameParkings();
        }

        public static async Task<int> AddParking(ParkingModel parking)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `parkings` (`type`, `posX`, `posY`, `posZ`) VALUES (@type, @posX, @posY, @posZ)";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@type", parking.Type }, { "@posX", parking.Position.X }, { "@posY", parking.Position.Y }, { "@posZ", parking.Position.Z }
            };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return (int)command.LastInsertedId;
        }

        public static void UpdateParking(ParkingModel parking)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `parkings` SET `type` = @type, `house` = @house, `posX` = @posX, `posY` = @posY, ";
            query += "`posZ` = @posZ, `capacity` = @capacity WHERE `id` = @id LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@type", parking.Type }, { "@house", parking.HouseId }, { "@posX", parking.Position.X }, { "@posY", parking.Position.Y },
                { "@posZ", parking.Position.Z }, { "@capacity", parking.Capacity }, { "@id", parking.Id }
            };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void RenamePoliceControl(string sourceName, string targetName)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `controls` SET `name` = @targetName WHERE `name` = @sourceName";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@targetName", targetName }, { "@sourceName", sourceName } };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static async Task<int> AddPoliceControlItem(PoliceControlModel policeControlItem)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `controls` (`name`, `item`, `posX`, `posY`, `posZ`, `rotation`) VALUES (@name, @item, @posX, @posY, @posZ, @rotation)";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@name", policeControlItem.Name }, { "@item", policeControlItem.Item }, { "@posX", policeControlItem.Position.X },
                { "@posY", policeControlItem.Position.Y }, { "@posZ", policeControlItem.Position.Z }, { "@rotation", policeControlItem.Rotation.Z }
            };
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return (int)command.LastInsertedId;
        }

        public static async Task<List<FineModel>> LoadPlayerFines(string name)
        {
            List<FineModel> fineList = new List<FineModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "SELECT * FROM `fines` WHERE `target` = @target";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@target", name } };
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                FineModel fine = new FineModel
                {
                    officer = reader.GetString(reader.GetOrdinal("officer")),
                    target = reader.GetString(reader.GetOrdinal("target")),
                    amount = reader.GetInt32(reader.GetOrdinal("amount")),
                    reason = reader.GetString(reader.GetOrdinal("reason")),
                    date = reader.GetDateTime(reader.GetOrdinal("date"))
                };

                fineList.Add(fine);
            }

            return fineList;
        }

        public static void InsertFine(FineModel fine)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "INSERT INTO `fines` VALUES (@officer, @target, @amount, @reason, NOW())";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@officer", fine.officer }, { "@target", fine.target }, { "@amount", fine.amount }, { "@reason", fine.reason }
            };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void RemoveFines(List<FineModel> fineList)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "DELETE FROM `fines` WHERE `officer` = @officer AND `target` = @target AND `date` = @date LIMIT 1";
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, new Dictionary<string, object>());

            foreach (FineModel fine in fineList)
            {
                command.Parameters.Clear();

                command.Parameters.AddWithValue("@officer", fine.officer);
                command.Parameters.AddWithValue("@target", fine.target);
                command.Parameters.AddWithValue("@date", fine.date);

                command.ExecuteNonQuery();
            }
        }

        public static void LoadAllChannels()
        {
            Faction.ChannelList = new Dictionary<int, ChannelModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `channels`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ChannelModel channel = new ChannelModel
                {
                    Id = reader.GetInt32("id"),
                    Owner = reader.GetInt32("owner"),
                    Password = reader.GetString("password")
                };

                Faction.ChannelList.Add(channel.Id, channel);
            }
        }

        public static async Task<int> AddChannel(ChannelModel channel)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `channels` (`owner`, `password`) VALUES (@owner, @password)";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@owner", channel.Owner }, { "@password", channel.Password } };
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return (int)command.LastInsertedId;
        }

        public static void UpdateChannel(ChannelModel channel)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `channels` SET `password` = @password WHERE `id` = @id LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@password", channel.Password }, { "@id", channel.Id } };
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void DisconnectFromChannel(int channelId)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `users` SET `radio` = 0 WHERE `radio` = @radio";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@radio", channelId } };
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void LoadAllBlood()
        {
            Emergency.BloodList = new List<BloodModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `blood`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                BloodModel blood = new BloodModel
                {
                    Id = reader.GetInt32("id"),
                    Doctor = reader.GetInt32("doctor"),
                    Patient = reader.GetInt32("patient"),
                    Used = reader.GetBoolean("used")
                };

                Emergency.BloodList.Add(blood);
            }
        }

        public static async Task<int> AddBloodTransaction(BloodModel blood)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `blood` (`doctor`, `patient`, `bloodType`, `used`, `date`) ";
            query += "VALUES (@doctor, @patient, @bloodType, @used, NOW())";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@doctor", blood.Doctor }, { "@patient", blood.Patient }, { "@bloodType", blood.Type }, { "@used", blood.Used }
            };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return (int)command.LastInsertedId;
        }

        public static void LoadAllAnnoucements()
        {
            WeazelNews.AnnoucementList = new List<AnnoucementModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `news`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                AnnoucementModel announcementModel = new AnnoucementModel
                {
                    Id = reader.GetInt32("id"),
                    Winner = reader.GetInt32("journalist"),
                    Amount = reader.GetInt32("amount"),
                    Annoucement = reader.GetString("annoucement"),
                    Journalist = reader.GetInt32("winner"),
                    Given = reader.GetBoolean("given")
                };

                WeazelNews.AnnoucementList.Add(announcementModel);
            }
        }

        public static async Task<int> AddNewsTransaction(AnnoucementModel annoucement)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `news` (`winner`, `journalist`, `annoucement`, `amount`, `given`, `date`) ";
            query += "VALUES (@winner, @journalist, @annoucement, @amount, @given, NOW())";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@winner", annoucement.Winner }, { "@journalist", annoucement.Journalist }, { "@annoucement", annoucement.Annoucement },
                { "@amount", annoucement.Amount }, { "@given", annoucement.Given }
            };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return (int)command.LastInsertedId;
        }

        public static void LoadAllClothes()
        {
            Customization.clothesList = new List<ClothesModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `clothes`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ClothesModel clothes = new ClothesModel
                {
                    id = reader.GetInt32("id"),
                    player = reader.GetInt32("player"),
                    type = reader.GetInt32("type"),
                    slot = reader.GetInt32("slot"),
                    drawable = reader.GetInt32("drawable"),
                    texture = reader.GetInt32("texture"),
                    dressed = reader.GetBoolean("dressed"),
                    Stored = reader.GetBoolean("stored")
                };

                Customization.clothesList.Add(clothes);
            }
        }

        public static async Task<int> AddClothes(ClothesModel clothes)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `clothes` (`player`, `type`, `slot`, `drawable`, `texture`, `dressed`, `stored`) ";
            query += "VALUES (@player, @type, @slot, @drawable, @texture, @dressed, @stored)";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@player", clothes.player }, { "@type", clothes.type }, { "@slot", clothes.slot }, { "@drawable", clothes.drawable },
                { "@texture", clothes.texture }, { "@dressed", clothes.dressed }, { "@stored", clothes.Stored }
            };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return (int)command.LastInsertedId;
        }

        public static void UpdateClothes(ClothesModel clothes)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `clothes` SET `dressed` = @dressed, `stored` = @stored WHERE `id` = @id LIMIT 1";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@dressed", clothes.dressed }, { "@stored", clothes.Stored }, { "@id", clothes.id }
            };

            // Execute the command
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void LoadAllTattoos()
        {
            Customization.tattooList = new List<TattooModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `tattoos`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                TattooModel tattoo = new TattooModel
                {
                    player = reader.GetInt32("player"),
                    slot = reader.GetInt32("zone"),
                    library = reader.GetString("library"),
                    hash = reader.GetString("hash")
                };

                Customization.tattooList.Add(tattoo);
            }
        }

        public static bool AddTattoo(TattooModel tattoo)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "INSERT INTO `tattoos` (`player`, `zone`, `library`, `hash`) VALUES (@player, @zone, @library, @hash)";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@player", tattoo.player }, { "@zone", tattoo.slot }, { "@library", tattoo.library }, { "@hash", tattoo.hash }
            };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();

            return true;
        }

        public static void LoadAllPhones()
        {
            Telephone.phoneList = new Dictionary<int, PhoneModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `phones`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                PhoneModel phone = new PhoneModel
                {
                    ItemId = reader.GetInt32("itemId"),
                    Number = reader.GetInt32("number")
                };

                Telephone.phoneList.Add(phone.Number, phone);
            }

            if (Telephone.phoneList.Count > 0)
            {
                // Add the contacts to their phones
                Telephone.AddPhoneContacts();
            }
        }

        public static void AddPhoneNumber(PhoneModel phone, string owner)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "INSERT INTO `phones` (`itemId`, `owner`, `number`) VALUES (@id, @owner, @number)";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@id", phone.ItemId }, { "@owner", owner }, { "@number", phone.Number }
            };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static Dictionary<int, ContactModel> LoadAllContacts()
        {
            Dictionary<int, ContactModel> contactList = new Dictionary<int, ContactModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `contacts`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                ContactModel contact = new ContactModel
                {
                    Id = reader.GetInt32("id"),
                    Owner = reader.GetInt32("owner"),
                    ContactNumber = reader.GetInt32("contactNumber"),
                    ContactName = reader.GetString("contactName")
                };

                contactList.Add(contact.Id, contact);
            }

            return contactList;
        }

        public static async Task<int> AddNewContact(ContactModel contact)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `contacts` (`owner`, `contactNumber`, `contactName`) VALUES (@owner, @contactNumber, @contactName)";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@owner", contact.Owner }, { "@contactNumber", contact.ContactNumber }, { "@contactName", contact.ContactName }
            };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return (int)command.LastInsertedId;
        }

        public static void ModifyContact(ContactModel contact)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `contacts` SET `contactNumber` = @contactNumber, `contactName` = @contactName WHERE `id` = @id LIMIT 1";

            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@contactNumber", contact.ContactNumber }, { "@contactName", contact.ContactName }, { "@id", contact.Id }
            };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void AddPhoneLog(int phone, int target, object value)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Get the column name
            string column = value is string ? "message" : "time";

            // Generate the command
            string query = "INSERT INTO `calls` (`phone`, `target`, `" + column + "` , `date`) VALUES (@phone, @target, @value, NOW())";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@phone", phone }, { "@target", target }, { "@value", value } };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static async Task<List<TestModel>> GetRandomQuestions(int license, int questions)
        {
            List<TestModel> testList = new List<TestModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "SELECT DISTINCT(`id`) AS `id`, `question` FROM `questions` WHERE `license` = @license ORDER BY RAND() LIMIT @questions";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@license", license }, { "@questions", questions } };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                TestModel test = new TestModel
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    text = reader.GetString(reader.GetOrdinal("question"))
                };

                testList.Add(test);
            }

            return testList;
        }

        public static List<TestModel> GetQuestionAnswers(List<int> questionIds)
        {
            List<TestModel> testList = new List<TestModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "SELECT `id`, `question`, `answer` FROM answers WHERE FIND_IN_SET(`question`, @question) != 0";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@question", string.Join(",", questionIds) } };

            // Execute the query
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                TestModel test = new TestModel
                {
                    id = reader.GetInt32("id"),
                    question = reader.GetInt32("question"),
                    text = reader.GetString("answer")
                };

                testList.Add(test);
            }

            return testList;
        }

        public static async Task<List<TestModel>> GetQuestionAnswers(int question)
        {
            List<TestModel> testList = new List<TestModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "SELECT `id`, `answer` FROM `answers` WHERE `question` = @question";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@question", question } };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                TestModel test = new TestModel
                {
                    id = reader.GetInt32(reader.GetOrdinal("id")),
                    text = reader.GetString(reader.GetOrdinal("answer")),
                    question = question
                };

                testList.Add(test);
            }

            return testList;
        }

        public static async Task<int> CheckCorrectAnswers(Dictionary<int, int> application)
        {
            int mistakes = 0;

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "SELECT `id`, `question` FROM `answers` WHERE FIND_IN_SET(`question`, @questions) != 0 AND `correct` = 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@questions", string.Join(",", new List<int>(application.Keys)) } };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                int answerId = reader.GetInt32(reader.GetOrdinal("id"));
                int questionId = reader.GetInt32(reader.GetOrdinal("question"));

                if (application[questionId] != answerId)
                {
                    // Add a mistake
                    mistakes++;
                }
            }

            return mistakes;
        }

        public static bool CheckAnswerCorrect(int answer)
        {
            bool correct = false;

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "SELECT `correct` FROM `answers` WHERE `id` = @id LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@id", answer } };

            // Execute the query
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using MySqlDataReader reader = command.ExecuteReader();

            if (reader.HasRows)
            {
                reader.Read();
                correct = reader.GetBoolean("correct");
            }

            return correct;
        }

        public static void AddAdminLog(string admin, string player, string action, int time, string reason)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "INSERT INTO `admin` (`source`, `target`, `action`, `time`, `reason`, `date`) VALUES (@source, @target, @action, @time, @reason, CURRENT_TIMESTAMP)";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@source", admin }, { "@target", player }, { "@action", action }, { "@time", time }, { "@reason", reason }
            };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void AddLicensedWeapon(int itemId, string buyer)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "INSERT INTO `licensed` (`item`, `buyer`, `date`) VALUES (@item, @buyer, NOW())";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@item", itemId }, { "@buyer", buyer } };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void LoadAllPermissions()
        {
            // Initialize the permission list
            Admin.PermissionList = new List<PermissionModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `permissions`", new Dictionary<string, object>());

            using MySqlDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                PermissionModel permission = new PermissionModel
                {
                    PlayerId = reader.GetInt32("playerId"),
                    Command = reader.GetString("command"),
                    Option = reader.GetString("option")
                };

                Admin.PermissionList.Add(permission);
            }
        }

        public static void LoadAllPlants()
        {
            // Initialize the plant list
            Drugs.Plants = new List<PlantModel>();

            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, "SELECT * FROM `plants`", new Dictionary<string, object>());

            using DbDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                float posX = reader.GetFloat(reader.GetOrdinal("posX"));
                float posY = reader.GetFloat(reader.GetOrdinal("posY"));
                float posZ = reader.GetFloat(reader.GetOrdinal("posZ"));

                PlantModel plant = new PlantModel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Position = new Vector3(posX, posY, posZ),
                    Dimension = Convert.ToUInt32(reader.GetInt32(reader.GetOrdinal("dimension"))),
                    GrowTime = reader.GetInt32(reader.GetOrdinal("growth"))
                };

                // Add the plant to the list
                Drugs.Plants.Add(plant);
            }

            // Initialize the plants growth
            foreach (PlantModel plant in Drugs.Plants) Drugs.UpdatePlant(plant);
        }

        public static async Task<int> AddPlant(PlantModel plant)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            // Generate the command
            string query = "INSERT INTO `plants` (`posX`, `posY`, `posZ`, `dimension`, `growth`) VALUES (@posX, @posY, @posZ, @dimension, @growth)";
            Dictionary<string, object> parameters = new Dictionary<string, object>()
            {
                { "@posX", plant.Position.X }, { "@posY", plant.Position.Y }, { "@posZ", plant.Position.Z },
                { "@dimension", plant.Dimension }, { "@growth", plant.GrowTime }
            };

            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            return (int)command.LastInsertedId;
        }

        public static void ModifyPlant(int plantId, int growth)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "UPDATE `plants` SET `growth` = @growth WHERE `id` = @id LIMIT 1";
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@growth", growth }, { "@id", plantId } };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static void DeleteSingleRow(string table, string filter, object value)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = string.Format("DELETE FROM `{0}` WHERE `{1}` = @value LIMIT 1", table, filter);
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "@value", value } };

            // Execute the query
            DatabaseManager.GenerateCommand(connection, query, parameters).ExecuteNonQuery();
        }

        public static async Task<List<PokerTable>> GetPokerTables()
        {
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            string query = "SELECT * FROM `poker_tables`";
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, new Dictionary<string, object>());

            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            List<PokerTable> pokerTables = new List<PokerTable>();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(reader.GetOrdinal("id"));
                    float x= reader.GetFloat(reader.GetOrdinal("x"));
                    float y= reader.GetFloat(reader.GetOrdinal("y"));
                    float z= reader.GetFloat(reader.GetOrdinal("z"));
                    int dimension = reader.GetInt16(reader.GetOrdinal("dimension"));
                    List<PokerTableSit> pokerSits = await GetPokerTableSits(id);

                    PokerTable pokerTable = new PokerTable(id,new Vector3(x,y,z),pokerSits,(uint)dimension);
                   /* PokerTable pokerTable = new PokerTable(reader.GetInt32(reader.GetOrdinal("id")),
                            new Vector3(reader.GetFloat(reader.GetOrdinal("x")),
                            reader.GetFloat(reader.GetOrdinal("y")), reader.GetFloat(reader.GetOrdinal("z"))),
                        await GetPokerTableSits(reader.GetInt32(reader.GetOrdinal("id"))), (uint)reader.GetInt16(reader.GetOrdinal("dimension")));
                    //pokerTables.Append(pokerTable);

                    */
                    pokerTables.Add(pokerTable);
                }
            }

            return pokerTables;
        }

        public static async Task<List<PokerTableSit>> GetPokerTableSits(int id)
        {
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            await connection.OpenAsync().ConfigureAwait(false);

            string query = "SELECT * FROM poker_sits WHERE pokerTable = @id";
            Dictionary<string, object> parameters = new Dictionary<string, object>
            {
                { "@id", id },
            };
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);

            using DbDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);

            List<PokerTableSit> pokerSits = new List<PokerTableSit>();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    PokerTableSit pokerTable = new PokerTableSit(reader.GetInt32(reader.GetOrdinal("id")),
                        new Vector3(reader.GetFloat(reader.GetOrdinal("x")),
                            reader.GetFloat(reader.GetOrdinal("y")), reader.GetFloat(reader.GetOrdinal("z"))),
                        new Vector3(reader.GetFloat(reader.GetOrdinal("rotX")),
                            reader.GetFloat(reader.GetOrdinal("rotY")), reader.GetFloat(reader.GetOrdinal("rotZ"))));

                    pokerSits.Add(pokerTable);
                }
            }

            return pokerSits;
        }

        public static void AddPokerSits(int pokerTableID, Vector3 position, Vector3 rotation)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "INSERT INTO poker_sits(x, y, z, rotX, rotY, rotZ, pokerTable) VALUES(@x, @y, @z, @rotX, @rotY, @rotZ, @pokerTable)";
            Dictionary<string, object> parameters = new Dictionary<string, object> { { "@x", position.X }, { "@y", position.Y }, { "@z", position.Z }, { "@rotX", rotation.X }, { "@rotY", rotation.Y }, { "@rotZ", rotation.Z }, { "@pokerTable", pokerTableID } };

            // Execute the query
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);
            command.ExecuteNonQuery();
            PokerTable pokerTable = Poker.PokerTables.Single(p => p.Id == pokerTableID);
            pokerTable.Sits.Add(new PokerTableSit((int)command.LastInsertedId, position, rotation));
        }

        public static void AddPokerTable(Vector3 position, uint dimension)
        {
            // Create the connection
            using MySqlConnection connection = new MySqlConnection(DatabaseManager.ConnectionString);
            connection.Open();

            // Generate the command
            string query = "INSERT INTO poker_tables(x,y,z,dimension) VALUES(@x, @y, @z, @dimension)";
            Dictionary<string, object> parameters = new Dictionary<string, object> { { "@x", position.X }, { "@y", position.Y }, { "@z", position.Z }, { "@dimension", dimension } };

            // Execute the query
            MySqlCommand command = DatabaseManager.GenerateCommand(connection, query, parameters);
            command.ExecuteNonQuery();
            Poker.PokerTables.Add(new PokerTable((int)command.LastInsertedId, position, new List<PokerTableSit> { }, dimension));
        }
    }
}