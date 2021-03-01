namespace WiredPlayers.Data.Persistent
{
    public class AccountModel
    {
        public string SocialName { get; set; }
        public string ForumName { get; set; }
        public int State { get; set; }
        public int LastCharacter { get; set; }
        public bool Registered { get; set; }

        public AccountModel() { }

        public AccountModel(string socialName, string forumName, int state, int lastCharacter)
        {
            SocialName = socialName;
            ForumName = forumName;
            State = state;
            LastCharacter = lastCharacter;
        }
    }
}
