﻿namespace WiredPlayers.Data.Persistent
{
    public class ClothesModel
    {
        public int id { get; set; }
        public int player { get; set; }
        public int type { get; set; }
        public int slot { get; set; }
        public int drawable { get; set; }
        public int texture { get; set; }
        public bool dressed { get; set; }
        public bool Stored { get; set; }
    }
}
