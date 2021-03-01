using GTANetworkAPI;
using System.Collections.Generic;
using System.Threading;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.success;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.jobs
{
    public class Hooker : Script
    {
        public static Dictionary<int, Timer> sexTimerList;

        public Hooker()
        {
            // Initialize the variables
            sexTimerList = new Dictionary<int, Timer>();
        }

        public static void OnPlayerDisconnected(Player player)
        {
            if (sexTimerList.TryGetValue(player.Value, out Timer sexTimer) == true)
            {
                sexTimer.Dispose();
                sexTimerList.Remove(player.Value);
            }
        }

        public static void OnSexServiceTimer(object playerObject)
        {
            Player player = (Player)playerObject;

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // We stop both animations
            player.StopAnimation();
            data.AlreadyFucking.StopAnimation();

            // Health the player
            player.Health = 100;

            if (sexTimerList.TryGetValue(player.Value, out Timer sexTimer))
            {
                sexTimer.Dispose();
                sexTimerList.Remove(player.Value);
            }

            // Send finish message to both players
            player.SendChatMessage(Constants.COLOR_SUCCESS + SuccRes.hooker_service_finished);
            data.AlreadyFucking.SendChatMessage(Constants.COLOR_SUCCESS + SuccRes.hooker_client_satisfied);

            // Delete all the data
            data.Animation = false;
            data.HookerService = HookerService.None;
            data.AlreadyFucking.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).AlreadyFucking = null;
            data.AlreadyFucking = null;
        }
    }
}


