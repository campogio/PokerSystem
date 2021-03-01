using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WiredPlayers.Administration;
using WiredPlayers.Buildings;
using WiredPlayers.Buildings.Businesses;
using WiredPlayers.character;
using WiredPlayers.chat;
using WiredPlayers.Data;
using WiredPlayers.Data.Base;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.drugs;
using WiredPlayers.factions;
using WiredPlayers.jobs;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using WiredPlayers.vehicles;
using WiredPlayers.weapons;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Server
{
    public class ConnectionHandler : Script
    {
        public static ApplicationForm ApplicationType;

        [ServerEvent(Event.PlayerConnected)]
        public async Task PlayerConnectedServerEventAsync(Player player)
        {
            // Create the initialization dictionary
            Dictionary<string, object> initData = new Dictionary<string, object>()
            {
                { "PLAYER_SOCIALNAME", player.SocialClubName },
                { "SERVER_TIME", DateTime.Now.ToString("HH:mm:ss") },
                { "BROWSER_LANGUAGE", Thread.CurrentThread.CurrentUICulture.Name }
            };

            // Initialize the player data
            Character.InitializePlayerData(player);

            // Set the default skin and transparency
            player.SetSkin(PedHash.Strperf01SMM);
            player.Transparency = 255;

            AccountModel account = await DatabaseOperations.GetAccount(player.SocialClubName).ConfigureAwait(false);

            switch (account.State)
            {
                case -1:
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.account_disabled);
                    player.Kick(InfoRes.account_disabled);
                    break;

                case 0:
                    if (ApplicationType == ApplicationForm.External)
                    {
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.account_disabled);
                        player.Kick(InfoRes.account_disabled);
                        return;
                    }

                    // Check if the account is registered or not
                    player.TriggerEvent("InitializeConnectionData", initData, account.Registered);
                    break;

                default:
                    // Welcome message
                    player.SendChatMessage(string.Format(GenRes.welcome_message, player.SocialClubName));
                    player.SendChatMessage(GenRes.welcome_hint);
                    player.SendChatMessage(GenRes.help_hint);
                    player.SendChatMessage(GenRes.ticket_hint);

                    if (account.LastCharacter > 0)
                    {
                        // Load selected character
                        await Task.Run(() => DatabaseOperations.LoadCharacterInformation(player, account.LastCharacter)).ConfigureAwait(false);
                        SkinModel skinModel = await DatabaseOperations.GetCharacterSkin(account.LastCharacter).ConfigureAwait(false);

                        // Get the character model from the player
                        CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

                        player.Name = characterModel.RealName;
                        player.SetSkin(NAPI.Util.GetHashKey(characterModel.Model));
                        characterModel.Skin = skinModel;

                        if (Customization.IsCustomCharacter(player))
                        {
                            // Load the clothes, tattoos and customization
                            Customization.ApplyPlayerClothes(player);
                            Customization.ApplyPlayerTattoos(player);
                            Customization.ApplyPlayerCustomization(player, skinModel, characterModel.Sex);
                        }

                        // Add the character to the initializing data
                        initData.Add("SELECTED_CHARACTER", account.LastCharacter);
                    }

                    // Activate the login window
                    player.TriggerEvent("InitializeConnectionData", initData, true);

                    break;
            }
        }

        [ServerEvent(Event.PlayerDisconnected)]
        public static void PlayerDisconnectedServerEvent(Player player, DisconnectionType _, string reason)
        {
            if (!Character.IsPlaying(player)) return;

            // Disconnect from the server
            CharacterModel character = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // Store the new values
            character.Position = player.Position;
            character.Rotation = player.Rotation;
            character.Health = player.Health;
            character.Armor = player.Armor;
            character.Playing = false;

            // Remove opened ticket if any
            Admin.AdminTicketCollection.Remove(player.Value);

            // Other classes' disconnect function
            CarShop.OnPlayerDisconnected(player);
            Telephone.OnPlayerDisconnected(player);
            Chat.OnPlayerDisconnected(player);
            DrivingSchool.OnPlayerDisconnected(player);
            FastFood.OnPlayerDisconnected(player);
            Fishing.OnPlayerDisconnected(player);
            Garbage.OnPlayerDisconnected(player);
            Hooker.OnPlayerDisconnected(player);
            Thief.OnPlayerDisconnected(player);
            Vehicles.OnPlayerDisconnected(player);
            Weapons.OnPlayerDisconnected(player);
            Drugs.OnPlayerDisconnected(player);

            // Save the character's data
            Character.SaveCharacterData(character);

            // Warn the players near to the disconnected one
            Chat.SendMessageToNearbyPlayers(player, string.Format(InfoRes.player_disconnected, player.Name, reason), ChatTypes.Disconnect, 10.0f);
        }

        [RemoteEvent("loginAccount")]
        public async Task LoginAccountRemoteEventAsync(Player player, string password)
        {
            // Get the status of the account
            int status = await DatabaseOperations.LoginAccount(player.SocialClubName, password).ConfigureAwait(false);

            switch (status)
            {
                case 0:
                    if (ApplicationType == ApplicationForm.Ingame)
                    {
                        // Show the ingame application
                        await LoadApplicationRemoteEventAsync(player);
                    }
                    else
                    {
                        // Let the character move and create the required ColShapes
                        player.TriggerEvent("clearLoginWindow", HelpRes.exit_lobby, HelpRes.manage_characters);
                    }
                    break;

                case 1:
                    // Let the character move and create the required ColShapes
                    player.TriggerEvent("clearLoginWindow", HelpRes.exit_lobby, HelpRes.manage_characters);
                    break;

                default:
                    player.TriggerEvent("showLoginError");
                    break;
            }
        }

        [RemoteEvent("registerAccount")]
        public async Task RegisterAccountRemoteEventAsync(Player player, string password)
        {
            // Register the account
            await Task.Run(() => DatabaseOperations.RegisterAccount(player.SocialClubName, password)).ConfigureAwait(false);

            if (ApplicationType == ApplicationForm.Ingame)
            {
                // Show the ingame application
                await LoadApplicationRemoteEventAsync(player);
            }
            else
            {
                // Let the character move and create the required ColShapes
                player.TriggerEvent("clearLoginWindow", HelpRes.exit_lobby, HelpRes.manage_characters);
            }
        }

        [RemoteEvent("submitApplication")]
        public async Task SubmitApplicationRemoteEventAsync(Player player, string answers)
        {
            // Get all the question and answers
            Dictionary<int, int> application = NAPI.Util.FromJson<Dictionary<int, int>>(answers);

            // Check if all the answers are correct
            int mistakes = await DatabaseOperations.CheckCorrectAnswers(application).ConfigureAwait(false);

            if (mistakes > 0)
            {
                // Tell the player his mistakes
                player.TriggerEvent("failedApplication", mistakes);
            }
            else
            {
                // Tell the player he passed the test
                player.SendChatMessage(Constants.COLOR_INFO + InfoRes.application_passed);

                // Destroy the test window and create the required ColShapes
                player.TriggerEvent("clearApplication", HelpRes.exit_lobby, HelpRes.manage_characters);

                // Accept the account on the server
                await Task.Run(() => DatabaseOperations.ApproveAccount(player.SocialClubName)).ConfigureAwait(false);
            }

            // Register the attempt on the database
            await Task.Run(() => DatabaseOperations.RegisterApplication(player.SocialClubName, mistakes)).ConfigureAwait(false);
        }

        [RemoteEvent("LoadPlayerCharacters")]
        public async Task LoadPlayerCharactersRemoteEventAsync(Player player)
        {
            // Show character menu
            List<string> playerList = await DatabaseOperations.GetAccountCharacters(player.SocialClubName).ConfigureAwait(false);
            player.TriggerEvent("showPlayerCharacters", playerList);
        }

        [RemoteEvent("changeCharacterSex")]
        public void ChangeCharacterSexRemoteEvent(Player player, int sex)
        {
            // Set the model of the player
            player.SetSkin(sex == (int)Sex.Male ? PedHash.FreemodeMale01 : PedHash.FreemodeFemale01);

            // Remove player's clothes
            Customization.RemovePlayerClothes(player, (Sex)sex, false);

            // Force the player's animation
            player.PlayAnimation("amb@world_human_hang_out_street@female_arms_crossed@base", "base", (int)AnimationFlags.Loop);
        }

        [RemoteEvent("createCharacter")]
        public async Task CreateCharacterRemoteEventAsync(Player player, string characterName, string characterModel, int characterAge, int characterSex, string skinJson)
        {
            CharacterModel character = new CharacterModel()
            {
                RealName = characterName,
                Model = characterModel,
                Age = characterAge,
                Sex = (Sex)characterSex
            };

            // Set the player model
            player.SetSkin(NAPI.Util.GetHashKey(character.Model));

            if (Customization.IsCustomCharacter(player))
            {
                // Add the customization
                character.Skin = NAPI.Util.FromJson<SkinModel>(skinJson);
                Customization.ApplyPlayerCustomization(player, character.Skin, character.Sex);
            }

            // Create the new character and get the identifier
            character.Id = await DatabaseOperations.CreateCharacter(player, character).ConfigureAwait(false);

            if (character.Id > 0)
            {
                // Initialize the data
                Character.InitializePlayerData(player);

                // Set the default spawn
                player.Transparency = 255;
                character.Health = 100;
                character.Position = Coordinates.WorldSpawn;
                character.Rotation = Coordinates.LobbyRotation;

                // Add the created data to the player
                player.SetExternalData((int)ExternalDataSlot.Database, character);

                // Clear the character creator
                player.TriggerEvent("characterCreatedSuccessfully", character.Id);
            }
        }

        [RemoteEvent("setCharacterIntoCreator")]
        public void SetCharacterIntoCreatorRemoteEvent(Player player)
        {
            // Change player's skin
            player.SetSkin(PedHash.Strperf01SMM);

            // Remove clothes
            Customization.RemovePlayerClothes(player, Sex.Male, false);

            // Remove all the tattoos
            Customization.RemovePlayerTattoos(player);

            // Set player's position
            player.Transparency = 255;
            player.Position = Coordinates.LobbySpawn;
            player.Rotation = Coordinates.LobbyRotation;

            // Play the idle animation
            PlayIdleCreatorAnimationRemoteEvent(player);
        }

        [RemoteEvent("playIdleCreatorAnimation")]
        public void PlayIdleCreatorAnimationRemoteEvent(Player player)
        {
            // Force the player's animation
            player.PlayAnimation("amb@world_human_hang_out_street@female_arms_crossed@base", "base", (int)AnimationFlags.Loop);
        }

        [RemoteEvent("loadCharacter")]
        public async Task LoadCharacterRemoteEventAsync(Player player, string name)
        {
            // Load the character information
            await DatabaseOperations.LoadCharacterInformation(player, name).ConfigureAwait(false);

            // Load player's model
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);
            player.Name = characterModel.RealName;
            player.SetSkin(NAPI.Util.GetHashKey(characterModel.Model));

            if (Customization.IsCustomCharacter(player))
            {
                // Get the customization from the database
                SkinModel skinModel = await DatabaseOperations.GetCharacterSkin(characterModel.Id).ConfigureAwait(false);
                characterModel.Skin = skinModel;

                // Customize the character
                Customization.ApplyPlayerClothes(player);
                Customization.ApplyPlayerTattoos(player);
                Customization.ApplyPlayerCustomization(player, skinModel, characterModel.Sex);
            }

            // Update last selected character
            await Task.Run(() => DatabaseOperations.UpdateLastCharacter(player.SocialClubName, characterModel.Id)).ConfigureAwait(false);
        }

        [RemoteEvent("loadApplication")]
        public async Task LoadApplicationRemoteEventAsync(Player player)
        {
            // Get random questions
            List<TestModel> applicationQuestions = await DatabaseOperations.GetRandomQuestions(Constants.APPLICATION_TEST, 10).ConfigureAwait(false);

            // Get the ids from each question
            List<int> questionIds = applicationQuestions.Select(q => q.id).Distinct().ToList();

            // Get the answers from the questions
            List<TestModel> applicationAnswers = DatabaseOperations.GetQuestionAnswers(questionIds);

            player.TriggerEvent("showApplicationTest", NAPI.Util.ToJson(applicationQuestions), NAPI.Util.ToJson(applicationAnswers));
        }

        [RemoteEvent("ShowNoCharacterSelectedError")]
        public void ShowNoCharacterSelectedError(Player player)
        {
            // Show the error to the player
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.no_character_selected);
        }

        [RemoteEvent("SpawnCharacterIntoWorld")]
        public void SpawnCharacterIntoWorldRemoteEvent(Player player)
        {
            NAPI.Task.Run(() =>
            {
                // Get the player's character model
                CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

                // Get the items on both hands
                ItemModel rightHand = Inventory.GetItemInEntity(characterModel.Id, Constants.ITEM_ENTITY_RIGHT_HAND);
                ItemModel leftHand = Inventory.GetItemInEntity(characterModel.Id, Constants.ITEM_ENTITY_LEFT_HAND);

                // Give the weapons to the player
                Weapons.GivePlayerWeaponItems(player);

                if (rightHand != null)
                {
                    BusinessItemModel businessItem = Business.GetBusinessItemFromHash(rightHand.hash);
                    uint hash = NAPI.Util.GetHashKey(rightHand.hash);

                    if (Enum.IsDefined(typeof(WeaponHash), hash))
                    {
                        // Give the weapon to the player
                        player.GiveWeapon((WeaponHash)hash, rightHand.amount);
                    }
                    else
                    {
                        // Give the item to the player
                        player.GiveWeapon(WeaponHash.Unarmed, 1);
                        UtilityFunctions.AttachItemToPlayer(player, rightHand.id, rightHand.hash, "IK_R_Hand", businessItem.position, businessItem.rotation, EntityData.PlayerRightHand);
                    }
                }

                if (leftHand != null)
                {
                    BusinessItemModel businessItem = Business.GetBusinessItemFromHash(leftHand.hash);
                    UtilityFunctions.AttachItemToPlayer(player, leftHand.id, leftHand.hash, "IK_L_Hand", businessItem.position, businessItem.rotation, EntityData.PlayerLeftHand);
                }

                if (characterModel.BuildingEntered.Type != BuildingTypes.None)
                {
                    // Calculate spawn dimension
                    CheckSpawnBuilding(player, characterModel.BuildingEntered);
                }

                // Spawn the player into the world
                player.Name = characterModel.RealName;
                player.Position = characterModel.Position;
                player.Rotation = characterModel.Rotation;
                player.Health = characterModel.Health;
                player.Armor = characterModel.Armor;
                player.Dimension = 0;

                if (characterModel.KilledBy != 0)
                {
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

                    // Create the report for the emergency department
                    Emergency.WarnEmergencyDepartment(player, deathPlace, deathPosition, deathHour);

                    player.SetData(EntityData.TimeHospitalRespawn, UtilityFunctions.GetTotalSeconds() + 240);
                    player.SendChatMessage(Constants.COLOR_INFO + InfoRes.emergency_warn);

                    // Change the death state
                    player.TriggerEvent("togglePlayerDead", true);
                }

                // Toggle connection flag
                characterModel.Playing = true;

                // Let the player play
                player.TriggerEvent("playerLoggedIn", characterModel.Money);
            }, 1250);
        }

        [RemoteEvent("forcePlayerLogout")]
        public async Task ForcePlayerLogoutRemoteEventAsync(Player player)
        {
            // Make the player logout
            await Task.Run(() => PlayerDisconnectedServerEvent(player, DisconnectionType.Left, string.Empty));

            // Initialize the player data
            Character.InitializePlayerData(player);

            // Initialize the lobby
            player.TriggerEvent("InitializeLobby", HelpRes.exit_lobby, HelpRes.manage_characters);

            // Send the player to the lobby
            await LoadCharacterRemoteEventAsync(player, player.Name);
        }

        private void CheckSpawnBuilding(Player player, BuildingModel building)
        {
            // Get the property given the building
            PropertyModel property = BuildingHandler.GetPropertyById(building.Id, building.Type);

            switch (building.Type)
            {
                case BuildingTypes.Interior:
                    player.Dimension = 0;
                    break;

                case BuildingTypes.House:
                case BuildingTypes.Business:
                    player.Dimension = Convert.ToUInt32(building.Id);
                    break;
            }

            // Generate the interior
            BuildingHandler.PlacePlayerIntoBuilding(player, property, building);
        }
    }
}