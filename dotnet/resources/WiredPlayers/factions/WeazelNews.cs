using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using WiredPlayers.character;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.general;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.factions
{
    public static class WeazelNews
    {
        public static List<AnnoucementModel> AnnoucementList;

        public static void SendNewsMessage(Player player, string message)
        {
            // Get the connected players
            Player[] connectedPlayers = NAPI.Pools.GetAllPlayers().Where(target => Character.IsPlaying(target)).ToArray();

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            foreach (Player target in connectedPlayers)
            {
                if (data.OnAir && target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction == PlayerFactions.News)
                {
                    target.SendChatMessage(Constants.COLOR_NEWS + GenRes.interviewer + player.Name + ": " + message);
                }
                else if (data.OnAir)
                {
                    target.SendChatMessage(Constants.COLOR_NEWS + GenRes.guest + player.Name + ": " + message);
                }
                else
                {
                    target.SendChatMessage(Constants.COLOR_NEWS + GenRes.announcement + message);
                }
            }
        }

        public static int GetRemainingFounds()
        {
            int remaining = 0;

            foreach (AnnoucementModel announcement in AnnoucementList)
            {
                remaining += announcement.Given ? -announcement.Amount : announcement.Amount;
            }
            
            return remaining;
        }
    }
}

