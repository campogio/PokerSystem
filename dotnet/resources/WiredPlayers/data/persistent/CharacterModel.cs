using GTANetworkAPI;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Persistent
{
    public class CharacterModel
    {
        public int Id { get; set; }
        public SkinModel Skin { get; set; }
        public string RealName { get; set; }
        public string Model { get; set; }
        public StaffRank AdminRank { get; set; }
        public string AdminName { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public int Money { get; set; }
        public int Bank { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }
        public int Hunger { get; set; }
        public int Thirst { get; set; }
        public int Age { get; set; }
        public Sex Sex { get; set; }
        public PlayerFactions Faction { get; set; }
        public PlayerJobs Job { get; set; }
        public int Rank { get; set; }
        public bool OnDuty { get; set; }
        public int Radio { get; set; }
        public int KilledBy { get; set; }
        public string Jailed { get; set; }
        public string VehicleKeys { get; set; }
        public int Documentation { get; set; }
        public string Licenses { get; set; }
        public int Insurance { get; set; }
        public int WeaponLicense { get; set; }
        public int Status { get; set; }
        public int Played { get; set; }
        public int HouseRent { get; set; }
        public BuildingModel BuildingEntered { get; set; }
        public int EmployeeCooldown { get; set; }
        public int JobCooldown { get; set; }
        public int JobDeliver { get; set; }
        public string JobPoints { get; set; }
        public int RolePoints { get; set; }
        public bool Playing { get; set; }

        public CharacterModel()
        {
            Skin = new SkinModel();
            AdminRank = StaffRank.None;
            Sex = Sex.None;
            Faction = PlayerFactions.None;
            Job = PlayerJobs.None;
            BuildingEntered = new BuildingModel()
            {
                Id = 0,
                Type = BuildingTypes.None
            };
        }
    }
}
