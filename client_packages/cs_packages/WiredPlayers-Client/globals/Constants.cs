using RAGE;
using WiredPlayers_Client.model;

namespace WiredPlayers_Client.globals
{
    class Constants
    {
        public static readonly ushort INVALID_VALUE = 65535;

        public static readonly string VEHICLE_DOORS_STATE = "VEHICLE_DOORS_STATE";
        public static readonly string VEHICLE_SIREN_SOUND = "VEHICLE_SIREN_SOUND";
        public static readonly string ITEM_ENTITY_RIGHT_HAND = "PLAYER_RIGHT_HAND";
        public static readonly string ITEM_ENTITY_WEAPON_CRATE = "PLAYER_WEAPON_CRATE";

        public static readonly float CONSUME_PER_METER = 0.00065f;

        public static readonly int VEHICLE_SEAT_DRIVER = -1;

        public static readonly float MAX_VOICE_RANGE = 25.0f;

        public static readonly ClothesModel[] ClothesTypes = new ClothesModel[]
        {
            new ClothesModel(0, 1, "clothes.masks"), new ClothesModel(0, 3, "clothes.torso"), new ClothesModel(0, 4, "clothes.legs"),
            new ClothesModel(0, 5, "clothes.bags"), new ClothesModel(0, 6, "clothes.feet"), new ClothesModel(0, 7, "clothes.complements"),
            new ClothesModel(0, 8, "clothes.undershirt"), new ClothesModel(0, 9, "clothes.body-armor"), new ClothesModel(0, 11, "clothes.tops"),
            new ClothesModel(1, 0, "clothes.hats"), new ClothesModel(1, 1, "clothes.glasses"), new ClothesModel(1, 2, "clothes.earrings"),
            new ClothesModel(1, 6, "clothes.watches"), new ClothesModel(1, 7, "clothes.bracelets")
        };

        public static readonly string[] TattooZones = new string[]
        {
            "tattoo.torso", "tattoo.head", "tattoo.left-arm", "tattoo.right-arm", "tattoo.left-leg", "tattoo.right-leg"
        };

        public static readonly FaceOption[] MaleFaceOptions = new FaceOption[]
        {
            new FaceOption("hairdresser.hair", 0, 36), new FaceOption("hairdresser.hair-primary", 0, 63), new FaceOption("hairdresser.hair-secondary", 0, 63),
            new FaceOption("hairdresser.eyebrows", 0, 33), new FaceOption("hairdresser.eyebrows-color", 0, 63), new FaceOption("hairdresser.beard", -1, 36),
            new FaceOption("hairdresser.beard-color", 0, 63)
        };

        public static readonly FaceOption[] FemaleFaceOptions = new FaceOption[]
        {
            new FaceOption("hairdresser.hair", 0, 38), new FaceOption("hairdresser.hair-primary", 0, 63), new FaceOption("hairdresser.hair-secondary", 0, 63),
            new FaceOption("hairdresser.eyebrows", 0, 33), new FaceOption("hairdresser.eyebrows-color", 0, 63)
        };

        public static readonly Procedure[] TownhallProcedures = new Procedure[]
        {
            new Procedure("townhall.identification", 500), new Procedure("townhall.insurance", 2000),
            new Procedure("townhall.taxi", 5000), new Procedure("townhall.fines", 0)
        };

        public static readonly CarPiece[] CarPieceCollection = new CarPiece[]
        {
            new CarPiece(0, "mechanic.spoiler", 250), new CarPiece(1, "mechanic.front-bumper", 250),new CarPiece(2, "mechanic.rear-bumper", 250),
            new CarPiece(3, "mechanic.side-skirt", 250), new CarPiece(4, "mechanic.exhaust", 100), new CarPiece(5, "mechanic.frame", 500),
            new CarPiece(6, "mechanic.grille", 200), new CarPiece(7, "mechanic.hood", 300), new CarPiece(8, "mechanic.fender", 100),
            new CarPiece(9, "mechanic.right-fender", 100), new CarPiece(10, "mechanic.roof", 400), new CarPiece(14, "mechanic.horn", 100),
            new CarPiece(15, "mechanic.suspension", 900), new CarPiece(22, "mechanic.xenon", 150), new CarPiece(23, "mechanic.front-wheels", 100),
            new CarPiece(24, "mechanic.back-wheels", 100), new CarPiece(25, "mechanic.plaque", 100), new CarPiece(27, "mechanic.trim-design", 800),
            new CarPiece(28, "mechanic.ornaments", 150), new CarPiece(33, "mechanic.steering-wheel", 100), new CarPiece(34, "mechanic.shift-lever", 100),
            new CarPiece(38, "mechanic.hydraulics", 1200), new CarPiece(69, "mechanic.window-tint", 200)
        };

