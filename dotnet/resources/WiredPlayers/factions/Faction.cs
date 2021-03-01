using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using WiredPlayers.character;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.factions
{
    public class Faction : Script
    {
        public static Dictionary<int, ChannelModel> ChannelList;
        public static List<FactionWarningModel> factionWarningList;

        public Faction()
        {
            // Initialize the required fields
            factionWarningList = new List<FactionWarningModel>();
        }

        public static string GetPlayerFactionRank(Player player)
        {
            // Get the player's character model
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);
            
            // Get the player faction
            FactionModel factionModel = Constants.FACTION_RANK_LIST.FirstOrDefault(fact => fact.Faction == characterModel.Faction && fact.Rank == characterModel.Rank);

            return factionModel == null ? string.Empty : characterModel.Sex == Sex.Male ? factionModel.DescriptionMale : factionModel.DescriptionFemale;
        }

        public static FactionWarningModel GetFactionWarnByTarget(int playerId, PlayerFactions faction)
        {
            // Get the faction warn for the given faction
            return factionWarningList.FirstOrDefault(factionWarn => factionWarn.PlayerId == playerId && factionWarn.Faction == faction);
        }

        public static bool IsPoliceMember(Player player)
        {
            // Check if the player is already connected
            if (!Character.IsPlaying(player)) return false;

            // Get the player's character model
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            return (characterModel.Faction == PlayerFactions.Police || characterModel.Faction == PlayerFactions.Sheriff) && characterModel.Rank > 0;
        }

        public static bool CheckInternalAffairs(PlayerFactions faction, Player target)
        {
            // Get the player's character model
            CharacterModel characterModel = target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // Check if the player is from Internal Affairs
            return faction == PlayerFactions.TownHall && (characterModel.Faction == PlayerFactions.Police && characterModel.Rank == 7);
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            StringBuilder sBuilder = new StringBuilder();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static ChannelModel GetPlayerOwnedChannel(int playerId)
        {
            // Get the channel owned by a player
            return ChannelList.Values.FirstOrDefault(channelModel => channelModel.Owner == playerId);
        }

        [RemoteEvent("removeWarning")]
        public void RemoveWarningEvent(Player player)
        {
            // Remove the report
            factionWarningList.RemoveAll(x => x.TakenBy == player.Value);
        }
    }
}
