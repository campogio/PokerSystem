using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Temporary
{
    public class FactionModel
    {
        public string DescriptionMale { get; set; }
        public string DescriptionFemale { get; set; }
        public PlayerFactions Faction { get; set; }
        public int Rank { get; set; }
        public int Salary { get; set; }

        public FactionModel(string descriptionMale, string descriptionFemale, PlayerFactions faction, int rank, int salary)
        {
            DescriptionMale = descriptionMale;
            DescriptionFemale = descriptionFemale;
            Faction = faction;
            Rank = rank;
            Salary = salary;
        }
    }
}
