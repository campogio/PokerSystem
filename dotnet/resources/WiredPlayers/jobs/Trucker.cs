using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using WiredPlayers.Data.Base;
using WiredPlayers.messages.error;
using WiredPlayers.Utility;

namespace WiredPlayers.jobs
{
    public class Trucker : Script
    {
        public static void CheckTruckerOrders(Player player)
        {
            // Get the deliverable orders
            List<OrderModel> truckerOrders = UtilityFunctions.truckerOrderCollection.Values.Where(o => !o.taken).ToList();

            if (truckerOrders.Count == 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.order_none);
                return;
            }

            List<float> distancesList = new List<float>();

            foreach (OrderModel order in truckerOrders)
            {
                float distance = player.Position.DistanceTo(order.position);
                distancesList.Add(distance);
            }

            player.TriggerEvent("showTruckerOrders", NAPI.Util.ToJson(truckerOrders), NAPI.Util.ToJson(distancesList));
        }
    }
}
