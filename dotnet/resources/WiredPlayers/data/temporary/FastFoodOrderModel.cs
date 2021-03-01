using WiredPlayers.Data.Base;

namespace WiredPlayers.Data.Temporary
{
    public class FastfoodOrderModel : OrderModel
    {
        public int pizzas { get; set; }
        public int hamburgers { get; set; }
        public int sandwitches { get; set; }
    }
}
