using System;
using RAGE;
using RAGE.Elements;

namespace WiredPlayers_Client.admin
{
    class Admin : Events.Script
    {
        private int SpectateCamera;

        public Admin()
        {
            // Add the events
            Events.Add("StartSpectating", StartSpectatingEvent);
            Events.Add("StopSpectating", StopSpectatingEvent);

            // Initialize the variables
            SpectateCamera = -1;
        }

        private void StartSpectatingEvent(object[] args)
        {
            // Get the target player
            ushort targetId = (ushort)Convert.ToUInt32(args[0]);
            Player target = Entities.Players.GetAtRemote(targetId);

            // Set the player spectating the target
            SpectateCamera = RAGE.Game.Cam.CreateCameraWithParams(RAGE.Game.Misc.GetHashKey("spectate-cam"), target.Position.X, target.Position.Y, target.Position.Z, 0.0f, 0.0f, 0.0f, 90.0f, true, 2);
            RAGE.Game.Cam.SetCamActive(SpectateCamera, true);
            RAGE.Game.Cam.RenderScriptCams(true, false, 0, true, false, 0);
            RAGE.Game.Entity.AttachEntityToEntity(SpectateCamera, target.Handle, 0, 0, 0, 0, 0, 0, 0, false, false, false, true, 0, true);

            // Freeze and hide the player
            Player.LocalPlayer.SetAlpha(0, false);
            Player.LocalPlayer.FreezePosition(true);
        }

        private void StopSpectatingEvent(object[] args)
        {
            // Destroy the spectate camera
            RAGE.Game.Cam.DestroyCam(SpectateCamera, true);
            RAGE.Game.Cam.RenderScriptCams(false, false, 0, true, false, 0);
            SpectateCamera = -1;
        }
    }
}
