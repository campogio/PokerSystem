using System.Collections.Generic;

namespace WiredPlayers.Data.Persistent
{
    public class PhoneModel
    {
        public int ItemId { get; set; }
        public int Number { get; set; }
        public Dictionary<int, ContactModel> Contacts { get; set; }
    }
}
