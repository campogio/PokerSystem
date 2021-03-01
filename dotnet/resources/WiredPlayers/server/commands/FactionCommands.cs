using GTANetworkAPI;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using WiredPlayers.chat;
using WiredPlayers.character;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.factions;
using WiredPlayers.Utility;
using WiredPlayers.messages.arguments;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class FactionCommands
    {
        [Command]
        public static void FCommand(Player player, string message)
        {
            // Get the character model for the player
            PlayerFactions faction = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction;

            if (faction == PlayerFactions.None || (int)faction >= Constants.LAST_STATE_FACTION)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_state_faction);
                return;
            }

            string rank = Faction.GetPlayerFactionRank(player);

            // Get the players on the faction
            Player[] targetList = NAPI.Pools.GetAllPlayers().Where(p => Character.IsPlaying(p) && p.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction == faction).ToArray();

            foreach (Player target in targetList)
            {
                // Send the message to the player
                target.SendChatMessage(Constants.COLOR_CHAT_FACTION + string.Format("(([ID: {0}] {1} {2}: {3}", player.Value, rank, player.Name, message));
            }
        }

        [Command]
        public static void RCommand(Player player, string message)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Faction != PlayerFactions.News && !characterModel.OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            if (characterModel.Faction == PlayerFactions.None || (int)characterModel.Faction >= Constants.LAST_STATE_FACTION)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_state_faction);
                return;
            }

            // Get player's rank in faction
            string rank = Faction.GetPlayerFactionRank(player);

            // Get all the players in the faction
            Player[] targetList = NAPI.Pools.GetAllPlayers().Where(p => Character.IsPlaying(p) && (p.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction == characterModel.Faction || Faction.CheckInternalAffairs(characterModel.Faction, p)) && p.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty).ToArray();

            foreach (Player target in targetList)
            {
                // Send the message to each one of the players
                target.SendChatMessage(Constants.COLOR_RADIO + GenRes.radio + rank + " " + player.Name + GenRes.chat_say + message);
            }

            // Send the chat message to near players
            Chat.SendMessageToNearbyPlayers(player, message, ChatTypes.Radio, player.Dimension > 0 ? 7.5f : 10.0f);
        }

        [Command]
        public static void PdCommand(Player player, string message)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Faction != PlayerFactions.Emergency)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_faction);
                return;
            }

            if (!characterModel.OnDuty)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_on_duty);
                return;
            }

            string rank = Faction.GetPlayerFactionRank(player);
            string radioMessage = GenRes.radio + rank + " " + player.Name + GenRes.chat_say + message;

            // Get the players playing and in service
            Player[] playerFactionMembers = NAPI.Pools.GetAllPlayers().Where(p => Character.IsPlaying(p) && p.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty).ToArray();

            foreach (Player target in playerFactionMembers)
            {
                // Send the message to the player
                if (Faction.IsPoliceMember(target))
                {
                    target.SendChatMessage(Constants.COLOR_RADIO + radioMessage);
                }
                else if (target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction == PlayerFactions.Emergency)
                {
                    target.SendChatMessage(Constants.COLOR_RADIO_POLICE + radioMessage);
                }
            }

            // Send the chat message to near players
            Chat.SendMessageToNearbyPlayers(player, message, ChatTypes.Radio, player.Dimension > 0 ? 7.5f : 10.0f);
        }

        [Command]
        public static void EdCommand(Player player, string message)
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

            string rank = Faction.GetPlayerFactionRank(player);
            string radioMessage = GenRes.radio + rank + " " + player.Name + GenRes.chat_say + message;

            // Get the players playing and in service
            Player[] playerFactionMembers = NAPI.Pools.GetAllPlayers().Where(p => Character.IsPlaying(p) && p.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).OnDuty).ToArray();

            foreach (Player target in NAPI.Pools.GetAllPlayers())
            {
                if (Faction.IsPoliceMember(target))
                {
                    target.SendChatMessage(Constants.COLOR_RADIO_POLICE + radioMessage);
                }
                else if (target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction == PlayerFactions.Emergency)
                {
                    target.SendChatMessage(Constants.COLOR_RADIO + radioMessage);
                }
            }

            // Send the chat message to near players
            Chat.SendMessageToNearbyPlayers(player, message, ChatTypes.Radio, player.Dimension > 0 ? 7.5f : 10.0f);
        }

        [Command]
        public static void FrCommand(Player player, string message)
        {
            if (Emergency.IsPlayerDead(player))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);
            
            if (characterModel.Radio == 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.radio_frequency_none);
                return;
            }

            // Get the players with the same radio frequency
            Player[] targetList = NAPI.Pools.GetAllPlayers().Where(p => Character.IsPlaying(p) && p.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Radio == characterModel.Radio).ToArray();

            foreach (Player target in targetList)
            {
                // Send the message to each player
                target.SendChatMessage(Constants.COLOR_RADIO + GenRes.radio + characterModel.RealName + GenRes.chat_say + message);
            }

            // Send the chat message to near players
            Chat.SendMessageToNearbyPlayers(player, message, ChatTypes.Radio, player.Dimension > 0 ? 7.5f : 10.0f);
        }

        [Command]
        public static async Task FrequencyCommand(Player player, string args)
        {
            if (!player.HasSharedData(EntityData.PlayerRightHand))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.right_hand_empty);
                return;
            }

            // Get the item identifier
            string rightHand = player.GetSharedData<string>(EntityData.PlayerRightHand);
            int itemId = NAPI.Util.FromJson<AttachmentModel>(rightHand).itemId;
            ItemModel item = Inventory.GetItemModelFromId(itemId);

            if (item == null || item.hash != Constants.ITEM_HASH_WALKIE)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_walkie_in_hand);
                return;
            }

            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            ChannelModel ownedChannel = Faction.GetPlayerOwnedChannel(characterModel.Id);
            string[] arguments = args.Trim().Split(' ');

            if (arguments[0].Equals(ArgRes.create, StringComparison.InvariantCultureIgnoreCase))
            {
                if (arguments.Length != 2)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.frequency_create);
                    return;
                }

                if (ownedChannel != null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.already_owned_channel);
                    return;
                }

                // We create the new frequency
                ChannelModel channel = new ChannelModel();
                {
                    MD5 hashInstance = MD5.Create();
                    channel.Owner = characterModel.Id;
                    channel.Password = Faction.GetMd5Hash(hashInstance, arguments[1]);
                    hashInstance.Dispose();
                }

                // Create the new channel
                channel.Id = await DatabaseOperations.AddChannel(channel);
                Faction.ChannelList.Add(channel.Id, channel);

                // Sending the message with created channel
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.channel_created, channel.Id));

                return;
            }

            if (arguments[0].Equals(ArgRes.modify, StringComparison.InvariantCultureIgnoreCase))
            {
                if (arguments.Length != 2)
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.frequency_modify);
                    return;
                }

                if (ownedChannel == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_owned_channel);
                    return;
                }

                // We encrypt the password
                MD5 hashInstance = MD5.Create();
                ownedChannel.Password = Faction.GetMd5Hash(hashInstance, arguments[1]);
                hashInstance.Dispose();

                // We kick all the players from the channel
                foreach (Player target in NAPI.Pools.GetAllPlayers())
                {
                    // Get the character model for the player
                    CharacterModel targetModel = target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

                    if (targetModel.Radio == ownedChannel.Id && targetModel.Id != ownedChannel.Owner)
                    {
                        targetModel.Radio = 0;
                        target.SendChatMessage(Constants.COLOR_INFO + InfoRes.channel_disconnected);
                    }
                }

                await Task.Run(() =>
                {
                    // Update the channel and disconnect the leader
                    DatabaseOperations.UpdateChannel(ownedChannel);
                    DatabaseOperations.DisconnectFromChannel(ownedChannel.Id);
                }).ConfigureAwait(false);


                // Message sent with the confirmation
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.channel_updated);

                return;
            }

            if (arguments[0].Equals(ArgRes.remove, StringComparison.InvariantCultureIgnoreCase))
            {
                if (ownedChannel == null)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_owned_channel);
                    return;
                }

                // Get all the players connected
                Player[] radioConnected = NAPI.Pools.GetAllPlayers().Where(t => t.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Radio == ownedChannel.Id).ToArray();

                foreach (Player target in radioConnected)
                {
                    // Get the character model for the player
                    CharacterModel targetModel = target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

                    // We kick all the players from the channel
                    targetModel.Radio = 0;

                    if (ownedChannel.Owner != targetModel.Id)
                    {
                        target.SendChatMessage(Constants.COLOR_INFO + InfoRes.channel_disconnected);
                    }
                }

                await Task.Run(() =>
                {
                    // Disconnect the leader from the channel
                    DatabaseOperations.DisconnectFromChannel(ownedChannel.Id);

                    // We destroy the channel
                    DatabaseOperations.DeleteSingleRow("channels", "id", ownedChannel.Id);
                }).ConfigureAwait(false);

                Faction.ChannelList.Remove(ownedChannel.Id);

                // Message sent with the confirmation
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.channel_deleted);

                return;
            }

            if (arguments[0].Equals(ArgRes.connect, StringComparison.InvariantCultureIgnoreCase))
            {
                if (arguments.Length != 3 || !int.TryParse(arguments[1], out int frequency))
                {
                    player.SendChatMessage(Constants.COLOR_HELP + HelpRes.frequency_connect);
                    return;
                }

                // We encrypt the password
                MD5 hashInstance = MD5.Create();
                string password = Faction.GetMd5Hash(hashInstance, arguments[2]);
                hashInstance.Dispose();

                // Check if the channel is correct
                if (Faction.ChannelList.Values.Any(c => c.Id == frequency && c.Password == password))
                {
                    characterModel.Radio = frequency;
                    player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.channel_connected, frequency));
                }
                else 
                {
                    // Channel connection failed
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.channel_not_found);
                }

                return;
            }

            if (arguments[0].Equals(ArgRes.disconnect, StringComparison.InvariantCultureIgnoreCase))
            {
                characterModel.Radio = 0;
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.channel_disconnected);
                return;
            }
        }

        [Command]
        public static void RecruitCommand(Player player, string targetString)
        {
            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Faction == PlayerFactions.None)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_faction);
                return;
            }

            // Get the target client
            Player target = UtilityFunctions.GetPlayer(targetString);

            // Get the character model for the target
            CharacterModel targetCharacterModel = target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (target == null || player.Position.DistanceTo(target.Position) > 5.0f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }

            if (targetCharacterModel.Faction != PlayerFactions.None)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_faction);
                return;
            }

            if ((int)characterModel.Faction < Constants.LAST_STATE_FACTION && targetCharacterModel.Job != PlayerJobs.None)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_job);
                return;
            }

            // Get the player's rank into the faction
            string targetMessage;

            switch (characterModel.Faction)
            {
                case PlayerFactions.Police:
                    if (characterModel.Rank < 6)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_recruit);
                        return;
                    }

                    // We get the player into the faction
                    targetCharacterModel.Faction = PlayerFactions.Police;
                    targetCharacterModel.Rank = 1;

                    targetMessage = string.Format(InfoRes.faction_recruited, GenRes.faction_lspd);
                    break;

                case PlayerFactions.Emergency:
                    if (characterModel.Rank < 10)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_recruit);
                        return;
                    }

                    // We get the player into the faction
                    targetCharacterModel.Faction = PlayerFactions.Emergency;
                    targetCharacterModel.Rank = 1;

                    targetMessage = string.Format(InfoRes.faction_recruited, GenRes.faction_ems);
                    break;

                case PlayerFactions.News:
                    if (characterModel.Rank < 5)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_recruit);
                        return;
                    }

                    // We get the player into the faction
                    targetCharacterModel.Faction = PlayerFactions.News;
                    targetCharacterModel.Rank = 1;

                    targetMessage = string.Format(InfoRes.faction_recruited, GenRes.faction_news);
                    break;

                case PlayerFactions.TownHall:
                    if (characterModel.Rank < 3)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_recruit);
                        return;
                    }

                    // We get the player into the faction
                    targetCharacterModel.Faction = PlayerFactions.TownHall;
                    targetCharacterModel.Rank = 1;

                    targetMessage = string.Format(InfoRes.faction_recruited, GenRes.faction_townhall);
                    break;

                case PlayerFactions.Taxi:
                    if (characterModel.Rank < 5)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_recruit);
                        return;
                    }

                    // We get the player into the faction
                    targetCharacterModel.Faction = PlayerFactions.Taxi;
                    targetCharacterModel.Rank = 1;

                    targetMessage = string.Format(InfoRes.faction_recruited, GenRes.faction_transport);
                    break;

                case PlayerFactions.Sheriff:
                    if (characterModel.Rank < 6)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_recruit);
                        return;
                    }

                    // We get the player into the faction
                    targetCharacterModel.Faction = PlayerFactions.Sheriff;
                    targetCharacterModel.Rank = 1;

                    targetMessage = string.Format(InfoRes.faction_recruited, GenRes.sheriff_faction);
                    break;

                default:
                    if (characterModel.Rank < 6)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_recruit);
                        return;
                    }

                    // We get the player into the faction
                    targetCharacterModel.Faction = characterModel.Faction;
                    targetCharacterModel.Rank = 1;


                    targetMessage = string.Format(InfoRes.faction_recruited, targetCharacterModel.Faction);
                    break;
            }

            // We send the message to the recruiter
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_recruited, target.Name));
            target.SendChatMessage(Constants.COLOR_INFO + targetMessage);
        }

        [Command]
        public static void DismissCommand(Player player, string targetString)
        {
            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Faction == PlayerFactions.None)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_faction);
                return;
            }

            // Get the target client
            Player target = UtilityFunctions.GetPlayer(targetString);

            if (target == null || player.Position.DistanceTo(target.Position) > 5.0f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }

            // Get the character model for the target
            CharacterModel targetCharacterModel = target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (targetCharacterModel.Faction != characterModel.Faction)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_in_same_faction);
                return;
            }

            switch (characterModel.Faction)
            {
                case PlayerFactions.Police:
                    if (characterModel.Rank < 6)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_dismiss);
                        return;
                    }

                    break;

                case PlayerFactions.Emergency:
                    if (characterModel.Rank < 10)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_dismiss);
                        return;
                    }

                    break;

                case PlayerFactions.News:
                    if (characterModel.Rank < 5)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_dismiss);
                        return;
                    }

                    break;

                case PlayerFactions.TownHall:
                    if (characterModel.Rank < 3)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_dismiss);
                        return;
                    }

                    break;

                case PlayerFactions.Taxi:
                    if (characterModel.Rank < 5)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_dismiss);
                        return;
                    }

                    break;

                case PlayerFactions.Sheriff:
                    if (characterModel.Rank < 6)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_dismiss);
                        return;
                    }

                    break;

                default:
                    if (characterModel.Rank < 6)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_dismiss);
                        return;
                    }

                    break;
            }

            // We kick the player from the faction
            targetCharacterModel.Faction = PlayerFactions.None;
            targetCharacterModel.Rank = 0;

            // Send the messages to both players
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_dismissed, target.Name));
            target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.faction_dismissed, player.Name));
        }

        [Command]
        public static void RankCommand(Player player, string args)
        {
            // Get the character model for the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (characterModel.Faction == PlayerFactions.None)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_faction);
                return;
            }

            // Check if the required parameters are met
            string[] arguments = args.Split(' ');

            if (arguments.Length != 2 && arguments.Length != 3)
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.rank);
                return;
            }

            // Get the target player
            Player target = UtilityFunctions.GetPlayer(ref arguments);
            
            if (target == null || player.Position.DistanceTo(target.Position) > 5.0f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_found);
                return;
            }

            // Get the character model for the target
            CharacterModel targetCharacterModel = target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (targetCharacterModel.Faction != characterModel.Faction)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_in_same_faction);
                return;
            }
            
            if (!int.TryParse(arguments[0], out int givenRank))
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.rank);
                return;
            }

            switch (characterModel.Faction)
            {
                case PlayerFactions.Police:
                    if (characterModel.Rank < 6)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_rank);
                        return;
                    }

                    break;

                case PlayerFactions.Emergency:
                    if (characterModel.Rank < 10)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_rank);
                        return;
                    }

                    break;

                case PlayerFactions.News:
                    if (characterModel.Rank < 5)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_rank);
                        return;
                    }

                    break;

                case PlayerFactions.TownHall:
                    if (characterModel.Rank < 3)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_rank);
                        return;
                    }

                    break;

                case PlayerFactions.Taxi:
                    if (characterModel.Rank < 5)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_rank);
                        return;
                    }

                    break;

                case PlayerFactions.Sheriff:
                    if (characterModel.Rank < 5)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_rank);
                        return;
                    }

                    break;

                default:
                    if (characterModel.Rank < 6)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.rank_too_low_rank);
                        return;
                    }

                    break;
            }

            // Change target's rank
            targetCharacterModel.Rank = givenRank;

            // Send the message to both players
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_rank_changed, target.Name, givenRank));
            target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.faction_rank_changed, player.Name, givenRank));
        }

        [Command]
        public static void ReportsCommand(Player player)
        {
            PlayerFactions faction = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction;

            if (!Faction.IsPoliceMember(player) && faction != PlayerFactions.Emergency)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_emergency_faction);
                return;
            }

            int currentElement = 0;

            // Reports' header
            player.SendChatMessage(Constants.COLOR_INFO + GenRes.reports_header);

            // Get all the warnings for the player's faction
            FactionWarningModel[] warnings = Faction.factionWarningList.Where(w => w.Faction == faction).ToArray();

            foreach (FactionWarningModel factionWarning in warnings)
            {
                string message = currentElement + ". " + GenRes.time + factionWarning.Hour;

                if (factionWarning.Place.Length > 0)
                {
                    message += ", " + GenRes.place + factionWarning.Place;
                }

                // Check if attended
                if (factionWarning.TakenBy > -1)
                {
                    Player target = UtilityFunctions.GetPlayer(factionWarning.TakenBy);
                    message += ", " + GenRes.attended_by + target.Name;
                }
                else
                {
                    message += ", " + GenRes.unattended;
                }

                // We send the message to the player
                player.SendChatMessage(Constants.COLOR_HELP + message);

                currentElement++;
            }

            if (currentElement == 0)
            {
                // There are no reports in the list
                player.SendChatMessage(Constants.COLOR_HELP + GenRes.not_faction_warning);
            }
        }

        [Command]
        public static void AttendCommand(Player player, int warning)
        {
            PlayerFactions faction = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction;

            if (!Faction.IsPoliceMember(player) && faction != PlayerFactions.Emergency)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_emergency_faction);
                return;
            }

            // Get all the warnings from the faction
            FactionWarningModel factionWarning = Faction.factionWarningList.Where(f => f.Faction == faction).ToArray()[warning];

            // Check the faction and whether the report is attended
            if (factionWarning == null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.faction_warning_not_found);
                return;
            }

            if (factionWarning.TakenBy > -1)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.faction_warning_taken);
                return;
            }

            if (factionWarning.PlayerId == player.Value)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_own_death);
                return;
            }

            // Get the player's temporary data
            PlayerTemporaryModel temporaryModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (temporaryModel.FactionWarning)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_have_faction_warning);
                return;
            }

            factionWarning.TakenBy = player.Value;
            temporaryModel.FactionWarning = true;

            player.TriggerEvent("showFactionWarning", factionWarning.Position);

            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.faction_warning_taken);
        }

        [Command]
        public static void ClearReportsCommand(Player player, int warning)
        {
            PlayerFactions faction = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction;

            if (!Faction.IsPoliceMember(player) && faction != PlayerFactions.Emergency)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_police_emergency_faction);
                return;
            }

            // Get all the warnings from the faction
            FactionWarningModel factionWarning = Faction.factionWarningList.Where(f => f.Faction == faction).ToArray()[warning];

            // Check the faction and whether the report is attended
            if (factionWarning == null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.faction_warning_not_found);
                return;
            }

            // We remove the report
            Faction.factionWarningList.Remove(factionWarning);

            // Send the message to the user
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.faction_warning_deleted, warning));
        }

        [Command]
        public static void MembersCommand(Player player)
        {
            PlayerFactions faction = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction;

            if (faction == PlayerFactions.None)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_faction);
                return;
            }

            player.SendChatMessage(Constants.COLOR_INFO + GenRes.members_online);

            foreach (Player target in NAPI.Pools.GetAllPlayers())
            {
                if (Character.IsPlaying(target) && target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Faction == faction)
                {
                    string rank = Faction.GetPlayerFactionRank(target);

                    if (rank == string.Empty)
                    {
                        player.SendChatMessage(Constants.COLOR_HELP + string.Format("[Id: {0}] {1}", target.Value, target.Name));
                    }
                    else
                    {
                        player.SendChatMessage(Constants.COLOR_HELP + string.Format("[Id: {0}] {1} {2}", target.Value, rank, target.Name));
                    }
                }
            }
        }

        [Command]
        public static void SirenCommand(Player player)
        {
            if (player.VehicleSeat != (int)VehicleSeat.Driver)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_vehicle_driving);
                return;
            }

            // Get the class of the vehicle
            if (player.Vehicle.Class != (int)VehicleClass.Emergency)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_emergency_vehicle);
                return;
            }

            // Toggle the siren status for all the players
            bool siren = player.Vehicle.GetSharedData<bool>(EntityData.VehicleSirenSound);
            player.Vehicle.SetSharedData(EntityData.VehicleSirenSound, !siren);
            Player[] connectedPlayers = NAPI.Pools.GetAllPlayers().Where(p => Character.IsPlaying(p)).ToArray();

            foreach (Player target in connectedPlayers)
            {
                // Synchronize the siren state
                target.TriggerEvent("toggleSirenState", player.Vehicle.Value, !siren);
            }
        }
    }
}
