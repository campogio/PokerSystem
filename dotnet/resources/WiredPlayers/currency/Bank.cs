using GTANetworkAPI;
using System.Collections.Generic;
using System.Threading.Tasks;
using WiredPlayers.character;
using WiredPlayers.Currency;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.help;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Curency
{
    public class Bank : Script
    {
        private const int BankOperationsPerPage = 25;

        public Bank()
        {
            // Add a new ColShape for each ATM
            foreach (Vector3 position in Coordinates.AtmArray)
            {
                // Add a new ColShape
                ColShape colShape = NAPI.ColShape.CreatCircleColShape(position.X, position.Y, 2.0f, 0);
                colShape.SetData(EntityData.ColShapeType, ColShapeTypes.Atm);
                colShape.SetData(EntityData.InstructionalButton, HelpRes.access_atm);
            }
        }
        
        public static void ShowAtmWindow(Player player)
        {
            // Show the ATM window
            player.TriggerEvent("showATM", player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Bank);
        }

        [RemoteEvent("executeBankOperation")]
        public void ExecuteBankOperationEvent(Player player, int operation, int amount, string targetName)
        {
            // Throw an error if the amount is less than zero.
            if (amount <= 0)
            {
                TriggerLocalResponse(player, ErrRes.bank_general_error);
                return;
            }

            // Figure out the action of the bank operation.
            switch (operation)
            {
                case (int)BankOperations.Withdraw:
                    WithdrawFromBank(player, amount);
                    break;
                    
                case (int)BankOperations.Deposit:
                    DepositToBank(player, amount);
                    break;
                    
                case (int)BankOperations.Transfer:
                    TransferFromBank(player, amount, targetName);
                    break;
                    
                case (int)BankOperations.Balance:
                    break;
            }
        }

        [RemoteEvent("loadPlayerBankBalance")]
        public async void LoadPlayerBankBalanceEvent(Player player)
        {
            // Show the bank operations for the player
            List<BankOperationModel> operations = await DatabaseOperations.GetBankOperations(player.Name, 1, BankOperationsPerPage).ConfigureAwait(false);
            player.TriggerEvent("showPlayerBankBalance", NAPI.Util.ToJson(operations), player.Name);
        }

        /// <summary>
        /// Update the player's data for money, bank, etc. Setting money to -99 will not update the money for the target.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="bank"></param>
        /// <param name="money"></param>
        private void UpdatePlayerMoney(Player player, int bank, int money = -99)
        {
            // Update the bank value
            player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Bank = bank;

            if (money == -99)
                return;

            Money.SetPlayerMoney(player, money);

            // Update the visual amount of money
            player.TriggerEvent("updateBankAccountMoney", bank);
        }

        /// <summary>
        /// Trigger a client event that tells the player what happened.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="response"></param>
        private void TriggerLocalResponse(Player player, string response)
        {
            player.TriggerEvent("bankOperationResponse", response);
        }

        /// <summary>
        /// Withdraws from the bank based on the player's input.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="bank"></param>
        /// <param name="amount"></param>
        private void WithdrawFromBank(Player player, int amount)
        {
            // Get the character model from the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // If the bank has less than the amount requested. Throw an error.
            if (characterModel.Bank < amount)
            {
                TriggerLocalResponse(player, ErrRes.bank_not_enough_money);
                return;
            }

            // Update data and then push it to the player's shared data.
            characterModel.Bank -= amount;
            characterModel.Money += amount;
            UpdatePlayerMoney(player, characterModel.Bank, characterModel.Money);

            // Log the transaction to the DatabaseHandler.
            Task.Run(() => DatabaseOperations.LogPayment("ATM", characterModel.RealName, GenRes.bank_op_withdraw, amount)).ConfigureAwait(false);
        }

        /// <summary>
        /// Deposit money into the bank based on exceeding amount or whatever amount the player provides.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="amount"></param>
        private void DepositToBank(Player player, int amount)
        {
            // Get the character model from the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // If the player's money is less than the amount he wants to deposit. Just move all of the player's money into the bank.
            if (characterModel.Money < amount)
            {
                characterModel.Bank += characterModel.Money;
                UpdatePlayerMoney(player, characterModel.Bank, characterModel.Money);

                // We log the transaction into the database
                Task.Run(() => DatabaseOperations.LogPayment(characterModel.RealName, "ATM", GenRes.bank_op_deposit, characterModel.Money)).ConfigureAwait(false);
                return;
            }

            // If the amount is less than the player's money deposit the amount instead.
            characterModel.Bank += amount;
            characterModel.Money -= amount;
            UpdatePlayerMoney(player, characterModel.Bank, characterModel.Money);

            // We log the transaction into the database
            Task.Run(() => DatabaseOperations.LogPayment("ATM", characterModel.RealName, GenRes.bank_op_deposit, amount)).ConfigureAwait(false);
        }

        private void TransferFromBank(Player player, int amount, string targetPlayer)
        {
            // Get the character model from the player
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // If the bank has less than the amount requested. End it here.
            if (characterModel.Bank < amount)
            {
                TriggerLocalResponse(player, ErrRes.bank_not_enough_money);
                return;
            }
            
            // Check if the account exists before we begin transferring.
            if (DatabaseOperations.FindCharacter(targetPlayer) != true)
            {
                TriggerLocalResponse(player, ErrRes.bank_account_not_found);
                return;
            }

            Player target = NAPI.Pools.GetAllPlayers().Find(x => x.Name == targetPlayer);

            // If the target player is the client transferring stop it.
            if (target == player)
            {
                TriggerLocalResponse(player, ErrRes.transfer_money_own);
                return;
            }

            // If the target is null they're probably not on the server currently.
            if (target == null)
            {
                characterModel.Bank -= amount;
                UpdatePlayerMoney(player, characterModel.Bank, characterModel.Money);
                Task.Run(() =>
                {
                    DatabaseOperations.TransferMoneyToPlayer(targetPlayer, amount);
                    DatabaseOperations.LogPayment(characterModel.RealName, targetPlayer, GenRes.bank_op_transfer, amount);
                }).ConfigureAwait(false);
                return;
            }

            if (Character.IsPlaying(target))
            {
                target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Bank += amount;
                characterModel.Bank -= amount;

                UpdatePlayerMoney(player, characterModel.Bank, characterModel.Money);
                UpdatePlayerMoney(target, target.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Bank);
            }
        }
    }
}
