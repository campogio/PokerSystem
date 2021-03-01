using GTANetworkAPI;
using WiredPlayers.messages.commands;
using WiredPlayers.messages.help;
using System;

namespace WiredPlayers.Server
{
    public static class CommandHandler
    {
        private static readonly string ADMIN_NAMESPACE = "WiredPlayers.Server.Commands.AdminCommands";
        private static readonly string CUSTOMIZATION_NAMESPACE = "WiredPlayers.Server.Commands.CustomizationCommands";
        private static readonly string LOGIN_NAMESPACE = "WiredPlayers.Server.Commands.LoginCommands";
        private static readonly string PLAYERDATA_NAMESPACE = "WiredPlayers.Server.Commands.PlayerDataCommands";
        private static readonly string TELEPHONE_NAMESPACE = "WiredPlayers.Server.Commands.TelephoneCommands";
        private static readonly string CHAT_NAMESPACE = "WiredPlayers.Server.Commands.ChatCommands";
        private static readonly string DRIVING_SCHOOL_NAMESPACE = "WiredPlayers.Server.Commands.DrivingSchoolCommands";
        private static readonly string DRUGS_NAMESPACE = "WiredPlayers.Server.Commands.DrugsCommands";
        private static readonly string EMERGENCY_NAMESPACE = "WiredPlayers.Server.Commands.EmergencyCommands";
        private static readonly string FACTION_NAMESPACE = "WiredPlayers.Server.Commands.FactionCommands";
        private static readonly string POLICE_NAMESPACE = "WiredPlayers.Server.Commands.PoliceCommands";
        private static readonly string WEAZEL_NEWS_NAMESPACE = "WiredPlayers.Server.Commands.WeazelNewsCommands";
        private static readonly string UTILITY_NAMESPACE = "WiredPlayers.Server.Commands.UtilityCommands";
        private static readonly string FURNITURE_NAMESPACE = "WiredPlayers.Server.Commands.FurnitureCommands";
        private static readonly string HOUSE_NAMESPACE = "WiredPlayers.Server.Commands.HouseCommands";
        private static readonly string FISHING_NAMESPACE = "WiredPlayers.Server.Commands.FishingCommands";
        private static readonly string GARBAGE_NAMESPACE = "WiredPlayers.Server.Commands.GarbageCommands";
        private static readonly string HOOKER_NAMESPACE = "WiredPlayers.Server.Commands.HookerCommands";
        private static readonly string JOB_NAMESPACE = "WiredPlayers.Server.Commands.JobCommands";
        private static readonly string MECHANIC_NAMESPACE = "WiredPlayers.Server.Commands.MechanicCommands";
        private static readonly string THIEF_NAMESPACE = "WiredPlayers.Server.Commands.ThiefCommands";
        private static readonly string TRUCKER_NAMESPACE = "WiredPlayers.Server.Commands.TruckerCommands";
        private static readonly string PARKING_NAMESPACE = "WiredPlayers.Server.Commands.ParkingCommands";
        private static readonly string VEHICLES_NAMESPACE = "WiredPlayers.Server.Commands.VehiclesCommands";
        private static readonly string WEAPONS_NAMESPACE = "WiredPlayers.Server.Commands.WeaponsCommands";

