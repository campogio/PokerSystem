using GTANetworkAPI;
using System;
using WiredPlayers.Buildings;
using WiredPlayers.Buildings.Businesses;
using WiredPlayers.Buildings.Houses;
using WiredPlayers.Curency;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.drugs;
using WiredPlayers.messages.error;
using WiredPlayers.messages.information;
using WiredPlayers.Server.Commands;
using WiredPlayers.Utility;
using WiredPlayers.vehicles;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Server
{
    public class KeyHandler : Script
    {
        [RemoteEvent("checkPlayerEventKeyStopAnim")]
        public void CheckPlayerEventKeyStopAnimRemoteEvent(Player player)
        {
            if (!player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).Animation)
            {
                // Stop the animation playing
                player.TriggerEvent("StopSecondaryAnimation");
            }
        }

        [RemoteEvent("checkPlayerInventoryKey")]
        public void CheckPlayerInventoryKeyRemoteEvent(Player player)
        {
            // Invoke the command associated
            UtilityCommands.InventoryCommand(player);
        }

        [RemoteEvent("checkPlayerEventKey")]
        public void CheckPlayerEventKeyRemoteEvent(Player player)
        {
            // Get the ColShape the player is inside
            ColShape colShape = player.GetData<ColShape>(EntityData.PlayerEnteredColShape);

            if (colShape == null || !colShape.Exists) return;

            // Get the type and identifier
            ColShapeTypes type = colShape.GetData<ColShapeTypes>(EntityData.ColShapeType);
            int id = colShape.HasData(EntityData.ColShapeId) ? colShape.GetData<int>(EntityData.ColShapeId) : 0;

            switch (type)
            {
                case ColShapeTypes.BusinessEntrance:
                    BuildingHandler.HandleBuildingEnter(player, BuildingTypes.Business, id);
                    break;

                case ColShapeTypes.BusinessPurchase:
                    UtilityCommands.PurchaseCommand(player);
                    break;

                case ColShapeTypes.HouseEntrance:
                    BuildingHandler.HandleBuildingEnter(player, BuildingTypes.House, id);
                    break;

                case ColShapeTypes.InteriorEntrance:
                    BuildingHandler.HandleBuildingEnter(player, BuildingTypes.Interior, id);
                    break;
                    
                case ColShapeTypes.VehicleDealer:
                    CarShop.ShowCatalog(player);
                    break;

                case ColShapeTypes.Atm:
                    Bank.ShowAtmWindow(player);
                    break;

                case ColShapeTypes.Plant:
                    // Check if the player is not picking any plant
                    if (!Drugs.PlantingTimer.ContainsKey(player.Value))
                    {
                        // Collect the weed
                        Drugs.AnimatePlayerWeedManagement(player, false);
                    }

                    break;
            }
        }
        
        [RemoteEvent("PlayerToggledAction")]
        public void PlayerToggledActionRemoteEvent(Player player)
        {
            // Check the building
            if (!BuildingHandler.IsIntoBuilding(player)) return;

            // Get the building where the player is
			BuildingModel building = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).BuildingEntered;

            switch(building.Type)
            {
                case BuildingTypes.Interior:
                    // Get the business given the id
                    InteriorModel interior = GenericInterior.GetInteriorById(building.Id);

                    if (interior != null && player.Position.DistanceTo(interior.Ipl.ActionPoint) < 1.5f)
                    {
                        // Show the purchase menu
                        GenericInterior.HandleAction(player, Convert.ToInt32(interior.Ipl.Type));
                    }
                    break;

                case BuildingTypes.Business:
                    // Get the business given the id
                    BusinessModel business = Business.GetBusinessById(building.Id);

                    if (business != null)
                    {
                        // Show the purchase menu
                        UtilityCommands.PurchaseCommand(player);
                    }

                    break;
            }
        }

        [RemoteEvent("engineOnEventKey")]
        public void EngineOnEventKeyEvent(Player player)
        {
            // Check if the player is in the right vehicle
            if (!player.IsInVehicle || player.Vehicle.Class == (int)VehicleClass.Cycle) return;
            
            // Get the player's vehicle info
            VehicleModel vehicle = Vehicles.GetVehicleById<VehicleModel>(player.Vehicle.GetData<int>(EntityData.VehicleId));

            if (vehicle.Testing && player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).TestingVehicle == player.Vehicle)
            {
                // Toggle the engine state
                player.Vehicle.EngineStatus = !player.Vehicle.EngineStatus;
                player.SendNotification(player.Vehicle.EngineStatus ? InfoRes.vehicle_turned_on : InfoRes.vehicle_turned_off);
                return;
            }

            // Get the player's character model
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).AlreadyFucking != null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.cant_toogle_engine_while_fucking);
                return;
            }
            
            if (player.Vehicle.HasData(EntityData.VehicleRefueling))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_start_refueling);
                return;
            }
            
            if (player.Vehicle.HasData(EntityData.VehicleWeaponUnpacking))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_start_weapon_unpacking);
                return;
            }
            
            if (!Vehicles.HasPlayerVehicleKeys(player, vehicle, false))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
                return;
            }
            
            if (characterModel.AdminRank == StaffRank.None && vehicle.Faction == (int)PlayerFactions.Admin)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.admin_vehicle);
                return;
            }
            
            if (vehicle.Faction > 0 && vehicle.Faction < Constants.MAX_FACTION_VEHICLES && (int)characterModel.Faction != vehicle.Faction && vehicle.Faction != (int)PlayerFactions.DrivingSchool && vehicle.Faction != (int)PlayerFactions.Admin)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle_faction);
                return;
            }
            
            if (vehicle.Faction > Constants.MAX_FACTION_VEHICLES && (int)characterModel.Job + Constants.MAX_FACTION_VEHICLES != vehicle.Faction)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle_job);
                return;
            }
            
            // Toggle the engine state
            player.Vehicle.EngineStatus = !player.Vehicle.EngineStatus;
            player.SendNotification(player.Vehicle.EngineStatus ? InfoRes.vehicle_turned_on : InfoRes.vehicle_turned_off);
        }
    }
}
