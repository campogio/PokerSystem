using RAGE;
using RAGE.Elements;
using WiredPlayers_Client.account;
using WiredPlayers_Client.vehicles;
using WiredPlayers_Client.jobs;
using WiredPlayers_Client.model;
using WiredPlayers_Client.factions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Drawing;
using System;
using WiredPlayers_Client.chat;
using WiredPlayers_Client.Scaleform;
using WiredPlayers_Client.character;
using System.Linq;
using WiredPlayers_Client.Utility;
using WiredPlayers_Client.Building.House;

namespace WiredPlayers_Client.globals
{
    class Globals : Events.Script
    {
        public static int playerMoney;
        public static bool viewingPlayers;
        public static int lastTime;
        public static List<Marker> ExitMarkerList;
        public static Marker ActionMarker;

        private static Dictionary<int, AttachmentModel> playerAttachments;
        private static List<Colshape> ExitColshapeList;
        private static Colshape ActionColshape;
        private static string ExitMessage;
        private static string ActionMessage;

        public Globals()
        {
            lastTime = RAGE.Game.Misc.GetGameTimer();
            playerAttachments = new Dictionary<int, AttachmentModel>();

            // Initialize the lists
            ExitColshapeList = new List<Colshape>();
            ExitMarkerList = new List<Marker>();

            // Register custom events
            Events.Add("updatePlayerList", UpdatePlayerListEvent);
            Events.Add("hideConnectedPlayers", HideConnectedPlayersEvent);
            Events.Add("changePlayerWalkingStyle", ChangePlayerWalkingStyleEvent);
            Events.Add("resetPlayerWalkingStyle", ResetPlayerWalkingStyleEvent);
            Events.Add("attachItemToPlayer", AttachItemToPlayerEvent);
            Events.Add("dettachItemFromPlayer", DettachItemFromPlayerEvent);
            Events.Add("playerLoggedIn", PlayerLoggedInEvent);
            Events.Add("GenerateDimensionColShapes", GenerateDimensionColShapesEvent);
            Events.Add("DestroyDimensionColShapes", DestroyDimensionColShapesEvent);
            Events.Add("StopSecondaryAnimation", StopSecondaryAnimationEvent);

            Events.OnEntityStreamIn += OnEntityStreamInEvent;
            Events.OnEntityStreamOut += OnEntityStreamOutEvent;
            Events.OnPlayerEnterColshape += OnPlayerEnterColshapeEvent;
            Events.Tick += TickEvent;

            // Freeze the player until he logs in
            Player.LocalPlayer.FreezePosition(true);
        }

        public static string EscapeJsonCharacters(string jsonString)
        {
            // Escape the apostrophe on JSON
            return jsonString.Replace("'", "\\'");
        }

        public static bool IsPlayerExiting()
        {
            // Check if the player is close to any exit point
            return ExitMarkerList.Any(m => m.Position.DistanceTo(Player.LocalPlayer.Position) < 1.5f);
        }

        private void UpdatePlayerListEvent(object[] args)
        {
            if (!AccountHandler.PlayerLogged || !viewingPlayers || Browser.CustomBrowser == null) return;

            // Update the player list
            Browser.ExecuteFunction("updatePlayerList", args[0].ToString());
        }

        private void HideConnectedPlayersEvent(object[] args)
        {
            // Cancel the player list view
            viewingPlayers = false;

            // Destroy the browser
            Browser.DestroyBrowserEvent(null);
        }

        private void ChangePlayerWalkingStyleEvent(object[] args)
        {
            // Get the player
            Player player = (Player)args[0];
            string clipSet = args[1].ToString();

            player.SetMovementClipset(clipSet, 0.1f);
        }

        private void ResetPlayerWalkingStyleEvent(object[] args)
        {
            // Get the player
            Player player = (Player)args[0];

            player.ResetMovementClipset(0.0f);
        }

