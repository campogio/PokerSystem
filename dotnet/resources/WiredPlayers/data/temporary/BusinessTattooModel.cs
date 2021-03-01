using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Temporary
{
    public class BusinessTattooModel
    {
        public TattoZone slot { get; set; }
        public string name { get; set; }
        public string library { get; set; }
        public string maleHash { get; set; }
        public string femaleHash { get; set; }
        public int price { get; set; }

        public BusinessTattooModel(TattoZone slot, string name, string library, string maleHash, string femaleHash, int price)
        {
            this.slot = slot;
            this.name = name;
            this.library = library;
            this.maleHash = maleHash;
            this.femaleHash = femaleHash;
            this.price = price;
        }
    }
}
