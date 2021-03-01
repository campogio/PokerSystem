using GTANetworkAPI;
using System;
using System.Linq;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Utility;
using WiredPlayers.messages.arguments;
using WiredPlayers.messages.error;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class HookerCommands
    {
        [Command]
        public static void ServiceCommand(Player player, string args)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Job != PlayerJobs.Hooker)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_hooker);
                return;
            }

            if (player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).AlreadyFucking != null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.already_fucking);
                return;
            }

            if (player.VehicleSeat != (int)VehicleSeat.RightFront)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_vehicle_passenger);
                return;
            }
            
            // Get all the arguments
            string[] arguments = args.Trim().Split(' ');
            
            if (arguments.Length != 3 && arguments.Length != 4)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + HelpRes.service);
                return;
            }
            
            // Declare the variables
            string service = arguments[0];
            arguments = arguments.Where(w => w != arguments[0]).ToArray();

            // Get the target player
            Player target = UtilityFunctions.GetPlayer(ref arguments);
            
            if (target == null || player.Position.DistanceTo(target.Position) > 2.5f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                return;
            }
            
            if (target.VehicleSeat != (int)VehicleSeat.Driver)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.client_not_vehicle_driving);
                return;
            }
            
            // Get the price
            if (!int.TryParse(arguments[0], out int price))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + HelpRes.service);
                return;
            }
            
            if (price <= 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.money_amount_positive);
                return;
            }

            // Get the temporary data
            PlayerTemporaryModel data = target.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (service.Equals(ArgRes.oral, StringComparison.InvariantCultureIgnoreCase))
            {
                data.JobPartner = player;
                data.SellingPrice = price;
                data.HookerService = HookerService.Oral;

                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.oral_service_offer, target.Name, price));
                target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.oral_service_receive, player.Name, price));
                
                return;
            }
            
            if (service.Equals(ArgRes.sex, StringComparison.InvariantCultureIgnoreCase))
            {
                data.JobPartner = player;
                data.SellingPrice = price;
                data.HookerService = HookerService.Sex;

                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.sex_service_offer, target.Name, price));
                target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.sex_service_receive, player.Name, price));
                
                return;
            }

            player.SendChatMessage(Constants.COLOR_ERROR + HelpRes.service);
        }
    }
}
