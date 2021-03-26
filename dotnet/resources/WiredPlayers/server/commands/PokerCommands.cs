using System;
using System.Collections.Generic;
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
                    PokerTable pokerTable = Poker.PokerTables.Single(p => Vector3.Distance(p.Position, player.Position)< 3f);
                    List<PokerTableSit> availableSeats = new List<PokerTableSit>();
                    foreach(PokerTableSit sit in pokerTable.Sits)
                    {
                        if(sit.Occupied == null)
                        {
                            availableSeats.Add(sit);
                        }
                    }
                    if (availableSeats.Count == 0)
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
                    availableSeats.First().Occupied = player;
                    Poker.OnPlayerSitTable(player, pokerTable, availableSeats.First(), fiches);
                    availableSeats.Clear();
                    
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.StackTrace);
                }
            });
        }
    }
}