namespace WiredPlayers.Data.Persistent
{
    public class AnnoucementModel
    {
        public int Id { get; set; }
        public int Winner { get; set; }
        public int Journalist { get; set; }
        public int Amount { get; set; }
        public string Annoucement { get; set; }
        public bool Given { get; set; }
    }
}
