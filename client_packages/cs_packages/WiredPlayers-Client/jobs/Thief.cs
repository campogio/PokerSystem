using RAGE;
using RAGE.Elements;

namespace WiredPlayers_Client.jobs
{
    class Thief : Events.Script
    {
        private Blip VehicleDeliveryBlip;
        private Checkpoint VehicleDeliveryCheckpoint;

        public Thief()
        {
            // Register custom events
            Events.Add("ShowVehicleDeliveryPoint", ShowVehicleDeliveryPointEvent);
            Events.Add("RemoveStolenVehicleDeliverPoint", RemoveStolenVehicleDeliverPointEvent);

            // Add RAGE's events
            Events.OnPlayerEnterCheckpoint += OnPlayerEnterCheckpoint;
        }

        private void ShowVehicleDeliveryPointEvent(object[] args)
        {
            // Get the position where the vehicle has to be delivered
            Vector3 position = (Vector3)args[0];

            // Set the checkpoint in the map
            VehicleDeliveryCheckpoint = new Checkpoint(4, position, 7.5f, new Vector3(), new RGBA(198, 40, 40, 200));

            // Show the mark in the map
            VehicleDeliveryBlip = new Blip(1, position, string.Empty, 1f, 1);
        }

        private void RemoveStolenVehicleDeliverPointEvent(object[] args)
        {
            VehicleDeliveryCheckpoint.Destroy();
            VehicleDeliveryBlip.Destroy();
        }

        private void OnPlayerEnterCheckpoint(Checkpoint checkpoint, Events.CancelEventArgs cancel)
        {
            // Check if the checkpoint is the correct one
            if (checkpoint != VehicleDeliveryCheckpoint || Player.LocalPlayer.Vehicle == null) return;

            // Deliver the player vehicle
            Events.CallRemote("DeliverStoleVehicle");

            // Cancel the rest of the events
            cancel.Cancel = true;
        }
    }
}
