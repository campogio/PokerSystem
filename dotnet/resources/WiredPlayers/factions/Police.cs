using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WiredPlayers.Currency;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.commands;
using WiredPlayers.messages.general;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.factions
{
    public class Police : Script
    {
        public static List<CrimeModel> crimeList;
        public static List<PoliceControlModel> policeControlList;

        public Police()
        {
            // Initialize reinforces updater
            new Timer(UpdateReinforcesRequests, null, 250, 250);

            // Create all the equipment places
            foreach (Vector3 pos in Coordinates.EquipmentPoints)
            {
                NAPI.TextLabel.CreateTextLabel("/" + ComRes.equipment, pos, 10.0f, 0.5f, 4, new Color(190, 235, 100), false, 0);
                NAPI.TextLabel.CreateTextLabel(GenRes.equipment_help, new Vector3(pos.X, pos.Y, pos.Z - 0.1f), 10.0f, 0.5f, 4, new Color(255, 255, 255), false, 0);

                // Create blips
                Blip policeBlip = NAPI.Blip.CreateBlip(pos);
                policeBlip.Name = GenRes.police_station;
                policeBlip.ShortRange = true;
                policeBlip.Sprite = 60;
            }
        }

        public static List<string> GetDifferentPoliceControls()
        {
            List<string> policeControls = new List<string>();

            foreach (PoliceControlModel policeControl in policeControlList)
            {
                if (!policeControls.Contains(policeControl.Name) && policeControl.Name != string.Empty)
                {
                    policeControls.Add(policeControl.Name);
                }
            }

            return policeControls;
        }

        public static void RemoveClosestPoliceControlItem(Player player, PoliceControlItems hash)
        {
            // Get the closest police control item
            PoliceControlModel policeControl = policeControlList.Where(control => control.ControlObject != null && control.ControlObject.Position.DistanceTo(player.Position) < 2.0f && control.Item == hash).FirstOrDefault();

            if (policeControl != null)
            {
                policeControl.ControlObject.Delete();
                policeControl.ControlObject = null;
            }
        }

        public static void UpdateReinforcesRequests(object unused)
        {
            Dictionary<int, Vector3> policeReinforces = new Dictionary<int, Vector3>();
            Player[] policeMembers = NAPI.Pools.GetAllPlayers().Where(x => Faction.IsPoliceMember(x)).ToArray();

            foreach (Player police in policeMembers)
            {
                if (police.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).Reinforces)
                {
                    // Add the position where the player is
                    policeReinforces.Add(police.Value, police.Position);
                }
            }

            if (policeReinforces.Count == 0) return;

            string reinforcesJsonList = NAPI.Util.ToJson(policeReinforces);

            foreach (Player police in policeMembers)
            {
                // Update reinforces position for each policeman
                police.TriggerEvent("updatePoliceReinforces", reinforcesJsonList);
            }
        }

        public static bool IsCloseToEquipmentLockers(Player player)
        {
            // Check if the player is close to any equipment label
            return Coordinates.EquipmentPoints.Count(p => player.Position.DistanceTo(p) < 2.0f) > 0;
        }

        public static bool IsPlayerInJailArea(Player player)
        {
            // Check if the player is in any of the jail areas
            return Coordinates.EquipmentPoints.Count(p => player.Position.DistanceTo(p) < 5.0f) > 0;
        }

        [RemoteEvent("applyCrimesToPlayer")]
        public void ApplyCrimesToPlayerEvent(Player player, string crimeJson)
        {
            int fine = 0, jail = 0;
            List<CrimeModel> crimeList = NAPI.Util.FromJson<List<CrimeModel>>(crimeJson);

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // Calculate fine amount and jail time
            foreach (CrimeModel crime in crimeList)
            {
                fine += crime.fine;
                jail += crime.jail;
            }

            Random random = new Random();
            data.IncriminatedTarget.Position = Coordinates.JailSpawns[random.Next(3)];

            // Remove money and jail the player
            Money.SetPlayerMoney(data.IncriminatedTarget, -fine);

            // Get the data from the target player
            PlayerTemporaryModel targetData = data.IncriminatedTarget.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);
            targetData.JailType = JailTypes.Ic;
            targetData.Jailed = jail;

            // Remove the incriminated target
            data.IncriminatedTarget = null;
        }

        [RemoteEvent("policeControlSelected")]
        public async void PoliceControlSelectedEvent(Player _, Actions action, string policeControl)
        {
            if (action == Actions.Load)
            {
                foreach (PoliceControlModel policeControlModel in policeControlList)
                {
                    if (policeControlModel.ControlObject == null && policeControlModel.Name == policeControl)
                    {
                        policeControlModel.ControlObject = NAPI.Object.CreateObject((int)policeControlModel.Item, policeControlModel.Position, policeControlModel.Rotation);
                    }
                }
            }
            else if (action == Actions.Save)
            {
                List<PoliceControlModel> copiedPoliceControlModels = new List<PoliceControlModel>();
                List<PoliceControlModel> deletedPoliceControlModels = new List<PoliceControlModel>();
                foreach (PoliceControlModel policeControlModel in policeControlList)
                {
                    if (policeControlModel.ControlObject != null && policeControlModel.Name != policeControl)
                    {
                        if (policeControlModel.Name != string.Empty)
                        {
                            PoliceControlModel policeControlCopy = policeControlModel;
                            policeControlCopy.Name = policeControl;

                            policeControlCopy.Id = await DatabaseOperations.AddPoliceControlItem(policeControlCopy).ConfigureAwait(false);
                            copiedPoliceControlModels.Add(policeControlCopy);
                        }
                        else
                        {
                            policeControlModel.Name = policeControl;

                            // Add the new element
                            policeControlModel.Id = await DatabaseOperations.AddPoliceControlItem(policeControlModel).ConfigureAwait(false);
                        }
                    }
                    else if (policeControlModel.ControlObject == null && policeControlModel.Name == policeControl)
                    {
                        await Task.Run(() => DatabaseOperations.DeleteSingleRow("controls", "id", policeControlModel.Id)).ConfigureAwait(false);
                        deletedPoliceControlModels.Add(policeControlModel);
                    }
                }
                policeControlList.AddRange(copiedPoliceControlModels);
                policeControlList = policeControlList.Except(deletedPoliceControlModels).ToList();
            }
            else
            {
                foreach (PoliceControlModel policeControlModel in policeControlList)
                {
                    if (policeControlModel.ControlObject != null && policeControlModel.Name == policeControl)
                    {
                        policeControlModel.ControlObject.Delete();
                    }
                }
                policeControlList.RemoveAll(control => control.Name == policeControl);

                // Delete the police control
                await Task.Run(() => DatabaseOperations.DeleteSingleRow("controls", "name", policeControl)).ConfigureAwait(false);
            }
        }

        [RemoteEvent("updatePoliceControlName")]
        public async void UpdatePoliceControlNameEvent(Player _, Actions action, string policeControlSource, string policeControlTarget)
        {
            if (action == Actions.Save)
            {
                List<PoliceControlModel> copiedPoliceControlModels = new List<PoliceControlModel>();
                List<PoliceControlModel> deletedPoliceControlModels = new List<PoliceControlModel>();
                foreach (PoliceControlModel policeControlModel in policeControlList)
                {
                    if (policeControlModel.ControlObject != null && policeControlModel.Name != policeControlTarget)
                    {
                        if (policeControlModel.Name != string.Empty)
                        {
                            PoliceControlModel policeControlCopy = policeControlModel.Copy();
                            policeControlModel.ControlObject = null;
                            policeControlCopy.Name = policeControlTarget;

                            policeControlCopy.Id = await DatabaseOperations.AddPoliceControlItem(policeControlCopy).ConfigureAwait(false);
                            copiedPoliceControlModels.Add(policeControlCopy);
                        }
                        else
                        {
                            policeControlModel.Name = policeControlTarget;

                            // Add new element to the control
                            policeControlModel.Id = await DatabaseOperations.AddPoliceControlItem(policeControlModel).ConfigureAwait(false);
                        }
                    }
                }
                policeControlList.AddRange(copiedPoliceControlModels);
            }
            else
            {
                foreach (PoliceControlModel p in policeControlList.FindAll(s => s.Name == policeControlSource)) p.Name = policeControlTarget;

                // Rename the control
                await Task.Run(() => DatabaseOperations.RenamePoliceControl(policeControlSource, policeControlTarget)).ConfigureAwait(false);
            }
        }
    }
}
