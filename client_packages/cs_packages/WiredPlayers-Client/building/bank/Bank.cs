using RAGE;
using WiredPlayers_Client.globals;
using System;

namespace WiredPlayers_Client.Building.Bank
{
    class BankHandler : Events.Script
    {
        private int money = 0;

        public BankHandler()
        {
            Events.Add("updatePlayerMoney", UpdatePlayerMoneyEvent);
            Events.Add("showATM", ShowATMEvent);
            Events.Add("updateBankAccountMoney", UpdateBankAccountMoneyEvent);
            Events.Add("executeBankOperation", ExecuteBankOperationEvent);
            Events.Add("bankOperationResponse", BankOperationResponseEvent);
            Events.Add("loadPlayerBankBalance", LoadPlayerBankBalanceEvent);
            Events.Add("showPlayerBankBalance", ShowPlayerBankBalanceEvent);
        }

        private void UpdatePlayerMoneyEvent(object[] args)
        {
            // Update the money amount
            Globals.playerMoney = Convert.ToInt32(args[0]);
        }

        private void ShowATMEvent(object[] args)
        {
            // Disable the chat
            Chat.Show(false);

            // Update the bank money
            money = Convert.ToInt32(args[0]);

            // Bank menu creation
            Browser.CreateBrowser("bankMenu.html", "destroyBrowser");
        }

        private void UpdateBankAccountMoneyEvent(object[] args)
        {
            if(args != null && args.Length > 0)
            {
                // Get the bank money from the player
                money = Convert.ToInt32(args[0]);
            }

            // Update the balance on the screen
            Browser.ExecuteFunction("updateAccountMoney", money);
        }

        private void ExecuteBankOperationEvent(object[] args)
        {
            // Get the arguments received
            int operation = Convert.ToInt32(args[0]);
            int amount = Convert.ToInt32(args[1]);
            string target = args[2].ToString();

            // Execute a bank operation
            Events.CallRemote("executeBankOperation", operation, amount, target);
        }

        private void BankOperationResponseEvent(object[] args)
        {
            // Get the arguments received
            string response = args[0].ToString();

            // Check the action taken
            if(response == null || response.Length == 0)
            {
                // Go back on the screen
                Browser.ExecuteFunction("bankBack");
            }
            else
            {
                // Show the error on the operation
                Browser.ExecuteFunction("showOperationError", response);
            }
        }

        private void LoadPlayerBankBalanceEvent(object[] args)
        {
            // Load player's bank balance
            Events.CallRemote("loadPlayerBankBalance");
        }

        private void ShowPlayerBankBalanceEvent(object[] args)
        {
            // Get the arguments received
            string operationJson = args[0].ToString();
            string playerName = args[1].ToString();

            // Show the player's bank operations
            Browser.ExecuteFunction("showBankOperations", operationJson, playerName);
        }
    }
}
