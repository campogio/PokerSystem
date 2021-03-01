using GTANetworkAPI;

namespace WiredPlayers.Data.Temporary
{
    public class GunModel
    {
        public WeaponHash Weapon { get; set; }
        public string Ammunition { get; set; }
        public int Capacity { get; set; }

        public GunModel(WeaponHash weapon, string ammunition, int capacity)
        {
            Weapon = weapon;
            Ammunition = ammunition;
            Capacity = capacity;
        }
    }
}