        public static readonly Vector3[] TruckerCrates = new Vector3[]
        {
            new Vector3(1275.89f, -3282.81f, 5.90159f),
            new Vector3(1275.54f, -3287.54f, 5.90159f),
            new Vector3(1275.4f, -3293.04f, 5.90159f)
        };

        public static readonly string[] ValidWeapons = new string[]
        {
            "weapon_pistol", "weapon_combatpistol", "weapon_pistol50", "weapon_snspistol", "weapon_heavypistol", "weapon_vintagepistol", "weapon_marksmanpistol",
            "weapon_revolver", "weapon_appistol", "weapon_flaregun", "weapon_microsmg", "weapon_machinepistol", "weapon_smg", "weapon_combatpdw", "weapon_mg",
            "weapon_combatmg", "weapon_gusenberg", "weapon_minismg", "weapon_assaultrifle", "weapon_carbinerifle", "weapon_advancedrifle", "weapon_specialcarbine",
            "weapon_bullpuprifle", "weapon_compactrifle", "weapon_sniperrifle", "weapon_heavysniper", "weapon_marksmanrifle", "weapon_pumpshotgun", "weapon_sawnoffshotgun",
            "weapon_assaultshotgun", "weapon_bullpupshotgun", "weapon_musket", "weapon_heavyshotgun", "weapon_dbshotgun", "weapon_autoshotgun"
        };
        
