using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.Utility;
using WiredPlayers.Data.Persistent;
using WiredPlayers.vehicles;
using WiredPlayers.Data;
using WiredPlayers.weapons;
using WiredPlayers.factions;
using WiredPlayers.character;
using WiredPlayers.messages.information;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.administration;
using WiredPlayers.messages.success;
using WiredPlayers.Currency;
using WiredPlayers.messages.help;
using WiredPlayers.messages.arguments;
using WiredPlayers.Administration;
using WiredPlayers.messages.commands;
using WiredPlayers.Buildings.Businesses;
using WiredPlayers.Buildings.Houses;
using WiredPlayers.Buildings;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class AdminCommands
    {
        [Command]
        public static void SkinCommand(Player player, string pedModel)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank > StaffRank.None)
            {
                PedHash pedHash = NAPI.Util.PedNameToModel(pedModel);
                player.SetSkin(pedHash);
            }
        }

        [Command]
        public static void AdminCommand(Player player, string message)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank > StaffRank.Support)
            {
                // We send the message to all the players in the server
                NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + GenRes.admin_notice + message);
            }
        }

        [Command]
        public static void CoordCommand(Player player, float posX, float posY, float posZ)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.Support) return;
            
            // Set the player into the position, out of any building
            BuildingHandler.RemovePlayerFromBuilding(player, new Vector3(posX, posY, posZ), 0);
        }

        [Command]
        public static void TpCommand(Player player, string targetString)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.Support) return;
            
            // We get the player from the input string
            Player target = int.TryParse(targetString, out int targetId) ? UtilityFunctions.GetPlayer(targetId) : NAPI.Player.GetPlayerFromName(targetString);

            if (!Character.IsPlaying(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }
            
            // We set the player into the building where the target is
            BuildingHandler.RemovePlayerFromBuilding(player, target.Position, target.Dimension);
            
            if (BuildingHandler.IsIntoBuilding(target))
            {
                // Add the building to the player
                BuildingHandler.PlacePlayerIntoBuilding(target, player);
            }

            // Confirmation message sent to the command executor
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.goto_player, target.Name));
        }

        [Command]
        public static void BringCommand(Player player, string targetString)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.Support) return;
            
            // We get the player from the input string
            Player target = UtilityFunctions.GetPlayer(targetString);

            if (!Character.IsPlaying(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }
            
            // We set the target into the building where the player is
            BuildingHandler.RemovePlayerFromBuilding(target, player.Position, player.Dimension);
            
            if (BuildingHandler.IsIntoBuilding(player))
            {
                // Add the building to the target
                BuildingHandler.PlacePlayerIntoBuilding(player, target);
            }

            // Confirmation message sent to the command executor
            target.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.bring_player, player.SocialClubName));
        }

        [Command]
        public static void GunCommand(Player player, string targetString, string weaponName, int ammo)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank > StaffRank.GameMaster)
            {
                // We get the player from the input string
                Player target = UtilityFunctions.GetPlayer(targetString);
                
                if (!Character.IsPlaying(target))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                    return;
                }

                if (Inventory.HasPlayerItemOnHand(target))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.target_right_hand_not_empty);
                    return;
                }

                // Get the weapon from the name
                WeaponHash weapon = NAPI.Util.WeaponNameToModel(weaponName);

                if (weapon == 0)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.gun);
                }
                else
                {
                    // Give the weapon to the player
                    Weapons.GivePlayerNewWeapon(target, weapon, ammo, false);
                }
            }
        }

        [Command]
        public static void VehicleCommand(Player player, string args)
        {
            // Get the staff rank from the player
            StaffRank rank = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank;

            if (rank == StaffRank.None) return;

            if (args == null || args.Trim().Length == 0)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.vehicle);
                return;
            }

            // Get the arguments from the parameters
            string[] arguments = args.Trim().Split(' ');

            if (arguments[0].Equals(ArgRes.info, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check the permission to execute the command
                if (rank <= StaffRank.Support) return;
                
                // Show the information related to the vehicle
                Admin.ShowVehicleInfo(player);

                return;
            }

            if (arguments[0].Equals(ArgRes.create, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check the permission to execute the command
                if (rank <= StaffRank.GameMaster) return;
                
                // Create a new vehicle bound to the staff
                Admin.CreateAdminVehicle(player, arguments);

                return;
            }

            if (arguments[0].Equals(ArgRes.modify, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check if it has the required arguments
                if (arguments.Length == 1)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.vehicle_modify);
                    return;
                }

                if (arguments[1].Equals(ArgRes.color, StringComparison.InvariantCultureIgnoreCase))
                {                   
                    // Check the permission to execute the command
                    if (rank <= StaffRank.Support) return;

                    if (arguments.Length != 4 || !UtilityFunctions.CheckColorStructure(arguments[2]) || !UtilityFunctions.CheckColorStructure(arguments[3]))
                    {
                        player.SendChatMessage(Constants.COLOR_HELP + HelpRes.vehicle_color);
                        return;
                    }

                    // Modify the vehicle's color
                    Admin.ModifyVehicleColor(player, arguments[2], arguments[3]);
                    
                    return;
                }

                if (arguments[1].Equals(ArgRes.dimension, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (arguments.Length != 4 || !int.TryParse(arguments[2], out int vehicleId) || !uint.TryParse(arguments[3], out uint dimension))
                    {
                        player.SendChatMessage(Constants.COLOR_HELP + HelpRes.vehicle_dimension);
                        return;
                    }

                    // Change the vehicle's dimension
                    Admin.ChangeVehicleDimension(player, vehicleId, dimension);

                    return;
                }

                if (arguments[1].Equals(ArgRes.faction, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Check the permission to execute the command
                    if (rank <= StaffRank.Support) return;

                    if (arguments.Length != 3 || !int.TryParse(arguments[2], out int faction))
                    {
                        player.SendChatMessage(Constants.COLOR_HELP + HelpRes.vehicle_faction);
                        return;
                    }

                    // Change the faction of the vehicle
                    Admin.ChangeVehicleFaction(player, faction);

                    return;
                }

                if (arguments[1].Equals(ArgRes.position, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Check the permission to execute the command
                    if (rank <= StaffRank.Support) return;
                    
                    if (!player.IsInVehicle)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle);
                        return;
                    }

                    // Update the vehicle's spawn position
                    Admin.UpdateVehicleSpawn(player);

                    return;
                }

                if (arguments[1].Equals(ArgRes.owner, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Check the permission to execute the command
                    if (rank <= StaffRank.Support) return;

                    if (arguments.Length != 4) 
                    {
                        player.SendChatMessage(Constants.COLOR_HELP + HelpRes.vehicle_owner);
                        return;
                    }

                    // Change the vehicle's owner
                    Admin.ChangeVehicleOwner(player, arguments[2] + " " + arguments[3]);

                    return;
                }

                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.vehicle_modify);
                return;
            }

            if (arguments[0].Equals(ArgRes.remove, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check the permission to execute the command
                if (rank <= StaffRank.GameMaster) return;

                if (arguments.Length != 2 || !int.TryParse(arguments[1], out int vehicleId))
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.vehicle_delete);
                    return;
                }

                // Remove the given vehicle
                Admin.RemoveVehicle(player, vehicleId);

                return;
            }

            if (arguments[0].Equals(ArgRes.repair, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check the permission to execute the command
                if (rank <= StaffRank.GameMaster) return;

                if (player.Vehicle == null || !player.Vehicle.Exists)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle);
                    return;
                }
                
                // Repair the player's vehicle
                player.Vehicle.Repair();
                
                // Send the message to the player
                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.vehicle_repaired);

                return;
            }

            if (arguments[0].Equals(ArgRes.lock_command, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check the permission to execute the command
                if (rank <= StaffRank.Support) return;

                // Get the closest vehicle
                Vehicle veh = Vehicles.GetClosestVehicle(player, 2.5f);

                if (veh == null || !veh.Exists)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_vehicles_near);
                    return;
                }

                // Toggle vehicle lock
                veh.Locked = !veh.Locked;
                
                // Send the message to the player
                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + (veh.Locked ? SuccRes.veh_locked : SuccRes.veh_unlocked));

                return;
            }

            if (arguments[0].Equals(ArgRes.start, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check the permission to execute the command
                if (rank <= StaffRank.Support) return;

                if (player.VehicleSeat != (int)VehicleSeat.Driver)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_vehicle_driving);
                    return;
                }

                // Turn the engine on
                player.Vehicle.EngineStatus = true;

                return;
            }

            if (arguments[0].Equals(ArgRes.bring, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check the permission to execute the command
                if (rank <= StaffRank.Support) return;

                if (arguments.Length != 2 || !int.TryParse(arguments[1], out int vehicleId))
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.vehicle_bring);
                    return;
                }

                // Teleport the vehicle to the player
                Admin.BringVehicle(player, vehicleId);
                
                return;
            }

            if (arguments[0].Equals(ArgRes.tp, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check the permission to execute the command
                if (rank <= StaffRank.Support) return;

                if (arguments.Length != 2 || !int.TryParse(arguments[1], out int vehicleId))
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.vehicle_bring);
                    return;
                }

                // Teleport the player to the vehicle's position
                Admin.MovePlayerToVehicle(player, vehicleId);

                return;
            }

            if (arguments[0].Equals(ArgRes.gas, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check the permission to execute the command
                if (rank <= StaffRank.Support) return;

                if (arguments.Length != 3 || !int.TryParse(arguments[1], out int vehicleId) || !float.TryParse(arguments[2], out float gas))
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.vehicle_gas);
                    return;
                }

                // Update the vehicle's gas
                Admin.ChangeVehicleGas(player, vehicleId, gas);
                
                return;
            }

            // The argument passed is not correct
            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.vehicle);
        }

        [Command]
        public static void GoCommand(Player player, string location)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank == StaffRank.None) return;

            if (location.Equals(ArgRes.workshop, StringComparison.InvariantCultureIgnoreCase))
            {
                // Set the player's position
                BuildingHandler.RemovePlayerFromBuilding(player, Coordinates.Workshop, 0);
                return;
            }

            if (location.Equals(ArgRes.electronics, StringComparison.InvariantCultureIgnoreCase))
            {
                // Set the player's position
                BuildingHandler.RemovePlayerFromBuilding(player, Coordinates.Electronics, 0);
                return;
            }

            if (location.Equals(ArgRes.police, StringComparison.InvariantCultureIgnoreCase))
            {
                // Set the player's position
                BuildingHandler.RemovePlayerFromBuilding(player, Coordinates.Police, 0);
                return;
            }

            if (location.Equals(ArgRes.townhall, StringComparison.InvariantCultureIgnoreCase))
            {
                // Set the player's position
                BuildingHandler.RemovePlayerFromBuilding(player, Coordinates.TownHall, 0);
                return;
            }

            if (location.Equals(ArgRes.license, StringComparison.InvariantCultureIgnoreCase))
            {
                // Set the player's position
                BuildingHandler.RemovePlayerFromBuilding(player, Coordinates.License, 0);
                return;
            }

            if (location.Equals(ArgRes.vanilla, StringComparison.InvariantCultureIgnoreCase))
            {
                // Set the player's position
                BuildingHandler.RemovePlayerFromBuilding(player, Coordinates.Vanilla, 0);
                return;
            }

            if (location.Equals(ArgRes.hospital, StringComparison.InvariantCultureIgnoreCase))
            {
                // Set the player's position
                BuildingHandler.RemovePlayerFromBuilding(player, Coordinates.Hospital, 0);
                return;
            }

            if (location.Equals(ArgRes.news, StringComparison.InvariantCultureIgnoreCase))
            {
                // Set the player's position
                BuildingHandler.RemovePlayerFromBuilding(player, Coordinates.News, 0);
                return;
            }

            if (location.Equals(ArgRes.bahama, StringComparison.InvariantCultureIgnoreCase))
            {
                // Set the player's position
                BuildingHandler.RemovePlayerFromBuilding(player, Coordinates.Bahama, 0);
                return;
            }

            if (location.Equals(ArgRes.mechanic, StringComparison.InvariantCultureIgnoreCase))
            {
                // Set the player's position
                BuildingHandler.RemovePlayerFromBuilding(player, Coordinates.Mechanic, 0);
                return;
            }

            if (location.Equals(ArgRes.garbage, StringComparison.InvariantCultureIgnoreCase))
            {
                // Set the player's position
                BuildingHandler.RemovePlayerFromBuilding(player, Coordinates.Dumper, 0);
                return;
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.go);
        }

        [Command]
        public static async Task BusinessCommand(Player player, string args)
        {
            // Get the staff rank from the player
            StaffRank rank = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank;

            if (!Admin.HasUserCommandPermission(player, ComRes.business) && rank == StaffRank.None) return;

            if (args == null || args.Trim().Length == 0)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.business);
                return;
            }

            BusinessModel business = new BusinessModel();
            string[] arguments = args.Trim().Split(' ');

            if (arguments[0].Equals(ArgRes.info, StringComparison.InvariantCultureIgnoreCase))
            {
                // Show the information for the business
                Admin.ShowBusinessInfo(player, Business.GetClosestBusiness(player));
                return;
            }

            if (arguments[0].Equals(ArgRes.create, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!Admin.HasUserCommandPermission(player, ComRes.business, ArgRes.create) && rank == StaffRank.None) return;
                            
                if (arguments.Length == 3 && int.TryParse(arguments[1], out int type) && Enum.IsDefined(typeof(BusinessTypes), type) && (arguments[2] == ArgRes.inner || arguments[2] == ArgRes.outer))
                {
                    // Create the business
                    await Admin.CreateBusinessAsync(player, type, arguments[2]);
                }
                else
                {
                    // Show the command help
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.business_create);
                    
                    // Show the available business list
                    player.SendChatMessage(Constants.COLOR_HELP + BuildingHandler.GetAvailableBusinesses());
                }

                return;
            }

            if (arguments[0].Equals(ArgRes.modify, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!Admin.HasUserCommandPermission(player, ComRes.business, ArgRes.modify) && rank == StaffRank.None) return;

                business = Business.GetClosestBusiness(player);

                if (business == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_business_close);
                    return;
                }

                if (arguments.Length < 1)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.business_modify);
                    return;
                }

                if (arguments[1].Equals(ArgRes.name, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (arguments.Length <= 2)
                    {
                        player.SendChatMessage(Constants.COLOR_HELP + HelpRes.business_modify_name);
                        return;
                    }
                    
                    // Change the business name
                    Admin.UpdateBusinessName(player, business, string.Join(' ', arguments.Skip(2)));
                    
                    return;
                }

                if (arguments[1].Equals(ArgRes.type, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (arguments.Length == 3 && int.TryParse(arguments[2], out int businessType) && Enum.IsDefined(typeof(BusinessTypes), businessType))
                    {
                        // Change the business type
                        Admin.UpdateBusinessType(player, business, (BusinessTypes)businessType);
                    }
                    else
                    {
                        // Show the command help
                        player.SendChatMessage(Constants.COLOR_HELP + HelpRes.business_create);
                        
                        // Show the available business list
                        player.SendChatMessage(Constants.COLOR_HELP + BuildingHandler.GetAvailableBusinesses());
                    }

                    return;
                }

                return;
            }

            if (arguments[0].Equals(ArgRes.remove, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rank <= StaffRank.GameMaster) return;

                business = Business.GetClosestBusiness(player);
                
                if (business == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_business_close);
                    return;
                }
                
                if (business.Ipl.Name.Length == 0)
                {
                    // Delete the business checkpoint
                    business.BusinessMarker.Delete();
                }
                else
                {
                    // Delete the business label
                    business.Label.Delete();
                }
                
                // Delete the ColShape
                business.ColShape.Delete();

                // Delete the business
                await Task.Run(() => DatabaseOperations.DeleteSingleRow("business", "id", business.Id)).ConfigureAwait(false);
                Business.BusinessCollection.Remove(business.Id);

                return;
            }

            //  The option doesn't exist
            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.business);
        }

        [Command]
        public static void CharacterCommand(Player player, string args)
        {
            // Get the staff rank from the player
            StaffRank rank = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank;

            if (rank == StaffRank.None) return;

            // Get the different arguments
            string[] arguments = args.Trim().Split(' ');

            if (arguments.Length != 3 && arguments.Length != 4)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.character);
                return;
            }
            
            // Remove the action part from the array
            string action = arguments[0];
            arguments = arguments.Where(w => w != arguments[0]).ToArray();

            // Get the player and amount 
            Player target = UtilityFunctions.GetPlayer(ref arguments);

            if (!Character.IsPlaying(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }

            if (!int.TryParse(arguments[0], out int value))
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.character);
                return;
            }

            if (action.Equals(ArgRes.bank, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rank <= StaffRank.GameMaster) return;

                // Change the requested value
                target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Bank = value;
                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.player_bank_modified, value, target.Name));

                return;
            }

            if (action.Equals(ArgRes.money, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rank <= StaffRank.GameMaster) return;
                
                // Change the requested value
                Money.SetPlayerMoney(target, value);
                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.player_money_modified, value, target.Name));

                return;
            }

            if (action.Equals(ArgRes.faction, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rank <= StaffRank.Support) return;

                // Change the requested value
                target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction = (PlayerFactions)value;
                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.player_faction_modified, value, target.Name));

                return;
            }

            if (action.Equals(ArgRes.job, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rank <= StaffRank.Support) return;

                // Change the requested value
                target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Job = (PlayerJobs)value;
                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.player_job_modified, value, target.Name));

                return;
            }

            if (action.Equals(ArgRes.rank, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rank <= StaffRank.Support) return;

                // Change the requested value
                target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Rank = value;
                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.player_rank_modified, value, target.Name));

                return;
            }

            if (action.Equals(ArgRes.dimension, StringComparison.InvariantCultureIgnoreCase))
            {
                // Change the requested value
                target.Dimension = Convert.ToUInt32(value);
                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.player_dimension_modified, value, target.Name));
                
                return;
            }

            // There was no matching argument
            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.character);
        }

        [Command]
        public static async Task HouseCommand(Player player, string args)
        {
            // Get the staff rank from the player
            StaffRank rank = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank;

            if (!Admin.HasUserCommandPermission(player, ComRes.house) && rank <= StaffRank.Support) return;

            HouseModel house = House.GetClosestHouse(player);
            string[] arguments = args.Trim().Split(' ');

            if (arguments[0].Equals(ArgRes.info, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rank <= StaffRank.Support) return;

                if (arguments.Length == 0 || arguments.Length > 2)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.house_info);
                    return;
                }

                // We get house identifier
                if (arguments.Length == 2 && int.TryParse(arguments[1], out int houseId))
                {
                    house = House.GetHouseById(houseId);
                }
                else if (arguments.Length == 2)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.house_info);
                    return;
                }

                if (house == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.house_not_exists);
                    return;
                }
                
                // Show the house information
                Admin.SendHouseInfo(player, house);

                return;
            }

            if (arguments[0].Equals(ArgRes.create, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!Admin.HasUserCommandPermission(player, ComRes.house, ArgRes.create) && rank <= StaffRank.GameMaster) return;

                if (arguments.Length != 2 || !int.TryParse(arguments[1], out int type) || !Enum.IsDefined(typeof(HouseTypes), type))
                {
                    // Show the command help
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.house_create);

                    // Show the available house list
                    player.SendChatMessage(Constants.COLOR_HELP + BuildingHandler.GetAvailableHouses());

                    return;
                }

                // Create the house given the type
                await Admin.CreateHouseAsync(player, type);

                return;
            }

            if (arguments[0].Equals(ArgRes.modify, StringComparison.InvariantCultureIgnoreCase))
            {
                if (arguments.Length < 3)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.house_modify);
                    return;
                }

                if (int.TryParse(arguments[2], out int value))
                {
                    // Numeric modifications
                    if(arguments[1].Equals(ArgRes.interior, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!Admin.HasUserCommandPermission(player, ComRes.house, ArgRes.interior) && rank <= StaffRank.Support) return;

                        if (!Enum.IsDefined(typeof(BuildingTypes), value))
                        {
                            // Show the command help
                            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.house_create);

                            // Show the available house list
                            player.SendChatMessage(Constants.COLOR_HELP + BuildingHandler.GetAvailableHouses());

                            return;
                        }
                        
                        // Update the IPL
                        house.Ipl = Constants.BuildingIplCollection.First(b => b.Type is HouseTypes type && (int)type == value);

                        // Update the house's information
                        await Task.Run(() => DatabaseOperations.UpdateHouse(house)).ConfigureAwait(false);

                        // Confirmation message sent to the player
                        player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.house_interior_modified, value));

                        return;
                    }

                    if(arguments[1].Equals(ArgRes.price, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!Admin.HasUserCommandPermission(player, ComRes.house, ArgRes.price) && rank <= StaffRank.Support) return;

                        if (value <= 0)
                        {
                            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.house_price_modify);
                            return;
                        }
                        
                        // Update the price
                        Admin.UpdateHousePrice(player, house, value);

                        return;
                    }

                    if(arguments[1].Equals(ArgRes.state, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!Enum.IsDefined(typeof(HouseState), value))
                        {
                            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.house_status_modify);
                            return;
                        }
                        
                        // Update the state
                        Admin.UpdateHouseState(player, house, (HouseState)value);

                        return;
                    }

                    if(arguments[1].Equals(ArgRes.rent, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (value <= 0)
                        {
                            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.house_price_modify);
                            return;
                        }
                        
                        // Set the house rent price
                        House.SetHouseRentable(player, house, value, true);

                        return;
                    }

                    // The option is not correct
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.house_modify_int);
                }
                else
                {
                    // Text based modifications
                    string name = string.Join(' ', arguments.Skip(2));

                    if(arguments[1].Equals(ArgRes.owner, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!Admin.HasUserCommandPermission(player, ComRes.house, ArgRes.owner) && rank <= StaffRank.Support) return;

                        house.Owner = name.Trim();

                        // Update the house's information
                        await Task.Run(() => DatabaseOperations.UpdateHouse(house)).ConfigureAwait(false);

                        // Confirmation message sent to the player
                        player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.house_owner_modified, house.Owner));

                        return;
                    }

                    if(arguments[1].Equals(ArgRes.name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!Admin.HasUserCommandPermission(player, ComRes.house, ArgRes.name) && rank <= StaffRank.Support) return;

                        house.Caption = name.Trim();
                        house.Label.Text = House.GetHouseLabelText(house);

                        // Update the house's information
                        await Task.Run(() => DatabaseOperations.UpdateHouse(house)).ConfigureAwait(false);

                        // Confirmation message sent to the player
                        player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.house_name_modified, house.Caption));

                        return;
                    }

                    // The option is not correct
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.house_modify_string);
                }

                return;
            }

            if (arguments[0].Equals(ArgRes.remove, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rank <= StaffRank.GameMaster) return;

                if (house == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_house_near);
                    return;
                }
                
                // Remove the house
                house.Label.Delete();
                house.ColShape.Delete();
                
                await Task.Run(() => DatabaseOperations.DeleteSingleRow("houses", "id", house.Id)).ConfigureAwait(false);
                House.HouseCollection.Remove(house.Id);

                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.house_deleted);

                return;
            }

            if (arguments[0].Equals(ArgRes.tp, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rank <= StaffRank.Support) return;

                // We get the house
                if (arguments.Length != 2 || !int.TryParse(arguments[1], out int houseId))
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.house_goto);
                    return;
                }
                
                // Get the house given the identifier
                house = House.GetHouseById(houseId);
                
                if (house == null) 
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.house_not_exists);
                    return;
                }
                
                // Teleport the player to the interior
                BuildingHandler.RemovePlayerFromBuilding(player, house.Entrance, house.Dimension);

                return;
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.house);
        }

        [Command]
        public static async Task InteriorCommand(Player player, string args)
        {
            // Get the staff rank from the player
            StaffRank rank = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank;

            if (!Admin.HasUserCommandPermission(player, ComRes.interior) && rank <= StaffRank.GameMaster) return;

            // Get the arguments
            string[] arguments = args.Trim().Split(' ');

            if (arguments[0].Equals(ArgRes.create, StringComparison.InvariantCultureIgnoreCase))
            {
                if (arguments.Length != 2 || !int.TryParse(arguments[1], out int type) || !Enum.IsDefined(typeof(InteriorTypes), type))
                {
                    // Show the command help
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.interior_create);
                    
                    // Show the available interior list
                    player.SendChatMessage(Constants.COLOR_HELP + BuildingHandler.GetAvailableInteriors());
                    
                    return;
                }

                // Create the interior given the type
                await Admin.CreateInteriorAsync(player, type);

                return;
            }

            // Get the closest interior for the player
            InteriorModel interior = GenericInterior.GetClosestInterior(player);
            
            if (arguments[0].Equals(ArgRes.modify, StringComparison.InvariantCultureIgnoreCase))
            {
                if (arguments.Length < 3)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.interior_modify);
                    return;
                }
                
                if (interior == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_interior_near);
                    return;
                }
                
                if (int.TryParse(arguments[2], out int value))
                {
                    // Numeric modifications
                    if (arguments[1].Equals(ArgRes.type, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (!Enum.IsDefined(typeof(InteriorTypes), value))
                        {
                            // Show the command help
                            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.interior_create);
                            
                            // Show the available interior list
                            player.SendChatMessage(Constants.COLOR_HELP + BuildingHandler.GetAvailableInteriors());
                            
                            return;
                        }
                        
                        // Change the interior type
                        Admin.ChangeInteriorType(player, interior, (InteriorTypes)value);
                        
                        return;
                    }
                    
                    if (arguments[1].Equals(ArgRes.mark, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Change the blip on the map
                        Admin.ChangeInteriorBlip(player, interior, value);
                        
                        return;
                    }
                }
                else if (arguments[1].Equals(ArgRes.name, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Get the name from the remaining arguments
                    interior.Caption = string.Join(' ', arguments.Skip(2));
                    interior.Label.Text = interior.Caption;
                    
                    if (interior.Icon != null && interior.Icon.Exists)
                    {
                        // Update the blip's caption
                        interior.Icon.Name = interior.Caption;
                    }
                    
                    // Update the interior on the database
                    await Task.Run(() => DatabaseOperations.UpdateInterior(interior)).ConfigureAwait(false);

                    // Confirmation message sent to the player
                    player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.interior_name_modified, interior.Caption));
                    
                    return;
                }
                
                // The option is not correct
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.interior_modify);
                
                return;
            }

            if (arguments[0].Equals(ArgRes.remove, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rank <= StaffRank.GameMaster) return;

                if (interior == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_interior_near);
                    return;
                }
                    
                // Remove the Label and ColShape
                interior.ColShape.Delete();
                interior.Label.Delete();
                
                if (interior.Icon != null && interior.Icon.Exists)
                {
                    // Remove the mark on the map
                    interior.Icon.Delete();
                }
                
                await Task.Run(() => DatabaseOperations.DeleteSingleRow("interiors", "id", interior.Id)).ConfigureAwait(false);
                GenericInterior.InteriorCollection.Remove(interior.Id);
                
                return;
            }

            if (arguments[0].Equals(ArgRes.tp, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rank <= StaffRank.Support) return;

                if (arguments.Length != 2 || !int.TryParse(arguments[1], out int interiorId))
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.interior_goto);
                    return;
                }
                
                // Get the interior
                interior = GenericInterior.GetInteriorById(interiorId);
                
                if (interior == null)
                {
                    //player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.interior_not_exists);
                    return;
                }
                
                // Teleport the player to the interior
                BuildingHandler.RemovePlayerFromBuilding(player, interior.Entrance, interior.Dimension);

                return;
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.interior);
        }

        [Command]
        public static async Task ParkingCommand(Player player, string args)
        {
            // Get the staff rank from the player
            StaffRank rank = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank;

            if (rank <= StaffRank.Support) return;

            string[] arguments = args.Trim().Split(' ');
            ParkingModel parking = Parking.GetClosestParking(player);

            if (arguments[0].Equals(ArgRes.info, StringComparison.InvariantCultureIgnoreCase))
            {
                if (parking == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_parking_near);
                    return;
                }
                
                // Send the header message
                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.parking_info, parking.Id));
                
                string vehicleList = string.Empty;
                
                foreach (VehicleModel vehModel in Vehicles.IngameVehicles.Values)
                {
                    if (vehModel.Parking == parking.Id)
                    {
                        // Add the vehicle to the list
                        vehicleList += string.Format("{0} LS-{1} ", vehModel.Model, vehModel.Id);
                    }
                }
                
                // We send the message with the vehicles in the parking, if any
                player.SendChatMessage(vehicleList.Length > 0 ? Constants.COLOR_HELP + vehicleList : Constants.COLOR_INFO + InfoRes.parking_empty);

                return;
            }

            if (arguments[0].Equals(ArgRes.create, StringComparison.InvariantCultureIgnoreCase))
            {
                if (rank <= StaffRank.GameMaster) return;

                if (arguments.Length != 2)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.parking_create);
                    return;
                }

                // We get the parking type
                if (!int.TryParse(arguments[1], out int type) || !Enum.IsDefined(typeof(ParkingTypes), type))
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.parking_create);
                    return;
                }
                
                parking = new ParkingModel();
                {
                    parking.Type = (ParkingTypes)type;
                    parking.Position = player.Position;
                }

                // Create the label
                parking.ParkingLabel = NAPI.TextLabel.CreateTextLabel(Parking.GetParkingLabelText(parking.Type), parking.Position, 20.0f, 0.75f, 4, new Color(255, 255, 255));
                
                // Add the new parking
                parking.Id = await DatabaseOperations.AddParking(parking);
                Parking.ParkingList.Add(parking.Id, parking);

                // Send the confirmation message to the player
                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.parking_created);

                return;
            }

            if (arguments[0].Equals(ArgRes.modify, StringComparison.InvariantCultureIgnoreCase))
            {
                if(arguments.Length != 3)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.parking_modify);
                    return;
                }

                if (parking == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_parking_near);
                    return;
                }

                // Get the rest of the arguments
                if(arguments[1] == ArgRes.house)
                {
                    if (parking.Type == ParkingTypes.Garage)
                    {
                        // We link the house to this parking
                        if (int.TryParse(arguments[2], out int houseId))
                        {
                            parking.HouseId = houseId;

                            // Update the parking's information
                            await Task.Run(() => DatabaseOperations.UpdateParking(parking)).ConfigureAwait(false);

                            // Confirmation message sent to the player
                            string message = string.Format(AdminRes.parking_house_modified, houseId);
                            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + message);
                        }
                        else
                        {
                            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.parking_modify);
                        }
                    }
                    else
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.parking_not_garage);
                    }

                    return;
                }

                if(arguments[1] == ArgRes.places)
                {
                    if (!int.TryParse(arguments[2], out int slots))
                    {
                        player.SendChatMessage(Constants.COLOR_HELP + HelpRes.parking_modify);
                        return;
                    }

                    parking.Capacity = slots;
                    parking.ParkingLabel = NAPI.TextLabel.CreateTextLabel(Parking.GetParkingLabelText(parking.Type), parking.Position, 20.0f, 0.75f, 4, new Color(255, 255, 255));

                    // Update the parking's information
                    await Task.Run(() => DatabaseOperations.UpdateParking(parking)).ConfigureAwait(false);

                    // Confirmation message sent to the player
                    player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.parking_slots_modified, slots));

                    return;
                }

                if(arguments[1] == ArgRes.type)
                {
                    if (!int.TryParse(arguments[2], out int type) || !Enum.IsDefined(typeof(ParkingTypes), type))
                    {
                        player.SendChatMessage(Constants.COLOR_HELP + HelpRes.parking_modify);
                        return;
                    }

                    parking.Type = (ParkingTypes)type;

                    // Update the parking's information
                    await Task.Run(() => DatabaseOperations.UpdateParking(parking)).ConfigureAwait(false);

                    // Confirmation message sent to the player
                    player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.parking_type_modified, type));

                    return;
                }

                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.parking_modify);
                return;
            }

            if (arguments[0].Equals(ArgRes.remove, StringComparison.InvariantCultureIgnoreCase))
            {
                if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.GameMaster) return;

                if (parking != null)
                {
                    // Update the parking's information
                    parking.ParkingLabel.Delete();
                    await Task.Run(() => DatabaseOperations.DeleteSingleRow("parkings", "id", parking.Id)).ConfigureAwait(false);
                    Parking.ParkingList.Remove(parking.Id);

                    // Confirmation message sent to the player
                    player.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.parking_deleted);
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_parking_near);
                }

                return;
            }

            // There was no matching argument
            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.parking);
        }

        [Command]
        public static void PosCommand(Player player)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank > StaffRank.Support)
            {
                Vector3 position = player.Position;
                NAPI.Util.ConsoleOutput("{0},{1},{2}", position.X, position.Y, position.Z);
            }
        }

        [Command]
        public static void ReviveCommand(Player player, string targetString)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.Support) return;
            
            // We get the target player
            Player target = UtilityFunctions.GetPlayer(targetString);

            if (!Character.IsPlaying(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }
                
            if (!Emergency.IsPlayerDead(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_dead);
                return;
            }
            
            // Revive the target player
            Emergency.CancelPlayerDeath(target);
            
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.player_revived, target.Name));
            target.SendChatMessage(Constants.COLOR_SUCCESS + string.Format(SuccRes.admin_revived, player.SocialClubName));
        }

        [Command]
        public static void WeatherCommand(Player player, int weather)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.Support) return;
            
            if (weather < 0 || weather > 14)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.weather_value_invalid);
                return;
            }
            
            // Change the weather
            NAPI.World.SetWeather((Weather)weather);
            NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.weather_changed, player.Name, weather));
        }

        [Command]
        public static void JailCommand(Player player, string args)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.Support) return;
            
            // Get the array with the arguments
            string[] arguments = args.Trim().Split(' ');
            
            if (arguments.Length < 3)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.jail);
                return;
            }
            
            // Get the target player
            Player target = UtilityFunctions.GetPlayer(ref arguments);
            
            if (!Character.IsPlaying(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }
            
            if (!int.TryParse(arguments[0], out int jailTime))
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.jail);
                return;
            }
            
            // Get the reason from the remaining text
            string reason = string.Join(" ", arguments.Where(w => w != arguments[0]).ToArray());
            
            if (reason == null || reason.Trim().Length == 0)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.jail);
                return;
            }
            
            // Set the player's position
            BuildingHandler.RemovePlayerFromBuilding(player, Coordinates.JailOoc, 0);

            // We set jail type
            PlayerTemporaryModel data = target.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);
            data.JailType = JailTypes.Ooc;
            data.Jailed = jailTime;

            // Message sent to the whole server
            NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.player_jailed, target.Name, jailTime, reason));

            // We add the log in the database
            Task.Run(() => DatabaseOperations.AddAdminLog(player.SocialClubName, target.Name, "jail", jailTime, reason)).ConfigureAwait(false);
        }

        [Command]
        public static void KickCommand(Player player, string args)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.Support) return;
            
            // Get the array with the arguments
            string[] arguments = args.Trim().Split(' ');
            
            if (arguments.Length < 2)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.jail);
                return;
            }
            
            // Get the target player
            Player target = UtilityFunctions.GetPlayer(ref arguments);
            
            if (!Character.IsPlaying(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }
            
            // Get the kick reason
            string reason = string.Join(" ", arguments.Where(w => w != arguments[0]).ToArray());
            
            // Kick the player sending the reason
            target.Kick(reason);
            
            // Message sent to the whole server
            NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.player_kicked, player.Name, target.Name, reason));
            
            // We add the log in the database
            Task.Run(() => DatabaseOperations.AddAdminLog(player.SocialClubName, target.Name, "kick", 0, reason)).ConfigureAwait(false);
        }

        [Command]
        public static void KickAllCommand(Player player)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.Support) return;
            
            // Kick all the players only but the command sender
            foreach (Player p in NAPI.Pools.GetAllPlayers().FindAll(t => t != player)) p.Kick();
            
            // Confirmation message sent to the player
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.kicked_all);
        }

        [Command]
        public static void BanCommand(Player player, string args)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.GameMaster) return;
            
            // Get the array with the arguments
            string[] arguments = args.Trim().Split(' ');
            
            if (arguments.Length < 2)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.jail);
                return;
            }
            
            // Get the target player
            Player target = UtilityFunctions.GetPlayer(ref arguments);
            
            if (!Character.IsPlaying(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }
            
            // Get the ban reason
            string reason = string.Join(" ", arguments.Where(w => w != arguments[0]).ToArray());
            
            // Ban the player sending the reason
            target.Ban(reason);
            
            // Message sent to the whole server
            NAPI.Chat.SendChatMessageToAll(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.player_banned, player.Name, target.Name, reason));
            
            // We add the log in the database
            Task.Run(() => DatabaseOperations.AddAdminLog(player.SocialClubName, target.Name, "ban", 0, reason)).ConfigureAwait(false);
        }

        [Command]
        public static void HealthCommand(Player player, string args)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.GameMaster) return;
            
            // Get the arguments from the parameters
            string[] arguments = args.Trim().Split(' ');
            
            if (arguments.Length != 2 && arguments.Length != 3)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.health);
                return;
            }
            
            // Get the target player            
            Player target = UtilityFunctions.GetPlayer(ref arguments);
            
            if (!Character.IsPlaying(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }
            
            // Check if the health is correct
            if (!int.TryParse(arguments[0], out int health) || health < 0 || health > 100)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.health_value_not_correct);
                return;
            }
            
            // Change the target's health
            target.Health = health;
            
            // Send the confirmation message to both players
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.player_health, target.Name, health));
            target.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.target_health, player.Name, health));
        }

        [Command]
        public static void SaveCommand(Player player)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.Support) return;
            
            string message = string.Empty;

            // We print a message saying when the command starts
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.save_start);

            // Saving all business
            DatabaseOperations.UpdateBusinesses(Business.BusinessCollection);
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.save_business, Business.BusinessCollection.Count));

            // Saving all connected players
            List<Player> connectedPlayers = NAPI.Pools.GetAllPlayers().FindAll(pl => Character.IsPlaying(pl));
            foreach (Player target in connectedPlayers)
            {
                // Get the character model
                CharacterModel character = target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

                // Store the new values
                character.Position = target.Position;
                character.Rotation = target.Rotation;
                character.Health = target.Health;
                character.Armor = target.Armor;

                // Save the player into the database
                Character.SaveCharacterData(character);
            }

            // All the characters saved
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.characters_saved);

            // Vehicles saving
            Vehicles.SaveAllVehicles();

            // All vehicles saved
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.vehicles_saved);

            // End of the command
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.save_finish);
        }

        [Command]
        public static void ADutyCommand(Player player)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank == StaffRank.None) return;

            // Get the player's external data
            PlayerTemporaryModel playerModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (playerModel.AdminOnDuty)
            {
                playerModel.AdminOnDuty = false;
                player.SendNotification(InfoRes.player_admin_free_time);
            }
            else
            {
                playerModel.AdminOnDuty = true;
                player.SendNotification(InfoRes.player_admin_on_duty);
            }
        }

        [Command]
        public static void TicketCommand(Player player, string message)
        {            
            if (Admin.AdminTicketCollection.ContainsKey(player.Value))
            {
                // Edit the value from the ticket
                Admin.AdminTicketCollection[player.Value] = message;
            }
            else 
            {
                // Add the ticket to the list
                Admin.AdminTicketCollection.Add(player.Value, message);
            }

            // Send the message to the staff online
            foreach (Player target in NAPI.Pools.GetAllPlayers())
            {
                if (Character.IsPlaying(target) && target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank > StaffRank.None)
                {
                    target.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.new_admin_ticket);
                }
            }
            
            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_SUCCESS + SuccRes.help_request_sent);
        }

        [Command]
        public static void TicketsCommand(Player player)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank == StaffRank.None) return;
            
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.ticket_list);
            
            foreach (KeyValuePair<int, string> entry in Admin.AdminTicketCollection)
            {
                // Get the client who opened the ticket
                Player target = UtilityFunctions.GetPlayer(entry.Key);
                
                if (Character.IsPlaying(target))
                {
                    // Add the ticket to the list
                    player.SendChatMessage(Constants.COLOR_HELP + string.Format("{0} ({1}): {2}", target.Name, target.Value, entry.Value));
                }
            }
        }

        [Command]
        public static void ATicketCommand(Player player, int ticket, string message)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank == StaffRank.None) return;

            if (!Admin.AdminTicketCollection.ContainsKey(ticket))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.admin_ticket_not_found);
                return;
            }

            // Remove the ticket
            Admin.AdminTicketCollection.Remove(ticket);

            // Get the player with the given identifier
            Player target = UtilityFunctions.GetPlayer(ticket);

            // We send the message to both players
            target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.ticket_answer, message));
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.ticket_answered, ticket));
        }

        [Command]
        public static void ACommand(Player player, string message)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank == StaffRank.None) return;
            
            // Get all the staff playing
            Player[] targetList = NAPI.Pools.GetAllPlayers().Where(p => Character.IsPlaying(p) && p.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank > StaffRank.None).ToArray();

            foreach (Player target in targetList)
            {
                // Send the message to each one of the staff members
                target.SendChatMessage(Constants.COLOR_ADMIN_INFO + "((Staff [ID: " + player.Value + "] " + player.Name + ": " + message + "))");
            }
        }

        [Command]
        public static void ReconCommand(Player player, string targetString)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.Support) return;

            Player target = UtilityFunctions.GetPlayer(targetString);

            if (!Character.IsPlaying(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }
            /*
            if (target.Spectating)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_spectating);
                return;
            }*/

            if (target == player)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.cant_spect_self);
                return;
            }

            // Force the player spectate the other
            player.TriggerEvent("StartSpectating", target.Value);

            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.spectating_player, target.Name));
        }

        [Command]
        public static void RecoffCommand(Player player)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.Support) return;
            /*
            if (!player.Spectating)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_spectating);
                return;
            }*/

            // Stop the player spectating
            player.TriggerEvent("StopSpectating");

            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.spect_stopped);
        }

        [Command]
        public static void InfoCommand(Player player, string targetString)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.Support) return;
            
            // Get the target player
            Player target = UtilityFunctions.GetPlayer(targetString);
            
            if (!Character.IsPlaying(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }
            
            // Get player's basic data
            Character.RetrieveBasicDataEvent(player, target.Value);
        }

        [Command]
        public static void PointsCommand(Player player, string args)
        {
            // Get the player's character model
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.AdminRank <= StaffRank.GameMaster) return;
            
            string[] arguments = args.Trim().Split(' ');
            
            if (arguments.Length != 3 && arguments.Length != 4)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.points);
                return;
            }
            
            // Get the action
            string action = arguments[0];
            arguments = arguments.Where(w => w != arguments[0]).ToArray();
            
            // Get the target player
            Player target = UtilityFunctions.GetPlayer(ref arguments);
            
            if (!Character.IsPlaying(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }
            
            if (!int.TryParse(arguments[0], out int rolePoints)) 
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.points);
                return;
            }
            
            if (action.Equals(ArgRes.give, StringComparison.InvariantCultureIgnoreCase))
            {
                // We give role points to the player
                characterModel.RolePoints += rolePoints;

                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.role_points_given, target.Name, rolePoints));
                target.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.role_points_received, player.SocialClubName, rolePoints));

                return;
            }
            
            if (action.Equals(ArgRes.remove, StringComparison.InvariantCultureIgnoreCase))
            {
                // We remove role points to the player
                characterModel.RolePoints -= rolePoints;

                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.role_points_removed, target.Name, rolePoints));
                target.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.role_points_lost, player.SocialClubName, rolePoints));

                return;
            }
            
            if (action.Equals(ArgRes.set, StringComparison.InvariantCultureIgnoreCase))
            {
                // We set player's role points
                characterModel.RolePoints = rolePoints;

                player.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.role_points_set, target.Name, rolePoints));
                target.SendChatMessage(Constants.COLOR_ADMIN_INFO + string.Format(AdminRes.role_points_established, player.SocialClubName, rolePoints));

                return;
            }

            // The action was not found
            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.points);
        }
    }
}
