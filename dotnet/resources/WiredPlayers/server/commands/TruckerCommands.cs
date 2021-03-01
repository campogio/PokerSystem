using GTANetworkAPI;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Utility;
using WiredPlayers.messages.error;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class TruckerCommands
    {
        [Command]
        public static void DeliverCommand(Player player)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Job != PlayerJobs.Trucker)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_trucker);
                return;
            }

            if (player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).JobCheckPoint > 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.already_in_route);
                return;
            }

            // Create the delivery crates
            player.TriggerEvent("createTruckerCrates");
        }
    }
}
