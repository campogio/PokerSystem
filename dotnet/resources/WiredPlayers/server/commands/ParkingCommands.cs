using GTANetworkAPI;
using System.Linq;
using WiredPlayers.Currency;
using WiredPlayers.Data.Persistent;
using WiredPlayers.factions;
using WiredPlayers.Utility;
using WiredPlayers.messages.error;
using WiredPlayers.messages.information;
using WiredPlayers.vehicles;
using WiredPlayers.Buildings.Houses;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Server.Commands
{
    public static class ParkingCommands
    {
        [Command]
        public static void ParkCommand(Player player)
        {
            if (player.VehicleSeat != (int)VehicleSeat.Driver)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_vehicle_driving);
                return;
            }

            if (player.Vehicle.TraileredBy != null && player.Vehicle.TraileredBy.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_is_trailered);
                return;
            }
            
            if (!Vehicles.HasPlayerVehicleKeys(player, player.Vehicle, true) && !Faction.IsPoliceMember(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
                return;
            }
            
            // Get the closest parking
            ParkingModel parking = Parking.ParkingList.Values.FirstOrDefault(p => player.Position.DistanceTo(p.Position) < 3.5f);

            if (parking == null)
            {
                // Player's not in any parking
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_parking_near);
                return;
            }

            switch (parking.Type)
            {
                case ParkingTypes.Public:
                    if(Vehicles.HasPlayerVehicleKeys(player, player.Vehicle, true))
                    {
                        string message = string.Format(InfoRes.parking_cost, (int)Prices.ParkingPublic);
                        player.SendChatMessage(Constants.COLOR_INFO + message);
                        Parking.PlayerParkVehicle(player, parking);
                    }
                    else
                    {
                        // The player doesn't have the keys
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
                    }
                    break;
                case ParkingTypes.Garage:
                    HouseModel house = House.GetHouseById(parking.HouseId);
                    if (house == null || !House.HasPlayerHouseKeys(player, house))
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_garage_access);
                    }
                    else if (Parking.GetParkedCarAmount(parking.Id) == parking.Capacity)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.parking_full);
                    }
                    else if(!Vehicles.HasPlayerVehicleKeys(player, player.Vehicle, true))
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
                    }
                    else
                    {
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.vehicle_garage_parked);
                        Parking.PlayerParkVehicle(player, parking);
                    }
                    break;
                case ParkingTypes.Deposit:
                    if (!Faction.IsPoliceMember(player))
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                    }
                    else
                    {
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.vehicle_deposit_parked);
                        Parking.PlayerParkVehicle(player, parking);
                    }
                    break;
                default:
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_parking_allowed);
                    break;
            }
        }

        [Command]
        public static void UnparkCommand(Player player, int vehicleId)
        {
            // Get the vehicle from the identifier
            VehicleModel vehicle = Vehicles.GetVehicleById<VehicleModel>(vehicleId);

            if (vehicle == null)
            {
                // There's no vehicle with that identifier
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_exists);
                return;
            }

            if (!Vehicles.HasPlayerVehicleKeys(player, vehicle, true))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_car_keys);
                return;
            }

            // Get the closest parking
            ParkingModel parking = Parking.ParkingList.Values.FirstOrDefault(p => player.Position.DistanceTo(p.Position) < 2.5f);

            if (parking == null)
            {
                // Player's not in any parking
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_parking_near);
                return;
            }

            if (parking.Id != vehicle.Parking)
            {
                // The vehicle is not in this parking
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_not_this_parking);
                return;
            }

            switch (parking.Type)
            {
                case ParkingTypes.Deposit:
                    // Remove player's money
                    if(!Money.SubstractPlayerMoney(player, (int)Prices.ParkingDeposit, out string error))
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + error);
                        return;
                    }

                    player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.unpark_money, Prices.ParkingDeposit));
                    break;

                default:
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.vehicle_unparked);
                    break;
            }

            // Set the values to unpark the vehicle
            vehicle.Dimension = player.Dimension;
            vehicle.Position = parking.Position;
            vehicle.Parking = 0;
            vehicle.Parked = 0;
            
            // Recreate the vehicle
            Vehicles.CreateIngameVehicle(vehicle);
        }
    }
}
