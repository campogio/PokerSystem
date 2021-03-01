using System;
using RAGE;
using RAGE.Elements;

namespace WiredPlayers_Client.Scaleform
{
    public class InstructionalButtons : Events.Script
    {
        private static int scaleformHandle;
        private static string buttonHandle;

        public InstructionalButtons()
        {
            // Custom events
            Events.Add("ShowInstructionalButton", ShowInstructionalButtonEvent);

            // Default events
            Events.OnPlayerExitColshape += OnPlayerExitColshapeEvent;

            // Load the scaleform
            scaleformHandle = RAGE.Game.Graphics.RequestScaleformMovie("instructional_buttons");
            RAGE.Game.Graphics.PushScaleformMovieFunction(scaleformHandle, "SET_DATA_SLOT_EMPTY");
            RAGE.Game.Graphics.PopScaleformMovieFunctionVoid();
        }

        public static void ShowInstructionalButtonEvent(object[] args)
        {
            // Get the text string and the button to show
            string text = args[0].ToString();
            buttonHandle = "t_" + args[1].ToString();

            // Add the parameters to the scaleform function
            RAGE.Game.Graphics.PushScaleformMovieFunction(scaleformHandle, "SET_DATA_SLOT");
            RAGE.Game.Graphics.PushScaleformMovieFunctionParameterInt(0);
            RAGE.Game.Graphics.PushScaleformMovieFunctionParameterString(buttonHandle);
            RAGE.Game.Graphics.PushScaleformMovieFunctionParameterString(text);
            RAGE.Game.Graphics.PopScaleformMovieFunctionVoid();

            // Set the drawing style
            RAGE.Game.Graphics.PushScaleformMovieFunction(scaleformHandle, "DRAW_INSTRUCTIONAL_BUTTONS");
            RAGE.Game.Graphics.PushScaleformMovieFunctionParameterInt(1);
            RAGE.Game.Graphics.PopScaleformMovieFunctionVoid();

            // Play the notification sound
            RAGE.Game.Audio.PlaySoundFrontend(-1, "WAYPOINT_SET", "HUD_FRONTEND_DEFAULT_SOUNDSET", true);
        }

        private void OnPlayerExitColshapeEvent(Colshape colshape, Events.CancelEventArgs cancel)
        {
            // Stop drawing the scaleform
            buttonHandle = null;
        }

        public static void RenderScaleforms()
        {
            if (buttonHandle == null || buttonHandle.Length == 0) return;

            // Render the scaleform
            RAGE.Game.Graphics.DrawScaleformMovieFullscreen(scaleformHandle, 255, 255, 255, 255, 255);
        }
    }
}
