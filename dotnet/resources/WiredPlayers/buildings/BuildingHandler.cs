using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using WiredPlayers.Buildings.Businesses;
using WiredPlayers.Buildings.Houses;
using WiredPlayers.Data.Base;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.help;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Buildings
{
    public class BuildingHandler : Script
    {
        public static void HandleBuildingEnter(Player player, BuildingTypes type, int buildingId)
        {
            // Get the property given the identifier
            PropertyModel property = GetPropertyById(buildingId, type);

            if (property is BusinessModel business && business.Locked && !Business.HasPlayerBusinessKeys(player, business))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.business_locked);
                return;
            }

            if (property is HouseModel house && house.Locked && !House.HasPlayerHouseKeys(player, house))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.house_locked);
                return;
            }

            // Set the player into the property
            player.Position = property.Ipl.ExitPoint;
            player.Dimension = Convert.ToUInt32(buildingId);

            // Handle the building enter
            PlacePlayerIntoBuilding(player, property, new BuildingModel() { Id = buildingId, Type = type });
        }

        public static void PlacePlayerIntoBuilding(Player player, PropertyModel propertyModel, BuildingModel building)
        {
            // Set the new building for the player
            player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered = building;

            // Load the elements from the building
            GenerateBuildingElements(player, building.Type, Convert.ToInt32(propertyModel.Ipl.Type), propertyModel.Ipl.Name);
        }

        public static void PlacePlayerIntoBuilding(Player source, Player target)
        {
            // Get the source's building
            BuildingModel building = source.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

            //Set the building for the target
            target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered = building;

            // Get the building IPL and type
            InteriorModel interior = GenericInterior.GetInteriorById(building.Id);

            // Load the elements from the building
            GenerateBuildingElements(target, building.Type, Convert.ToInt32(interior.Ipl.Type), interior.Ipl.Name);
        }

        public static void HandleBuildingExit(Player player, PropertyModel property)
        {
            if (property is BusinessModel business)
            {
                // Check the business conditions
                if (business.Locked && !Business.HasPlayerBusinessKeys(player, business))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.business_locked);
                    return;
                }

                if (player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).RobberyStart > 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.stealing_progress);
                    return;
                }
            }

            if (property is HouseModel house)
            {
                // Check the house conditions
                if (house.Locked && !House.HasPlayerHouseKeys(player, house))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.house_locked);
                    return;
                }

                if (player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).RobberyStart > 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.stealing_progress);
                    return;
                }
            }

            if (GenericInterior.GetExitPoints(Convert.ToInt32(property.Ipl.Type)).Count > 1)
            {
                // Get the closest exit point
                InteriorModel closestInterior = GenericInterior.InteriorCollection.Values.Where(i => i.Ipl.Name == property.Ipl.Name).OrderBy(i => player.Position.DistanceTo2D(i.Ipl.ExitPoint)).First();

                // Handle the building exit
                RemovePlayerFromBuilding(player, closestInterior.Entrance, closestInterior.Dimension);
            }
            else
            {
                // Make the player exit the interior
                RemovePlayerFromBuilding(player, property.Entrance, property.Dimension);
            }
        }

        public static void RemovePlayerFromBuilding(Player player, Vector3 position, uint dimension)
        {
            // Place the player on the new place
            player.Position = position;
            player.Dimension = dimension;

            // Get the building entered
            BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

            if (building.Type != BuildingTypes.None)
            {
                // Destroy the ColShapes and Markers
                player.TriggerEvent("DestroyDimensionColShapes", GetPropertyById(building.Id, building.Type).Ipl.Name);
            }

            // Unload the interior of the player
            SetDefaultBuilding(player);
        }

        public static void GetBuildingInformation(Player player, ref Vector3 position, ref uint dimension, ref string name)
        {
            // Get the player building
            PropertyModel property = new PropertyModel();
            BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

            switch (building.Type)
            {
                case BuildingTypes.Interior:
                    // Get the interior based on the id
                    property = GenericInterior.GetInteriorById(building.Id);
                    break;

                case BuildingTypes.Business:
                    // Get the business based on the id
                    property = Business.GetBusinessById(building.Id);
                    break;

                case BuildingTypes.House:
                    // Get the house based on the id
                    property = House.GetHouseById(building.Id);
                    break;
            }

            // Get the values required
            position = property.Entrance;
            dimension = property.Dimension;
            name = property.Caption;
        }

        public static string GetAvailableBusinesses()
        {
            // Get the output list
            List<string> descriptions = new List<string>();

            foreach (IplModel ipl in Constants.BuildingIplCollection.Where(b => b.Type is BusinessTypes).ToArray())
            {
                // Add each description
                descriptions.Add(Convert.ChangeType(ipl.Type, ipl.Type.GetTypeCode()) + ".- " + ipl.Description);
            }

            // Return the message with the whole list
            return string.Format(GenRes.available_types, string.Join(" | ", descriptions));
        }

        public static string GetAvailableHouses()
        {
            // Get the output list
            List<string> descriptions = new List<string>();

            foreach (IplModel ipl in Constants.BuildingIplCollection.Where(b => b.Type is HouseTypes).ToArray())
            {
                // Add each description
                descriptions.Add(Convert.ChangeType(ipl.Type, ipl.Type.GetTypeCode()) + ".- " + ipl.Description);
            }

            // Return the message with the whole list
            return string.Format(GenRes.available_types, string.Join(" | ", descriptions));
        }

        public static string GetAvailableInteriors()
        {
            // Get the output list
            List<string> descriptions = new List<string>();

            foreach (IplModel ipl in Constants.BuildingIplCollection.Where(b => b.Type is InteriorTypes).ToArray())
            {
                // Add each description
                descriptions.Add(Convert.ChangeType(ipl.Type, ipl.Type.GetTypeCode()) + ".- " + ipl.Description);
            }

            // Return the message with the whole list
            return string.Format(GenRes.available_types, string.Join(" | ", descriptions));
        }

        public static bool IsIntoBuilding(Player player)
        {
            // Check if the player is in any building
            return player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered.Type != BuildingTypes.None;
        }

        public static void SetDefaultBuilding(Player player)
        {
            // Reset the player building
            player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered = new BuildingModel() { Id = 0, Type = BuildingTypes.None };
        }

        public static PropertyModel GetPropertyById(int id, BuildingTypes type)
        {
            List<PropertyModel> properties = new List<PropertyModel>();

            switch (type)
            {
                case BuildingTypes.Business:
                    properties.AddRange(Business.BusinessCollection.Values);
                    break;

                case BuildingTypes.House:
                    properties.AddRange(House.HouseCollection.Values);
                    break;

                case BuildingTypes.Interior:
                    properties.AddRange(GenericInterior.InteriorCollection.Values);
                    break;
            }

            return properties.First(p => p.Id == id);
        }

        public static IplModel GetBusinessIpl(int value)
        {
            // Return the business corresponding to the value
            return Constants.BuildingIplCollection.First(i => i.Type is BusinessTypes type && (int)type == value).Copy();
        }

        public static IplModel GetHouseIpl(int value)
        {
            // Return the business corresponding to the value
            return Constants.BuildingIplCollection.First(i => i.Type is HouseTypes type && (int)type == value).Copy();
        }

        public static IplModel GetInteriorIpl(int value)
        {
            // Return the business corresponding to the value
            return Constants.BuildingIplCollection.First(i => i.Type is InteriorTypes type && (int)type == value).Copy();
        }

        [RemoteEvent("PlayerExitBuilding")]
        public void PlayerExitBuildingRemoteEvent(Player player)
        {
            // Check if the player is in a building
            BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

            if (building.Type == BuildingTypes.None) return;

            // Get the player property
            PropertyModel property = GetPropertyById(building.Id, building.Type);

            // Force the player exit
            HandleBuildingExit(player, property);

            // Destroy the markers and ColShapes
            player.TriggerEvent("DestroyDimensionColShapes", property.Ipl.Name);
        }

        private static List<Vector3> GetExitPoints(BuildingTypes buildingType, int type)
        {
            List<Vector3> exitPoints = new List<Vector3>();
            
            switch (buildingType)
            {
                case BuildingTypes.Interior:
                    exitPoints.AddRange(GenericInterior.GetExitPoints(type));
                    break;
                    
                case BuildingTypes.Business:
                    exitPoints.Add(GetBusinessIpl(type).ExitPoint);
                    break;
                    
                case BuildingTypes.House:
                    exitPoints.Add(GetHouseIpl(type).ExitPoint);
                    break;
            }
            
            return exitPoints;
        }
        
        private static Vector3 GetActionPoint(BuildingTypes buildingType, int type)
        {
            Vector3 actionPoint = new Vector3();
            
            switch (buildingType)
            {
                case BuildingTypes.Interior:
                    actionPoint = GetInteriorIpl(type).ActionPoint;
                    break;
                    
                case BuildingTypes.Business:
                    actionPoint = GetBusinessIpl(type).ActionPoint;
                    break;
            }
            
            return actionPoint;
        }
        
        private static string[] GetBuildingMessages(BuildingTypes type)
        {
            string[] messages = new string[2];
        
            switch (type)
            {
                case BuildingTypes.Interior:
                    messages[0] = HelpRes.exit_interior;
                    messages[1] = HelpRes.action_tramitate;
                    break;
                    
                case BuildingTypes.Business:
                    messages[0] = HelpRes.exit_business;
                    messages[1] = HelpRes.action_business;
                    break;
                    
                case BuildingTypes.House:
                    messages[0] = HelpRes.exit_house;
                    messages[1] = string.Empty;
                    break;
            }
            
            return messages;
        }
        
        private static void GenerateBuildingElements(Player player, BuildingTypes buildingType, int type, string ipl)
        {
            // Get all the information required to generate the interior
            string exitPoints = NAPI.Util.ToJson(GetExitPoints(buildingType, type));
            Vector3 actionPoint = GetActionPoint(buildingType, type);
            string[] messages = GetBuildingMessages(buildingType);

            // Generate the ColShapes and markers inside the building
            player.TriggerEvent("GenerateDimensionColShapes", ipl, exitPoints, actionPoint, messages[0], messages[1], buildingType == BuildingTypes.Interior);
        }
    }
}
