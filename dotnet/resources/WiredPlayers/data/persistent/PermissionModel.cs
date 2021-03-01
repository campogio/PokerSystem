namespace WiredPlayers.Data.Persistent
{
    public class PermissionModel
    {
        public int PlayerId { get; set; }
        public string Command { get; set; }
        public string Option { get; set; }
    }
}
