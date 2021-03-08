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

        public TextLabel CommandLabel { get; set; }
        public TextLabel HelpLabel { get; set; }
        public GTANetworkAPI.Object Model { get; set; }

        public PokerTable(int id, Vector3 position, List<PokerTableSit> sits, uint dimension)
        {
            Id = id;
            Position = position;
            Sits = sits;
            Dimension = dimension;
            int maxPlayers = sits == null ? 0 : sits.Count;
            TableManager = new PokerTableManager(maxPlayers);
            CommandLabel = NAPI.TextLabel.CreateTextLabel("/poker [Fiches]", position, 2.5f, 0.5f, 4, new Color(190, 235, 100), false, dimension);
            HelpLabel = NAPI.TextLabel.CreateTextLabel("Digita il comando per entrare al tavolo", new Vector3(position.X, position.Y, position.Z - 0.1f), 4.0f, 0.5f, 4, new Color(255, 255, 255), false, dimension);

            Model = NAPI.Object.CreateObject(2796928242, new Vector3(position.X,position.Y,position.Z-0.5f),new Vector3(0,0,0),255,dimension);

        }
    }
}