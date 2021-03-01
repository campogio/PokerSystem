using GTANetworkAPI;
using WiredPlayers.Data.Persistent;
using WiredPlayers.drugs;
using WiredPlayers.Utility;
using WiredPlayers.messages.error;
using WiredPlayers.factions;

namespace WiredPlayers.Server.Commands
{
    public static class DrugsCommands
    {
        [Command]
        public static void PlantCommand(Player player)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            if (player.Dimension != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.cant_plant_indoor);
                return;
            }

            if (Drugs.PlantingTimer.ContainsKey(player.Value))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_planting);
                return;
            }

            // Check the closest plant
            PlantModel plant = Drugs.GetClosestPlant(player);

            if(plant != null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_has_plants_close);
                return;
            }

            // Plant the seeds
            Drugs.AnimatePlayerWeedManagement(player, true);
        }
    }
}
