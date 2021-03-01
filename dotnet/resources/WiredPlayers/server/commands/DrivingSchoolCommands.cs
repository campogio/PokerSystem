using GTANetworkAPI;
using System;
using System.Collections.Generic;
using WiredPlayers.Currency;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Utility;
using WiredPlayers.messages.arguments;
using WiredPlayers.messages.error;
using WiredPlayers.messages.help;
using WiredPlayers.messages.information;
using WiredPlayers.Buildings;
using static WiredPlayers.Utility.Enumerators;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Server.Commands
{
    public static class DrivingSchoolCommands
    {
        [Command]
        public static async void DrivingSchoolCommand(Player player, string type)
        {
            if (player.Position.DistanceTo(DrivingSchool.DrivingSchoolTextLabel.Position) > 2.5f)
            {
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_not_driving_school);
                return;
            }

            List<TestModel> questions;
            List<TestModel> answers = new List<TestModel>();

            // Get the player's money
            int money = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Money;

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (type.Equals(ArgRes.car, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check for the status if the license
                int licenseStatus = DrivingSchool.GetPlayerLicenseStatus(player, DrivingLicenses.Car);

                switch (licenseStatus)
                {
                    case -1:
                        // Check if the player has enough money
                        if (money >= (int)Prices.DrivingTheorical)
                        {
                            // Add the questions
                            questions = await DatabaseOperations.GetRandomQuestions((int)DrivingLicenses.Car + 1, Constants.MAX_LICENSE_QUESTIONS).ConfigureAwait(false);
                            foreach (TestModel question in questions)
                            {
                                answers.AddRange(await DatabaseOperations.GetQuestionAnswers(question.id).ConfigureAwait(false));
                            }

                            player.SetSharedData(EntityData.PlayerLicenseQuestion, 0);
                            player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).LicenseType = DrivingLicenses.Car;

                            Money.SetPlayerMoney(player, -(int)Prices.DrivingTheorical);

                            // Start the exam
                            player.TriggerEvent("startLicenseExam", NAPI.Util.ToJson(questions), NAPI.Util.ToJson(answers));
                        }
                        else
                        {
                            string message = string.Format(ErrRes.driving_school_money, Prices.DrivingTheorical);
                            player.SendChatMessage(Constants.COLOR_ERROR + message);
                        }
                        break;
                    case 0:
                        // Check if the player has enough money
                        if (money >= (int)Prices.DrivingPractical)
                        {
                            data.LicenseType = DrivingLicenses.Car;
                            data.DrivingExam = DrivingExams.CarPractical;
                            data.DrivingCheckpoint = 0;

                            Money.SetPlayerMoney(player, -(int)Prices.DrivingPractical);

                            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.enter_license_car_vehicle);
                        }
                        else
                        {
                            string message = string.Format(ErrRes.driving_school_money, Prices.DrivingPractical);
                            player.SendChatMessage(Constants.COLOR_ERROR + message);
                        }
                        break;
                    default:
                        // License up to date
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_license);
                        break;
                }

                return;
            }

            if(type.Equals(ArgRes.motorcycle, StringComparison.InvariantCultureIgnoreCase))
            {
                // Check for the status if the license
                int licenseStatus = DrivingSchool.GetPlayerLicenseStatus(player, DrivingLicenses.Motorcycle);

                switch (licenseStatus)
                {
                    case -1:
                        // Check if the player has enough money
                        if (money >= (int)Prices.DrivingTheorical)
                        {
                            // Add the questions
                            questions = await DatabaseOperations.GetRandomQuestions((int)DrivingLicenses.Motorcycle + 1, Constants.MAX_LICENSE_QUESTIONS).ConfigureAwait(false);
                            foreach (TestModel question in questions)
                            {
                                answers.AddRange(await DatabaseOperations.GetQuestionAnswers(question.id).ConfigureAwait(false));
                            }

                            player.SetSharedData(EntityData.PlayerLicenseQuestion, 0);
                            player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).LicenseType = DrivingLicenses.Motorcycle;

                            Money.SetPlayerMoney(player, -(int)Prices.DrivingTheorical);

                            // Start the exam
                            player.TriggerEvent("startLicenseExam", NAPI.Util.ToJson(questions), NAPI.Util.ToJson(answers));
                        }
                        else
                        {
                            string message = string.Format(ErrRes.driving_school_money, Prices.DrivingTheorical);
                            player.SendChatMessage(Constants.COLOR_ERROR + message);
                        }
                        break;
                    case 0:
                        // Check if the player has enough money
                        if (money >= (int)Prices.DrivingPractical)
                        {
                            data.LicenseType = DrivingLicenses.Motorcycle;
                            data.DrivingExam = DrivingExams.MotorcyclePractical;
                            data.DrivingCheckpoint = 0;

                            Money.SetPlayerMoney(player, -(int)Prices.DrivingPractical);

                            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.enter_license_bike_vehicle);
                        }
                        else
                        {
                            string message = string.Format(ErrRes.driving_school_money, Prices.DrivingPractical);
                            player.SendChatMessage(Constants.COLOR_ERROR + message);
                        }
                        break;
                    default:
                        // License up to date
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.player_already_license);
                        break;
                }

                return;
            }

            // The option is not correct
            player.SendChatMessage(Constants.COLOR_HELP + HelpRes.driving_school);
        }

        [Command]
        public static void LicensesCommand(Player player)
        {
            // Send the chat message to the player
            player.SendChatMessage(Constants.COLOR_INFO + InfoRes.license_list);
            
            // Show the license to the player
            DrivingSchool.ShowDrivingLicense(player, player);
        }
    }
}
