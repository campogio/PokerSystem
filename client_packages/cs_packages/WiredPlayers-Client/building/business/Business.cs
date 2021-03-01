using RAGE;
using WiredPlayers_Client.globals;
using WiredPlayers_Client.model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

namespace WiredPlayers_Client.Building.Business
{
    class Business : Events.Script
    {
        private List<BusinessItem> BusinessItems;

        public Business()
        {
            Events.Add("showBusinessPurchaseMenu", ShowBusinessPurchaseMenuEvent);
            Events.Add("purchaseItem", PurchaseItemEvent);
        }
        
        private void ShowBusinessPurchaseMenuEvent(object[] args)
        {
            // Store the products and price
            BusinessItems = JsonConvert.DeserializeObject<List<BusinessItem>>(args[0].ToString());

            // Bank menu creation
            Browser.CreateBrowser("sideMenu.html", "destroyBrowser", "populateBusinessItems", args[0].ToString(), args[1].ToString(), (float)Convert.ToDouble(args[2]));
        }

        private void PurchaseItemEvent(object[] args)
        {
            // Store the products and price
            int index = Convert.ToInt32(args[0]);
            int amount = Convert.ToInt32(args[1]);

            // Get the purchased item and its cost
            Events.CallRemote("businessPurchaseMade", BusinessItems[index].hash, amount);
        }
    }
}