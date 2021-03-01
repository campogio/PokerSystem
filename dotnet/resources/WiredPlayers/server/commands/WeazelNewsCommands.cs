using GTANetworkAPI;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.Currency;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.factions;
using WiredPlayers.Utility;
using WiredPlayers.vehicles;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class WeazelNewsCommands
    {
        [Command]
        public static void InterviewCommand(Player player, string targetString)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction != PlayerFactions.News)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_news_faction);
                return;
            }

            if (player.Vehicle == null || !player.Vehicle.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle);
                return;
            }
            
            // Get the vehicle's faction
            int vehicleFaction = Vehicles.GetVehicleById<VehicleModel>(player.Vehicle.GetData<int>(EntityData.VehicleId)).Faction;
            
            if (vehicleFaction != (int)PlayerFactions.News && player.VehicleSeat != (int)VehicleSeat.LeftRear)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_in_news_van);
                return;
            }
            
            Player target = UtilityFunctions.GetPlayer(targetString);

            if (target.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).OnAir)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.on_air);
                return;
            }

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            data.OnAir = true;
            data.JobPartner = target;

            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.wz_offer_onair);
            target.SendChatMessage(Constants.COLOR_INFO + InfoRes.wz_accept_onair);
        }

        [Command]
        public static void CutInterviewCommand(Player player, string targetString)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction != PlayerFactions.News)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_news_faction);
                return;
            }
            
            Player target = UtilityFunctions.GetPlayer(targetString);

            if (target.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).OnAir)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_on_air);
                return;
            }
            
            if (target == player)
            {
                foreach (Player interviewed in NAPI.Pools.GetAllPlayers())
                {
                    // Get the temporary data
                    PlayerTemporaryModel interviewedData = interviewed.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

                    if (interviewedData.OnAir && interviewed != player)
                    {
                        interviewedData.OnAir = false;
                        interviewedData.JobPartner = null;

                        interviewed.SendChatMessage(Constants.COLOR_INFO + InfoRes.player_on_air_cutted);
                    }
                }

                // Get the temporary data
                PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);
                data.OnAir = false;
                data.JobPartner = null;

                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.reporter_on_air_cutted);
            }
            else
            {
                target.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).OnAir = false;

                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.reporter_on_air_cutted);
                target.SendChatMessage(Constants.COLOR_INFO + InfoRes.player_on_air_cutted);
            }
        }

        [Command]
        public static async Task PrizeCommand(Player player, string args)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get the player's character model
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Faction != PlayerFactions.News)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_news_faction);
                return;
            }
            
            string[] arguments = args.Trim().Split(' ');

            if (arguments.Length < 2)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.prize);
                return;
            }
            
            // Get the client based on the input
            Player target = UtilityFunctions.GetPlayer(ref arguments);
            
            if (target == null || player.Position.DistanceTo(target.Position) > 2.5f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                return;
            }
            
            if (!int.TryParse(arguments[0], out int prize) || prize <= 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.money_amount_positive);
                return;
            }
            
            // Get the contest prize reason
            int prizeAmount = WeazelNews.GetRemainingFounds();

            if (prizeAmount < prize)
            {
                player.SendChatMessage(Constants.COLOR_INFO + ErrRes.faction_not_enough_money);
                return;
            }

            string contest = string.Join(' ', arguments.Skip(1));
            AnnoucementModel prizeModel = new AnnoucementModel();
            {
                prizeModel.Amount = prize;
                prizeModel.Winner = target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
                prizeModel.Annoucement = contest;
                prizeModel.Journalist = characterModel.Id;
                prizeModel.Given = true;
            }

            // Add the money to the player
            Money.GivePlayerMoney(player, prize, out string error);

            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.prize_given, prize, target.Name));
            target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.prize_received, player.Name, prize, contest));

            prizeModel.Id = await DatabaseOperations.AddNewsTransaction(prizeModel).ConfigureAwait(false);
            WeazelNews.AnnoucementList.Add(prizeModel);

            // Log the money won
            await Task.Run(() => DatabaseOperations.LogPayment(player.Name, target.Name, GenRes.news_prize, prize)).ConfigureAwait(false);
        }

        [Command]
        public static async Task AnnounceCommand(Player player, string message)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (!Money.SubstractPlayerMoney(player, (int)Prices.Announcement, out string error))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + error);
                return;
            }

            AnnoucementModel annoucement = new AnnoucementModel
            {
                Winner = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id,
                Amount = (int)Prices.Announcement,
                Annoucement = message,
                Given = false
            };

            WeazelNews.SendNewsMessage(player, message);
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.announce_published);

            // Log the announcement into the database
            annoucement.Id = await DatabaseOperations.AddNewsTransaction(annoucement).ConfigureAwait(false);
            WeazelNews.AnnoucementList.Add(annoucement);
            
            await Task.Run(() => DatabaseOperations.LogPayment(player.Name, GenRes.faction_news, GenRes.news_announce, (int)Prices.Announcement)).ConfigureAwait(false);
        }

        [Command]
        public static void NewsCommand(Player player, string message)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction != PlayerFactions.News)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_news_faction);
                return;
            }

            if(!player.IsInVehicle)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle);
                return;
            }
            
            // Get the vehicle's faction
            int vehicleFaction = Vehicles.GetVehicleById<VehicleModel>(player.Vehicle.GetData<int>(EntityData.VehicleId)).Faction;

            if (vehicleFaction != (int)PlayerFactions.News && player.VehicleSeat != (int)VehicleSeat.LeftRear && player.VehicleSeat != (int)VehicleSeat.RightRear)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_in_news_van);
                return;
            }

            // Send the news message to the players
            WeazelNews.SendNewsMessage(player, message);
        }
    }
}
