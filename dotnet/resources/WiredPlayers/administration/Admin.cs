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
using WiredPlayers.jobs;
using WiredPlayers.messages.administration;
using WiredPlayers.messages.arguments;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.help;
using WiredPlayers.Utility;
using WiredPlayers.vehicles;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Administration
{
    public static class Admin
    {
        public static List<PermissionModel> PermissionList;
        public static Dictionary<int, string> AdminTicketCollection = new Dictionary<int, string>();

        public static bool HasUserCommandPermission(Player player, string command, string option = "")
        {
            int playerId = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
            
            return PermissionList.Any(p => p.PlayerId == playerId && command == p.Command && option == string.Empty || option == p.Option);
        }
        
        public static void ShowVehicleInfo(Player player)
        {
            // Get the closest vehicle
            Vehicle vehicle = Vehicles.GetClosestVehicle(player, 2.5f);

            if (vehicle == null || !vehicle.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_vehicles_near);
                return;
            }

            // Get the vehicle's information
            VehicleModel vehModel = Vehicles.GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId));            
            
            player.SendChatMessage(Constants.COLOR_HELP + string.Format(GenRes.vehicle_check_title, vehModel.Id));
            player.SendChatMessage(Constants.COLOR_HELP + GenRes.vehicle_model + (VehicleHash)vehModel.Model);
            player.SendChatMessage(Constants.COLOR_HELP + GenRes.owner + vehModel.Owner);
        }
        
        public static void CreateAdminVehicle(Player player, string[] arguments)
        {
            if (arguments.Length != 4 || !UtilityFunctions.CheckColorStructure(arguments[2]) || !UtilityFunctions.CheckColorStructure(arguments[3]))
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.vehicle_create);
                return;
            }

            // Basic data for vehicle creation
            VehicleModel vehicle = new VehicleModel()
            {
                Model = NAPI.Util.GetHashKey(arguments[1]),
                Faction = (int)PlayerFactions.Admin,
                Position = UtilityFunctions.GetForwardPosition(player, 2.5f),
                Heading = player.Rotation.Z,
                Dimension = player.Dimension,
                ColorType = Constants.VEHICLE_COLOR_TYPE_CUSTOM,
                FirstColor = arguments[2],
                SecondColor = arguments[3],
                Pearlescent = 0,
                Owner = string.Empty,
                Plate = string.Empty,
                Price = 0,
                Parking = 0,
                Parked = 0,
                Gas = 50.0f,
                Kms = 0.0f
            };

            // Create the vehicle
            _ = Vehicles.CreateVehicle(player, vehicle, true);
        }
        
        public static void ModifyVehicleColor(Player player, string firstColor, string secondColor)
        {
            // Get the closest vehicle
            Vehicle vehicle = Vehicles.GetClosestVehicle(player, 2.5f);

            if (vehicle == null || !vehicle.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_vehicles_near);
                return;
            }
            
            // Update the vehicle's colors
            VehicleModel vehModel = Vehicles.GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId));
            vehModel.ColorType = Constants.VEHICLE_COLOR_TYPE_CUSTOM;
            vehModel.FirstColor = firstColor;
            vehModel.SecondColor = secondColor;
            vehModel.Pearlescent = 0;
            
            // Repaint the vehicle
            Mechanic.RepaintVehicle(vehicle, vehModel);
            
            // Create the dictionary with the key and values
            Dictionary<string, object> keyValues = new Dictionary<string, object>()
            {
                { "colorType", vehModel.ColorType },
                { "firstColor", vehModel.FirstColor },
                { "secondColor", vehModel.SecondColor },
                { "pearlescent", vehModel.Pearlescent }
            };

            // Update the vehicle's colors into the database
            Task.Run(() => DatabaseOperations.UpdateVehicleValues(keyValues, vehModel.Id)).ConfigureAwait(false);
        }
        
        public static void ChangeVehicleDimension(Player player, int vehicleId, uint dimension)
        {
            // Get the vehicle given the identifier
            Vehicle veh = Vehicles.GetVehicleById<Vehicle>(vehicleId);

            if (veh == null || !veh.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_exists);
                return;
            }

            veh.Dimension = dimension;
            Vehicles.GetVehicleById<VehicleModel>(veh.GetData<int>(EntityData.VehicleId)).Dimension = dimension;

            // Update the vehicle's dimension into the database
            Dictionary<string, object> keyValues = new Dictionary<string, object>() { { "dimension", dimension } };
            Task.Run(() => DatabaseOperations.UpdateVehicleValues(keyValues, vehicleId)).ConfigureAwait(false);

            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.vehicle_dimension_modified, dimension));
        }
        
        public static void ChangeVehicleFaction(Player player, int faction)
        {
            // Get the closest vehicle
            Vehicle veh = Vehicles.GetClosestVehicle(player, 2.5f);

            if (veh == null || !veh.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_vehicles_near);
                return;
            }
            
            // Update the vehicle's faction
            Vehicles.GetVehicleById<VehicleModel>(veh.GetData<int>(EntityData.VehicleId)).Faction = faction;

            // Update the vehicle's faction into the database
            Dictionary<string, object> keyValues = new Dictionary<string, object>() { { "faction", faction } };
            Task.Run(() => DatabaseOperations.UpdateVehicleValues(keyValues, veh.GetData<int>(EntityData.VehicleId))).ConfigureAwait(false);
            
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.vehicle_faction_modified, faction));
        }
        
        public static void UpdateVehicleSpawn(Player player)
        {
            // Get the vehicle from the player
            VehicleModel vehicle = Vehicles.GetVehicleById<VehicleModel>(player.Vehicle.GetData<int>(EntityData.VehicleId));
            
            // Set the new values
            vehicle.Position = player.Vehicle.Position;
            vehicle.Heading = player.Vehicle.Heading;
            
            // Create the dictionary with the key and values
            Dictionary<string, object> keyValues = new Dictionary<string, object>()
            {
                { "posX", vehicle.Position.X },
                { "posY", vehicle.Position.Y },
                { "posZ", vehicle.Position.Z },
                { "rotation", vehicle.Heading }
            };

            // Update the vehicle's position into the database
            Task.Run(() => DatabaseOperations.UpdateVehicleValues(keyValues, vehicle.Id)).ConfigureAwait(false);
            
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.vehicle_pos_updated);
        }
        
        public static void ChangeVehicleOwner(Player player, string owner)
        {
            // Get the closest vehicle
            Vehicle veh = Vehicles.GetClosestVehicle(player, 2.5f);

            if (veh == null || !veh.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_vehicles_near);
                return;
            }

            // Update the owner
            Vehicles.GetVehicleById<VehicleModel>(veh.GetData<int>(EntityData.VehicleId)).Owner = owner;

            // Update the vehicle's owner into the database
            Dictionary<string, object> keyValues = new Dictionary<string, object>() { { "owner", owner } };
            Task.Run(() => DatabaseOperations.UpdateVehicleValues(keyValues, veh.GetData<int>(EntityData.VehicleId))).ConfigureAwait(false);
            
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.vehicle_owner_modified, owner));
        }
        
        public static void RemoveVehicle(Player player, int vehicleId)
        {
            // Get the vehicle given the identifier
            Vehicle veh = Vehicles.GetVehicleById<Vehicle>(vehicleId);

            if (veh == null || !veh.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_exists);
                return;
            }

            // Remove the vehicle
            veh.Delete();
            Task.Run(() => DatabaseOperations.DeleteSingleRow("vehicles", "id", vehicleId)).ConfigureAwait(false);
        }
        
        public static void BringVehicle(Player player, int vehicleId)
        {
            // Get the vehicle given the identifier
            Vehicle veh = Vehicles.GetVehicleById<Vehicle>(vehicleId);

            if (veh == null || !veh.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_vehicles_near);
                return;
            }

            // Get the vehicle to the player's position
            veh.Position = UtilityFunctions.GetForwardPosition(player, 2.5f);
            Vehicles.GetVehicleById<VehicleModel>(veh.GetData<int>(EntityData.VehicleId)).Position = veh.Position;

            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.vehicle_bring, vehicleId));
        }
        
        public static void MovePlayerToVehicle(Player player, int vehicleId)
        {
            // Get the vehicle given the identifier
            VehicleModel veh = Vehicles.GetVehicleById<VehicleModel>(vehicleId);
            
            if (veh == null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_exists);
                return;
            }
            
            // Get the player to the vehicle's position
            Vector3 position = veh.Parking == 0 ? UtilityFunctions.GetForwardPosition(Vehicles.GetVehicleById<Vehicle>(vehicleId), 2.5f) : Parking.GetParkingById(veh.Parking).Position;
                
            // Teleport the player to the vehicle
            BuildingHandler.RemovePlayerFromBuilding(player, position, 0);
            
            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.vehicle_goto, vehicleId));
        }
        
        public static void ChangeVehicleGas(Player player, int vehicleId, float gas)
        {
            if (gas < 0 || gas > 50)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_gas_incorrect);
                return;
            }
            
            // Get the vehicle given the identifier
            VehicleModel veh = Vehicles.GetVehicleById<VehicleModel>(vehicleId);
            
            if (veh == null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_exists);
                return;
            }
            
            // Set the gasoline to the vehicle
            veh.Gas = gas;
            
            if (veh.Parked == 0)
            {
                // Search the driver from the vehicle
                Player driver = Vehicles.GetVehicleById<Vehicle>(vehicleId).Occupants.Cast<Player>().ToList().FirstOrDefault(o => o.VehicleSeat == (int)VehicleSeat.Driver);
                
                if (driver != null && driver.Exists)
                {
                    // Update the gas on the counter
                    driver.TriggerEvent("UpdateVehicleGas", gas);
                }
            }
            
            // Update the vehicle's gas into the database
            Dictionary<string, object> keyValues = new Dictionary<string, object>() { { "gas", gas } };
            Task.Run(() => DatabaseOperations.UpdateVehicleValues(keyValues, vehicleId)).ConfigureAwait(false);

            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.vehicle_gas, gas));
        }

        public static void ShowBusinessInfo(Player player, BusinessModel business)
        {
            player.SendChatMessage(string.Format(GenRes.business_check_title, business.Id));
            player.SendChatMessage(GenRes.name + business.Caption);
            player.SendChatMessage(GenRes.ipl + business.Ipl);
            player.SendChatMessage(GenRes.owner + business.Owner);
            player.SendChatMessage(GenRes.products + business.Products);
            player.SendChatMessage(GenRes.multiplier + business.Multiplier);
        }

        public static async Task CreateBusinessAsync(Player player, int type, string place)
        {
            // We get the business type
            BusinessModel business = new BusinessModel()
            {
                Caption = GenRes.business,
                Ipl = BuildingHandler.GetBusinessIpl(type),
                Entrance = player.Position,
                Dimension = place == ArgRes.inner ? player.Dimension : 0,
                Multiplier = 3.0f,
                Owner = string.Empty,
                Locked = false
            };

            if (place.Equals(ArgRes.outer, StringComparison.InvariantCultureIgnoreCase))
            {
                // Remove the IPL from the business
                business.Ipl.Name = string.Empty;
            }

            // Get the id from the business
            business.Id = await DatabaseOperations.AddNewBusiness(business).ConfigureAwait(false);
            
            // Generate business elements
            Business.GenerateBusinessElements(business);

            // Add the business to the collection
            Business.BusinessCollection.Add(business.Id, business);
        }
        
        public static void UpdateBusinessName(Player player, BusinessModel business, string businessName)
        {
            // We change business name
            business.Caption = businessName;
            business.Label.Text = businessName;

            // Update the business information
            Dictionary<int, BusinessModel> businessCollection = new Dictionary<int, BusinessModel>() { { business.Id, business } };
            Task.Run(() => DatabaseOperations.UpdateBusinesses(businessCollection)).ConfigureAwait(false);
            
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.business_name_modified, businessName));
        }
        
        public static void UpdateBusinessType(Player player, BusinessModel business, BusinessTypes businessType)
        {          
            // Changing business type
            business.Ipl = BuildingHandler.GetBusinessIpl((int)businessType);

            // Update the business information
            Dictionary<int, BusinessModel> businessCollection = new Dictionary<int, BusinessModel>() { { business.Id, business } };
            Task.Run(() => DatabaseOperations.UpdateBusinesses(businessCollection)).ConfigureAwait(false);
            
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.business_type_modified, businessType));
        }

        public static void SendHouseInfo(Player player, HouseModel house)
        {
            player.SendChatMessage(string.Format(GenRes.house_check_title, house.Id));
            player.SendChatMessage(GenRes.name + house.Caption);
            player.SendChatMessage(GenRes.ipl + house.Ipl);
            player.SendChatMessage(GenRes.owner + house.Owner);
            player.SendChatMessage(GenRes.price + house.Price);
            player.SendChatMessage(GenRes.status + house.State);
        }
        
        public static async Task CreateHouseAsync(Player player, int type)
        {
            HouseModel house = new HouseModel()
            {
                Ipl = BuildingHandler.GetHouseIpl(type),
                Caption = GenRes.house,
                Entrance = player.Position,
                Dimension = player.Dimension,
                Price = 10000,
                Owner = string.Empty,
                State = HouseState.Buyable,
                Tenants = 2,
                Rental = 0,
                Locked = true
            };

            // Add a new house
            house.Id = await DatabaseOperations.AddHouse(house).ConfigureAwait(false);
            
            house.Label = NAPI.TextLabel.CreateTextLabel(House.GetHouseLabelText(house), house.Entrance, 20.0f, 0.75f, 4, new Color(255, 255, 255));
            
            // Create the ColShape
            house.ColShape = NAPI.ColShape.CreateCylinderColShape(house.Entrance, 2.5f, 1.0f);
            house.ColShape.SetData(EntityData.ColShapeId, house.Id);
            house.ColShape.SetData(EntityData.ColShapeType, ColShapeTypes.HouseEntrance);
            house.ColShape.SetData(EntityData.InstructionalButton, HelpRes.enter_house);
            
            // Add the house to the dictionary
            House.HouseCollection.Add(house.Id, house);

            // Send the confirmation message
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.house_created);
        }
        
        public static void UpdateHousePrice(Player player, HouseModel house, int price)
        {
            house.Price = price;
            house.State = HouseState.Buyable;
            house.Label.Text = House.GetHouseLabelText(house);

            // Update the house's information
            Task.Run(() => DatabaseOperations.UpdateHouse(house)).ConfigureAwait(false);

            // Confirmation message sent to the player
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.house_price_modified, price));
        }
        
        public static void UpdateHouseState(Player player, HouseModel house, HouseState state)
        {
            house.State = state;
            house.Label.Text = House.GetHouseLabelText(house);

            // Update the house's information
            Task.Run(() => DatabaseOperations.UpdateHouse(house)).ConfigureAwait(false);

            // Confirmation message sent to the player
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.house_status_modified, state));
        }
        
        public static async Task CreateInteriorAsync(Player player, int type)
        {
            // Create the interior model
            InteriorModel interior = new InteriorModel()
            {
                Caption = GenRes.interior,
                Entrance = player.Position,
                Dimension = player.Dimension,
                Label = NAPI.TextLabel.CreateTextLabel(GenRes.interior, player.Position, 30.0f, 0.75f, 4, new Color(255, 255, 255)),
                ColShape = NAPI.ColShape.CreateCylinderColShape(player.Position, 2.5f, 1.0f, player.Dimension),
                Ipl = BuildingHandler.GetInteriorIpl(type)
            };

            // Add the interior into the database
            interior.Id = await DatabaseOperations.AddInteriorAsync(interior);
            GenericInterior.InteriorCollection.Add(interior.Id, interior);

            // Add the information to the ColShape
            interior.ColShape.SetData(EntityData.ColShapeId, interior.Id);
            interior.ColShape.SetData(EntityData.ColShapeType, ColShapeTypes.InteriorEntrance);
            interior.ColShape.SetData(EntityData.InstructionalButton, HelpRes.enter_building);
        }
        
        public static void ChangeInteriorType(Player player, InteriorModel interior, InteriorTypes type)
        {
            // Change the IPL and coordinates from the interior
            interior.Ipl = BuildingHandler.GetInteriorIpl((int)type);
            
            // Update the IPL on the database
            Task.Run(() => DatabaseOperations.UpdateInterior(interior)).ConfigureAwait(false);

            // Confirmation message sent to the player
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.interior_type_modified, type));
        }
        
        public static void ChangeInteriorBlip(Player player, InteriorModel interior, int blipId)
        {
            // Change the blip from the interior
            interior.BlipSprite = blipId;
            
            if (blipId == 0 && interior.Icon != null && interior.Icon.Exists)
            {
                // Remove the existing blip
                interior.Icon.Delete();
            }
            else if (interior.Icon != null && interior.Icon.Exists)
            {
                // Change the blip's model
                interior.Icon.Sprite = (uint)blipId;
            }
            else
            {
                // Create the blip
                interior.Icon = NAPI.Blip.CreateBlip(interior.BlipSprite, interior.Entrance, 1.0f, 0, interior.Caption);
                interior.Icon.ShortRange = true;
            }
            
            // Update the interior on the database
            Task.Run(() => DatabaseOperations.UpdateInterior(interior)).ConfigureAwait(false);

            // Confirmation message sent to the player
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.interior_blip_modified, interior.BlipSprite));
        }
    }
}
