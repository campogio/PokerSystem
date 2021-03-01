using GTANetworkAPI;
using System;

namespace WiredPlayers.Data.Temporary
{
    public class IplModel
    {
        public string Name { get; set; }
        public Enum Type { get; set; }
        public string Description { get; set; }
        public Vector3 ExitPoint { get; set; }
        public Vector3 ActionPoint { get; set; }
        public SpawnModel PedPoint { get; set; }
        
        public IplModel() { }
        
        public IplModel(string ipl, Enum type, string desc, Vector3 exit)
        {
            Name = ipl;
            Type = type;
            Description = desc;
            ExitPoint = exit;
        }

        public IplModel Copy()
        {
            return new IplModel()
            {
                Name = Name,
                Type = Type,
                Description = Description,
                ExitPoint = ExitPoint,
                ActionPoint = ActionPoint,
                PedPoint = PedPoint
            };
        }
    }
}
