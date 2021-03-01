using RAGE;
using RAGE.Elements;
using WiredPlayers_Client.globals;
using System.Linq;

namespace WiredPlayers_Client.weapons
{
    class Weapons : Events.Script
    {
        private Blip weaponBlip = null;

        public Weapons()
        {
            Events.Add("makePlayerReload", MakePlayerReloadEvent);
            Events.Add("showWeaponCheckpoint", ShowWeaponCheckpointEvent);
            Events.Add("deleteWeaponCheckpoint", DeleteWeaponCheckpointEvent);

            Events.OnPlayerWeaponShot += OnPlayerWeaponShotEvent;
        }

        public static bool IsValidWeapon(uint weapon)
        {
            return Constants.ValidWeapons.Count(w => RAGE.Game.Misc.GetHashKey(w) == weapon) > 0;
        }

        private void MakePlayerReloadEvent(object[] args)
        {
            // Reload the weapon
            Player.LocalPlayer.TaskReloadWeapon(true);
        }

        private void ShowWeaponCheckpointEvent(object[] args)
        {
            // Get the variables from the array
            Vector3 position = (Vector3)args[0];

            // Set the checkpoint with the crates
            weaponBlip = new Blip(1, position, string.Empty, 1, 1);
        }

        private void DeleteWeaponCheckpointEvent(object[] args)
        {
            // Delete the checkpoint on the map
            weaponBlip.Destroy();
            weaponBlip = null;
        }

        private void OnPlayerWeaponShotEvent(Vector3 targetPos, Player target, Events.CancelEventArgs cancel)
        {
            // Calculate the weapon the player is holding
            uint weaponHash = RAGE.Game.Weapon.GetSelectedPedWeapon(Player.LocalPlayer.Handle);

            // Get the bullets remaining
            int bullets = Player.LocalPlayer.GetAmmoInWeapon(weaponHash);

            // Update the weapon's bullet amount
            Events.CallRemote("updateWeaponBullets", bullets);
        }
    }
}
