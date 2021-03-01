namespace WiredPlayers.Data.Persistent
{
    public class BloodModel
    {
        public int Id { get; set; }
        public int Doctor { get; set; }
        public int Patient { get; set; }
        public string Type { get; set; }
        public bool Used { get; set; }
    }
}
