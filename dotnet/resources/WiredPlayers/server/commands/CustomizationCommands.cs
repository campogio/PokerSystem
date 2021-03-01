using GTANetworkAPI;
using WiredPlayers.Utility;
using WiredPlayers.Data.Persistent;
using WiredPlayers.messages.error;
using WiredPlayers.messages.arguments;
using WiredPlayers.messages.help;
using WiredPlayers.character;
using System.Linq;
using System;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Server.Commands
{
    public static class CustomizationCommands
    {
        [Command]
        public static void ComplementCommand(Player player, string type, string action)
        {
            if (!action.Equals(ArgRes.wear, StringComparison.InvariantCultureIgnoreCase) && !action.Equals(ArgRes.remove, StringComparison.InvariantCultureIgnoreCase))
            {
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.complement);
                return;
            }

            // Get the variables
            int clothesType = -1;
            int slot = -1;
            int defaultDrawable = -1;

            // Get the error messages
            string clothesNotBoughtError = string.Empty;
            string clothesEquipedError = string.Empty;
            string clothesNotEquipedError = string.Empty;

            // Get the player's character model
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            if (type.Equals(ArgRes.mask, StringComparison.InvariantCultureIgnoreCase))
            {
                clothesType = 0;
                slot = (int)ClothesSlots.Mask;
                defaultDrawable = 0;

                clothesNotBoughtError = ErrRes.no_mask_bought;
                clothesEquipedError = ErrRes.mask_equiped;
                clothesNotEquipedError = ErrRes.no_mask_equiped;
            }
            else if (type.Equals(ArgRes.bag, StringComparison.InvariantCultureIgnoreCase))
            {
                clothesType = 0;
                slot = (int)ClothesSlots.Bag;
                defaultDrawable = 0;

                clothesNotBoughtError = ErrRes.no_bag_bought;
                clothesEquipedError = ErrRes.bag_equiped;
                clothesNotEquipedError = ErrRes.no_bag_equiped;
            }
            else if (type.Equals(ArgRes.accessory, StringComparison.InvariantCultureIgnoreCase))
            {
                clothesType = 0;
                slot = (int)ClothesSlots.Accessories;
                defaultDrawable = 0;

                clothesNotBoughtError = ErrRes.no_accessory_bought;
                clothesEquipedError = ErrRes.accessory_equiped;
                clothesNotEquipedError = ErrRes.no_accessory_equiped;
            }
            else if (type.Equals(ArgRes.hat, StringComparison.InvariantCultureIgnoreCase))
            {
                clothesType = 1;
                slot = (int)AccessorySlots.Hat;
                defaultDrawable = characterModel.Sex == Sex.Female ? 57 : 8;

                clothesNotBoughtError = ErrRes.no_hat_bought;
                clothesEquipedError = ErrRes.hat_equiped;
                clothesNotEquipedError = ErrRes.no_hat_equiped;
            }
            else if (type.Equals(ArgRes.glasses, StringComparison.InvariantCultureIgnoreCase))
            {
                clothesType = 1;
                slot = (int)AccessorySlots.Glasses;
                defaultDrawable = characterModel.Sex == Sex.Female ? 5 : 0;

                clothesNotBoughtError = ErrRes.no_glasses_bought;
                clothesEquipedError = ErrRes.glasses_equiped;
                clothesNotEquipedError = ErrRes.no_glasses_equiped;
            }
            else if (type.Equals(ArgRes.earrings, StringComparison.InvariantCultureIgnoreCase))
            {
                clothesType = 1;
                slot = (int)AccessorySlots.Ears;
                defaultDrawable = characterModel.Sex == Sex.Female ? 12 : 3;

                clothesNotBoughtError = ErrRes.no_ear_bought;
                clothesEquipedError = ErrRes.ear_equiped;
                clothesNotEquipedError = ErrRes.no_ear_equiped;
            }
            else
            {   
                // There was no matching option
                player.SendChatMessage(Constants.COLOR_HELP + HelpRes.complement);
                return;
            }

            // Check if the player has any clothes in the slot
            ClothesModel clothes = Customization.GetPlayerClothes(characterModel.Id).First(c => c.slot == slot && c.type == clothesType && !c.Stored);

            if (clothes == null)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + clothesNotBoughtError);
                return;
            }

            if (action.Equals(ArgRes.wear, StringComparison.InvariantCultureIgnoreCase))
            {
                if (clothes.dressed)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + clothesEquipedError);
                    return;
                }

                if (clothesType == 0)
                {
                    player.SetClothes(clothes.slot, clothes.drawable, clothes.texture);
                }
                else
                {
                    player.SetAccessories(clothes.slot, clothes.drawable, clothes.texture);
                }

                // Dress the new clothes
                clothes.dressed = !clothes.dressed;
            }
            else
            {
                if (!clothes.dressed)
                {
                    player.SendChatMessage(Constants.COLOR_ERROR + clothesNotEquipedError);
                    return;
                }

                if (clothesType == 0)
                {
                    player.SetClothes(slot, defaultDrawable, 0);
                }
                else
                {
                    player.SetAccessories(slot, defaultDrawable, 0);
                }

                // Undress the clothes
                Customization.UndressClothes(characterModel.Id, clothesType, slot);
            }
        }
    }
}
