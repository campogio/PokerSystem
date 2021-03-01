using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WiredPlayers.Data;
using WiredPlayers.Data.Persistent;
using WiredPlayers.Data.Temporary;
using WiredPlayers.messages.commands;
using WiredPlayers.messages.error;
using WiredPlayers.messages.general;
using WiredPlayers.messages.information;
using WiredPlayers.messages.success;
using WiredPlayers.Utility;
using WiredPlayers.vehicles;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Buildings
{
    public class DrivingSchool : Script
    {
        public static TextLabel DrivingSchoolTextLabel;
        private static int lastUpdate = Environment.TickCount;
        private static Dictionary<int, Timer> DrivingSchoolTimerList;

        public DrivingSchool()
        {
            // Initialize the variables
            DrivingSchoolTimerList = new Dictionary<int, Timer>();

            // Create the text label
            DrivingSchoolTextLabel = NAPI.TextLabel.CreateTextLabel("/" + ComRes.driving_school, Coordinates.DrivingSchool, 10.0f, 0.5f, 4, new Color(190, 235, 100), false, 0);
            NAPI.TextLabel.CreateTextLabel(GenRes.driving_school, new Vector3(Coordinates.DrivingSchool.X, Coordinates.DrivingSchool.Y, Coordinates.DrivingSchool.Z - 0.5f), 10.0f, 0.5f, 4, new Color(255, 255, 255), false, 0);
        }

        public static void OnPlayerDisconnected(Player player)
        {
            if (!DrivingSchoolTimerList.TryGetValue(player.Value, out Timer drivingSchoolTimer)) return;

            // We remove the timer
            drivingSchoolTimer.Dispose();
            DrivingSchoolTimerList.Remove(player.Value);
        }

        private void OnDrivingTimer(object playerObject)
        {
            // We get the player and his vehicle
            Player player = (Player)playerObject;

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // We finish the exam
            FinishDrivingExam(player, data.LastVehicle);

            // Deleting timer from the list
            if (DrivingSchoolTimerList.TryGetValue(player.Value, out Timer drivingSchoolTimer))
            {
                drivingSchoolTimer.Dispose();
                DrivingSchoolTimerList.Remove(player.Value);
            }

            // Confirmation message sent to the player
            player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.license_failed_not_in_vehicle);
        }

        private void FinishDrivingExam(Player player, Vehicle vehicle)
        {
            // Get the vehicle information
            VehicleModel vehModel = Vehicles.GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId));
            
            // Vehicle reseting
            vehicle.Repair();
            vehicle.Position = vehModel.Position;
            vehicle.Rotation = new Vector3(0.0f, 0.0f, vehModel.Heading);

            if (player.VehicleSeat == (int)VehicleSeat.Driver && player.Vehicle == vehicle)
            {
                // Delete the checkpoint
                player.TriggerEvent("deleteLicenseCheckpoint");
            }

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            // Remove the exam data
            data.LastVehicle = null;
            data.DrivingExam = DrivingExams.None;
            data.DrivingCheckpoint = 0;

            // Remove player from vehicle
            player.WarpOutOfVehicle();
        }

        public static int GetPlayerLicenseStatus(Player player, DrivingLicenses license)
        {
            // Get the status of the selected license
            return Array.ConvertAll(player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Licenses.Split(','), int.Parse)[(int)license];
        }

        public static void SetPlayerLicense(Player player, int license, int value)
        {
            // Get the character model from the player
            CharacterModel model = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database);

            // We get player licenses
            string[] licenses = model.Licenses.Split(',');

            // Changing license status
            licenses[license] = value.ToString();
            model.Licenses = string.Join(",", licenses);
        }
        
        public static void ShowDrivingLicense(Player player, Player target)
        {
            int currentLicense = 0;
            string playerLicenses = player.GetExternalData<CharacterModel>((int)ExternalDataSlot.Database).Licenses;
            int[] playerLicensesArray = Array.ConvertAll(playerLicenses.Split(','), int.Parse);

            foreach (int license in playerLicensesArray)
            {
                switch (currentLicense)
                {
                    case (int)DrivingLicenses.Car:
                        switch (license)
                        {
                            case -1:
                                target.SendChatMessage(Constants.COLOR_HELP + InfoRes.car_license_not_available);
                                break;
                            case 0:
                                target.SendChatMessage(Constants.COLOR_HELP + InfoRes.car_license_practical_pending);
                                break;
                            default:
                                target.SendChatMessage(Constants.COLOR_HELP + string.Format(InfoRes.car_license_points, license));
                                break;
                        }
                        break;
                    case (int)DrivingLicenses.Motorcycle:
                        switch (license)
                        {
                            case -1:
                                target.SendChatMessage(Constants.COLOR_HELP + InfoRes.motorcycle_license_not_available);
                                break;
                            case 0:
                                target.SendChatMessage(Constants.COLOR_HELP + InfoRes.motorcycle_license_practical_pending);
                                break;
                            default:
                                target.SendChatMessage(Constants.COLOR_HELP + string.Format(InfoRes.motorcycle_license_points, license));
                                break;
                        }
                        break;
                    case (int)DrivingLicenses.Taxi:
                        target.SendChatMessage(Constants.COLOR_HELP + (license == -1 ? InfoRes.taxi_license_not_available : InfoRes.taxi_license_up_to_date));
                        break;
                }
                currentLicense++;
            }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public void OnPlayerEnterVehicle(Player player, Vehicle vehicle, sbyte _)
        {
            // Get the vehicle information
            VehicleModel vehModel = Vehicles.GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId));
            
            if (vehModel.Faction != (int)PlayerFactions.DrivingSchool) return;

            // Get the temporary data for the player
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            switch (data.DrivingExam)
            {
                case DrivingExams.None:
                    player.WarpOutOfVehicle();
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.not_in_car_practice);
                    break;

                case DrivingExams.CarPractical:
                    // We check the class of the vehicle
                    if (vehicle.Class == (int)VehicleClass.Sedan)
                    {
                        if (DrivingSchoolTimerList.TryGetValue(player.Value, out Timer drivingSchoolTimer))
                        {
                            drivingSchoolTimer.Dispose();
                            DrivingSchoolTimerList.Remove(player.Value);
                        }

                        // We place a mark on the map
                        data.LastVehicle = vehicle;
                        player.TriggerEvent("showLicenseCheckpoint", Coordinates.CarLicenseCheckpoints[data.DrivingCheckpoint], Coordinates.CarLicenseCheckpoints[data.DrivingCheckpoint + 1], CheckpointType.CylinderSingleArrow);
                    }
                    else
                    {
                        player.WarpOutOfVehicle();
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_driving_not_suitable);
                    }
                    break;

                case DrivingExams.MotorcyclePractical:
                    // We check the class of the vehicle
                    if (vehicle.Class == (int)VehicleClass.Motorcycle)
                    {
                        if (DrivingSchoolTimerList.TryGetValue(player.Value, out Timer drivingSchoolTimer) == true)
                        {
                            drivingSchoolTimer.Dispose();
                            DrivingSchoolTimerList.Remove(player.Value);
                        }

                        // We place a mark on the map
                        data.LastVehicle = vehicle;
                        player.TriggerEvent("showLicenseCheckpoint", Coordinates.BikeLicenseCheckpoints[data.DrivingCheckpoint], Coordinates.BikeLicenseCheckpoints[data.DrivingCheckpoint + 1], CheckpointType.CylinderSingleArrow);
                    }
                    else
                    {
                        player.WarpOutOfVehicle();
                        player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.vehicle_driving_not_suitable);
                    }
                    break;
            }
        }

        [ServerEvent(Event.PlayerExitVehicle)]
        public void PlayerExitVehicleServerEvent(Player player, Vehicle vehicle)
        {
            // Check if the vehicle isn't destroyed
            if (vehicle == null || !vehicle.Exists) return;

            // Get the temporary data for the player
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (data.DrivingExam != DrivingExams.None || data.LastVehicle != vehicle) return;
                        
            if (Vehicles.GetVehicleById<VehicleModel>(vehicle.GetData<int>(EntityData.VehicleId)).Faction == (int)PlayerFactions.DrivingSchool)
            {
                // Send the warning to the player
                player.SendChatMessage(Constants.COLOR_INFO + string.Format(InfoRes.license_vehicle_exit, 15));

                // Removing the checkpoint marker
                player.TriggerEvent("deleteLicenseCheckpoint");

                // When the timer finishes, the exam will be failed
                Timer drivingSchoolTimer = new Timer(OnDrivingTimer, player, 15000, Timeout.Infinite);
                DrivingSchoolTimerList.Add(player.Value, drivingSchoolTimer);
            }
        }

        [ServerEvent(Event.VehicleDamage)]
        public void VehicleDamageServerEvent(Vehicle vehicle, float lossFirst, float _)
        {
            Player player = (Player)NAPI.Vehicle.GetVehicleDriver(vehicle);

            if (player == null || !player.Exists) return;

            // Get the temporary data for the player
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (data.DrivingExam == DrivingExams.CarPractical || data.DrivingExam == DrivingExams.MotorcyclePractical)
            {
                // Check if the damage was high enough
                if (lossFirst - vehicle.Health < 5.0f) return;

                // Exam finished
                FinishDrivingExam(player, vehicle);

                // Inform the player about his failure
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.license_drive_failed);
            }
        }

        [ServerEvent(Event.Update)]
        public void UpdateServerEvent()
        {
            if (Environment.TickCount - lastUpdate < 500) return;

            lastUpdate = Environment.TickCount;

            // Get all the players driving a exam vehicle
            Player[] licenseDrivers = NAPI.Pools.GetAllPlayers().Where(d => d.VehicleSeat == (int)VehicleSeat.Driver && d.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame).DrivingExam != DrivingExams.None).ToArray();

            foreach (Player player in licenseDrivers)
            {
                // Get the player's vehicle
                VehicleModel vehicle = Vehicles.GetVehicleById<VehicleModel>(player.Vehicle.GetData<int>(EntityData.VehicleId));

                if (vehicle.Faction != (int)PlayerFactions.DrivingSchool) continue;

                Vector3 velocity = NAPI.Entity.GetEntityVelocity(player.Vehicle);
                double speed = Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y + velocity.Z * velocity.Z);

                if (Math.Round(speed * 3.6f) > Constants.MAX_DRIVING_VEHICLE)
                {
                    // Exam finished
                    FinishDrivingExam(player, player.Vehicle);

                    // Inform the player about his failure
                    player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.license_drive_failed);
                }
            }
        }

        [RemoteEvent("checkAnswer")]
        public void CheckAnswerRemoteEvent(Player player, int answer)
        {
            // Get the player's external data
            PlayerTemporaryModel playerModel = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (DatabaseOperations.CheckAnswerCorrect(answer))
            {
                // We add the correct answer
                int nextQuestion = player.GetSharedData<int>(EntityData.PlayerLicenseQuestion) + 1;

                if (nextQuestion < Constants.MAX_LICENSE_QUESTIONS)
                {
                    // Go for the next question
                    player.SetSharedData(EntityData.PlayerLicenseQuestion, nextQuestion);
                    player.TriggerEvent("getNextTestQuestion");
                }
                else
                {
                    // Player passed the exam
                    SetPlayerLicense(player, (int)playerModel.LicenseType, 0);

                    // Reset the entity data
                    playerModel.LicenseType = DrivingLicenses.None;
                    player.ResetSharedData(EntityData.PlayerLicenseQuestion);

                    // Send the message to the player
                    player.SendChatMessage(Constants.COLOR_SUCCESS + SuccRes.license_exam_passed);

                    // Exam window close
                    player.TriggerEvent("finishLicenseExam");
                }
            }
            else
            {
                // Player failed the exam
                player.SendChatMessage(Constants.COLOR_ERROR + ErrRes.license_exam_failed);

                // Reset the entity data
                playerModel.LicenseType = DrivingLicenses.None;
                player.ResetSharedData(EntityData.PlayerLicenseQuestion);

                // Exam window close
                player.TriggerEvent("finishLicenseExam");
            }
        }

        [RemoteEvent("licenseCheckpointReached")]
        public void LicenseCheckpointReachedRemoteEvent(Player player)
        {
            // Get the vehicle faction
            VehicleModel vehicle = Vehicles.GetVehicleById<VehicleModel>(player.Vehicle.GetData<int>(EntityData.VehicleId));
            
            // Check if the player is driving the correct vehicle
            if (vehicle.Faction != (int)PlayerFactions.DrivingSchool) return;

            // Get the checkpoints and license
            int license = 0;
            List<Vector3> checkpointList = new List<Vector3>();

            // Get the temporary data
            PlayerTemporaryModel data = player.GetExternalData<PlayerTemporaryModel>((int)ExternalDataSlot.Ingame);

            if (data.DrivingExam == DrivingExams.CarPractical)
            {
                // Get the car checkpoints
                license = (int)DrivingLicenses.Car;
                checkpointList = Coordinates.CarLicenseCheckpoints.ToList();
            }
            else if (data.DrivingExam == DrivingExams.MotorcyclePractical)
            {
                // Get the motorcycle checkpoints
                license = (int)DrivingLicenses.Motorcycle;
                checkpointList = Coordinates.BikeLicenseCheckpoints.ToList();
            }

            // Obtain the current checkpoint and increase the counter
            int checkpointNumber = data.DrivingCheckpoint;
            data.DrivingCheckpoint++;

            if (checkpointNumber < checkpointList.Count - 2)
            {
                // Get the next checkpoint
                player.TriggerEvent("showLicenseCheckpoint", checkpointList[checkpointNumber + 1], checkpointList[checkpointNumber + 2], CheckpointType.CylinderSingleArrow);
            }
            else if (checkpointNumber == checkpointList.Count - 2)
            {
                // Get the next checkpoint
                player.TriggerEvent("showLicenseCheckpoint", checkpointList[checkpointNumber + 1], vehicle.Position, CheckpointType.CylinderSingleArrow);
            }
            else if (checkpointNumber == Coordinates.CarLicenseCheckpoints.Length - 1)
            {
                // Get the next checkpoint
                player.TriggerEvent("showLicenseCheckpoint", vehicle.Position.Subtract(new Vector3(0.0f, 0.0f, 0.4f)), new Vector3(), CheckpointType.CylinderCheckerboard);
            }
            else
            {
                // Exam finished
                FinishDrivingExam(player, player.Vehicle);

                // We add points to the license
                SetPlayerLicense(player, license, 12);

                // Confirmation message sent to the player
                player.SendChatMessage(Constants.COLOR_SUCCESS + SuccRes.license_drive_passed);
            }
        }
    }
}
