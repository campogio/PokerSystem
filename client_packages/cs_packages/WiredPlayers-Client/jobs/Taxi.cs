using RAGE;
using RAGE.Elements;

namespace WiredPlayers_Client.jobs
{
    class Taxi : Events.Script
    {
        private Blip Waypoint;

        public Taxi()
        {
            Events.Add("CreateTaxiPath", CreateTaxiPathEvent);

            Events.OnPlayerCreateWaypoint += OnPlayerCreateWaypoint;
        }

        private void CreateTaxiPathEvent(object[] args)
        {
            // Create the waypoint
            Waypoint = new Blip(8, (Vector3)args[0]);
            Waypoint.SetRoute(true);
        }

        private void OnPlayerCreateWaypoint(Vector3 position)
        {
            // Check if the player is in any vehicle
            if (Player.LocalPlayer.Vehicle == null || !Player.LocalPlayer.IsInAnyTaxi()) return;

            // Send the waypoint to the driver
            Events.CallRemote("RequestTaxiDestination", position);
        }
    }
}
