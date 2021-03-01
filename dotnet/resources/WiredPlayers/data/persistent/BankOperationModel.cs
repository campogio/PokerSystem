namespace WiredPlayers.Data.Persistent
{
    public class BankOperationModel
    {
        public string Source { get; set; }
        public string Receiver { get; set; }
        public string Type { get; set; }
        public int Amount { get; set; }
        public string Day { get; set; }
        public string Time { get; set; }
    }
}