        public static void RegisterServerCommands()
        {
            // Admin.cs class
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("SkinCommand"), new RuntimeCommandInfo(ComRes.skin, HelpRes.skin));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("AdminCommand"), GetGreedyCommand(ComRes.admin, HelpRes.admin));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("CoordCommand"), new RuntimeCommandInfo(ComRes.coord, HelpRes.coord));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("TpCommand"), GetGreedyCommand(ComRes.tp, HelpRes.tp));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("BringCommand"), GetGreedyCommand(ComRes.bring, HelpRes.bring));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("GunCommand"), new RuntimeCommandInfo(ComRes.gun, HelpRes.gun));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("VehicleCommand"), GetGreedyCommand(ComRes.vehicle, HelpRes.vehicle));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("GoCommand"), new RuntimeCommandInfo(ComRes.go, HelpRes.go));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("BusinessCommand"), GetGreedyCommand(ComRes.business, HelpRes.business));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("CharacterCommand"), GetGreedyCommand(ComRes.character, HelpRes.character));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("HouseCommand"), GetGreedyCommand(ComRes.house, HelpRes.house));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("InteriorCommand"), GetGreedyCommand(ComRes.interior, HelpRes.interior));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("ParkingCommand"), GetGreedyCommand(ComRes.parking, HelpRes.parking));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("PosCommand"), new RuntimeCommandInfo(ComRes.pos));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("ReviveCommand"), GetGreedyCommand(ComRes.revive, HelpRes.revive));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("WeatherCommand"), new RuntimeCommandInfo(ComRes.weather, HelpRes.weather));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("JailCommand"), GetGreedyCommand(ComRes.jail, HelpRes.jail));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("KickCommand"), GetGreedyCommand(ComRes.kick, HelpRes.kick));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("KickAllCommand"), new RuntimeCommandInfo(ComRes.kick_all));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("BanCommand"), GetGreedyCommand(ComRes.ban, HelpRes.ban));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("HealthCommand"), GetGreedyCommand(ComRes.health, HelpRes.health));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("SaveCommand"), new RuntimeCommandInfo(ComRes.save));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("ADutyCommand"), new RuntimeCommandInfo(ComRes.a_duty));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("TicketCommand"), GetGreedyCommand(ComRes.ticket, HelpRes.ticket));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("TicketsCommand"), new RuntimeCommandInfo(ComRes.tickets));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("ATicketCommand"), GetGreedyCommand(ComRes.a_ticket, HelpRes.a_ticket));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("ACommand"), GetGreedyCommand(ComRes.a, HelpRes.a));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("ReconCommand"), GetGreedyCommand(ComRes.recon, HelpRes.recon));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("RecoffCommand"), new RuntimeCommandInfo(ComRes.recoff));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("InfoCommand"), GetGreedyCommand(ComRes.info, HelpRes.info));
            NAPI.Command.Register(Type.GetType(ADMIN_NAMESPACE).GetMethod("PointsCommand"), GetGreedyCommand(ComRes.points, HelpRes.points));

            // Customization.cs class
            NAPI.Command.Register(Type.GetType(CUSTOMIZATION_NAMESPACE).GetMethod("ComplementCommand"), new RuntimeCommandInfo(ComRes.complement, HelpRes.complement));

            // Login.cs class
            NAPI.Command.Register(Type.GetType(LOGIN_NAMESPACE).GetMethod("DisconnectCommand"), new RuntimeCommandInfo(ComRes.disconnect));

            // PlayerData.cs class
            NAPI.Command.Register(Type.GetType(PLAYERDATA_NAMESPACE).GetMethod("PlayerCommand"), new RuntimeCommandInfo(ComRes.player));

            // Telephone.cs class
            NAPI.Command.Register(Type.GetType(TELEPHONE_NAMESPACE).GetMethod("AnswerCommand"), new RuntimeCommandInfo(ComRes.answer));
            NAPI.Command.Register(Type.GetType(TELEPHONE_NAMESPACE).GetMethod("HangCommand"), new RuntimeCommandInfo(ComRes.hang));
            NAPI.Command.Register(Type.GetType(TELEPHONE_NAMESPACE).GetMethod("SmsCommand"), GetGreedyCommand(ComRes.sms, HelpRes.sms));
            NAPI.Command.Register(Type.GetType(TELEPHONE_NAMESPACE).GetMethod("ContactsCommand"), new RuntimeCommandInfo(ComRes.contacts, HelpRes.contacts));

            // Chat.cs class
            NAPI.Command.Register(Type.GetType(CHAT_NAMESPACE).GetMethod("SayCommand"), GetGreedyCommand(ComRes.say, HelpRes.say));
            NAPI.Command.Register(Type.GetType(CHAT_NAMESPACE).GetMethod("YellCommand"), GetGreedyCommand(ComRes.yell, HelpRes.yell));
            NAPI.Command.Register(Type.GetType(CHAT_NAMESPACE).GetMethod("WhisperCommand"), GetGreedyCommand(ComRes.whisper, HelpRes.whisper));
            NAPI.Command.Register(Type.GetType(CHAT_NAMESPACE).GetMethod("MeCommand"), GetGreedyCommand(ComRes.me, HelpRes.me));
            NAPI.Command.Register(Type.GetType(CHAT_NAMESPACE).GetMethod("DoCommand"), GetGreedyCommand(ComRes._do, HelpRes._do));
            NAPI.Command.Register(Type.GetType(CHAT_NAMESPACE).GetMethod("OocCommand"), GetGreedyCommand(ComRes.ooc, HelpRes.ooc));
            NAPI.Command.Register(Type.GetType(CHAT_NAMESPACE).GetMethod("LuckCommand"), new RuntimeCommandInfo(ComRes.luck));
            NAPI.Command.Register(Type.GetType(CHAT_NAMESPACE).GetMethod("AmeCommand"), GetGreedyCommand(ComRes.ame, HelpRes.ame));
            NAPI.Command.Register(Type.GetType(CHAT_NAMESPACE).GetMethod("MegaphoneCommand"), GetGreedyCommand(ComRes.megaphone, HelpRes.megaphone));
            NAPI.Command.Register(Type.GetType(CHAT_NAMESPACE).GetMethod("PmCommand"), GetGreedyCommand(ComRes.pm, HelpRes.pm));
            NAPI.Command.Register(Type.GetType(CHAT_NAMESPACE).GetMethod("SilenceCommand"), new RuntimeCommandInfo(ComRes.silence));

            // DrivingSchool.cs class
            NAPI.Command.Register(Type.GetType(DRIVING_SCHOOL_NAMESPACE).GetMethod("DrivingSchoolCommand"), new RuntimeCommandInfo(ComRes.driving_school, HelpRes.driving_school));
            NAPI.Command.Register(Type.GetType(DRIVING_SCHOOL_NAMESPACE).GetMethod("LicensesCommand"), new RuntimeCommandInfo(ComRes.licenses));

            // Drugs.cs class
            NAPI.Command.Register(Type.GetType(DRUGS_NAMESPACE).GetMethod("PlantCommand"), new RuntimeCommandInfo(ComRes.plant));

            // Emergency.cs class
            NAPI.Command.Register(Type.GetType(EMERGENCY_NAMESPACE).GetMethod("HealCommand"), GetGreedyCommand(ComRes.heal, HelpRes.heal));
            NAPI.Command.Register(Type.GetType(EMERGENCY_NAMESPACE).GetMethod("ReanimateCommand"), GetGreedyCommand(ComRes.reanimate, HelpRes.reanimate));
            NAPI.Command.Register(Type.GetType(EMERGENCY_NAMESPACE).GetMethod("ExtractCommand"), GetGreedyCommand(ComRes.extract, HelpRes.extract));
            NAPI.Command.Register(Type.GetType(EMERGENCY_NAMESPACE).GetMethod("DieCommand"), new RuntimeCommandInfo(ComRes.die));

            // Faction.cs class
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("FCommand"), GetGreedyCommand(ComRes.f, HelpRes.f));
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("RCommand"), GetGreedyCommand(ComRes.r, HelpRes.r));
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("PdCommand"), GetGreedyCommand(ComRes.pd, HelpRes.pd));
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("EdCommand"), GetGreedyCommand(ComRes.ed, HelpRes.ed));
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("FrCommand"), GetGreedyCommand(ComRes.fr, HelpRes.fr));
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("FrequencyCommand"), GetGreedyCommand(ComRes.frequency, HelpRes.frequency));
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("RecruitCommand"), GetGreedyCommand(ComRes.recruit, HelpRes.recruit));
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("DismissCommand"), GetGreedyCommand(ComRes.dismiss, HelpRes.dismiss));
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("RankCommand"), GetGreedyCommand(ComRes.rank, HelpRes.rank));
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("ReportsCommand"), new RuntimeCommandInfo(ComRes.reports));
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("AttendCommand"), new RuntimeCommandInfo(ComRes.attend, HelpRes.attend));
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("ClearReportsCommand"), new RuntimeCommandInfo(ComRes.clear_reports));
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("MembersCommand"), new RuntimeCommandInfo(ComRes.members));
            NAPI.Command.Register(Type.GetType(FACTION_NAMESPACE).GetMethod("SirenCommand"), new RuntimeCommandInfo(ComRes.siren));

            // Police.cs class
            NAPI.Command.Register(Type.GetType(POLICE_NAMESPACE).GetMethod("CheckCommand"), new RuntimeCommandInfo(ComRes.check));
            NAPI.Command.Register(Type.GetType(POLICE_NAMESPACE).GetMethod("FriskCommand"), GetGreedyCommand(ComRes.frisk, HelpRes.frisk));
            NAPI.Command.Register(Type.GetType(POLICE_NAMESPACE).GetMethod("IncriminateCommand"), GetGreedyCommand(ComRes.incriminate, HelpRes.incriminate));
            NAPI.Command.Register(Type.GetType(POLICE_NAMESPACE).GetMethod("FineCommand"), GetGreedyCommand(ComRes.fine, HelpRes.fine));
            NAPI.Command.Register(Type.GetType(POLICE_NAMESPACE).GetMethod("HandcuffCommand"), GetGreedyCommand(ComRes.handcuff, HelpRes.handcuff));
            NAPI.Command.Register(Type.GetType(POLICE_NAMESPACE).GetMethod("EquipmentCommand"), GetGreedyCommand(ComRes.equipment, HelpRes.equipment));
            NAPI.Command.Register(Type.GetType(POLICE_NAMESPACE).GetMethod("ControlCommand"), new RuntimeCommandInfo(ComRes.control, HelpRes.control));
            NAPI.Command.Register(Type.GetType(POLICE_NAMESPACE).GetMethod("PutCommand"), new RuntimeCommandInfo(ComRes.put, HelpRes.put));
            NAPI.Command.Register(Type.GetType(POLICE_NAMESPACE).GetMethod("RemoveCommand"), new RuntimeCommandInfo(ComRes.remove, HelpRes.remove));
            NAPI.Command.Register(Type.GetType(POLICE_NAMESPACE).GetMethod("ReinforcesCommand"), new RuntimeCommandInfo(ComRes.reinforces));
            NAPI.Command.Register(Type.GetType(POLICE_NAMESPACE).GetMethod("LicenseCommand"), GetGreedyCommand(ComRes.license, HelpRes.license));
            NAPI.Command.Register(Type.GetType(POLICE_NAMESPACE).GetMethod("BreathalyzerCommand"), GetGreedyCommand(ComRes.breathalyzer, HelpRes.breathalyzer));
            NAPI.Command.Register(Type.GetType(POLICE_NAMESPACE).GetMethod("ComputerCommand"), GetGreedyCommand(ComRes.computer, HelpRes.computer));

            // WeazelNews.cs class
            NAPI.Command.Register(Type.GetType(WEAZEL_NEWS_NAMESPACE).GetMethod("InterviewCommand"), GetGreedyCommand(ComRes.interview, HelpRes.interview));
            NAPI.Command.Register(Type.GetType(WEAZEL_NEWS_NAMESPACE).GetMethod("CutInterviewCommand"), GetGreedyCommand(ComRes.cut_interview, HelpRes.cut_interview));
            NAPI.Command.Register(Type.GetType(WEAZEL_NEWS_NAMESPACE).GetMethod("PrizeCommand"), GetGreedyCommand(ComRes.prize, HelpRes.prize));
            NAPI.Command.Register(Type.GetType(WEAZEL_NEWS_NAMESPACE).GetMethod("AnnounceCommand"), GetGreedyCommand(ComRes.announce, HelpRes.announce));
            NAPI.Command.Register(Type.GetType(WEAZEL_NEWS_NAMESPACE).GetMethod("NewsCommand"), GetGreedyCommand(ComRes.news, HelpRes.news));

            // Globals.cs class
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("StoreCommand"), new RuntimeCommandInfo(ComRes.store));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("ConsumeCommand"), new RuntimeCommandInfo(ComRes.consume));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("InventoryCommand"), new RuntimeCommandInfo(ComRes.inventory));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("PurchaseCommand"), new RuntimeCommandInfo(ComRes.purchase));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("SellCommand"), GetGreedyCommand(ComRes.sell, HelpRes.sell));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("HelpCommand"), new RuntimeCommandInfo(ComRes.help));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("WelcomeCommand"), new RuntimeCommandInfo(ComRes.welcome));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("ShowCommand"), GetGreedyCommand(ComRes.show, HelpRes.show));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("PayCommand"), GetGreedyCommand(ComRes.pay, HelpRes.pay));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("GiveCommand"), GetGreedyCommand(ComRes.give, HelpRes.give));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("CancelCommand"), new RuntimeCommandInfo(ComRes.cancel, HelpRes.cancel));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("AcceptCommand"), new RuntimeCommandInfo(ComRes.accept, HelpRes.accept));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("PickUpCommand"), new RuntimeCommandInfo(ComRes.pick_up));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("DropCommand"), new RuntimeCommandInfo(ComRes.drop));
            NAPI.Command.Register(Type.GetType(UTILITY_NAMESPACE).GetMethod("DoorCommand"), new RuntimeCommandInfo(ComRes.door));

            // Furniture.cs class
            NAPI.Command.Register(Type.GetType(FURNITURE_NAMESPACE).GetMethod("FurnitureCommand"), new RuntimeCommandInfo(ComRes.furniture, HelpRes.furniture));

            // House.cs class
            NAPI.Command.Register(Type.GetType(HOUSE_NAMESPACE).GetMethod("RentableCommand"), new RuntimeCommandInfo(ComRes.rentable, HelpRes.rentable));
            NAPI.Command.Register(Type.GetType(HOUSE_NAMESPACE).GetMethod("RentCommand"), new RuntimeCommandInfo(ComRes.rent));
            NAPI.Command.Register(Type.GetType(HOUSE_NAMESPACE).GetMethod("UnrentCommand"), new RuntimeCommandInfo(ComRes.unrent));
            NAPI.Command.Register(Type.GetType(HOUSE_NAMESPACE).GetMethod("WardrobeCommand"), new RuntimeCommandInfo(ComRes.wardrobe));

            // Fishing.cs class
            NAPI.Command.Register(Type.GetType(FISHING_NAMESPACE).GetMethod("FishCommand"), new RuntimeCommandInfo(ComRes.fish));

            // Garbage.cs class
            NAPI.Command.Register(Type.GetType(GARBAGE_NAMESPACE).GetMethod("GarbageCommand"), new RuntimeCommandInfo(ComRes.garbage, HelpRes.garbage));

            // Hooker.cs class
            NAPI.Command.Register(Type.GetType(HOOKER_NAMESPACE).GetMethod("ServiceCommand"), GetGreedyCommand(ComRes.service, HelpRes.service));

            // Job.cs class
            NAPI.Command.Register(Type.GetType(JOB_NAMESPACE).GetMethod("JobCommand"), new RuntimeCommandInfo(ComRes.job, HelpRes.job));
            NAPI.Command.Register(Type.GetType(JOB_NAMESPACE).GetMethod("DutyCommand"), new RuntimeCommandInfo(ComRes.duty));
            NAPI.Command.Register(Type.GetType(JOB_NAMESPACE).GetMethod("OrdersCommand"), new RuntimeCommandInfo(ComRes.orders));

            // Mechanic.cs class
            NAPI.Command.Register(Type.GetType(MECHANIC_NAMESPACE).GetMethod("RepairCommand"), new RuntimeCommandInfo(ComRes.repair, HelpRes.repair));
            NAPI.Command.Register(Type.GetType(MECHANIC_NAMESPACE).GetMethod("RepaintCommand"), new RuntimeCommandInfo(ComRes.repaint, HelpRes.repaint));
            NAPI.Command.Register(Type.GetType(MECHANIC_NAMESPACE).GetMethod("TunningCommand"), new RuntimeCommandInfo(ComRes.tunning));

            // Thief.cs class
            NAPI.Command.Register(Type.GetType(THIEF_NAMESPACE).GetMethod("ForceCommand"), new RuntimeCommandInfo(ComRes.force));
            NAPI.Command.Register(Type.GetType(THIEF_NAMESPACE).GetMethod("StealCommand"), new RuntimeCommandInfo(ComRes.steal));
            NAPI.Command.Register(Type.GetType(THIEF_NAMESPACE).GetMethod("HotwireCommand"), new RuntimeCommandInfo(ComRes.hotwire));
            NAPI.Command.Register(Type.GetType(THIEF_NAMESPACE).GetMethod("PawnCommand"), new RuntimeCommandInfo(ComRes.pawn));

            // Trucker.cs class
            NAPI.Command.Register(Type.GetType(TRUCKER_NAMESPACE).GetMethod("DeliverCommand"), new RuntimeCommandInfo(ComRes.deliver));

            // Parking.cs class
            NAPI.Command.Register(Type.GetType(PARKING_NAMESPACE).GetMethod("ParkCommand"), new RuntimeCommandInfo(ComRes.park));
            NAPI.Command.Register(Type.GetType(PARKING_NAMESPACE).GetMethod("UnparkCommand"), new RuntimeCommandInfo(ComRes.unpark, HelpRes.unpark));

            // Vehicles.cs class
            NAPI.Command.Register(Type.GetType(VEHICLES_NAMESPACE).GetMethod("SeatbeltCommand"), new RuntimeCommandInfo(ComRes.seatbelt));
            NAPI.Command.Register(Type.GetType(VEHICLES_NAMESPACE).GetMethod("LockCommand"), new RuntimeCommandInfo(ComRes._lock));
            NAPI.Command.Register(Type.GetType(VEHICLES_NAMESPACE).GetMethod("HoodCommand"), new RuntimeCommandInfo(ComRes.hood));
            NAPI.Command.Register(Type.GetType(VEHICLES_NAMESPACE).GetMethod("TrunkCommand"), new RuntimeCommandInfo(ComRes.trunk, HelpRes.trunk));
            NAPI.Command.Register(Type.GetType(VEHICLES_NAMESPACE).GetMethod("KeysCommand"), new RuntimeCommandInfo(ComRes.keys, HelpRes.keys));
            NAPI.Command.Register(Type.GetType(VEHICLES_NAMESPACE).GetMethod("LocateCommand"), new RuntimeCommandInfo(ComRes.locate, HelpRes.locate));
            NAPI.Command.Register(Type.GetType(VEHICLES_NAMESPACE).GetMethod("RefuelCommand"), new RuntimeCommandInfo(ComRes.refuel, HelpRes.refuel));
            NAPI.Command.Register(Type.GetType(VEHICLES_NAMESPACE).GetMethod("FillCommand"), new RuntimeCommandInfo(ComRes.fill));
            NAPI.Command.Register(Type.GetType(VEHICLES_NAMESPACE).GetMethod("ScrapCommand"), new RuntimeCommandInfo(ComRes.scrap));

            // Weapons.cs class
            NAPI.Command.Register(Type.GetType(WEAPONS_NAMESPACE).GetMethod("WeaponsEventCommand"), new RuntimeCommandInfo(ComRes.weapons_event));
        }

        private static RuntimeCommandInfo GetGreedyCommand(string name, string help)
        {
            // Add the GreedyArg argument to the command
            return new RuntimeCommandInfo(name, help) { GreedyArg = true };
        }
    }
}
