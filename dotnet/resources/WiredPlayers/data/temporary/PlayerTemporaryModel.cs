using GTANetworkAPI;
using static WiredPlayers.Utility.Enumerators;

namespace WiredPlayers.Data.Temporary
{
    class PlayerTemporaryModel
    {
        public TextLabel Ame { get; set; }
        public DrivingExams DrivingExam { get; set; }
        public int DrivingCheckpoint { get; set; }
        public bool Handcuffed { get; set; }
        public Player IncriminatedTarget { get; set; }
        public Vehicle JobVehicle { get; set; }
        public GarbageRoute JobRoute { get; set; }
        public Player JobPartner { get; set; }
        public Checkpoint JobColShape { get; set; }
        public int JobCheckPoint { get; set; }
        public int JobWon { get; set; }
        public int DeliverOrder { get; set; }
        public int DeliverStart { get; set; }
        public int DeliverTime { get; set; }
        public Vehicle Lockpicking { get; set; }
        public int RobberyStart { get; set; }
        public Player AlreadyFucking { get; set; }
        public HookerService HookerService { get; set; }
        public Vehicle Hotwiring { get; set; }
        public Vehicle LastVehicle { get; set; }
        public Vehicle TestingVehicle { get; set; }
        public string LeftHand { get; set; }
        public string RightHand { get; set; }
        public Object GarbageBag { get; set; }
        public bool OnAir { get; set; }
        public JailTypes JailType { get; set; }
        public int Jailed { get; set; }
        public Player SearchedTarget { get; set; }
        public bool Animation { get; set; }
        public Vehicle OpenedTrunk { get; set; }
        public object Calling { get; set; }
        public int PhoneCallStarted { get; set; }
        public Player PhoneTalking { get; set; }
        public DrivingLicenses LicenseType { get; set; }
        public int LicenseQuestion { get; set; }
        public Player Payment { get; set; }
        public bool AdminOnDuty { get; set; }
        public Vehicle Refueling { get; set; }
        public bool Reinforces { get; set; }
        public bool FactionWarning { get; set; }
        public int SellingPrice { get; set; }
        public int SellingProducts { get; set; }
        public int SellingVehicle { get; set; }
        public int SellingHouse { get; set; }
        public int SellingHouseState { get; set; }
        public string WeaponCrate { get; set; }
        public Vehicle RepairVehicle { get; set; }
        public string RepairType { get; set; }
        public RepaintModel Repaint { get; set; }
        public bool Fishing { get; set; }
        public float DrunkLevel { get; set; }
        public bool TaxiPath { get; set; }
        public ColShape EnteredColShape { get; set; }
    }
}
