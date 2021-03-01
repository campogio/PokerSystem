using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.Buildings.Businesses;
using WiredPlayers.character;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.error;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using WiredPlayers.vehicles;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.jobs
{
    class Mechanic : Script
    {
        public static List<TunningModel> tunningList;

        public static void AddTunningToVehicle(int vehicleId, Vehicle vehicle)
        {
            foreach (TunningModel tunning in tunningList)
            {
                if (tunning.vehicle == vehicleId)
                {
                    vehicle.SetMod(tunning.slot, tunning.component);
                }
            }
        }

        public static bool PlayerInValidRepairPlace(Player player)
        {
            // Check if the player is in any workshop
            foreach (BusinessModel business in Business.BusinessCollection.Values)
            {
                if ((BusinessTypes)business.Ipl.Type == BusinessTypes.Mechanic && player.Position.DistanceTo(business.Entrance) < 25.0f)
                {
                    return true;
                }
            }

            // Check if the player has a towtruck near
            foreach (Vehicle vehicle in NAPI.Pools.GetAllVehicles())
            {
                VehicleHash vehicleHash = (VehicleHash)vehicle.Model;
                if (vehicleHash == VehicleHash.Towtruck || vehicleHash == VehicleHash.Towtruck2)
                {
                    return true;
                }
            }

            return false;
        }
        
        public static void RepaintVehicle(Vehicle vehicle, VehicleModel vehModel)
        {
            if (vehModel.ColorType == Constants.VEHICLE_COLOR_TYPE_PREDEFINED)
            {
                vehicle.PrimaryColor = int.Parse(vehModel.FirstColor);
                vehicle.SecondaryColor = int.Parse(vehModel.SecondColor);
                vehicle.PearlescentColor = vehModel.Pearlescent;
            }
            else
            {
                int[] firstColorArray = Array.ConvertAll(vehModel.FirstColor.Split(','), int.Parse);
                int[] secondColorArray = Array.ConvertAll(vehModel.SecondColor.Split(','), int.Parse);
                vehicle.CustomPrimaryColor = new Color(firstColorArray[0], firstColorArray[1], firstColorArray[2]);
                vehicle.CustomSecondaryColor = new Color(secondColorArray[0], secondColorArray[1], secondColorArray[2]);
            }
        }

        private int GetVehicleTunningComponent(int vehicleId, int slot)
        {
            // Get the component on the specified slot
            TunningModel tunning = tunningList.FirstOrDefault(tunningModel => tunningModel.vehicle == vehicleId && tunningModel.slot == slot);

            return tunning?.component ?? 255;
        }

        [RemoteEvent("RepaintVehicle")]
        public void RepaintVehicleEvent(Player player, int colorType, string firstColor, string secondColor, int pearlescentColor, int vehiclePaid)
        {
            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            switch (colorType)
            {
                case 0:
                    // Predefined color
                    data.LastVehicle.PrimaryColor = int.Parse(firstColor);
                    data.LastVehicle.SecondaryColor = int.Parse(secondColor);
                    
                    if (pearlescentColor >= 0)
                    {
                        data.LastVehicle.PearlescentColor = pearlescentColor;
                    }
                    break;
                case 1:
                    // Custom color
                    string[] firstColorArray = firstColor.Split(',');
                    string[] secondColorArray = secondColor.Split(',');
                    data.LastVehicle.CustomPrimaryColor = new Color(int.Parse(firstColorArray[0]), int.Parse(firstColorArray[1]), int.Parse(firstColorArray[2]));
                    data.LastVehicle.CustomSecondaryColor = new Color(int.Parse(secondColorArray[0]), int.Parse(secondColorArray[1]), int.Parse(secondColorArray[2]));
                    break;
            }
            
            if (vehiclePaid > 0)
            {
                // Check for the product amount
                int playerId = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
                ItemModel item = Inventory.GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_BUSINESS_PRODUCTS);

                if (item == null || item.amount < Constants.RepaintProducts)
                {
                    // The player doesn't have the required product amount
                    player.SendChatMessage(Constants.COLOR_ERROR + string.Format(ErrRes.not_required_products, Constants.RepaintProducts));
                    return;
                }

                // Get all the players who have keys for the vehicle
                List<Player> vehicleOwners = NAPI.Pools.GetAllPlayers().Where(p => Vehicles.HasPlayerVehicleKeys(p, data.LastVehicle, false)).ToList();

                // Search for a player with vehicle keys
                foreach (Player target in vehicleOwners)
                {
                    if (target.Position.DistanceTo(player.Position) < 4.0f)
                    {
                        // Get the temporary data
                        PlayerTemporaryModel targetData = target.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

                        // Vehicle repaint data
                        targetData.JobPartner = player;
                        targetData.Repaint = new RepaintModel()
                        {
                            Vehicle = data.LastVehicle,
                            ColorType = colorType,
                            FirstColor = firstColor,
                            SecondColor = secondColor,
                            Pearlescent = pearlescentColor
                        };
                        targetData.SellingPrice = vehiclePaid;
                        targetData.SellingProducts = Constants.RepaintProducts;

                        player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.mechanic_repaint_offer, target.Name, vehiclePaid));
                        target.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.mechanic_repaint_accept, player.Name, vehiclePaid));
                        return;
                    }
                }

                // There's no player with vehicle's keys near
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_too_far);
            }
        }

        [RemoteEvent("cancelVehicleRepaint")]
        public void CancelVehicleRepaintEvent(Player player)
        {
            // Get player's vehicle
            Vehicle vehicle = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).LastVehicle;
            VehicleModel vehModel = Vehicles.GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId));

            // Repaint the vehicle
            RepaintVehicle(vehicle, vehModel);
        }

        [RemoteEvent("modifyVehicle")]
        public void ModifyVehicleEvent(Player player, int slot, int component)
        {
            Vehicle vehicle = player.Vehicle;

            if (component > 0)
            {
                vehicle.SetMod(slot, component);
            }
            else
            {
                vehicle.RemoveMod(slot);
            }
        }

        [RemoteEvent("cancelVehicleModification")]
        public void CancelVehicleModificationEvent(Player player)
        {
            int vehicleId = player.Vehicle.GetData<int>(EntityData.VehicleId);
            
            for (int i = 0; i < 49; i++)
            {
                // Get the component in the slot
                int component = GetVehicleTunningComponent(vehicleId, i);

                // Remove or add the tunning part
                player.Vehicle.SetMod(i, component);
            }
        }

        [RemoteEvent("confirmVehicleModification")]
        public async void ConfirmVehicleModificationEvent(Player player, int slot, int mod)
        {
            // Get the vehicle's id
            int vehicleId = player.Vehicle.GetData<int>(EntityData.VehicleId);

            // Get player's product amount
            int playerId = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
            ItemModel item = Inventory.GetPlayerItemModelFromHash(playerId, Constants.ITEM_HASH_BUSINESS_PRODUCTS);

            // Calculate the cost for the tunning
            int totalProducts = Constants.TuningPricesCollection[(VehicleModSlot)slot];

            if (item == null || item.amount < totalProducts)
            {
                // Send the error message
                player.SendChatMessage(Constants.COLOR_ERROR + string.Format(ErrRes.not_required_products, totalProducts));
                return;
            }

            // Remove any component in the slot
            TunningModel component = tunningList.FirstOrDefault(c => c.slot == slot && c.vehicle == vehicleId);

            if (component != null)
            {
                _ = Task.Run(() => DatabaseOperations.DeleteSingleRow("tunning", "id", component.id)).ConfigureAwait(false);
                tunningList.Remove(component);
            }

            if (mod != 0)
            {
                // Create the new component
                component = new TunningModel()
                {
                    slot = slot,
                    component = mod,
                    vehicle = vehicleId
                };

                // Add component to database
                component.id = await DatabaseOperations.AddTunning(component);
                tunningList.Add(component);
            }

            // Remove consumed products
            item.amount -= totalProducts;

            // Update the amount into the database
            _ = Task.Run(() => DatabaseOperations.UpdateItem(item)).ConfigureAwait(false);

            // Confirmation message
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.vehicle_tunning);
        }
    }
}
