using Newtonsoft.Json;
using RAGE;
using RAGE.Elements;
using System;
using WiredPlayers_Client.globals;
using WiredPlayers_Client.model;

namespace WiredPlayers_Client.character
{
    public class Animations : Events.Script
    {
        private string[] Categories;
        private AnimationModel[] AnimationCollection;

        public Animations()
        {
            Events.Add("ShowAnimationCategories", ShowAnimationCategoriesEvent);
            Events.Add("AnimationCategorySelected", AnimationCategorySelectedEvent);
            Events.Add("ShowAnimationList", ShowAnimationListEvent);
            Events.Add("RunAnimation", RunAnimationEvent);
        }

        private void ShowAnimationCategoriesEvent(object[] args)
        {
            if (args != null && args.Length > 0)
            {
                // Get the animation categories
                Categories = JsonConvert.DeserializeObject<string[]>(args[0].ToString());

                // Show the list
                Browser.CreateBrowser("sideMenu.html", "destroyBrowser", "loadAnimationCategories", args[0].ToString());
            }
            else
            {
                // Animation categories are loaded, show them
                Browser.ExecuteFunction("loadAnimationCategories", JsonConvert.SerializeObject(Categories));
            }
        }

        private void AnimationCategorySelectedEvent(object[] args)
        {
            // Get the animation index
            int index = Convert.ToInt32(args[0]);

            // Show the animations from the category
            Events.CallRemote("LoadCategoryAnimations", index);
        }

        private void ShowAnimationListEvent(object[] args)
        {
            // Store the animation list
            AnimationCollection = JsonConvert.DeserializeObject<AnimationModel[]>(args[0].ToString());

            // Show the animations given the category
            Browser.ExecuteFunction("showAnimations", args[0].ToString());
        }

        private void RunAnimationEvent(object[] args)
        {
            // Get the animation given the index
            AnimationModel anim = AnimationCollection[Convert.ToInt32(args[0])];

            // Load the animation dictionary
            RAGE.Game.Streaming.RequestAnimDict(anim.Library);
            RAGE.Game.Invoker.Wait(150);

            // Play the animation
            Player.LocalPlayer.TaskPlayAnim(anim.Library, anim.Name, 8.0f, 1.0f, -1, anim.Flag, 0.0f, false, false, false);
        }
    }
}
