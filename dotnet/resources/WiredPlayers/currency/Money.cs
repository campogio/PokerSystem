using GTANetworkAPI;
using WiredPlayers.Data.Persistent;
using WiredPlayers.messages.error;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Currency
{
    public class Money
    {
        public static void SetPlayerMoney(Player player, int amount)
        {
            // Update the player's money with the given value
            player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Money = amount;

            // Visual money update
            player.TriggerEvent("updatePlayerMoney", amount);
        }

        public static bool GivePlayerMoney(Player player, int amount, out string error)
        {
            // Initialize the output value
            error = string.Empty;

            // Check if the given amount is positive
            if (amount <= 0)
            {
                error = ErrRes.price_positive;
                return false;
            }

            // Add the given amount of money
            player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Money += amount;

            return true;
        }

        public static bool SubstractPlayerMoney(Player player, int amount, out string error)
        {
            // Initialize the output value
            error = string.Empty;

            if (amount <= 0)
            {
                error = ErrRes.price_positive;
                return false;
            }

            // Get the player's money
            int money = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Money;

            if (money < amount)
            {
                error = string.Format(ErrRes.player_needs_more_money, amount - money);
                return false;
            }

            // Substract the given amount of money
            SetPlayerMoney(player, money - amount);

            return true;
        }

        public static bool ExchangePlayersMoney(Player sender, Player paid, int amount, out string senderError, out string paidError)
        {
            // Initialize the output value
            senderError = string.Empty;
            paidError = string.Empty;

            if (amount <= 0)
            {
                senderError = ErrRes.price_positive;
                paidError = ErrRes.price_positive;
                return false;
            }

            // Get the player's money
            int money = sender.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Money;

            if (money < amount)
            {
                senderError = string.Format(ErrRes.player_needs_more_money, amount - money);
                paidError = ErrRes.target_needs_more_money;
                return false;
            }

            // Substract the given amount of money to the player paying
            SetPlayerMoney(sender, money - amount);

            // Add the given amount of money to the target
            SetPlayerMoney(paid, paid.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Money + amount);

            return true;
        }
    }
}
