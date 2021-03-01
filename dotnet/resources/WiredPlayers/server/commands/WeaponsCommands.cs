using GTANetworkAPI;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Utility;
using WiredPlayers.messages.administration;
using WiredPlayers.messages.error;
using WiredPlayers.weapons;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Server.Commands
{
    public static class WeaponsCommands
    {
        [Command]
        public static void WeaponsEventCommand(Player player)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).AdminRank <= StaffRank.SuperGameMaster) return;
            
            if (Weapons.weaponTimer != null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.weapon_event_on_course);
                return;
            }

            Weapons.WeaponsPrewarn();
            player.SendChatMessage(Constants.COLOR_ADMIN_INFO + AdminRes.weapon_event_started);
        }
    }
}