        // Author: Arch
        // Date: 05/16/2019 1:41 AM
        // Male skins on the creator
        public static readonly string[] MaleSkins = new string[]
        {
            "u_m_y_abner", "a_m_m_acult_01", "a_m_o_acult_01", "a_m_y_acult_01", "a_m_o_acult_02", "a_m_y_acult_02", "a_m_m_afriamer_01", "ig_mp_agent14", "csb_agent",
            "s_m_y_airworker", "u_m_m_aldinapoli", "s_m_y_ammucity_01", "s_m_m_ammucountry", "ig_andreas", "u_m_y_antonb", "g_m_m_armboss_01", "g_m_m_armgoon_01",
            "g_m_y_armgoon_02", "g_m_m_armlieut_01", "s_m_y_armymech_01", "s_m_y_autopsy_01", "s_m_m_autoshop_01", "s_m_m_autoshop_02", "ig_money", "g_m_y_azteca_01",
            "u_m_y_babyd", "g_m_y_ballaeast_01", "g_m_y_ballaorig_01", "g_m_y_ballaorig_01", "ig_ballasog", "g_m_y_ballasout_01", "u_m_m_bankman", "ig_bankman",
            "s_m_y_barman_01", "ig_barry", "s_m_y_baywatch_01", "u_m_y_baygor", "a_m_m_beach_01", "a_m_o_beach_01", "a_m_y_beach_01", "a_m_m_beach_02", "a_m_y_beach_02",
            "a_m_y_beach_03", "a_m_y_beachvesp_01", "a_m_y_beachvesp_02", "ig_benny", "ig_bestmen", "ig_beverly", "a_m_m_bevhills_01", "a_m_y_bevhills_01", "a_m_m_bevhills_02",
            "a_m_y_bevhills_02", "u_m_m_bikehire_01", "mp_m_boatstaff_01", "s_m_m_bouncer_01", "ig_brad", "a_m_y_breakdance_01", "u_m_y_burgerdrug_01", "s_m_y_busboy_01",
            "a_m_y_busicas_01", "a_m_m_business_01", "a_m_y_business_01", "a_m_y_business_02", "a_m_y_business_03", "s_m_o_busker_01", "ig_car3guy1", "ig_car3guy2",
            "cs_carbuyer", "s_m_m_ccrew_01", "s_m_y_chef_01", "ig_chef2", "g_m_m_chemwork_01", "g_m_m_chiboss_01", "g_m_m_chigoon_01", "g_m_m_chigoon_02", "csb_chin_goon",
            "u_m_y_chip", "s_m_m_ciasec_01", "ig_clay", "ig_claypain", "ig_cletus", "s_m_m_cntrybar_01", "s_m_y_construct_01", "s_m_y_construct_02", "csb_customer",
            "u_m_y_cyclist_01", "a_m_y_cyclist_01", "ig_dale", "ig_davenorton", "s_m_y_dealer_01", "ig_devin", "s_m_y_devinsec_01", "a_m_y_dhill_01", "u_m_m_doa_01",
            "s_m_m_dockwork_01", "s_m_y_dockwork_01", "s_m_m_doctor_01", "ig_dom", "s_m_y_doorman_01", "a_m_y_downtown_01", "ig_dreyfuss", "ig_drfriedlander",
            "s_m_y_dwservice_01", "Name: s_m_y_dwservice_02", "a_m_m_eastsa_01", "a_m_y_eastsa_01", "a_m_m_eastsa_02", "a_m_y_eastsa_02", "u_m_m_edtoh", "a_m_y_epsilon_01",
            "a_m_y_epsilon_02", "mp_m_exarmy_01", "ig_fabien", "s_m_y_factory_01", "g_m_y_famca_01", "mp_m_famdd_01", "g_m_y_famdnf_01", "g_m_y_famfor_01", "a_m_m_farmer_01",
            "a_m_m_fatlatin_01", "ig_fbisuit_01", "u_m_m_fibarchitect", "u_m_y_fibmugger_01", "s_m_m_fiboffice_01", "s_m_m_fiboffice_02", "u_m_m_filmdirector", "u_m_o_finguru_01",
            "ig_floyd", "csb_fos_rep", "ig_g", "s_m_m_gaffer_01", "s_m_y_garbage", "s_m_m_gardener_01", "a_m_y_gay_01", "a_m_y_gay_02", "csb_g", "a_m_m_genfat_01",
            "a_m_m_genfat_02", "a_m_o_genstreet_01", "a_m_y_genstreet_01", "a_m_y_genstreet_02", "s_m_m_gentransport", "u_m_m_glenstank_01", "a_m_m_golfer_01", "a_m_y_golfer_01",
            "u_m_m_griff_01", "s_m_y_grip_01", "ig_groom", "csb_grove_str_dlr", "u_m_y_guido_01", "u_m_y_gunvend_01", "hc_hacker", "s_m_m_hairdress_01", "ig_hao", "a_m_m_hasjew_01",
            "a_m_y_hasjew_01", "s_m_m_highsec_01", "s_m_m_highsec_02", "a_m_y_hiker_01", "a_m_m_hillbilly_01", "a_m_m_hillbilly_02", "u_m_y_hippie_01", "a_m_y_hippy_01",
            "a_m_y_hipster_01", "a_m_y_hipster_03", "csb_hugh", "csb_imran", "a_m_m_indian_01", "a_m_y_indian_01", "csb_jackhowitzer", "s_m_m_janitor", "ig_jay_norris",
            "u_m_m_jesus_01", "a_m_y_jetski_01", "u_m_m_jewelsec_01", "u_m_m_jewelthief", "ig_jimmyboston", "ig_jimmydisanto", "ig_joeminuteman", "ig_josef", "ig_josh",
            "a_m_y_juggalo_01", "g_m_m_korboss_01", "g_m_y_korean_01", "g_m_y_korean_02", "g_m_y_korlieut_01", "a_m_m_ktown_01", "a_m_o_ktown_01", "a_m_y_ktown_01",
            "a_m_y_ktown_02", "ig_lamardavis", "s_m_m_lathandy_01", "a_m_y_latino_01", "ig_lazlow", "ig_lestercrest", "ig_lifeinvad_01", "s_m_m_lifeinvad_01", "ig_lifeinvad_02",
            "g_m_y_lost_01", "s_m_m_linecook", "g_m_y_lost_02", "g_m_y_lost_03", "s_m_m_lsmetro_01", "a_m_m_malibu_01", "u_m_y_mani", "ig_manuel", "s_m_m_mariachi_01",
            "s_m_y_marine_02", "u_m_m_markfost", "ig_marnie", "cs_martinmadrazo", "ig_maryann", "a_m_y_methhead_01", "g_m_m_mexboss_01", "g_m_m_mexboss_02", "a_m_m_mexcntry_01",
            "g_m_y_mexgang_01", "g_m_y_mexgoon_01", "g_m_y_mexgoon_02", "g_m_y_mexgoon_03", "a_m_m_mexlabor_01", "a_m_y_mexthug_01", "ig_michelle", "s_m_m_migrant_01",
            "u_m_y_militarybum", "ig_milton", "a_m_y_motox_02", "cs_movpremmale", "s_m_m_movprem_01", "mp_g_m_pros_01", "ig_mrk", "a_m_y_musclbeac_01", "a_m_y_musclbeac_02",
            "ig_nervousron", "ig_nigel", "a_m_m_og_boss_01", "ig_old_man1a", "ig_old_man2", "ig_omega", "ig_oneil", "ig_ortega", "csb_oscar", "a_m_m_paparazzi_01",
            "u_m_y_paparazzi", "ig_paper", "u_m_y_party_01", "u_m_m_partytarget", "s_m_y_pestcont_01", "hc_driver", "s_m_m_pilot_01", "s_m_y_pilot_01", "s_m_m_pilot_02",
            "g_m_y_pologoon_01", "g_m_y_pologoon_02", "a_m_m_polynesian_01", "a_m_y_polynesian_01", "ig_popov", "s_m_m_postal_01", "s_m_m_postal_02", "ig_priest",
            "s_m_y_prismuscl_01", "u_m_y_prisoner_01", "s_m_y_prisoner_01", "u_m_y_proldriver_01", "a_m_m_prolhost_01", "u_m_m_prolsec_01", "csb_prolsec", "ig_prolsec_02",
            "ig_ramp_gang", "ig_ramp_hic", "ig_ramp_hipster", "ig_ramp_mex", "ig_rashcosvki", "csb_reporter", "u_m_m_rivalpap", "a_m_y_roadcyc_01", "s_m_y_robber_01",
            "ig_roccopelosi", "a_m_y_runner_01", "a_m_y_runner_02", "a_m_m_rurmeth_01", "ig_russiandrunk", "a_m_m_salton_01", "a_m_o_salton_01", "a_m_y_salton_01",
            "a_m_m_salton_02", "a_m_m_salton_03", "a_m_m_salton_04", "g_m_y_salvaboss_01", "g_m_y_salvagoon_01", "g_m_y_salvagoon_02", "g_m_y_salvagoon_03", "g_m_y_salvagoon_03",
            "mp_m_shopkeep_01", "s_m_y_shop_mask", "ig_siemonyetarian", "a_m_m_skater_01", "a_m_y_skater_01", "a_m_y_skater_02", "a_m_m_skidrow_01", "a_m_m_socenlat_01",
            "ig_solomon", "a_m_m_soucent_01", "a_m_o_soucent_01", "a_m_m_soucent_02", "a_m_o_soucent_02", "a_m_y_soucent_02", "a_m_m_soucent_03", "a_m_o_soucent_03",
            "a_m_y_soucent_03", "a_m_m_soucent_04", "a_m_y_soucent_04", "u_m_m_spyactor", "a_m_y_stbla_01", "a_m_y_stbla_02", "ig_stevehains", "a_m_y_stlat_01", "a_m_m_stlat_02",
            "ig_stretch", "s_m_m_strpreach_01", "g_m_y_strpunk_01", "g_m_y_strpunk_02", "s_m_m_strvend_01", "s_m_y_strvend_01", "a_m_y_stwhi_01", "a_m_y_stwhi_02",
            "a_m_y_sunbathe_01", "a_m_y_surfer_01", "ig_taocheng", "ig_taostranslator", "u_m_o_taphillbilly", "u_m_y_tattoo_01", "a_m_m_tennis_01", "ig_tenniscoach", "ig_terry",
            "cs_tom", "ig_tomepsilon", "a_m_m_tourist_01", "ig_trafficwarden", "u_m_o_tramp_01", "a_m_m_tramp_01", "a_m_o_tramp_01", "a_m_m_trampbeac_01", "s_m_m_trucker_01",
            "ig_tylerdix", "csb_undercover", "s_m_m_ups_01", "s_m_y_uscg_01", "mp_m_g_vagfun_01", "ig_vagspeak", "s_m_y_valet_01", "a_m_y_vinewood_02", "a_m_y_vinewood_03",
            "a_m_y_vinewood_04", "ig_wade", "s_m_y_waiter_01", "ig_chengsr", "u_m_m_willyfist", "s_m_y_winclean_01", "s_m_y_xmech_02", "a_m_y_yoga_01", "ig_zimbor",
            "g_m_importexport_01", "ig_agent", "ig_malc", "mp_m_cocaine_01", "mp_m_counterfeit_01", "mp_m_execpa_01", "mp_m_forgery_01", "mp_m_meth_01", "mp_m_securoguard_01",
            "mp_m_waremech_01", "mp_m_weapexp_01", "mp_m_weapwork_01", "mp_m_weed_01", "s_m_y_xmech_02_mp", "ig_lestercrest_2", "ig_avon"
        };

