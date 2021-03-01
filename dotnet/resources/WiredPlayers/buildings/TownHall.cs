using GTANetworkAPI;
using System.Collections.Generic;
using System.Threading.Tasks;
using WiredPlayers.Currency;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.information;
using WiredPlayers.Utility;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Buildings
{
    public class TownHall : Script
    {
        [RemoteEvent("documentOptionSelected")]
        public async Task DocumentOptionSelectedEvent(Player player, int tramitation)
        {
            // Get the player's character model
            CharacterModel characterModel = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            switch (tramitation)
            {
                case (int)Formalities.Identification:
                    if (characterModel.Documentation > 0)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_has_identification);
                        return;
                    }

                    if(!Money.SubstractPlayerMoney(player, (int)Prices.Identification, out string errorIdentification))
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + errorIdentification);
                        return;
                    }

                    characterModel.Documentation = UtilityFunctions.GetTotalSeconds();
                    player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_has_indentification, Prices.Identification));

                    // Log the payment made
                    await Task.Run(() => DatabaseOperations.LogPayment(player.Name, GenRes.townhall_faction, GenRes.identification, (int)Prices.Identification)).ConfigureAwait(false);

                    break;

                case (int)Formalities.MedicalInsurance:
                    if (characterModel.Insurance > UtilityFunctions.GetTotalSeconds())
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_has_medical_insurance);
                        return;
                    }

                    if(!Money.SubstractPlayerMoney(player, (int)Prices.MedicalInsurance, out string errorMedical))
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + errorMedical);
                        return;
                    }

                    characterModel.Insurance = UtilityFunctions.GetTotalSeconds() + 1209600;
                    player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_has_medical_insurance, Prices.MedicalInsurance));

                    // Log the payment made
                    await Task.Run(() => DatabaseOperations.LogPayment(player.Name, GenRes.townhall_faction, GenRes.medical_insurance, (int)Prices.MedicalInsurance)).ConfigureAwait(false);

                    break;

                case (int)Formalities.TaxiLicense:
                    if (DrivingSchool.GetPlayerLicenseStatus(player, DrivingLicenses.Taxi) > 0)
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_has_taxi_license);
                        return;
                    }

                    if(!Money.SubstractPlayerMoney(player, (int)Prices.TaxiLicense, out string errorTaxi))
                    {
                        player.SendChatMessage(Constants.COLOR_ERROR + errorTaxi);
                        return;
                    }

                    DrivingSchool.SetPlayerLicense(player, (int)DrivingLicenses.Taxi, 1);
                    player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_has_taxi_license, Prices.TaxiLicense));

                    // Log the payment made
                    await Task.Run(() => DatabaseOperations.LogPayment(player.Name, GenRes.townhall_faction, GenRes.taxi_license, (int)Prices.TaxiLicense)).ConfigureAwait(false);

                    break;

                case (int)Formalities.Fines:
                    List<FineModel> fineList = await DatabaseOperations.LoadPlayerFines(player.Name).ConfigureAwait(false);

                    if (fineList.Count > 0)
                    {
                        player.TriggerEvent("showPlayerFineList", NAPI.Util.ToJson(fineList));
                    }
                    else
                    {
                        player.SendChatMessage(Constants.COLOR_INFO + InfoRes.player_no_fines);
                    }

                    break;
            }
        }

        [RemoteEvent("payPlayerFines")]
        public async Task PayPlayerFinesEvent(Player player, string finesJson)
        {
            int paidAmount = 0;
            
            List<FineModel> fineList = await DatabaseOperations.LoadPlayerFines(player.Name).ConfigureAwait(false);
            List<FineModel> removedFines = NAPI.Util.FromJson<List<FineModel>>(finesJson);

            foreach (FineModel fine in removedFines)
            {
                // Get the money amount for all the fines
                paidAmount += fine.amount;
            }

            if (paidAmount == 0)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_no_fines);
                return;
            }

            if (!Money.SubstractPlayerMoney(player, paidAmount, out string error))
            {
                player.SendChatMessage(Constants.COLOR_ERROR + error);
                return;
            }

            if (removedFines.Count == fineList.Count)
            {
                // All the fines were paid, back to the previous window
                player.TriggerEvent("backTownHallIndex");
            }

            // Send the message to the player
            player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.player_fines_paid, paidAmount));

            // Delete paid fines
            await Task.Run(() =>
            {
                DatabaseOperations.RemoveFines(removedFines);
                DatabaseOperations.LogPayment(player.Name, GenRes.townhall_faction, GenRes.fines_payment, paidAmount);
            }).ConfigureAwait(false);
        }
    }
}
