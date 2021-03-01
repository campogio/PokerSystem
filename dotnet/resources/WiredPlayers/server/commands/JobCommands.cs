using GTANetworkAPI;
using System;
using WiredPlayers.character;
using WiredPlayers.Data.Persistent;
using WiredPlayers.factions;
using WiredPlayers.Utility;
using WiredPlayers.jobs;
using WiredPlayers.messages.arguments;
using WiredPlayers.messages.error;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class JobCommands
    {
        [Command]
        public static void JobCommand(Player player, string action)
        {
            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);
            
            if (action.Equals(ArgRes.info, StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (JobPickModel jobPick in Constants.JobPickArray)
                {
                    if (player.Position.DistanceTo(jobPick.Position) < 1.5f)
                    {
                        player.SendChatMessage(Constants.COLOR_INFO + jobPick.Description);
                        break;
                    }
                }
                
                return;
            }
            
            if (action.Equals(ArgRes.accept, StringComparison.InvariantCultureIgnoreCase))
            {
                if (characterModel.Faction != PlayerFactions.None && (int)characterModel.Faction < Constants.LAST_STATE_FACTION)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_job_state_faction);
                    return;
                }

                if (characterModel.Job != PlayerJobs.None)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_has_job);
                    return;
                }

                foreach (JobPickModel jobPick in Constants.JobPickArray)
                {
                    if (player.Position.DistanceTo(jobPick.Position) < 1.5f)
                    {
                        characterModel.Job = jobPick.Job;
                        characterModel.EmployeeCooldown = 5;
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.job_accepted);
                        break;
                    }
                }
                
                return;
            }
            
            if (action.Equals(ArgRes.leave, StringComparison.InvariantCultureIgnoreCase))
            {
                if (characterModel.Job == PlayerJobs.None)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_job);
                    return;
                }

                if (characterModel.EmployeeCooldown > 0)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + string.Format(ErrRes.employee_cooldown, characterModel.EmployeeCooldown));
                    return;
                }

                characterModel.Job = PlayerJobs.None;
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.job_left);
                
                return;
            }
            
            if (action.Equals(ArgRes.help, StringComparison.InvariantCultureIgnoreCase))
            {
                if(characterModel.Job == PlayerJobs.None)
                {
                    // The player has no job
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_job);
                    return;
                }
                
                // Show the commands for the player's job
                Job.ShowPlayerJobCommands(player);
                
                return;
            }

            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.job);
        }

        [Command]
        public static void DutyCommand(Player player)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Job == PlayerJobs.None && characterModel.Faction == PlayerFactions.None)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_job);
                return;
            }

            if(!Job.IsPlayerOnWorkPlace(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_work_place);
                return;
            }

            if (characterModel.OnDuty)
            {
                // Initialize player clothes
                Customization.SetDefaultClothes(player);

                // Populate player's clothes
                Customization.ApplyPlayerClothes(player);

                // We set the player off duty
                characterModel.OnDuty = false;

                // Notification sent to the player
                player.SendNotification(InfoRes.player_free_time);
                
                return;
            }
            
            // Dress the player with the uniform
            foreach (UniformModel uniform in Constants.UniformArray)
            {
                if (uniform.FactionJob is PlayerFactions && (PlayerFactions)uniform.FactionJob == characterModel.Faction && uniform.CharacterSex == characterModel.Sex)
                {
                    // Set the faction uniform
                    player.SetClothes(uniform.UniformSlot, uniform.UniformDrawable, uniform.UniformTexture);
                }
                else if (uniform.FactionJob is PlayerJobs && (PlayerJobs)uniform.FactionJob == characterModel.Job && uniform.CharacterSex == characterModel.Sex)
                {
                    // Set the job uniform
                    player.SetClothes(uniform.UniformSlot, uniform.UniformDrawable, uniform.UniformTexture);
                }
            }

            // We set the player on duty
            characterModel.OnDuty = true;

            // Notification sent to the player
            player.SendNotification(InfoRes.player_on_duty);
        }

        [Command]
        public static void OrdersCommand(Player player)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (!characterModel.OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            if (player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).DeliverOrder > 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.order_delivering);
                return;
            }

            if (characterModel.Job == PlayerJobs.Fastfood)
            {
                // Get the fastfood deliverer orders
                FastFood.CheckFastfoodOrders(player);
                return;
            }

            if (characterModel.Job == PlayerJobs.Trucker)
            {
                // Get the trucker orders
                Trucker.CheckTruckerOrders(player);
                return;
            }
        }
    }
}
