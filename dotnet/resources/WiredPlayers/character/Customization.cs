using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.Server;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.character
{
    public class Customization
    {
        public static List<ClothesModel> clothesList;
        public static List<TattooModel> tattooList;

        public static void ApplyPlayerCustomization(Player player, SkinModel skinModel, Sex sex)
        {
            // Populate the head
            HeadBlend headBlend = new HeadBlend
            {
                ShapeFirst = Convert.ToByte(skinModel.firstHeadShape),
                ShapeSecond = Convert.ToByte(skinModel.secondHeadShape),
                SkinFirst = Convert.ToByte(skinModel.firstSkinTone),
                SkinSecond = Convert.ToByte(skinModel.secondSkinTone),
                ShapeMix = skinModel.headMix,
                SkinMix = skinModel.skinMix
            };

            // Get the hair and eyes colors
            byte eyeColor = Convert.ToByte(skinModel.eyesColor);
            byte hairColor = Convert.ToByte(skinModel.firstHairColor);
            byte hightlightColor = Convert.ToByte(skinModel.secondHairColor);

            // Add the face features
            float[] faceFeatures = new float[]
            {
                skinModel.noseWidth, skinModel.noseHeight, skinModel.noseLength, skinModel.noseBridge, skinModel.noseTip, skinModel.noseShift, skinModel.browHeight,
                skinModel.browWidth, skinModel.cheekboneHeight, skinModel.cheekboneWidth, skinModel.cheeksWidth, skinModel.eyes, skinModel.lips, skinModel.jawWidth,
                skinModel.jawHeight, skinModel.chinLength, skinModel.chinPosition, skinModel.chinWidth, skinModel.chinShape, skinModel.neckWidth
            };

            // Populate the head overlays
            Dictionary<int, HeadOverlay> headOverlays = new Dictionary<int, HeadOverlay>();

            for (int i = 0; i < Constants.MAX_HEAD_OVERLAYS; i++)
            {
                // Get the overlay model and color
                int[] overlayData = GetOverlayData(skinModel, i);

                // Create the overlay
                HeadOverlay headOverlay = new HeadOverlay
                {
                    Index = Convert.ToByte(overlayData[0]),
                    Color = Convert.ToByte(overlayData[1]),
                    SecondaryColor = 0,
                    Opacity = 1.0f
                };

                // Add the overlay
                headOverlays[i] = headOverlay;
            }

            // Update the character's skin
            player.SetCustomization(sex == Sex.Male, headBlend, eyeColor, hairColor, hightlightColor, faceFeatures, headOverlays, new Decoration[] { });
            player.SetClothes(2, skinModel.hairModel, 0);
        }

        public static void ApplyPlayerClothes(Player player)
        {
            // Get the player's identifier
            int playerId = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;

            foreach (ClothesModel clothes in clothesList)
            {
                if (clothes.player != playerId || !clothes.dressed) continue;

                if (clothes.type == 0)
                {
                    player.SetClothes(clothes.slot, clothes.drawable, clothes.texture);
                }
                else
                {
                    player.SetAccessories(clothes.slot, clothes.drawable, clothes.texture);
                }
            }
        }

        public static void ApplyPlayerTattoos(Player player)
        {
            // Get the tattoos from the player
            int playerId = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
            TattooModel[] playerTattoos = tattooList.Where(t => t.player == playerId).ToArray();

            foreach (TattooModel tattoo in playerTattoos)
            {
                // Add each tattoo to the player
                Decoration decoration = new Decoration
                {
                    Collection = NAPI.Util.GetHashKey(tattoo.library),
                    Overlay = NAPI.Util.GetHashKey(tattoo.hash)
                };

                player.SetDecoration(decoration);
            }
        }

        public static void RemovePlayerTattoos(Player player)
        {
            // Check if the player has been registered
            if (!player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Playing) return;

            // Get the tattoos from the player
            int playerId = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
            TattooModel[] playerTattoos = tattooList.Where(t => t.player == playerId).ToArray();

            foreach (TattooModel tattoo in playerTattoos)
            {
                // Add each tattoo to the player
                Decoration decoration = new Decoration
                {
                    Collection = NAPI.Util.GetHashKey(tattoo.library),
                    Overlay = NAPI.Util.GetHashKey(tattoo.hash)
                };

                player.RemoveDecoration(decoration);
            }
        }

        private static int[] GetOverlayData(SkinModel skinModel, int index)
        {
            int[] overlayData = new int[2];

            switch (index)
            {
                case 0:
                    overlayData[0] = skinModel.blemishesModel;
                    overlayData[1] = 0;
                    break;
                case 1:
                    overlayData[0] = skinModel.beardModel;
                    overlayData[1] = skinModel.beardColor;
                    break;
                case 2:
                    overlayData[0] = skinModel.eyebrowsModel;
                    overlayData[1] = skinModel.eyebrowsColor;
                    break;
                case 3:
                    overlayData[0] = skinModel.ageingModel;
                    overlayData[1] = 0;
                    break;
                case 4:
                    overlayData[0] = skinModel.makeupModel;
                    overlayData[1] = 0;
                    break;
                case 5:
                    overlayData[0] = skinModel.blushModel;
                    overlayData[1] = skinModel.blushColor;
                    break;
                case 6:
                    overlayData[0] = skinModel.complexionModel;
                    overlayData[1] = 0;
                    break;
                case 7:
                    overlayData[0] = skinModel.sundamageModel;
                    overlayData[1] = 0;
                    break;
                case 8:
                    overlayData[0] = skinModel.lipstickModel;
                    overlayData[1] = skinModel.lipstickColor;
                    break;
                case 9:
                    overlayData[0] = skinModel.frecklesModel;
                    overlayData[1] = 0;
                    break;
                case 10:
                    overlayData[0] = skinModel.chestModel;
                    overlayData[1] = skinModel.chestColor;
                    break;
            }

            return overlayData;
        }

        public static List<ClothesModel> GetPlayerClothes(int playerId)
        {
            // Get a list with the player's clothes
            return clothesList.Where(c => c.player == playerId).ToList();
        }

        public static ClothesModel GetDressedClothesInSlot(int playerId, int type, int slot)
        {
            // Get the clothes in the selected slot
            return clothesList.FirstOrDefault(c => c.player == playerId && c.type == type && c.slot == slot && c.dressed);
        }

        public static List<string> GetClothesNames(ClothesModel[] clothesArray)
        {
            List<string> clothesNames = new List<string>();
            foreach (ClothesModel clothes in clothesArray)
            {
                foreach (BusinessClothesModel businessClothes in Constants.BUSINESS_CLOTHES_LIST)
                {
                    if (businessClothes.clothesId == clothes.drawable && (int)Convert.ChangeType(businessClothes.BodyPart, businessClothes.BodyPart.GetTypeCode()) == clothes.slot && businessClothes.type == clothes.type)
                    {
                        clothesNames.Add(businessClothes.description);
                        break;
                    }
                }
            }
            return clothesNames;
        }

        public static void SetDefaultClothes(Player player)
        {
            // Get the clothes list
            Dictionary<int, ComponentVariation> clothes = new Dictionary<int, ComponentVariation>()
            {
                { (int)ClothesSlots.Mask, new ComponentVariation(0, 0) },
                { (int)ClothesSlots.Torso, new ComponentVariation(0, 0) },
                { (int)ClothesSlots.Legs, new ComponentVariation(0, 0) },
                { (int)ClothesSlots.Bag, new ComponentVariation(0, 0) },
                { (int)ClothesSlots.Feet, new ComponentVariation(0, 0) },
                { (int)ClothesSlots.Accessories, new ComponentVariation(0, 0) },
                { (int)ClothesSlots.Undershirt, new ComponentVariation(0, 0) },
                { (int)ClothesSlots.Armor, new ComponentVariation(0, 0) },
                { (int)ClothesSlots.Decal, new ComponentVariation(0, 0) },
                { (int)ClothesSlots.Top, new ComponentVariation(0, 0) }
            };

            // Set the default clothes for the player
            player.SetClothes(clothes);
        }
        
        public static void RemovePlayerClothes(Player player, Sex playerSex, bool removePants)
        {
            player.SetClothes(11, 15, 0);
            player.SetClothes(3, 15, 0);
            player.SetClothes(8, 15, 0);
            
            if (!removePants) return;

            if (playerSex == Sex.Male)
            {
                player.SetClothes(4, 61, 0);
                player.SetClothes(6, 34, 0);
            }
            else
            {
                player.SetClothes(4, 15, 0);
                player.SetClothes(6, 35, 0);
            }
        }

        public static void UndressClothes(int playerId, int type, int slot, bool store = false)
        {
            // Get the clothes in the selected slot
            ClothesModel clothes = clothesList.FirstOrDefault(c => c.player == playerId && c.type == type && c.slot == slot && c.dressed);

            if (clothes != null)
            {
                clothes.dressed = false;
                clothes.Stored = store;

                // Update the clothes' state
                Task.Run(() => DatabaseOperations.UpdateClothes(clothes)).ConfigureAwait(false);
            }
        }

        public static bool IsCustomCharacter(Player player)
        {
            return (PedHash)player.Model == PedHash.FreemodeMale01 || (PedHash)player.Model == PedHash.FreemodeFemale01;
        }

        [RemoteEvent("getPlayerTattoos")]
        public void GetPlayerTattoosRemoteEvent(Player player, Player targetPlayer)
        {
            int targetId = targetPlayer.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Id;
            List<TattooModel> playerTattooList = tattooList.Where(t => t.player == targetId).ToList();

            player.TriggerEvent("updatePlayerTattoos", NAPI.Util.ToJson(playerTattooList), targetPlayer);
        }

        [RemoteEvent("loadCharacterClothes")]
        public static void LoadCharacterClothesEvent(Player player)
        {
            // Initialize player clothes
            SetDefaultClothes(player);

            // Generate player's clothes
            ApplyPlayerClothes(player);
        }
    }
}
