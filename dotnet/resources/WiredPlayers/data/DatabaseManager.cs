using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace WiredPlayers.Data
{
    public class DatabaseManager
    {
        public static string ConnectionString;

        public static void InitializeDatabaseConnection(Script scriptClass)
        {
            // Create the database connection string
            string host = NAPI.Resource.GetSetting<string>(scriptClass, "host");
            string user = NAPI.Resource.GetSetting<string>(scriptClass, "username");
            string pass = NAPI.Resource.GetSetting<string>(scriptClass, "password");
            string db = NAPI.Resource.GetSetting<string>(scriptClass, "database");
            string ssl = NAPI.Resource.GetSetting<string>(scriptClass, "ssl");
            ConnectionString = "SERVER=" + host + "; DATABASE=" + db + "; UID=" + user + "; PASSWORD=" + pass + "; SSLMODE=" + ssl + ";";

            // Check Database connection
            if (CheckMySqlConnection())
            {
                // Load all the database data
                LoadDatabaseData();
            }
        }

        private static bool CheckMySqlConnection()
        {
            // Declare the new connection
            using MySqlConnection con = new MySqlConnection(ConnectionString);

            try
            {
                con.Open();
                return true;
            }
            catch (Exception ex)
            {
                NAPI.Util.ConsoleOutput("Error starting the database: " + ex);
                return false;
            }
        }

        private static void LoadDatabaseData()
        {
            // Animation loading
            DatabaseOperations.LoadAllAnimationCategories();

            // Building loading
            DatabaseOperations.LoadAllBusinesses();
            DatabaseOperations.LoadAllHouses();
            DatabaseOperations.LoadAllInteriors();

            // Furniture loading
            DatabaseOperations.LoadAllFurniture();

            // Tunning loading
            DatabaseOperations.LoadAllTunning();

            // Vehicle loading
            DatabaseOperations.LoadAllDealerVehicles();
            DatabaseOperations.LoadAllParkings();
            DatabaseOperations.LoadAllVehicles();

            // Item loading
            DatabaseOperations.LoadAllItems();

            // Phone contacts loading
            DatabaseOperations.LoadAllPhones();

            // Drug plants loading
            DatabaseOperations.LoadAllPlants();

            // Crimes loading
            DatabaseOperations.LoadCrimes();

            // Police controls loading
            DatabaseOperations.LoadAllPoliceControls();

            // Radio frequency channels loading
            DatabaseOperations.LoadAllChannels();

            // Blood units loading
            DatabaseOperations.LoadAllBlood();

            // Announcements loading
            DatabaseOperations.LoadAllAnnoucements();

            // Clothes loading
            DatabaseOperations.LoadAllClothes();

            // Tattoos loading
            DatabaseOperations.LoadAllTattoos();

            // Special permission loading
            DatabaseOperations.LoadAllPermissions();
        }

        public static MySqlCommand GenerateCommand(MySqlConnection connection, string query, Dictionary<string, object> parameters)
        {
            MySqlCommand command = connection.CreateCommand();

            // Add the query text to the command
            command.CommandText = query;

            foreach (KeyValuePair<string, object> entry in parameters)
            {
                // Add each one of the parameters to the query
                command.Parameters.AddWithValue(entry.Key, entry.Value);
            }

            return command;
        }
    }
}
