using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.help;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Buildings
{
    public class GenericInterior : Script
    {
        public static Dictionary<int, InteriorModel> InteriorCollection;

        public static void GenerateEntrancePoints()
        {
            foreach (InteriorModel interiorModel in InteriorCollection.Values)
            {
                // Create the textlabel
                interiorModel.Label = NAPI.TextLabel.CreateTextLabel(interiorModel.Caption, interiorModel.Entrance, 20.0f, 0.75f, 4, new Color(255, 255, 255), false, interiorModel.Dimension);

                // Create the blip if exists
                if (interiorModel.BlipSprite > 0)
                {
                    interiorModel.Icon = NAPI.Blip.CreateBlip(interiorModel.BlipSprite, interiorModel.Entrance, 1.0f, 0, interiorModel.Caption);
                    interiorModel.Icon.ShortRange = true;
                }

                // Create the ColShape
                interiorModel.ColShape = NAPI.ColShape.CreateCylinderColShape(interiorModel.Entrance, 2.5f, 1.0f);
                interiorModel.ColShape.SetData(EntityData.ColShapeId, interiorModel.Id);
                interiorModel.ColShape.SetData(EntityData.ColShapeType, ColShapeTypes.InteriorEntrance);
                interiorModel.ColShape.SetData(EntityData.InstructionalButton, HelpRes.enter_building);
            }
        }

        public static void HandleAction(Player player, int interior)
        {
			switch (interior)
			{
				case (int)InteriorTypes.TownHall:
					player.TriggerEvent("showTownHallMenu");
					break;
			}
        }

        public static List<Vector3> GetExitPoints(int type)
        {
            List<Vector3> exitPoints = new List<Vector3>();

            // Add all the exit points to the list
            InteriorCollection.Values.Where(i => i.Ipl.Type is InteriorTypes intType && (int)intType == type).ToList().ForEach(i => exitPoints.Add(i.Ipl.ExitPoint));
			
            return exitPoints;
        }

        public static InteriorModel GetInteriorById(int interiorId)
        {
            // Get the interior given the identifier
            return InteriorCollection.ContainsKey(interiorId) ? InteriorCollection[interiorId] : null;
        }

        public static InteriorModel GetClosestInterior(Player player, float distance = 2.5f)
        {
            // Check if the collection is empty
            if (InteriorCollection.Count == 0) return null;

            // Get the closest interior given the distance
            return InteriorCollection.Values.OrderBy(i => player.Position.DistanceTo2D(i.Entrance)).FirstOrDefault(i => player.Position.DistanceTo2D(i.Entrance) <= distance);
        }
    }
}
