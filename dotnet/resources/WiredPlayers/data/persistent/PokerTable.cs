using System.Collections.Generic;
using GTANetworkAPI;
using SouthValleyFive.Scripts.Poker;

namespace SouthValleyFive.data.persistent
{
    public class PokerTable
    {
        public int Id { get; set; }
        public Vector3 Position { get; set; }
        public List<PokerTableSit> Sits { get; set; }

        public PokerTableManager TableManager { get; set; }

        public uint Dimension { get; set; }

        public PokerTable(int id, Vector3 position, List<PokerTableSit> sits, uint dimension)
        {
            Id = id;
            Position = position;
            Sits = sits;
            Dimension = dimension;
            int maxPlayers = sits == null ? 0 : sits.Count;
            TableManager = new PokerTableManager(maxPlayers);
        }
    }
}