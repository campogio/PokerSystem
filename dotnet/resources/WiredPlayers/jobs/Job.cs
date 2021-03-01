using GTANetworkAPI;
using System.Linq;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.commands;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.jobs
{
    public class Job : Script
    {
        public Job()
        {
            foreach (JobPickModel job in Constants.JobPickArray)
            {
                // Create the label for the command
                NAPI.TextLabel.CreateTextLabel("/" + ComRes.job, job.Position, 10.0f, 0.5f, 4, new Color(190, 235, 100), false, 0);
                NAPI.TextLabel.CreateTextLabel(GenRes.job_help, job.Position.Subtract(new Vector3(0.0f, 0.0f, 0.1f)), 10.0f, 0.5f, 4, new Color(255, 255, 255), false, 0);

                if (job.Sprite > 0)
                {
                    // Create the blip for the job
                    Blip jobBlip = NAPI.Blip.CreateBlip(job.Position);
                    jobBlip.Sprite = job.Sprite;
                    jobBlip.Name = job.Name;
                    jobBlip.ShortRange = true;
                }
            }
        }

        public static int GetJobPoints(Player player, PlayerJobs job)
        {
            // Return the job points for the player
            return int.Parse(player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).JobPoints.Split(',')[(int)job]);
        }

        public static void SetJobPoints(Player player, PlayerJobs job, int points)
        {
            // Get the character model for the given player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            string[] jobPointsArray = characterModel.JobPoints.Split(',');
            jobPointsArray[(int)job] = points.ToString();
            characterModel.JobPoints = string.Join(",", jobPointsArray);
        }

        public static void ShowPlayerJobCommands(Player player)
        {
            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Job == PlayerJobs.None)
            {
                // The player doesn't have any job
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_job);
                return;
            }

            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.commands_from, Constants.JobArray[(int)characterModel.Job].DescriptionMale));
            player.SendChatMessage(Constants.COLOR_HELP + string.Join(", ", Constants.JobCommands[(int)characterModel.Job]));
        }

        public static bool IsPlayerOnWorkPlace(Player player)
        {
            bool onWorkPlace = false;

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Job != PlayerJobs.None)
            {
                // Check if it's close to the point where he got the job
                onWorkPlace = player.Position.DistanceTo(Constants.JobPickArray.First(j => j.Job == characterModel.Job).Position) < 2.0f;
            }
            else if(characterModel.Faction != PlayerFactions.None)
            {
                // Store the Vector where the lockers are located
                Vector3 lockers = new Vector3();

                switch(characterModel.Faction)
                {
                    case PlayerFactions.Police:
                        lockers = Coordinates.PoliceLockers;
                        break;
                    case PlayerFactions.Emergency:
                        lockers = Coordinates.EmergencyLockers;
                        break;
                    case PlayerFactions.Sheriff:
                        // Get the closest lockers
                        lockers = player.Position.DistanceTo(Coordinates.PaletoLockers) < player.Position.DistanceTo(Coordinates.SandyLockers) ? Coordinates.PaletoLockers : Coordinates.SandyLockers;
                        break;
                }

                onWorkPlace = lockers != null && player.Position.DistanceTo(lockers) < 5.0f;
            }

            return onWorkPlace;
        }
    }
}
