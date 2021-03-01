using System;
using System.Linq;
using GTANetworkAPI;
using SouthValleyFive.data.persistent;
using SouthValleyFive.poker;
using WiredPlayers.Data.Persistent;
using WiredPlayers.messages.error;
using WiredPlayers.Utility;

namespace SouthValleyFive.Server.Commands
{
    public class PokerCommands
    {
        [Command]
        public static void PokerCommand(Player player, int fiches)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    PokerTable pokerTable = Poker.PokerTables.Single(p => Vector3.Distance(p.Position, player.Position)< 1.5f);
                    PokerTableSit freeSit = pokerTable.Sits.SingleOrDefault(s => s.Occupied == null);
                    if (freeSit == null)
                    {
                        player.SendChatMessage(ErrRes.poker_table_full);
                        return;
                    }   
                    CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)Enumerators.ExternalDataSlot.Database);

                    if (fiches > characterModel.Money || fiches <= 0)
                    {
                        player.SendChatMessage(ErrRes.player_not_enough_money);
                        return;
                    }
                    Poker.OnPlayerSitTable(player, pokerTable, freeSit, fiches);
                    
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.StackTrace);
                }
            });
        }
    }
}