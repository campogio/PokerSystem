using Newtonsoft.Json;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using WiredPlayers_Client.globals;
using WiredPlayers_Client.Scaleform;

namespace WiredPlayers_Client.account
{
    class AccountHandler : Events.Script
    {
        public static string SocialName;
        public static bool PlayerLogged;
        public static int CharacterSelected;

        private static Colshape LobbyExitColShape;
        private static Colshape CharacterCreatorColShape;

        private static Marker ExitMarker;
        private static Marker CharacterCreatorMarker;

        private static string ExitMessage;
        private static string CharacterCreatorMessage;

        public AccountHandler()
        {
            // Add custom event
            Events.Add("InitializeConnectionData", InitializeConnectionDataEvent);
            Events.Add("InitializeLobby", InitializeLobbyEvent);

            // Add the events
            Events.OnPlayerEnterColshape += OnPlayerEnterColshapeEvent;

            // Store the social name
            Events.AddDataHandler("PLAYER_SOCIALNAME", PlayerSocialNameLoaded);
        }

        public static void CreateLobbyColShapes(string exitMessage, string characterCreatorMessage)
        {
            // Add the messages
            ExitMessage = exitMessage;
            CharacterCreatorMessage = characterCreatorMessage;

            // Create the lobby ColShapes
            LobbyExitColShape = new CircleColshape(404.6483f, -997.2951f, 1.25f, Player.LocalPlayer.Dimension);
            CharacterCreatorColShape = new CircleColshape(401.8016f, -1001.897f, 1.25f, Player.LocalPlayer.Dimension);

            // Create the markers
            ExitMarker = new Marker(0, new Vector3(404.1018f, -997.409f, -99.00404f), 0.75f, new Vector3(), new Vector3(), new RGBA(244, 126, 23), true, Player.LocalPlayer.Dimension);
            CharacterCreatorMarker = new Marker(30, new Vector3(401.8016f, -1001.897f, -99.00404f), 0.5f, new Vector3(), new Vector3(), new RGBA(244, 126, 23), true, Player.LocalPlayer.Dimension);
        }

        public static void HandleLobbyColShape()
        {
            // Check if the lobby has been generated
            if (ExitMarker == null || LobbyExitColShape == null) return;

            // Get the player position
            Vector3 position = Player.LocalPlayer.Position;

            if (ExitMarker.Position.DistanceTo(position) <= 1.25f)
            {
                if (CharacterSelected == 0)
                {
                    // The player didn't select any character
                    Events.CallRemote("ShowNoCharacterSelectedError");
                }

                // Show the loading screen
                RAGE.Game.Cam.DoScreenFadeOut(500);

                // Make player log into the world
                Events.CallRemote("SpawnCharacterIntoWorld");
            }
            else if (CharacterCreatorMarker.Position.DistanceTo(position) <= 1.25f)
            {
                // Show the character creator
                Events.CallRemote("LoadPlayerCharacters");
            }
        }

        public static void ClearLobby()
        {
            // Remove the created ColShapes
            LobbyExitColShape.Destroy();
            CharacterCreatorColShape.Destroy();

            // Remove the created markers
            ExitMarker.Destroy();
            CharacterCreatorMarker.Destroy();
        }

        private void InitializeConnectionDataEvent(object[] args)
        {
            // Retrieve the parameters
            Dictionary<string, object> initData = JsonConvert.DeserializeObject<Dictionary<string, object>>(args[0].ToString());

            // Get the social club name
            SocialName = initData["PLAYER_SOCIALNAME"].ToString();

            // Set the browser's locale
            Browser.CurrentLanguage = initData["BROWSER_LANGUAGE"].ToString();

            // There was a character selected, set the game time
            string[] serverTime = initData["SERVER_TIME"].ToString().Split(":");

            int hours = int.Parse(serverTime[0]);
            int minutes = int.Parse(serverTime[1]);
            int seconds = int.Parse(serverTime[2]);

            // Set the hour from the server
            RAGE.Game.Clock.SetClockTime(hours, minutes, seconds);

            if (initData.ContainsKey("SELECTED_CHARACTER"))
            {
                // Choose the character selected
                CharacterSelected = Convert.ToInt32(initData["SELECTED_CHARACTER"]);
            }

            if (Convert.ToBoolean(args[1]))
            {
                // The account is already registered, show the login form
                Login.ShowLoginForm();
            }
            else
            {
                // The player needs to make a new account
                Register.ShowRegisterForm();
            }
        }

        private void InitializeLobbyEvent(object[] args)
        {
            // Log the character out
            PlayerLogged = false;

            // Get the messages
            string exitMessage = args[0].ToString();
            string characterCreatorMessage = args[1].ToString();

            // Create the ColShapes and markers
            CreateLobbyColShapes(exitMessage, characterCreatorMessage);
        }

        private void OnPlayerEnterColshapeEvent(Colshape colshape, Events.CancelEventArgs cancel)
        {
            if (colshape != LobbyExitColShape && colshape != CharacterCreatorColShape) return;

            // Show the instructional button
            InstructionalButtons.ShowInstructionalButtonEvent(new object[] { colshape.Id == LobbyExitColShape.Id ? ExitMessage : CharacterCreatorMessage, "F" });

            // Cancel the rest of the events
            cancel.Cancel = true;
        }

        private void PlayerSocialNameLoaded(Entity entity, object arg, object oldArg)
        {
            if (entity != Player.LocalPlayer) return;

            // Get the language from the server
            SocialName = arg.ToString();
        }
    }
}
