namespace WiredPlayers.Data.Temporary
{
    public class WeaponChanceModel
    {
        public int Type { get; set; }
        public string Hash { get; set; }
        public int Amount { get; set; }
        public int MinChance { get; set; }
        public int MaxChance { get; set; }

        public WeaponChanceModel(int type, string hash, int amount, int minChance, int maxChance)
        {
            Type = type;
            Hash = hash;
            Amount = amount;
            MinChance = minChance;
            MaxChance = maxChance;
        }
    }
}
