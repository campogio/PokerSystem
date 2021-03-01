using GTANetworkAPI;
using System;
using System.Threading;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Utility;
using WiredPlayers.jobs;
using WiredPlayers.messages.arguments;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class GarbageCommands
    {
        [Command]
        public static void GarbageCommand(Player player, string action)
        {
            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Job != PlayerJobs.GarbageCollector)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_garbage);
                return;
            }

            if (!characterModel.OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (action.Equals(ArgRes.route, StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.JobRoute != GarbageRoute.None)
                {
                    player.SendChatMessage(ErrRes.already_in_route);
                    return;
                }
            	
                Random random = new Random();
                data.JobRoute = (GarbageRoute)random.Next(1, Enum.GetNames(typeof(GarbageRoute)).Length);
                
                switch (data.JobRoute)
                {
                    case GarbageRoute.North:
                        player.SendChatMessage(Constants.COLOR_INFO + GenRes.route_north);
                        break;
                    case GarbageRoute.South:
                        player.SendChatMessage(Constants.COLOR_INFO + GenRes.route_south);
                        break;
                    case GarbageRoute.East:
                        player.SendChatMessage(Constants.COLOR_INFO + GenRes.route_east);
                        break;
                    case GarbageRoute.West:
                        player.SendChatMessage(Constants.COLOR_INFO + GenRes.route_west);
                        break;
                }
                
                return;
            }
            
            if (action.Equals(ArgRes.pick_up, StringComparison.InvariantCultureIgnoreCase))
            {
                if (player.IsInVehicle)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.garbage_in_vehicle);
                    return;
                }

                if (data.GarbageBag == null || !data.GarbageBag.Exists || player.Position.DistanceTo(data.GarbageBag.Position) > 3.5f)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_garbage_near);
                    return;
                }
                
                if (Garbage.garbageTimerList.TryGetValue(player.Value, out Timer garbageTimer))
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.already_garbage);
                    return;
                }
                
                player.PlayAnimation("anim@move_m@trash", "pickup", (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl));
                data.Animation = true;

                // Make the timer for garbage collection
                garbageTimer = new Timer(Garbage.OnGarbageCollectedTimer, player, 15000, Timeout.Infinite);
                Garbage.garbageTimerList.Add(player.Value, garbageTimer);
                
                return;
            }
        
            if (action.Equals(ArgRes.cancel, StringComparison.InvariantCultureIgnoreCase))
            {
                if (data.JobPartner != null && data.JobPartner.Exists)
                {                    
                    if (data.JobPartner == player)
                    {
                        // Player doesn't have any partner
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.route_canceled);
                    }
                    
                    GTANetworkAPI.Object trashBag = null;

                    if (player.VehicleSeat == (int)VehicleSeat.Driver)
                    {
                        // Driver canceled
                        trashBag = data.GarbageBag;
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.route_finished);
                        data.JobPartner.TriggerEvent("deleteGarbageCheckPoint");

                        // Create finish checkpoint
                        player.TriggerEvent("showGarbageCheckPoint", Coordinates.Dumper, new Vector3(), CheckpointType.CylinderCheckerboard);
                    }
                    else
                    {
                        // Passenger canceled
                        trashBag = data.JobPartner.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).GarbageBag;
                        player.TriggerEvent("deleteGarbageCheckPoint");

                        // Create finish checkpoint
                        data.JobPartner.TriggerEvent("showGarbageCheckPoint", Coordinates.Dumper, new Vector3(), CheckpointType.CylinderCheckerboard);
                    }

                    // Delete the garbage bag
                    trashBag.Delete();

                    // Remove player from partner search
                    data.JobPartner = null;
                }
                else if (data.JobRoute != GarbageRoute.None)
                {
                    // Cancel the route
                    data.JobRoute = GarbageRoute.None;
                    data.JobPartner = null;

                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.garbage_route_canceled);
                }
                else
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_route);
                }
                
                return;
            }
	        
            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.garbage);
        }
    }
}
