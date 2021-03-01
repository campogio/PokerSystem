using GTANetworkAPI;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Data.Base
{
    public class PropertyModel
    {
        public int Id { get; set; }
        public string Caption { get; set; }
        public Vector3 Entrance { get; set; }
        public uint Dimension { get; set; }
        public TextLabel Label { get; set; }
        public ColShape ColShape { get; set; }
        public IplModel Ipl { get; set; }
    }
}
