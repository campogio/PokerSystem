using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WiredPlayers.character;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.factions;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.weapons
{
    public class Weapons : Script
    {
        public static Timer weaponTimer;
        private static Dictionary<int, Timer> VehicleWeaponTimer;
        private static List<WeaponCrateModel> weaponCrateList;

        public Weapons()
        {
            VehicleWeaponTimer = new Dictionary<int, Timer>();
            weaponCrateList = new List<WeaponCrateModel>();
        }

        public static void GivePlayerWeaponItems(Player player)
        {
            // Get the player's identifier
            int playerId = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;

            foreach (ItemModel item in Inventory.ItemCollection.Values)
            {
                if (!int.TryParse(item.hash, out int _) && item.ownerIdentifier == playerId && item.ownerEntity == Constants.ITEM_ENTITY_WHEEL)
                {
                    WeaponHash weaponHash = NAPI.Util.WeaponNameToModel(item.hash);
                    player.GiveWeapon(weaponHash, 0);
                    player.SetWeaponAmmo(weaponHash, item.amount);
                }
            }
        }

        public static async void GivePlayerNewWeapon(Player player, WeaponHash weapon, int bullets, bool licensed)
        {
            // Create weapon model
            ItemModel weaponModel = new ItemModel
            {
                hash = weapon.ToString(),
                amount = bullets,
                ownerEntity = Constants.ITEM_ENTITY_WHEEL,
                ownerIdentifier = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id,
                position = new Vector3(),
                dimension = 0
            };

            weaponModel.id = await DatabaseOperations.AddNewItem(weaponModel).ConfigureAwait(false);
            Inventory.ItemCollection.Add(weaponModel.id, weaponModel);

            // Give the weapon to the player
            player.SetWeaponAmmo(weapon, bullets);

            if (licensed)
            {
                // We add the weapon as a registered into database
                await Task.Run(() => DatabaseOperations.AddLicensedWeapon(weaponModel.id, player.Name)).ConfigureAwait(false);
            }
        }

        public static ItemModel GetWeaponItem(Player player, WeaponHash weaponHash)
        {
            // Get the identifier from the player
            int playerId = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;

            // Get the item identifier from the weapon hash
            return Inventory.ItemCollection.Values.FirstOrDefault(i => i.ownerEntity == Constants.ITEM_ENTITY_WHEEL && i.ownerIdentifier == playerId && NAPI.Util.WeaponNameToModel(i.hash) == weaponHash);
        }

        public static string GetGunAmmunitionType(WeaponHash weapon)
        {
            // Get the ammunition type given a weapon
            GunModel gunModel = Constants.GUN_LIST.FirstOrDefault(gun => weapon == gun.Weapon);

            return gunModel?.Ammunition ?? string.Empty;
        }

        public static int GetGunAmmunitionCapacity(WeaponHash weapon)
        {
            // Get the capacity from a weapons's clip magazine
            GunModel gunModel = Constants.GUN_LIST.FirstOrDefault(gun => weapon == gun.Weapon);

            return gunModel?.Capacity ?? 0;
        }

        public static ItemModel GetEquippedWeaponItemModelByHash(int playerId, WeaponHash weapon)
        {
            // Get the equipped weapon's item model
            return Inventory.ItemCollection.Values.FirstOrDefault(itemModel => itemModel.ownerIdentifier == playerId && (itemModel.ownerEntity == Constants.ITEM_ENTITY_WHEEL || itemModel.ownerEntity == Constants.ITEM_ENTITY_RIGHT_HAND) && weapon.ToString() == itemModel.hash);
        }

        public static WeaponCrateModel GetClosestWeaponCrate(Player player, float distance = 1.5f)
        {
            // Get the closest weapon crate
            return weaponCrateList.FirstOrDefault(w => player.Position.DistanceTo(w.Position) < distance && w.CarriedEntity == string.Empty);
        }

        public static WeaponCrateModel GetPlayerCarriedWeaponCrate(int playerId)
        {
            // Get the weapon crate carried by the player
            return weaponCrateList.FirstOrDefault(w => w.CarriedEntity == Constants.ITEM_ENTITY_PLAYER && w.CarriedIdentifier == playerId);
        }

        public static void WeaponsPrewarn()
        {
            // Send the warning message to all factions
            foreach (Player player in NAPI.Pools.GetAllPlayers())
            {
                if (Character.IsPlaying(player) && (int)player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction > Constants.LAST_STATE_FACTION)
                {
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.weapon_prewarn);
                }
            }

            // Timer for the next warning
            weaponTimer = new Timer(OnWeaponPrewarn, null, 600000, Timeout.Infinite);
        }

        public static void OnPlayerDisconnected(Player player)
        {
            WeaponCrateModel weaponCrate = GetPlayerCarriedWeaponCrate(player.Value);

            if (weaponCrate != null)
            {
                weaponCrate.Position = new Vector3(player.Position.X, player.Position.Y, player.Position.X - 1.0f);
                weaponCrate.CarriedEntity = string.Empty;
                weaponCrate.CarriedIdentifier = 0;

                // Place the crate on the floor
                //weaponCrate.CrateObject.Detach();
                weaponCrate.CrateObject.Position = weaponCrate.Position;
            }
        }
        
        public static void StoreCrateIntoTrunk(Player player, Vehicle vehicle)
        {
            // Get the vehicle identifier
            int vehicleId = vehicle.GetData<int>(EntityData.VehicleId);
            
            // Get player's hand item
            string attachmentJson = player.GetSharedData<string>(EntityData.PlayerWeaponCrate);
            AttachmentModel attachment = NAPI.Util.FromJson<AttachmentModel>(attachmentJson);
            WeaponCrateModel weaponCrate = weaponCrateList[attachment.itemId];

            // Store the item in the trunk
            weaponCrate.CarriedEntity = Constants.ITEM_ENTITY_VEHICLE;
            weaponCrate.CarriedIdentifier = vehicleId;

            // Remove player's weapon box
            UtilityFunctions.RemoveItemOnHands(player);
            player.StopAnimation();

            if (weaponCrateList.Count(c => c.CarriedEntity == Constants.ITEM_ENTITY_VEHICLE && vehicleId == c.CarriedIdentifier) == 1)
            {
                // Get the driver of the vehicle
                Player driver = vehicle.Occupants.Cast<Player>().ToList().FirstOrDefault(o => o.VehicleSeat == (int)VehicleSeat.Driver);
                
                if (driver != null && driver.Exists)
                {
                    // Create the checkpoint with the delivery point
                    Checkpoint weaponCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, Coordinates.CrateDeliver, new Vector3(), 2.5f, new Color(198, 40, 40, 200));
                    player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).JobCheckPoint = weaponCheckpoint.Value;
                    driver.SendChatMessage(Constants.COLOR_INFO + InfoRes.weapon_position_mark);
                    driver.TriggerEvent("showWeaponCheckpoint", Coordinates.CrateDeliver);
                }
            }

            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.trunk_stored_items);
        }
        
        public static void PickUpCrate(Player player, WeaponCrateModel weaponCrate)
        {
            int index = weaponCrateList.IndexOf(weaponCrate);
            weaponCrate.CarriedEntity = Constants.ITEM_ENTITY_PLAYER;
            weaponCrate.CarriedIdentifier = player.Value;
            player.PlayAnimation("anim@heists@box_carry@", "idle", (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl));

            // Create the object on the player's arms
            UtilityFunctions.AttachItemToPlayer(player, index, weaponCrate.CrateObject.Model.ToString(), "IK_R_Hand", new Vector3(0.0f, -0.5f, -0.25f), new Vector3(), EntityData.PlayerWeaponCrate);

            // Remove the item's object
            weaponCrate.CrateObject.Delete();
            weaponCrate.CrateObject = null;
        }

        private static List<Vector3> GetRandomWeaponSpawns(int spawnPosition)
        {
            Random random = new Random();
            List<Vector3> weaponSpawns = new List<Vector3>();
            CrateSpawnModel[] cratesInSpawn = GetSpawnsInPosition(spawnPosition);

            while (weaponSpawns.Count < Constants.MAX_CRATES_SPAWN)
            {
                Vector3 crateSpawn = cratesInSpawn[random.Next(cratesInSpawn.Length)].Position;
                if (!weaponSpawns.Contains(crateSpawn))
                {
                    weaponSpawns.Add(crateSpawn);
                }
            }
            
            return weaponSpawns;
        }

        private static CrateSpawnModel[] GetSpawnsInPosition(int spawnPosition)
        {
            // Get all the spawns in the position given
            return Constants.CrateSpawnCollection.Where(c => c.SpawnPoint == spawnPosition).ToArray();
        }

        private static CrateContentModel GetRandomCrateContent(int type, int chance)
        {
            // Get the weapon received
            WeaponChanceModel weaponAmmo = Constants.WeaponChanceArray.First(w => w.Type == type && w.MinChance <= chance && w.MaxChance >= chance);
            
            CrateContentModel crateContent = new CrateContentModel();
            {
                crateContent.item = weaponAmmo.Hash;
                crateContent.amount = weaponAmmo.Amount;
            }

            return crateContent;
        }

        private static void OnWeaponPrewarn(object unused)
        {
            weaponTimer.Dispose();

            int currentSpawn = 0;
            weaponCrateList = new List<WeaponCrateModel>();

            Random random = new Random();
            int spawnPosition = random.Next(Constants.MAX_WEAPON_SPAWNS);

            // Get crates' spawn points
            List<Vector3> weaponSpawns = GetRandomWeaponSpawns(spawnPosition);

            foreach (Vector3 spawn in weaponSpawns)
            {
                // Calculate weapon or ammunition crate
                int type = currentSpawn % 2;
                int chance = random.Next(type == 0 ? Constants.MAX_WEAPON_CHANCE : Constants.MAX_AMMO_CHANCE);
                CrateContentModel crateContent = GetRandomCrateContent(type, chance);

                // We create the crate
                WeaponCrateModel weaponCrate = new WeaponCrateModel();
                {
                    weaponCrate.ContentItem = crateContent.item;
                    weaponCrate.ContentAmount = crateContent.amount;
                    weaponCrate.Position = spawn;
                    weaponCrate.CarriedEntity = string.Empty;
                    weaponCrate.CrateObject = NAPI.Object.CreateObject(481432069, spawn, new Vector3(), 0);
                }

                weaponCrateList.Add(weaponCrate);
                currentSpawn++;
            }

            // Warn all the factions about the place
            foreach (Player player in NAPI.Pools.GetAllPlayers())
            {
                if (Character.IsPlaying(player) && (int)player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction > Constants.LAST_STATE_FACTION)
                {
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.weapon_spawn_island);
                }
            }

            // Timer to warn the police
            weaponTimer = new Timer(OnPoliceCalled, null, 240000, Timeout.Infinite);
        }

        private static void OnPoliceCalled(object unused)
        {
            weaponTimer.Dispose();

            // Send the warning message to all the police members
            foreach (Player player in NAPI.Pools.GetAllPlayers())
            {
                if (Faction.IsPoliceMember(player))
                {
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.weapon_spawn_island);
                }
            }

            // Finish the event
            weaponTimer = new Timer(OnWeaponEventFinished, null, 3600000, Timeout.Infinite);
        }

        private static async void OnVehicleUnpackWeapons(object vehicleObject)
        {
            Vehicle vehicle = (Vehicle)vehicleObject;
            int vehicleId = vehicle.GetData<int>(EntityData.VehicleId);
            
            foreach (WeaponCrateModel weaponCrate in weaponCrateList)
            {
                if (weaponCrate.CarriedEntity != Constants.ITEM_ENTITY_VEHICLE || weaponCrate.CarriedIdentifier != vehicleId) continue;

                // Unpack the weapon in the crate
                ItemModel item = new ItemModel()
                {
                    hash = weaponCrate.ContentItem,
                    amount = weaponCrate.ContentAmount,
                    ownerEntity = Constants.ITEM_ENTITY_VEHICLE,
                    ownerIdentifier = vehicleId
                };

                // Delete the crate
                weaponCrate.CarriedIdentifier = 0;
                weaponCrate.CarriedEntity = string.Empty;

                item.id = await DatabaseOperations.AddNewItem(item).ConfigureAwait(false);
                Inventory.ItemCollection.Add(item.id, item);
            }

            // Warn driver about unpacked crates
            Player driver = NAPI.Pools.GetAllPlayers().FirstOrDefault(p => p.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).LastVehicle == vehicle);
            
            if (driver != null && driver.Exists)
            {
                driver.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).LastVehicle = null;
                driver.SendChatMessage(Constants.COLOR_INFO + InfoRes.weapons_unpacked);
            }
            
            VehicleWeaponTimer[vehicleId].Dispose();
            VehicleWeaponTimer.Remove(vehicleId);
            vehicle.ResetData(EntityData.VehicleWeaponUnpacking);
        }

        private static void OnWeaponEventFinished(object unused)
        {
            weaponTimer.Dispose();

            foreach (WeaponCrateModel crate in weaponCrateList)
            {
                // Destroy the remaining crates
                if (crate.CrateObject != null && crate.CrateObject.Exists) crate.CrateObject.Delete();
            }

            // Destroy weapon crates
            weaponCrateList = new List<WeaponCrateModel>();
            weaponTimer = null;
        }

        private int GetVehicleWeaponCrates(int vehicleId)
        {
            // Get the crates on the vehicle
            return weaponCrateList.Count(w => w.CarriedEntity == Constants.ITEM_ENTITY_VEHICLE && w.CarriedIdentifier == vehicleId);
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte _)
        {
            if (vehicle.HasData(EntityData.VehicleId) && player.VehicleSeat == (int)VehicleSeat.Driver)
            {
                int vehicleId = vehicle.GetData<int>(EntityData.VehicleId);
                if (!vehicle.HasData(EntityData.VehicleWeaponUnpacking) && GetVehicleWeaponCrates(vehicleId) > 0)
                {
                    // Mark the delivery point
                    Checkpoint weaponCheckpoint = NAPI.Checkpoint.CreateCheckpoint(4, Coordinates.CrateDeliver, new Vector3(), 2.5f, new Color(198, 40, 40, 200));
                    player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).JobColShape = weaponCheckpoint;
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.weapon_position_mark);
                    player.TriggerEvent("showWeaponCheckpoint", Coordinates.CrateDeliver);
                }
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void OnPlayerExitVehicle(Player player, Vehicle vehicle)
        {
            if (vehicle != null && vehicle.HasData(EntityData.VehicleId))
            {
                int vehicleId = vehicle.GetData<int>(EntityData.VehicleId);

                // Get the temporary data
                PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

                if (data.JobColShape != null && data.JobColShape.Exists && GetVehicleWeaponCrates(vehicleId) > 0)
                {
                    player.TriggerEvent("deleteWeaponCheckpoint");
                }
            }
        }

        [ServerEvent(Event.PlayerEnterCheckpoint)]
        public void OnPlayerEnterCheckpoint(Checkpoint checkpoint, Player player)
        {
            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // Check if the checkpoint has been created and the player is driving
            if (data.JobColShape == null || data.JobColShape != checkpoint || player.VehicleSeat != (int)VehicleSeat.Driver) return;

            Vehicle vehicle = player.Vehicle;
            int vehicleId = vehicle.GetData<int>(EntityData.VehicleId);

            if (GetVehicleWeaponCrates(vehicleId) > 0)
            {
                data.JobColShape.Delete();
                data.JobColShape = null;

                // Delete the checkpoint
                player.TriggerEvent("deleteWeaponCheckpoint");

                // Freeze the vehicle
                vehicle.EngineStatus = false;
                data.LastVehicle = vehicle;
                vehicle.SetData(EntityData.VehicleWeaponUnpacking, true);

                VehicleWeaponTimer.Add(vehicleId, new Timer(OnVehicleUnpackWeapons, vehicle, 60000, Timeout.Infinite));

                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.wait_for_weapons);
            }
        }

        [RemoteEvent("reloadPlayerWeapon")]
        public void ReloadPlayerWeaponEvent(Player player, int currentBullets)
        {
            WeaponHash weapon = player.CurrentWeapon;
            int maxCapacity = GetGunAmmunitionCapacity(weapon);

            if (currentBullets < maxCapacity)
            {
                string bulletType = GetGunAmmunitionType(weapon);
                int playerId = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
                ItemModel bulletItem = Inventory.GetPlayerItemModelFromHash(playerId, bulletType);
                if (bulletItem != null)
                {
                    int bulletsLeft = maxCapacity - currentBullets;
                    if (bulletsLeft >= bulletItem.amount)
                    {
                        currentBullets += bulletItem.amount;

                        Task.Run(() => DatabaseOperations.DeleteSingleRow("items", "id", bulletItem.id)).ConfigureAwait(false);
                        Inventory.ItemCollection.Remove(bulletItem.id);
                    }
                    else
                    {
                        currentBullets += bulletsLeft;
                        bulletItem.amount -= bulletsLeft;

                        // Update the remaining bullets
                        Task.Run(() => DatabaseOperations.UpdateItem(bulletItem)).ConfigureAwait(false);
                    }

                    // Add ammunition to the weapon
                    ItemModel weaponItem = GetEquippedWeaponItemModelByHash(playerId, weapon);
                    weaponItem.amount = currentBullets;

                    // Update the bullets in the weapon
                    Task.Run(() => DatabaseOperations.UpdateItem(weaponItem)).ConfigureAwait(false);

                    // Reload the weapon
                    player.SetWeaponAmmo(weapon, currentBullets);
                    player.TriggerEvent("makePlayerReload");
                }
            }
        }

        [RemoteEvent("updateWeaponBullets")]
        public void UpdateWeaponBullets(Player player, int bullets)
        {
            if(player.CurrentWeapon != WeaponHash.Unarmed)
            {
                // Get the weapon from the hand
                ItemModel item = GetEquippedWeaponItemModelByHash(player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id, player.CurrentWeapon);

                // Set the bullets on the weapon
                item.amount = bullets;

                // Update the remaining bullets
                Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);
            }
        }
    }
}
