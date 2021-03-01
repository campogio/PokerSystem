using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.character;
using WiredPlayers.chat;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.factions;
using WiredPlayers.Utility;
using WiredPlayers.jobs;
using WiredPlayers.messages.arguments;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using WiredPlayers.vehicles;
using WiredPlayers.weapons;
using WiredPlayers.Buildings;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class PoliceCommands
    {
        [Command]
        public static void CheckCommand(Player player)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (!player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            if (!Faction.IsPoliceMember(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                return;
            }

            // Get the closest vehicle
            Vehicle vehicle = Vehicles.GetClosestVehicle(player, 3.5f);

            if (vehicle == null || !vehicle.Exists)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_vehicles_near);
                return;
            }

            // Get the vehicle information
            VehicleModel vehModel = Vehicles.GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId));

            player.SendChatMessage(Constants.COLOR_INFO + string.Format(GenRes.vehicle_check_title, vehModel.Id));
            player.SendChatMessage(Constants.COLOR_INFO + GenRes.vehicle_model + Constants.COLOR_HELP + (VehicleHash)vehModel.Model);
            player.SendChatMessage(Constants.COLOR_INFO + GenRes.vehicle_plate + Constants.COLOR_HELP + vehModel.Plate);
            player.SendChatMessage(Constants.COLOR_INFO + GenRes.owner + Constants.COLOR_HELP + vehModel.Owner);

            string message = string.Format(InfoRes.check_vehicle_plate, player.Name, (VehicleHash)vehModel.Model);
            Chat.SendMessageToNearbyPlayers(player, message, ChatTypes.Me, 20.0f, true);
        }

        [Command]
        public static void FriskCommand(Player player, string targetString)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (!player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            if (!Faction.IsPoliceMember(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                return;
            }

            // Get the target player
            Player target = UtilityFunctions.GetPlayer(targetString);

            if (target == null || player.Position.DistanceTo(target.Position) > 2.5f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                return;
            }

            if (target == player)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_searched_himself);
                return;
            }

            List<InventoryModel> inventory = Inventory.GetEntityInventory(target, true);
            player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).SearchedTarget = target;

            // Send the message to the players near
            Chat.SendMessageToNearbyPlayers(player, string.Format(InfoRes.player_frisk, player.Name, target.Name), ChatTypes.Me, 20.0f, true);

            // Show target's inventory to the player
            player.TriggerEvent("showPlayerInventory", NAPI.Util.ToJson(inventory), InventoryTarget.Player);
        }

        [Command]
        public static void IncriminateCommand(Player player, string targetString)
        {
            if (!Job.IsPlayerOnWorkPlace(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_jail_area);
                return;
            }

            if (!player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (!Faction.IsPoliceMember(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                return;
            }

            // Get the target player
            Player target = UtilityFunctions.GetPlayer(targetString);

            if (target == null || player.Position.DistanceTo(target.Position) > 2.5f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                return;
            }

            if (target == player)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_incriminated_himself);
                return;
            }

            player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).IncriminatedTarget = target;
            player.TriggerEvent("showCrimesMenu", NAPI.Util.ToJson(Police.crimeList));
        }

        [Command]
        public static void FineCommand(Player player, string args)
        {
            if (!Faction.IsPoliceMember(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                return;
            }

            if (!player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            // Get the message splitted
            string[] arguments = args.Trim().Split(' ');

            if (arguments.Length < 3)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.fine);
                return;
            }

            // Initialize the target player
            Player target = UtilityFunctions.GetPlayer(ref arguments);

            if (target == null || player.Position.DistanceTo(target.Position) > 2.5f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                return;
            }

            if (target == player)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_fined_himself);
                return;
            }

            if (arguments.Length < 2)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.fine);
                return;
            }

            // Get the money amount
            if (!int.TryParse(arguments[0], out int money) || money <= 0)
            {
                // Send the error message to the player
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.fine);
                return;
            }

            FineModel fine = new FineModel();
            {
                fine.officer = player.Name;
                fine.target = target.Name;
                fine.amount = money;
                fine.reason = string.Join(' ', arguments.Skip(1).ToArray());
            }

            // Insert the fine into the database
            Task.Run(() => DatabaseOperations.InsertFine(fine)).ConfigureAwait(false);

            // Send the message to both players
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.fine_given, target.Name));
            target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.fine_received, player.Name));
        }

        [Command]
        public static void HandcuffCommand(Player player, string targetString)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (!player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            if (!Faction.IsPoliceMember(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                return;
            }

            // Get the target player
            Player target = UtilityFunctions.GetPlayer(targetString);

            if (target == null || player.Position.DistanceTo(target.Position) > 2.5f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                return;
            }

            if (target == player)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_fined_himself);
                return;
            }

            if (!target.HasSharedData(EntityData.PlayerHandcuffed))
            {
                if (target.HasSharedData(EntityData.PlayerRightHand))
                {
                    // Remove the item on the player's hand
                    Inventory.StoreItemOnHand(target);
                }

                // Remove the player weapon
                player.GiveWeapon(WeaponHash.Unarmed, 0);

                // Handcuff the player
                UtilityFunctions.AttachItemToPlayer(target, 0, NAPI.Util.GetHashKey("prop_cs_cuffs_01").ToString(), "IK_R_Hand", new Vector3(), new Vector3(), EntityData.PlayerHandcuffed);
                target.PlayAnimation("mp_arresting", "idle", (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl));

                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.cuffed, target.Name));
                target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.cuffed_by, player.Name));
            }
            else
            {
                // Remove the cuffs from the player
                NAPI.ClientEvent.TriggerClientEventInDimension(target.Dimension, "dettachItemFromPlayer", target.Value);
                target.ResetSharedData(EntityData.PlayerHandcuffed);
                target.StopAnimation();

                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.uncuffed, target.Name));
                target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.uncuffed_by, player.Name));
            }
        }

        [Command]
        public static async Task EquipmentCommand(Player player, string action, string type = "")
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (!Faction.IsPoliceMember(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                return;
            }

            if (!Police.IsCloseToEquipmentLockers(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_in_room_lockers);
                return;
            }

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (!characterModel.OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            if (action.Equals(ArgRes.basic, StringComparison.InvariantCultureIgnoreCase))
            {
                Weapons.GivePlayerNewWeapon(player, WeaponHash.Flashlight, 0, false);
                Weapons.GivePlayerNewWeapon(player, WeaponHash.Nightstick, 0, true);
                Weapons.GivePlayerNewWeapon(player, WeaponHash.Stungun, 0, true);

                player.Armor = 100;

                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.equip_basic_received);

                return;
            }

            if (action.Equals(ArgRes.ammunition, StringComparison.InvariantCultureIgnoreCase))
            {
                if (characterModel.Rank <= 1)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_enough_police_rank);
                    return;
                }

                // Check if it's a valid weapon
                string ammunition = Weapons.GetGunAmmunitionType(player.CurrentWeapon);

                if (ammunition == string.Empty)
                {
                    // The player has no weapon on the hand
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.weapon_not_valid);
                    return;
                }

                ItemModel bulletItem = Inventory.GetPlayerItemModelFromHash(characterModel.Id, ammunition);

                if (bulletItem != null)
                {
                    switch (player.CurrentWeapon)
                    {
                        case WeaponHash.Combatpistol:
                            bulletItem.amount += Constants.STACK_PISTOL_CAPACITY;
                            break;

                        case WeaponHash.Smg:
                            bulletItem.amount += Constants.STACK_MACHINEGUN_CAPACITY;
                            break;

                        case WeaponHash.Carbinerifle:
                            bulletItem.amount += Constants.STACK_ASSAULTRIFLE_CAPACITY;
                            break;

                        case WeaponHash.Pumpshotgun:
                            bulletItem.amount += Constants.STACK_SHOTGUN_CAPACITY;
                            break;

                        case WeaponHash.Sniperrifle:
                            bulletItem.amount += Constants.STACK_SNIPERRIFLE_CAPACITY;
                            break;
                    }

                    await Task.Run(() => DatabaseOperations.UpdateItem(bulletItem)).ConfigureAwait(false);
                }
                else
                {
                    bulletItem = new ItemModel()
                    {
                        hash = ammunition,
                        ownerEntity = Constants.ITEM_ENTITY_PLAYER,
                        ownerIdentifier = characterModel.Id,
                        amount = 30,
                        position = new Vector3(),
                        dimension = 0
                    };

                    bulletItem.id = await DatabaseOperations.AddNewItem(bulletItem).ConfigureAwait(false);
                    Inventory.ItemCollection.Add(bulletItem.id, bulletItem);
                }

                // Send the message to the player
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.equip_ammo_received);

                return;
            }

            if (action.Equals(ArgRes.weapon, StringComparison.InvariantCultureIgnoreCase))
            {
                if (characterModel.Rank <= 1)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_enough_police_rank);
                    return;
                }

                // Check if the player typed any weapon
                if (string.IsNullOrEmpty(type))
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.equipment_weapon);
                    return;
                }

                WeaponHash selectedWeap;
                if (type.Equals(ArgRes.pistol, StringComparison.InvariantCultureIgnoreCase))
                {
                    selectedWeap = WeaponHash.Combatpistol;
                }
                else if (type.Equals(ArgRes.revolver, StringComparison.InvariantCultureIgnoreCase))
                {
                    selectedWeap = WeaponHash.Revolver;
                }
                else if (type.Equals(ArgRes.machinegun, StringComparison.InvariantCultureIgnoreCase))
                {
                    selectedWeap = WeaponHash.Smg;
                }
                else if (type.Equals(ArgRes.assault, StringComparison.InvariantCultureIgnoreCase))
                {
                    selectedWeap = WeaponHash.Carbinerifle;
                }
                else if (type.Equals(ArgRes.sniper, StringComparison.InvariantCultureIgnoreCase))
                {
                    selectedWeap = WeaponHash.Sniperrifle;
                }
                else if (type.Equals(ArgRes.shotgun, StringComparison.InvariantCultureIgnoreCase))
                {
                    selectedWeap = WeaponHash.Pumpshotgun;
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.equipment_weapon);
                    return;
                }

                Weapons.GivePlayerNewWeapon(player, selectedWeap, 0, true);
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.equip_weap_received);

                return;
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.equipment);
        }

        [Command]
        public static void ControlCommand(Player player, string action)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (!Faction.IsPoliceMember(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                return;
            }

            if (!player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            // Get all the police controls
            List<string> policeControls = Police.GetDifferentPoliceControls();

            if (action.Equals(ArgRes.load, StringComparison.InvariantCultureIgnoreCase))
            {
                if (policeControls.Count == 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_police_controls);
                    return;
                }

                player.TriggerEvent("loadPoliceControlList", NAPI.Util.ToJson(policeControls), Actions.Load);

                return;
            }

            if (action.Equals(ArgRes.save, StringComparison.InvariantCultureIgnoreCase))
            {
                if (policeControls.Count == 0)
                {
                    player.TriggerEvent("showPoliceControlName");
                }
                else
                {
                    player.TriggerEvent("loadPoliceControlList", NAPI.Util.ToJson(policeControls));
                }

                return;
            }

            if (action.Equals(ArgRes.rename, StringComparison.InvariantCultureIgnoreCase))
            {
                if (policeControls.Count == 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_police_controls);
                    return;
                }

                player.TriggerEvent("loadPoliceControlList", NAPI.Util.ToJson(policeControls), Actions.Rename);

                return;
            }

            if (action.Equals(ArgRes.remove, StringComparison.InvariantCultureIgnoreCase))
            {
                if (policeControls.Count == 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_police_controls);
                    return;
                }

                player.TriggerEvent("loadPoliceControlList", NAPI.Util.ToJson(policeControls), Actions.Delete);

                return;
            }

            if (action.Equals(ArgRes.clear, StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (PoliceControlModel policeControl in Police.policeControlList)
                {
                    if (policeControl.ControlObject != null)
                    {
                        policeControl.ControlObject.Delete();
                    }
                }

                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.police_control_cleared);

                return;
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.control);
        }

        [Command]
        public static void PutCommand(Player player, string item)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (!player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            if (!Faction.IsPoliceMember(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                return;
            }

            PoliceControlModel policeControl;

            if (item.Equals(ArgRes.cone, StringComparison.InvariantCultureIgnoreCase))
            {
                policeControl = new PoliceControlModel(0, string.Empty, PoliceControlItems.Cone, player.Position, player.Rotation);
                policeControl.Position = new Vector3(policeControl.Position.X, policeControl.Position.Y, policeControl.Position.Z - 1.0f);
                policeControl.ControlObject = NAPI.Object.CreateObject((int)PoliceControlItems.Cone, policeControl.Position, policeControl.Rotation);
                Police.policeControlList.Add(policeControl);
                return;
            }

            if (item.Equals(ArgRes.beacon, StringComparison.InvariantCultureIgnoreCase))
            {
                policeControl = new PoliceControlModel(0, string.Empty, PoliceControlItems.Beacon, player.Position, player.Rotation);
                policeControl.Position = new Vector3(policeControl.Position.X, policeControl.Position.Y, policeControl.Position.Z - 1.0f);
                policeControl.ControlObject = NAPI.Object.CreateObject((int)PoliceControlItems.Beacon, policeControl.Position, policeControl.Rotation);
                Police.policeControlList.Add(policeControl);
                return;
            }

            if (item.Equals(ArgRes.barrier, StringComparison.InvariantCultureIgnoreCase))
            {
                policeControl = new PoliceControlModel(0, string.Empty, PoliceControlItems.Barrier, player.Position, player.Rotation);
                policeControl.Position = new Vector3(policeControl.Position.X, policeControl.Position.Y, policeControl.Position.Z - 1.0f);
                policeControl.ControlObject = NAPI.Object.CreateObject((int)PoliceControlItems.Barrier, policeControl.Position, policeControl.Rotation);
                Police.policeControlList.Add(policeControl);
                return;
            }

            if (item.Equals(ArgRes.spikes, StringComparison.InvariantCultureIgnoreCase))
            {
                policeControl = new PoliceControlModel(0, string.Empty, PoliceControlItems.Spikes, player.Position, player.Rotation);
                policeControl.Position = new Vector3(policeControl.Position.X, policeControl.Position.Y, policeControl.Position.Z - 1.0f);
                policeControl.ControlObject = NAPI.Object.CreateObject((int)PoliceControlItems.Spikes, policeControl.Position, policeControl.Rotation);
                Police.policeControlList.Add(policeControl);
                return;
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.put);
        }

        [Command]
        public static void RemoveCommand(Player player, string item)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (!Faction.IsPoliceMember(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                return;
            }

            if (!player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            if (item.Equals(ArgRes.cone, StringComparison.InvariantCultureIgnoreCase))
            {
                Police.RemoveClosestPoliceControlItem(player, PoliceControlItems.Cone);
                return;
            }

            if (item.Equals(ArgRes.beacon, StringComparison.InvariantCultureIgnoreCase))
            {
                Police.RemoveClosestPoliceControlItem(player, PoliceControlItems.Beacon);
                return;
            }

            if (item.Equals(ArgRes.barrier, StringComparison.InvariantCultureIgnoreCase))
            {
                Police.RemoveClosestPoliceControlItem(player, PoliceControlItems.Barrier);
                return;
            }

            if (item.Equals(ArgRes.spikes, StringComparison.InvariantCultureIgnoreCase))
            {
                Police.RemoveClosestPoliceControlItem(player, PoliceControlItems.Spikes);
                return;
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.remove);
        }

        [Command]
        public static void ReinforcesCommand(Player player)
        {
            if (!Faction.IsPoliceMember(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                return;
            }

            if (!player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get police department's members
            Player[] policeMembers = NAPI.Pools.GetAllPlayers().Where(p => Faction.IsPoliceMember(p) && p.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty).ToArray();

            // Get the player's external data
            PlayerTemporaryModel playerModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (playerModel.Reinforces)
            {
                string targetMessage = string.Format(InfoRes.target_reinforces_canceled, player.Name);

                foreach (Player target in policeMembers)
                {
                    // Remove the blip from the map
                    target.TriggerEvent("reinforcesRemove", player.Value);

                    // Send the message for each member
                    player.SendChatMessage(Constants.COLOR_INFO + (player == target ? InfoRes.player_reinforces_canceled : targetMessage));
                }

                // Remove player's reinforces
                playerModel.Reinforces = false;
            }
            else
            {
                string targetMessage = string.Format(InfoRes.target_reinforces_asked, player.Name);

                foreach (Player target in policeMembers)
                {
                    // Send the message for each member
                    player.SendChatMessage(Constants.COLOR_INFO + (player == target ? InfoRes.player_reinforces_asked : targetMessage));
                }

                // Ask for reinforces
                playerModel.Reinforces = true;
            }
        }

        [Command]
        public static void LicenseCommand(Player player, string args)
        {
            if (!Faction.IsPoliceMember(player) || player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Rank != 6)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_police_chief);
                return;
            }

            // Get the arguments
            string[] arguments = args.Trim().Split(' ');

            if (arguments.Length != 3 && arguments.Length != 4)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.license);
                return;
            }

            // Get the action and item
            string action = arguments[0];
            string item = arguments[1];

            // Remove the first parameters
            arguments = arguments.Skip(2).ToArray();

            // Get the target player
            Player target = UtilityFunctions.GetPlayer(ref arguments);

            // Check whether the target player is connected
            if (target == null || player.Position.DistanceTo(target.Position) > 2.5f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                return;
            }

            if (action.Equals(ArgRes.give, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!item.Equals(ArgRes.weapon, StringComparison.InvariantCultureIgnoreCase))
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.license);
                    return;
                }

                // Add one month to the license
                target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).WeaponLicense = UtilityFunctions.GetTotalSeconds() + 2628000;

                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.weapon_license_given, target.Name));
                target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.weapon_license_received, player.Name));

                return;
            }

            if (action.Equals(ArgRes.remove, StringComparison.InvariantCultureIgnoreCase))
            {
                if (item.Equals(ArgRes.weapon, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Adjust the date to the current one
                    target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).WeaponLicense = UtilityFunctions.GetTotalSeconds();

                    player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.weapon_license_removed, target.Name));
                    target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.weapon_license_lost, player.Name));

                    return;
                }

                if (item.Equals(ArgRes.car, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Remove car license
                    DrivingSchool.SetPlayerLicense(target, (int)DrivingLicenses.Car, -1);

                    player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.car_license_removed, target.Name));
                    target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.car_license_lost, player.Name));

                    return;
                }

                if (item.Equals(ArgRes.motorcycle, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Remove motorcycle license
                    DrivingSchool.SetPlayerLicense(target, (int)DrivingLicenses.Motorcycle, -1);

                    player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.moto_license_removed, target.Name));
                    target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.moto_license_lost, player.Name));

                    return;
                }

                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.license);
                return;
            }
        }

        [Command]
        public static void BreathalyzerCommand(Player player, string targetString)
        {
            if (!Faction.IsPoliceMember(player) || player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Rank == 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                return;
            }

            float alcoholLevel = 0.0f;
            Player target = UtilityFunctions.GetPlayer(targetString);

            if (target == null || player.Position.DistanceTo(target.Position) > 2.5f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
                return;
            }

            // Get the target's temporary data
            PlayerTemporaryModel data = target.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (data.DrunkLevel > 0.0f)
            {
                alcoholLevel = data.DrunkLevel;
            }

            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.alcoholimeter_test, target.Name, alcoholLevel));
            target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.alcoholimeter_receptor, player.Name, alcoholLevel));
        }

        [Command]
        public static void ComputerCommand(Player player, string targetString)
        {
            if (!Faction.IsPoliceMember(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                return;
            }

            if (!player.IsInVehicle)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_vehicle);
                return;
            }

            // Get the vehicle's faction
            int vehicleFaction = Vehicles.GetVehicleById<VehicleModel>(player.Vehicle.GetData<int>(EntityData.VehicleId)).Faction;

            if (vehicleFaction != (int)PlayerFactions.Police && vehicleFaction != (int)PlayerFactions.Sheriff)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_your_job_vehicle);
                return;
            }

            // Get the player from the input string
            Player target = UtilityFunctions.GetPlayer(targetString);

            if (!Character.IsPlaying(target))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }

            // Show the data from the player
            Character.RetrieveBasicDataEvent(player, target.Value);
        }
    }
}