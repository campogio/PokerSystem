using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Persistent
{
    public class BuildingModel
    {
        public int Id { get; set; }
        public BuildingTypes Type { get; set; }
		
		public override string ToString()
		{
			return string.Format("{0},{1}", (int)Type, Id);
		}
    }
}
