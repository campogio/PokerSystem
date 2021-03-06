using System;
using System.Collections.Generic;
using GTANetworkAPI;
using SouthValleyFive.data.persistent;
using WiredPlayers.Data;
using WiredPlayers.Utility;

namespace SouthValleyFive.poker
{
    public class Poker : Script
    {
        public static List<PokerTable> PokerTables = new List<PokerTable>();

        public static async void GeneratePokerTables()
        {
            try
            {
                PokerTables = await DatabaseOperations.GetPokerTables();
               /* foreach (PokerTable pokerTable in PokerTables)
                {
                    NAPI.TextLabel.CreateTextLabel("/poker [Fiches]", pokerTable.Position, 2.5f, 0.5f, 4, new Color(190, 235, 100), false, pokerTable.Dimension);
                    NAPI.TextLabel.CreateTextLabel("Digita il comando per entrare al tavolo", new Vector3(pokerTable.Position.X, pokerTable.Position.Y, pokerTable.Position.Z - 0.1f), 4.0f, 0.5f, 4, new Color(255, 255, 255), false, pokerTable.Dimension);
                } */
            }
            catch (Exception e)
            {
                NAPI.Util.ConsoleOutput(e.StackTrace);
            }
        }

        public static void OnPlayerSitTable(Player player, PokerTable pokerTable, PokerTableSit sit, int amount)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    player.Position = sit.Position;
                    player.Rotation = sit.Rotation;
                    player.PlayAnimation("switch@michael@sitting", "idle", (int)(Enumerators.AnimationFlags.AllowPlayerControl | Enumerators.AnimationFlags.Loop));
                    pokerTable.TableManager.JoinMatch(player, amount);
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.StackTrace);
                }
            });
        }
        public static void OnPlayerLeaveTable(Player player)
        {
            NAPI.Task.Run(() =>
            {
                try
                {
                    player.StopAnimation();
                }
                catch (Exception e)
                {
                    NAPI.Util.ConsoleOutput(e.StackTrace);
                }
            });
        }
    }
}