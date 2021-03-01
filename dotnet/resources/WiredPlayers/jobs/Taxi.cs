using GTANetworkAPI;
using System;
using System.Linq;
using WiredPlayers.Buildings;
using WiredPlayers.Currency;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.error;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.jobs
{
    public class Taxi : Script
    {
        [ServerEvent(Event.PlayerEnterVehicle)]
        public void PlayerEnterVehicleEvent(Player player, Vehicle vehicle, sbyte seat)
        {
            if(vehicle.Model == (uint)VehicleHash.Taxi && seat == (sbyte)VehicleSeat.Driver)
            {
                // Check if the player has a taxi driver license
                if(DrivingSchool.GetPlayerLicenseStatus(player, DrivingLicenses.Taxi) == -1)
                {
                    player.WarpOutOfVehicle();
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_taxi_license);
                }
            }
        }

        [RemoteEvent("RequestTaxiDestination")]
        public void RequestTaxiDestinationEvent(Player player, Vector3 position)
        {
            // Check if there's someone driving the taxi
            Player driver = player.Vehicle.Occupants.Cast<Player>().ToList().FirstOrDefault(d => d.VehicleSeat == (int)VehicleSeat.Driver);

            if (driver == null || driver == player)
            {
                // Nobody's driving the vehicle
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_taxi_driver);
                return;
            }

            // Get the temporary data
            PlayerTemporaryModel data = driver.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (data.TaxiPath)
            {
                // There's already a path set for the driver
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.taxi_has_path);
                return;
            }

            // Join driver and client
            data.JobPartner = player;
            player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).JobPartner = driver;

            // Create the path for the driver
            driver.TriggerEvent("CreateTaxiPath", position);
        }

        [RemoteEvent("TaxiDestinationReached")]
        public void TaxiDestinationReachedEvent(Player player)
        {
            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // Get the character model for the customer
            CharacterModel characterModel = data.JobPartner.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // Make the payment
            int amount = 500;
            int customerMoney = characterModel.Money - amount;
            
            if(customerMoney < 0)
            {
                amount = Math.Abs(customerMoney);
                customerMoney = 0;

                // Get the remaining money from the bank account
                characterModel.Bank -= amount;
            }

            // Remove customer's money and give to the driver
            Money.ExchangePlayersMoney(data.JobPartner, player, customerMoney, out _, out _);

            // Remove the link between players
            data.JobPartner.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).JobPartner = null;
            data.JobPartner = null;
        }
    }
}
