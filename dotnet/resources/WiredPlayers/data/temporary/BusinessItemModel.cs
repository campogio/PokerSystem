using GTANetworkAPI;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Temporary
{
    public class BusinessItemModel
    {
        public string description { get; set; }
        public string hash { get; set; }
        public ItemTypes type { get; set; }
        public int products { get; set; }
        public float weight { get; set; }
        public int health { get; set; }
        public int uses { get; set; }
        public Vector3 position { get; set; }
        public Vector3 rotation { get; set; }
        public BusinessTypes Business { get; set; }
        public float alcoholLevel { get; set; }

        public BusinessItemModel(string description, string hash, ItemTypes type, int products, float weight, int health, int uses, Vector3 position, Vector3 rotation, BusinessTypes business, float alcoholLevel)
        {
            this.description = description;
            this.hash = hash;
            this.type = type;
            this.products = products;
            this.weight = weight;
            this.health = health;
            this.uses = uses;
            this.position = position;
            this.rotation = rotation;
            Business = business;
            this.alcoholLevel = alcoholLevel;
        }
    }
}
