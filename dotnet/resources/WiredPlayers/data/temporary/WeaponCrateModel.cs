using GTANetworkAPI;

namespace WiredPlayers.Data.Temporary
{
    public class WeaponCrateModel
    {
        public string ContentItem { get; set; }
        public int ContentAmount { get; set; }
        public Vector3 Position { get; set; }
        public string CarriedEntity { get; set; }
        public int CarriedIdentifier { get; set; }
        public Object CrateObject { get; set; }
    }
}
