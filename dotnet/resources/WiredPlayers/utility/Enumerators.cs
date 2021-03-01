namespace WiredPlayers.Utility
{
    public static class Enumerators
    {
        // Application form state
        public enum ApplicationForm
        {
            None = 0,
            Ingame = 1,
            External = 2
        }
        
        // Administrative ranks
        public enum StaffRank
        {
            None = 0, Support = 1, GameMaster = 2,
            SuperGameMaster = 3, Administrator = 4
        }

        // Bank operations
        public enum BankOperations
        {
            Withdraw = 1,
            Deposit = 2,
            Transfer = 3,
            Balance = 4
        }

        // Character sex
        public enum Sex
        {
            None = -1, 
            Male = 0, 
            Female = 1
        }
        
        // Service phone numbers
        public enum ServiceNumbers
        {
            Police = 911, Emergency = 112, News = 114,
            Fastfood = 115, Mechanic = 116, Taxi = 555
        }
        
        // Factions
        public enum PlayerFactions
        {
            None = 0, Police = 1, Emergency = 2, News = 3, TownHall = 4,
            DrivingSchool = 5, Taxi = 6, Sheriff = 7, Admin = 9
        }

        // Jobs
        public enum PlayerJobs
        {
            None = 0, Fastfood = 1, Thief = 2, Mechanic = 3, GarbageCollector = 4,
            Hooker = 5, Fisherman = 6, Taxi = 7, Trucker = 8
        }

        // Chat message types
        public enum ChatTypes
        {
            Talk = 0, Yell = 1, Whisper = 2, Me = 3, Do = 4, Ooc = 5,
            Lucky = 6, Unlucky = 7, News = 8, Phone = 9, Disconnect = 10,
            Megaphone = 11, Radio = 12
        }
        
        // Building types
        public enum BuildingTypes
        {
			None = 0,
            Interior = 1,
            Business = 2,
            House = 3
        }

        // Generic interior types
        public enum InteriorTypes
        {
            FaceLobby = 1, TownHall = 2, Morgue = 3, Jewelry = 4, PrivateOffice = 5, Slaughterhouse = 6, Factory = 7,
            TortureRoom = 8, GarageLowEnd = 9, Warehouse = 10, WarehouseMedium = 11, BennyWorkshop = 12, Clubhouse1 = 13,
            Clubhouse2 = 14, MetaLab = 15, WeedWarehouse = 16, CocaineLab = 17, CashFactory = 18, DocumentOffice = 19,
            WarehouseSmall = 20, WarehouseLarge = 21, VehicleWarehouse = 22
        }

        // House types
        public enum HouseTypes
        {
            Apartment1a = 1, Apartment1c = 2, Apartment1b = 3, Apartment2a = 4, Apartment2c = 5, Apartment2b = 6, Apartment3a = 7, 
            Apartment3c = 8, Apartment3b = 9, Apartment4a = 10, Apartment4c = 11, Apartment4b = 12, Apartment5a = 13, Apartment5c = 14, 
            Apartment5b = 15, Apartment6a = 16, Apartment6c = 17, Apartment6b = 18, Apartment7a = 19, Apartment7c = 20, Apartment7b = 21, 
            Apartment9a = 22, Apartment9c = 23, Apartment9b = 24, LowEnd = 25, MediumEnd = 26, Integrity28 = 27, Integrity30 = 28, 
            DelPerro4 = 29, DelPerro7 = 30, Richard = 31, Tinsel = 32, Eclipse = 33, WildOats = 34, NorthCoker2044 = 35, 
            NorthCoker2045 = 36, Hillcrest2862 = 37, Hillcrest2868 = 38, Hillcrest2874 = 39, Whispymound = 40, MadWayne = 41, 
            Trevor = 42, TrevorTidy = 43, Floyd = 44, Lester = 45, Janitor = 46, Mansion = 47, Flat = 48, FranklinAunt = 49,
            Franklin = 50, Oneil = 51, Motel = 52
        }
        
        // Business types
        public enum BusinessTypes
        {
            None = -1, Market = 1, Electronics = 2, Hardware = 3, Clothes = 4, Bar = 5, Disco = 6,
            Ammunation = 7, TheLost = 8, GasStation = 9, BarberShop = 10, SocialClub = 11,
            Mechanic = 12, TattooShop = 13, Vanilla = 14, Fishing = 15
        }
        
        // Parking types
        public enum ParkingTypes
        {
            Public = 0,
            Garage = 1,
            Scrapyard = 2,
            Deposit = 3
        }

        // House state
        public enum HouseState
        {
            None = 0,
            Rentable = 1,
            Buyable = 2
        }
        
        // ColShape types
        public enum ColShapeTypes
        {
            BusinessEntrance = 0, BusinessPurchase = 1, HouseEntrance = 2,
            InteriorEntrance = 3, VehicleDealer = 5, Atm = 6, Plant = 7
        }
        
        // Vehicle types
        public enum VehicleClass
        {
            Compact = 0, Sedan = 1, Suv = 2, Coupe = 3, Muscle = 4, Sports = 5,
            Classic = 6, SuperSport = 7, Motorcycle = 8, OffRoad = 9, Industrial = 10,
            Utility = 11, Van = 12, Cycle = 13, Boat = 14, Helicopter = 15, Plane = 16,
            Service = 17, Emergency = 18, Military = 19, Commercial = 20, Train = 21
        }

        // Vehicle doors
        public enum VehicleDoor
        {
            DriverFront = 0, PassengerFront = 1, DriverRead = 2,
            PassengerRear = 3, Hood = 4, Trunk = 5
        }

        // Vehicle components
        public enum VehicleModSlot
        {
            Spoiler = 0, FrontBumper = 1, RearBumper = 2, SideSkirt = 3, Exhaust = 4, Frame = 5, Grille = 6, Hood = 7,
            Fender = 8, RightFender = 9, Roof = 10, Engine = 11, Brakes = 12, Transmission = 13, Horn = 14, Suspension = 15,
            Armor = 16, Xenon = 22, FrontWheels = 23, BackWheels = 24, PlateHolders = 25, TrimDesign = 27, Ornaments = 28,
            DialDesign = 30, SteeringWheel = 33, ShifterLeavers = 34, Plaques = 35, Hydraulics = 38, Livery = 48
        }

        // Animation flags
        public enum AnimationFlags
        {
            Loop = 1 << 0, StopOnLastFrame = 1 << 1, OnlyAnimateUpperBody = 1 << 4,
            AllowPlayerControl = 1 << 5, Cancellable = 1 << 7
        }

        // Item types
        public enum ItemTypes
        {
            Consumable = 0,
            Equipable = 1,
            Openable = 2,
            Weapon = 3,
            Ammunition = 4,
            Miscellaneous = 5
        }
        
        // Inventory targets
        public enum InventoryTarget 
        {
            Self = 0,
            Player = 1,
            Trunk = 2,
            VehiclePlayer = 3,
            PawnShop = 4
        }

        // Police control's items
        public enum PoliceControlItems
        {
            Cone = 1245865676,
            Beacon = 93871477,
            Barrier = -143315610,
            Spikes = -874338148
        }
        
        // Town hall formalities
        public enum Formalities
        {
            Identification = 0,
            MedicalInsurance = 1,
            TaxiLicense = 2,
            Fines = 3
        }
        
        // Driving school's licenses
        public enum DrivingLicenses
        {
            None = -1,
            Car = 0,
            Motorcycle = 1,
            Taxi = 2
        }

        // Driving school exam type
        public enum DrivingExams
        {
            None = 0,
            CarTheorical = 1,
            CarPractical = 2,
            MotorcycleTheorical = 3,
            MotorcyclePractical = 4
        }
        
        // Clothes bodyparts
        public enum ClothesSlots
        {
            Mask = 1, Torso = 3, Legs = 4, Bag = 5, Feet = 6, Accessories = 7,
            Undershirt = 8, Armor = 9, Decal = 10, Top = 11
        }

        // Accessory slots
        public enum AccessorySlots
        {
            Hat = 0, Glasses = 1, Ears = 2,
            Watch = 6, Bracelet = 7
        }
        
        // Tattoo zones
        public enum TattoZone
        {
            Torso = 0, Head = 1, LeftArm = 2,
            RightArm = 3, LeftLeg = 4, RightLeg = 5
        }
        
        // Gargabe collector's routes
        public enum GarbageRoute
        {
            None = 0,
            North = 1,
            South = 2,
            East = 3,
            West = 4
        }

        public enum ExternalDataSlot
        {
            Database = 0,
            Ingame = 1,
        }

        public enum HookerService
        {
            None = 0,
            Oral = 1,
            Sex = 2
        }

        public enum JailTypes
        {
            None = -1,
            Ic = 1,
            Ooc = 2
        }

        public enum Actions
        {
            Load = 0,
            Save = 1,
            Rename = 2,
            Delete = 3,
            Add = 4,
            Sms = 5
        }

        // Price from products
        public enum Prices
        {
            VehicleChassis = 300, VehicleDoors = 60, VehicleWindows = 15, VehicleTyres = 10,
            BarberShop = 100, Announcement = 500, DrivingTheorical = 200, DrivingPractical = 300,
            Identification = 500, MedicalInsurance = 2000, TaxiLicense = 5000, Products = 1, StolenProducts = 130,
            ParkingPublic = 50, ParkingDeposit = 500, Pizza = 20, Burger = 10, Sandwich = 5, Gas = 1, Fish = 20
        }
    }
}