        private void AttachItemToPlayerEvent(object[] args)
        {
            // Get the remote player
            int playerId = Convert.ToInt32(args[0]);
            Player attachedPlayer = Entities.Players.GetAtRemote((ushort)playerId);

            // Check if the player is in the stream range
            if (Entities.Players.Streamed.Contains(attachedPlayer) || Player.LocalPlayer.Equals(attachedPlayer))
            {
                // Get the attachment
                AttachmentModel attachment = JsonConvert.DeserializeObject<AttachmentModel>(args[1].ToString());

                // Create the object for that player
                int boneIndex = attachedPlayer.GetBoneIndexByName(attachment.bodyPart);
                attachment.attach = new MapObject(RAGE.Game.Misc.GetHashKey(attachment.hash), attachedPlayer.Position, new Vector3(), 255, attachedPlayer.Dimension);
                RAGE.Game.Entity.AttachEntityToEntity(attachment.attach.Handle, attachedPlayer.Handle, boneIndex, attachment.offset.X, attachment.offset.Y, attachment.offset.Z, attachment.rotation.X, attachment.rotation.Y, attachment.rotation.Z, false, false, false, false, 2, true);

                // Add the attachment to the dictionary
                playerAttachments.Add(playerId, attachment);
            }
        }

        private void DettachItemFromPlayerEvent(object[] args)
        {
            // Get the remote player
            int playerId = Convert.ToInt32(args[0]);

            if (playerAttachments.ContainsKey(playerId))
            {
                // Get the attachment
                MapObject attachment = playerAttachments[playerId].attach;

                // Remove it from the player and world
                attachment.Destroy();
                playerAttachments.Remove(playerId);
            }
        }

        private void PlayerLoggedInEvent(object[] args)
        {
            // Unlock the chat
            ChatManager.Locked = false;

            // Set the player's hand money
            playerMoney = Convert.ToInt32(args[0]);

            // Remove health regeneration
            RAGE.Game.Player.SetPlayerHealthRechargeMultiplier(0.0f);

            // Remove weapons from the vehicles
            RAGE.Game.Player.DisablePlayerVehicleRewards();

            // Remove the fade out after player's death
            RAGE.Game.Misc.SetFadeOutAfterDeath(false);

            // Remove the automatic engine
            Player.LocalPlayer.SetConfigFlag(429, true);

            // Initialize voice chat
            VoiceChat.Initialize();

            // Destroy the lobby's ColShapes
            AccountHandler.ClearLobby();

            // Remove the loading screen
            RAGE.Game.Cam.DoScreenFadeIn(750);

            // Show the player as logged
            AccountHandler.PlayerLogged = true;
        }

        private void GenerateDimensionColShapesEvent(object[] args)
        {
            // Lock all the interiors
            Doors.LockInteriorDoors();

            // Get the exit positions
            List<Vector3> exitPositions = JsonConvert.DeserializeObject<List<Vector3>>(args[1].ToString().ToUpper());
            ExitMessage = args[3].ToString();

            // Load the IPL
            RAGE.Game.Streaming.RequestIpl(args[0].ToString());

            foreach (Vector3 exitPoint in exitPositions)
            {
                // Add the elements to the lists
                ExitColshapeList.Add(new CircleColshape(exitPoint.X, exitPoint.Y, 1.5f, Player.LocalPlayer.Dimension));
                ExitMarkerList.Add(new Marker(0, exitPoint, 0.75f, new Vector3(), new Vector3(), new RGBA(244, 126, 23), true, Player.LocalPlayer.Dimension));
            }

            if (args[2] != null)
            {
                Vector3 actionPosition = (Vector3)args[2];
                ActionMessage = args[4].ToString();

                // Create the marker and ColShape
                ActionColshape = new CircleColshape(actionPosition.X, actionPosition.Y, 1.5f, Player.LocalPlayer.Dimension);
                ActionMarker = new Marker(Convert.ToBoolean(args[5]) ? 30 : (uint)27, actionPosition, 0.75f, new Vector3(), new Vector3(), new RGBA(244, 126, 23), true, Player.LocalPlayer.Dimension);
            }
        }

        public static void DestroyDimensionColShapesEvent(object[] args)
        {
            foreach (Colshape exitColshape in ExitColshapeList)
            {
                // Remove each colshape
                if (exitColshape != null && !exitColshape.IsNull) exitColshape.Destroy();
            }

            foreach (Marker exitMarker in ExitMarkerList)
            {
                // Remove each marker
                if (exitMarker != null && !exitMarker.IsNull) exitMarker.Destroy();
            }

            if (ActionColshape != null && !ActionColshape.IsNull)
            {
                ActionColshape.Destroy();
                ActionColshape = null;
            }

            if (ActionMarker != null && !ActionMarker.IsNull)
            {
                ActionMarker.Destroy();
                ActionMarker = null;
            }

            // Clear the lists
            ExitColshapeList.Clear();
            ExitMarkerList.Clear();

            // Unload the IPL
            RAGE.Game.Streaming.RemoveIpl(args[0].ToString());

            // Unock all the interiors
            Doors.UnlockInteriorDoors();
        }

