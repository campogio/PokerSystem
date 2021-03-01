using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.messages.error;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.character
{
    public class Animations : Script
    {
        public static Dictionary<int, CategoryModel> AnimationGroup;

        public static void AddAnimations()
        {
            // Load the animation list
            Dictionary<int, AnimationModel> animations = DatabaseOperations.LoadAnimationList();

            foreach (CategoryModel category in AnimationGroup.Values)
            {
                // Get all the animations from the category
                category.Animations = animations.Where(a => a.Value.Category == category.Id).ToDictionary(i => i.Key, i => i.Value);
            }
        }

        [RemoteEvent("ShowAnimationCategories")]
        private void ShowAnimationCategoriesRemoteEvent(Player player)
        {
            if (player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).KilledBy != 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_is_dead);
                return;
            }

            // Show the animations on the menu
            player.TriggerEvent("ShowAnimationCategories", AnimationGroup.Values.Select(a => a.Description).ToList());
        }
        
        [RemoteEvent("LoadCategoryAnimations")]
        private void LoadCategoryAnimationsRemoteEvent(Player player, int index)
        {
            // Get the object matching the category
            List<AnimationModel> animations = AnimationGroup.Values.ElementAt(index).Animations.Values.ToList();
            
            // Show the animations in the menu
            player.TriggerEvent("ShowAnimationList", animations);
        }
    }
}
