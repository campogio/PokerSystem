using RAGE;
using RAGE.Ui;
using RAGE.Elements;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WiredPlayers_Client.Building.House
{
    class Furniture : Events.Script
    {
        public static MapObject SelectedFurniture;

        private static bool MovingFurniture;
        private static List<MapObject> FurnitureCollection;

        public Furniture()
        {
            Events.Add("MoveFurniture", MoveFurnitureEvent);
        }

        private void MoveFurnitureEvent(object[] args)
        {
            // Initialize the collection containing the furniture
            FurnitureCollection = new List<MapObject>();

            // Get the placed furniture identifiers
            ushort[] furnitureIds = JsonConvert.DeserializeObject<ushort[]>(args[0].ToString());

            foreach (ushort fId in furnitureIds)
            {
                // Add the object to the list
                FurnitureCollection.Add(Entities.Objects.GetAtRemote(fId));
            }

            // Enable the cursor and disable the chat
            Cursor.Visible = true;
            Chat.Show(false);

            // Set the flag for moving furniture
            MovingFurniture = true;
        }

        public static void CheckFurnitureClicked(bool pushed)
        {
            // Check if the player can move the furniture
            if (!MovingFurniture) return;

            // Check if the player clicked on any furniture
            if (SelectedFurniture == null && pushed)
            {
                SelectedFurniture = GetClickedFurniture(Cursor.Position.X, Cursor.Position.Y);
                return;
            }

            // Check if the player stopped holding the furniture
            if (SelectedFurniture != null && !pushed)
            {
                SelectedFurniture = null;
                return;
            }
        }

        private static MapObject GetClickedFurniture(float pointerX, float pointerY)
        {
            // Get the screen resolution
            int resX = 0, resY = 0;
            RAGE.Game.Graphics.GetActiveScreenResolution(ref resX, ref resY);

            // Get the clicked Vector3
            Vector3 clickedPosition = new Vector3(pointerX / resX, pointerY / resY, 0.0f);

            foreach (MapObject furniture in FurnitureCollection)
            {
                // Get the screen coordinates for the object
                float screenX = 0.0f, screenY = 0.0f;
                RAGE.Game.Graphics.GetScreenCoordFromWorldCoord(furniture.Position.X, furniture.Position.Y, furniture.Position.Z, ref screenX, ref screenY);

                // Check if the object has been clicked
                if (clickedPosition.DistanceTo2D(new Vector3(screenX, screenY, 0.0f)) < 0.1f) return furniture;
            }

            return null;
        }
    }
}
