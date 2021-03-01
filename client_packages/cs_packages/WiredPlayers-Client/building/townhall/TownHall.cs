using RAGE;
using System;
using Newtonsoft.Json;
using WiredPlayers_Client.globals;

namespace WiredPlayers_Client.Building.TownHall
{
    class TownHall : Events.Script
    {
        public TownHall()
        {
            Events.Add("showTownHallMenu", ShowTownHallMenuEvent);
            Events.Add("executeTownHallOperation", ExecuteTownHallOperationEvent);
            Events.Add("showPlayerFineList", ShowPlayerFineListEvent);
            Events.Add("payPlayerFines", PayPlayerFinesEvent);
            Events.Add("backTownHallIndex", BackTownHallIndexEvent);
        }

        private void ShowTownHallMenuEvent(object[] args)
        {
            // Show the Town Hall's menu
            Browser.CreateBrowser("sideMenu.html", "destroyBrowser", "populateTownHallMenu", JsonConvert.SerializeObject(Constants.TownhallProcedures));
        }

        private void ExecuteTownHallOperationEvent(object[] args)
        {
            // Get the variables from the array
            int selectedOption = Convert.ToInt32(args[0]);

            // Execute the selected procedure
            Events.CallRemote("documentOptionSelected", selectedOption);
        }

        private void ShowPlayerFineListEvent(object[] args)
        {
            // Show fines menu
            Browser.ExecuteFunction("populateFinesMenu", args[0].ToString());
        }

        private void PayPlayerFinesEvent(object[] args)
        {
            // Pay the selected fines
            Events.CallRemote("payPlayerFines", args[0].ToString());
        }

        private void BackTownHallIndexEvent(object[] args)
        {
            // Show the Town Hall's menu
            Browser.ExecuteFunction("populateTownHallMenu", JsonConvert.SerializeObject(Constants.TownhallProcedures));
        }
    }
}