        public static void StopSecondaryAnimationEvent(object[] args)
        {
            // Stop the animation the player is playing
            Player.LocalPlayer.ClearSecondaryTask();
        }

        public static void OnEntityStreamInEvent(Entity entity)
        {
            if (entity.Type != RAGE.Elements.Type.Player) return;

            // Get the identifier of the player
            int playerId = entity.RemoteId;
            Player attachedPlayer = Entities.Players.GetAtRemote((ushort)playerId);

            // Get the attachment on the right hand
            object attachmentJson = attachedPlayer.GetSharedData(Constants.ITEM_ENTITY_RIGHT_HAND);

            if(attachmentJson == null)
            {
                // Check if the player has a crate
                attachmentJson = attachedPlayer.GetSharedData(Constants.ITEM_ENTITY_WEAPON_CRATE);
            }
            else
            {
                AttachmentModel attachment = JsonConvert.DeserializeObject<AttachmentModel>(attachmentJson.ToString());

                // If the attached item is a weapon, we don't stream it
                if (RAGE.Game.Weapon.IsWeaponValid(Convert.ToUInt32(attachment.hash))) return;

                int boneIndex = attachedPlayer.GetBoneIndexByName(attachment.bodyPart);
                attachment.attach = new MapObject(Convert.ToUInt32(attachment.hash), attachedPlayer.Position, new Vector3(), 255, attachedPlayer.Dimension);
                RAGE.Game.Entity.AttachEntityToEntity(attachment.attach.Handle, attachedPlayer.Handle, boneIndex, attachment.offset.X, attachment.offset.Y, attachment.offset.Z, attachment.rotation.X, attachment.rotation.Y, attachment.rotation.Z, false, false, false, true, 0, true);

                // Add the attachment to the dictionary
                playerAttachments.Add(playerId, attachment);
            }
        }

        public static void OnEntityStreamOutEvent(Entity entity)
        {
            if (entity.Type != RAGE.Elements.Type.Player) return;

            // Get the player's identifier
            int playerId = entity.RemoteId;

            if(playerAttachments.ContainsKey(playerId))
            {
                // Get the attached object
                MapObject attachment = playerAttachments[playerId].attach;

                // Destroy the attachment
                attachment.Destroy();
                playerAttachments.Remove(playerId);
            }
        }

        private void OnPlayerEnterColshapeEvent(Colshape colshape, Events.CancelEventArgs cancel)
        {
            if (!ExitColshapeList.Contains(colshape) && colshape != ActionColshape) return;

            // Show the instructional button
            InstructionalButtons.ShowInstructionalButtonEvent(new object[] { colshape.Id == ActionColshape.Id ? ActionMessage : ExitMessage, "F" });

            // Cancel the rest of the events
            cancel.Cancel = true;
        }

