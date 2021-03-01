using GTANetworkAPI;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Persistent
{
    public class PoliceControlModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PoliceControlItems Item { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Object ControlObject { get; set; }

        public PoliceControlModel() { }

        public PoliceControlModel(int id, string name, PoliceControlItems item, Vector3 position, Vector3 rotation)
        {
            Id = id;
            Name = name;
            Item = item;
            Position = position;
            Rotation = rotation;
        }

        public PoliceControlModel Copy()
        {
            PoliceControlModel policeControlModel = new PoliceControlModel();
            {
                Id = Id;
                Name = Name;
                Item = Item;
                Position = Position;
                Rotation = Rotation;
            }

            return policeControlModel;
        }
    }
}
