using System;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Temporary
{
    public class BusinessClothesModel
    {
        public int type { get; set; }
        public string description { get; set; }
        public Enum BodyPart { get; }
        public int clothesId { get; set; }
        public int texture { get; set; }
        public Sex sex { get; set; }
        public int products { get; set; }

        public BusinessClothesModel() { }

        public BusinessClothesModel(int type, string description, AccessorySlots bodyPart, int clothesId, Sex sex, int products)
        {
            this.type = type;
            this.description = description;
            BodyPart = bodyPart;
            this.clothesId = clothesId;
            this.sex = sex;
            this.products = products;
        }

        public BusinessClothesModel(int type, string description, ClothesSlots bodyPart, int clothesId, Sex sex, int products)
        {
            this.type = type;
            this.description = description;
            BodyPart = bodyPart;
            this.clothesId = clothesId;
            this.sex = sex;
            this.products = products;
        }
    }
}