        private void TickEvent(List<Events.TickNametagData> nametags)
        {
            // Get the current time
            int currentTime = RAGE.Game.Misc.GetGameTimer();

            if (AccountHandler.PlayerLogged)
            {
                // Get the local player
                Player localPlayer = Player.LocalPlayer;

                // Disable hitting with the weapon
                RAGE.Game.Pad.DisableControlAction(0, 140, true);
                RAGE.Game.Pad.DisableControlAction(0, 141, true);

                // Remove vehicle, area and street names
                RAGE.Game.Ui.HideHudComponentThisFrame(6);
                RAGE.Game.Ui.HideHudComponentThisFrame(7);
                RAGE.Game.Ui.HideHudComponentThisFrame(8);
                RAGE.Game.Ui.HideHudComponentThisFrame(9);

                if (Vehicles.lastPosition != null)
                {
                    if (localPlayer.Vehicle == null)
                    {
                        // He fell from the vehicle, save the data
                        Vehicles.RemoveSpeedometerEvent(null);
                    }
                    else
                    {
                        // Update the speedometer
                        Vehicles.UpdateSpeedometer();
                    }
                }

                if (Fishing.fishingState > 0)
                {
                    // Start the fishing minigame
                    Fishing.DrawFishingMinigame();
                }

                // Draw the money
                RAGE.NUI.UIResText.Draw(playerMoney + "$", 1900, 60, RAGE.Game.Font.Pricedown, 0.5f, Color.DarkOliveGreen, RAGE.NUI.UIResText.Alignment.Right, true, true, 0);

                if (localPlayer.Vehicle != null && localPlayer.Vehicle.Exists)
                {
                    int street = 0, crossroads = 0;
                    RAGE.Game.Pathfind.GetStreetNameAtCoord(localPlayer.Position.X, localPlayer.Position.Y, localPlayer.Position.Z, ref street, ref crossroads);

                    // Draw the street and area name
                    RAGE.NUI.UIResText.Draw(RAGE.Game.Ui.GetStreetNameFromHashKey((uint)street), 860, 1010, RAGE.Game.Font.HouseScript, 0.5f, Color.White, RAGE.NUI.UIResText.Alignment.Centered, true, true, 0);
                    RAGE.NUI.UIResText.Draw(RAGE.Game.Ui.GetStreetNameFromHashKey((uint)crossroads), 860, 1040, RAGE.Game.Font.HouseScript, 0.5f, Color.White, RAGE.NUI.UIResText.Alignment.Centered, true, true, 0);
                }

                // Check if the player
                if (RAGE.Game.Pad.IsControlJustPressed(0, (int)RAGE.Game.Control.VehicleSubPitchDownOnly) && localPlayer.Vehicle != null)
                {
                    // Check if the player is on a forklift
                    Trucker.CheckPlayerStoredCrate();
                }

                if (Police.handcuffed || playerAttachments.ContainsKey(localPlayer.RemoteId))
                {
                    // The player has an item on the hand, we don't let him change the weapon
                    RAGE.Game.Pad.DisableControlAction(0, 12, true);
                    RAGE.Game.Pad.DisableControlAction(0, 13, true);
                    RAGE.Game.Pad.DisableControlAction(0, 14, true);
                    RAGE.Game.Pad.DisableControlAction(0, 15, true);
                    RAGE.Game.Pad.DisableControlAction(0, 16, true);
                    RAGE.Game.Pad.DisableControlAction(0, 17, true);
                }

                // Check if the player is handcuffed
                if (Police.handcuffed)
                {
                    RAGE.Game.Pad.DisableControlAction(0, 22, true);
                    RAGE.Game.Pad.DisableControlAction(0, 24, true);
                    RAGE.Game.Pad.DisableControlAction(0, 25, true);
                }
                
                if (Browser.CustomBrowser != null)
                {
                    // Disable the ESC menu
                    RAGE.Game.Pad.DisableControlAction(0, 200, true);
                }

                if (VoiceChat.voiceEnabled && !Voice.Muted && (currentTime - lastTime) > 250.0d)
                {
                    // Update the check time
                    lastTime = currentTime;

                    // Process the voice stream
                    VoiceChat.ProcessVoiceStream();
                }

                if (RAGE.Game.Pad.IsControlJustReleased(0, 27) && playerAttachments.ContainsKey(localPlayer.RemoteId) && Browser.CustomBrowser == null)
                {
                    string attachmentHash = playerAttachments[localPlayer.RemoteId].hash;

                    // Check if the item is a phone
                    if (!Telephone.IsValidPhone(attachmentHash)) return;

                    // Show the phone for the player
                    Telephone.ShowPhoneHome();
                }

                if (RAGE.Game.Pad.IsDisabledControlJustPressed(0, 24))
                {
                    // Check if any furniture has been clicked
                    Furniture.CheckFurnitureClicked(true);
                }
                
                if (RAGE.Game.Pad.IsDisabledControlJustReleased(0, 24))
                {
                    // Check if any furniture has been placed
                    Furniture.CheckFurnitureClicked(false);
                }

                if (Furniture.SelectedFurniture != null)
                {
                    // Update the furniture position
                    Events.CallRemote("UpdateFurniturePosition", Furniture.SelectedFurniture.RemoteId, Furniture.SelectedFurniture.Position);
                }
            }

            // Process the scaleforms
            InstructionalButtons.RenderScaleforms();

            // Check if any key has been toggled
            KeyHandler.CheckKeysToggled();
        }
    }
}
