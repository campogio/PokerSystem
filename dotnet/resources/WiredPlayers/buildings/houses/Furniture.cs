using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using WiredPlayers.Data.Persistent;

namespace WiredPlayers.Buildings.Houses
{
    public static class Furniture
    {
        public static Dictionary<int, FurnitureModel> FurnitureCollection;

        public static void GenerateIngameFurniture()
        {
            foreach (FurnitureModel furnitureModel in FurnitureCollection.Values)
            {
                furnitureModel.Handle = NAPI.Object.CreateObject(furnitureModel.Hash, furnitureModel.Position, furnitureModel.Rotation, (byte)furnitureModel.House);
            }
        }

        public static ushort[] GetFurnitureInHouse(int houseId)
        {
            // Get all the furniture in the specified house
            return FurnitureCollection.Values.Where(f => f.House == houseId && f.Handle != null && f.Handle.Exists).Select(f => f.Handle.Id).ToArray();
        }

        public static FurnitureModel GetFurnitureById(int id)
        {
            // Get the furniture given the specific identifier
            return FurnitureCollection.Values.FirstOrDefault(furniture => furniture.Id == id);
        }

        [RemoteEvent("UpdateFurniturePosition")]
        private static void UpdateFurniturePositionEvent(Player player, int furnitureId, Vector3 position)
        {
            player.SendChatMessage("asdasd");
            // Get the furniture given the id
            Object obj = NAPI.Pools.GetAllObjects().First(o => o.Id == furnitureId);
            FurnitureModel furniture = FurnitureCollection.Values.First(f => f.Handle == obj);

            // Update the furniture position
            furniture.Position = position;
        }
    }
}
