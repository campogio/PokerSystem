using GTANetworkAPI;
using System;
using System.Collections.Generic;
using WiredPlayers.Buildings;
using WiredPlayers.character;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.factions
{
    public class Emergency : Script
    {
        public static List<BloodModel> BloodList;

        private void CreateEmergencyReport(DeathModel death)
        {
            // Get the player's character model
            CharacterModel characterModel = death.player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (death.killer == null || death.killer.Value == Constants.UndefinedValue)
            {
                // Check if the player was dead
                if (!IsPlayerDead(death.player))
                {
                    // There's no killer, we set the environment as killer
                    characterModel.KilledBy = Constants.UndefinedValue;
                }
            }
            else
            {
                // Set the killer identifier
                characterModel.KilledBy = death.killer.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
            }

            // Warn the player
            death.player.SendChatMessage(Constants.COLOR_INFO + InfoRes.emergency_warn);
        }

        public static bool IsPlayerDead(Player player)
        {
            // Check if the player has been killed
            return player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).KilledBy != 0;
        }

        public static int GetRemainingBlood()
        {
            int remaining = 0;
            foreach (BloodModel blood in BloodList)
            {
                if (blood.Used) remaining--;
                else  remaining++;
            }
            return remaining;
        }

        public static void CancelPlayerDeath(Player player)
        {
            NAPI.Player.SpawnPlayer(player, player.Position);
            player.ResetData(EntityData.TimeHospitalRespawn);
            player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).KilledBy = 0;

            // Get the death warning
            FactionWarningModel factionWarn = Faction.GetFactionWarnByTarget(player.Value, PlayerFactions.Emergency);

            if (factionWarn != null)
            {
                if (factionWarn.TakenBy >= 0)
                {
                    // Tell the player who attended the report it's been canceled
                    Player doctor = UtilityFunctions.GetPlayer(factionWarn.TakenBy);
                    doctor.SendChatMessage(Constants.COLOR_INFO + InfoRes.faction_warn_canceled);
                }

                // Remove the report from the list
                Faction.factionWarningList.Remove(factionWarn);
            }

            // Change the death state
            player.TriggerEvent("togglePlayerDead", false);
        }

        public static void TeleportPlayerToHospital(Player player)
        {           
            NAPI.Player.SpawnPlayer(player, Coordinates.Hospital);
            
            // Reset building variables
            BuildingHandler.RemovePlayerFromBuilding(player, Coordinates.Hospital, 0);

            // Change the death state
            player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).KilledBy = 0;
            player.ResetData(EntityData.TimeHospitalRespawn);
            player.TriggerEvent("togglePlayerDead", false);
        }

        public static void WarnEmergencyDepartment(Player player, string deathPlace, Vector3 deathPosition, string deathHour)
        {
            FactionWarningModel factionWarning = new FactionWarningModel(PlayerFactions.Emergency, player.Value, deathPlace, deathPosition, -1, deathHour);
            Faction.factionWarningList.Add(factionWarning);

            // Report message
            string warnMessage = string.Format(InfoRes.emergency_warning, Faction.factionWarningList.Count - 1);

            // Sending the report to all the emergency department's members
            foreach (Player target in NAPI.Pools.GetAllPlayers().FindAll(p => Character.IsPlaying(p)))
            {
                // Get the player's character model
                CharacterModel characterModel = target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

                if (characterModel.Faction == PlayerFactions.Emergency && characterModel.OnDuty)
                {
                    target.SendChatMessage(Constants.COLOR_INFO + warnMessage);
                }
            }
        }

        [ServerEvent(Event.PlayerDeath)]
        public void PlayerDeathServerEvent(Player player, Player killer, uint weapon)
        {
            if(IsPlayerDead(player)) return;
            
            DeathModel death = new DeathModel(player, killer, weapon);

            string deathPlace = string.Empty;
            Vector3 deathPosition = new Vector3();
            string deathHour = DateTime.Now.ToString("h:mm:ss tt");

            if (BuildingHandler.IsIntoBuilding(player))
            {
                uint dimension = 0;

                // Get the interior information
                BuildingHandler.GetBuildingInformation(player, ref deathPosition, ref dimension, ref deathPlace);
            }
            else
            {
                deathPosition = player.Position;
            }

            if (killer == null || killer.Value == Constants.UndefinedValue || killer == player)
            {
                // We add the report to the list
                WarnEmergencyDepartment(player, deathPlace, deathPosition, deathHour);

                // Create the emergency report
                CreateEmergencyReport(death);
            }
            else
            {
                // Set the new killer
                player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).KilledBy = killer.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
            }

            // Time to let player accept his dead
            player.SetData(EntityData.TimeHospitalRespawn, UtilityFunctions.GetTotalSeconds() + 240);

            // Set the player into dead state
            player.TriggerEvent("togglePlayerDead", true);
        }
    }
}