        // Female skins on the creator
        public static readonly string[] FemaleSkins = new string[]
        {
            "ig_abigail", "csb_abigail", "s_f_y_airhostess_01", "ig_amandatownley", "csb_anita", "ig_ashley", "g_f_y_ballas_01", "s_f_y_bartender_01", "s_f_y_baywatch_01",
            "a_f_m_beach_01", "a_f_y_beach_01", "a_f_m_bevhills_01", "a_f_y_bevhills_01", "a_f_m_bevhills_02", "a_f_y_bevhills_02", "a_f_y_bevhills_03", "a_f_y_bevhills_04",
            "u_f_y_bikerchic", "mp_f_boatstaff_01", "ig_bride", "a_f_y_business_01", "a_f_m_business_02", "a_f_y_business_02", "a_f_y_business_03", "a_f_y_business_04",
            "u_f_y_comjane", "cs_debra", "ig_denise", "csb_denise_friend", "a_f_m_downtown_01", "mp_f_deadhooker", "a_f_m_eastsa_01", "a_f_y_eastsa_01", "a_f_m_eastsa_02",
            "a_f_y_eastsa_02", "a_f_y_eastsa_03", "a_f_y_epsilon_01", "s_f_y_factory_01", "g_f_y_families_01", "a_f_m_fatbla_01", "a_f_m_fatcult_01", "a_f_m_fatwhite_01",
            "s_f_m_fembarber", "a_f_y_fitness_01", "a_f_y_fitness_02", "a_f_y_genhot_01", "a_f_o_genstreet_01", "a_f_y_golfer_01", "cs_guadalope", "cs_gurk", "a_f_y_hiker_01",
            "a_f_y_hippie_01", "a_f_y_hipster_01", "a_m_y_hipster_02", "a_f_y_hipster_04", "s_f_y_hooker_01", "s_f_y_hooker_02", "s_f_y_hooker_03", "u_f_y_hotposh_01",
            "ig_hunter", "a_f_o_indian_01", "a_f_y_indian_01", "ig_janet", "u_f_y_jewelass_01", "ig_jewelass", "a_f_y_juggalo_01", "ig_karen_daniels", "ig_kerrymcintosh",
            "a_f_m_ktown_01", "a_f_o_ktown_01", "a_f_m_ktown_02", "g_f_y_lost_01", "ig_magenta", "s_f_m_maid_01", "ig_maude", "u_f_m_miranda", "u_f_y_mistress", "mp_f_misty_01",
            "ig_molly", "cs_movpremf_01", "u_f_o_moviestar", "ig_mrsphillips", "ig_mrs_thornhill", "ig_natalia", "ig_paige", "ig_patricia", "u_f_y_poppymich", "u_f_y_princess",
            "u_f_o_prolhost_01", "a_f_m_prolhost_01", "u_f_m_promourn_01", "a_f_y_runner_01", "a_f_y_rurmeth_01", "a_f_m_salton_01", "a_f_o_salton_01", "a_f_y_scdressy_01",
            "ig_screen_writer", "s_f_m_shop_high", "s_f_y_shop_low", "s_f_y_shop_mid", "a_f_y_skater_01", "a_f_m_skidrow_01", "a_f_m_soucent_01", "a_f_o_soucent_01",
            "a_f_y_soucent_01", "a_m_y_soucent_01", "a_f_m_soucent_02", "a_f_o_soucent_02", "a_f_y_soucent_02", "a_f_y_soucent_03", "a_f_m_soucentmc_01", "u_f_y_spyactress",
            "csb_stripper_01", "s_f_y_stripper_02", "s_f_m_sweatshop_01", "s_f_y_sweatshop_01", "ig_tanisha", "a_f_y_tennis_01", "ig_tonya", "a_f_y_topless_01", "a_f_m_tourist_01",
            "a_f_y_tourist_01", "a_f_y_tourist_02", "ig_tracydisanto", "a_f_m_tramp_01", "a_f_m_trampbeac_01", "g_f_y_vagos_01", "a_f_y_vinewood_03", "a_f_y_vinewood_04",
            "a_f_y_yoga_01", "a_f_y_femaleagent", "g_f_importexport_01", "mp_f_cardesign_01", "mp_f_chbar_01", "mp_f_cocaine_01", "mp_f_counterfeit_01", "mp_f_execpa_01",
            "mp_f_execpa_02", "mp_f_forgery_01", "mp_f_helistaff_01", "mp_f_meth_01", "mp_f_weed_01", "a_f_y_hipster_02", "a_f_y_hipster_03", "s_f_y_migrant_01"
        };
    }
}
