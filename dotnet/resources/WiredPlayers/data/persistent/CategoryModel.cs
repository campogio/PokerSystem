using System.Collections.Generic;

namespace WiredPlayers.Data.Persistent
{
    public class CategoryModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public Dictionary<int, AnimationModel> Animations { get; set; }
    }
}
