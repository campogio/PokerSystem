using GTANetworkAPI;
using WiredPlayers.Data.Temporary;

namespace WiredPlayers.Utility
{
    public class Coordinates
    {
        public static readonly Vector3 CarShop = new Vector3(-215.6841f, 6219.168f, 31.49166f);
        public static readonly Vector3 MotorbikeShop = new Vector3(2129.325f, 4794.172f, 40.88499f);
        public static readonly Vector3 BoatShop = new Vector3(1529.877f, 3778.535f, 34.51152f);

        public static readonly Vector3 TestCarSpawn = new Vector3(-238.6294f, 6196.433f, 31.48921f);
        public static readonly Vector3 TestMotorbikeSpawn = new Vector3(2150.572f, 4798.39f, 41.11817f);
        public static readonly Vector3 TestBoatSpawn = new Vector3(1352.241f, 3750.85f, 30.10234f);

        public static readonly Vector3 TestCarCheckpoint = new Vector3(-239.7822f, 6231.539f, 30.70019f);
        public static readonly Vector3 TestMotorbikeCheckpoint = new Vector3(2134.755f, 4777.85f, 40.97029f);
        public static readonly Vector3 TestBoatCheckpoint = new Vector3(1427.16f, 3761.225f, 30.42179f);

        public static readonly Vector3 DrivingSchool = new Vector3(-227.6895f, 6333.742f, 32.41962f);

        public static readonly Vector3 LobbySpawn = new Vector3(402.9364f, -996.7154f, -99.81024f);
        public static readonly Vector3 LobbyRotation = new Vector3(0.0f, 0.0f, 180.0f);
        public static readonly Vector3 WorldSpawn = new Vector3(-136.0034f, 6198.949f, 32.38448f);

        public static readonly Vector3 PoliceLockers = new Vector3(450.8223f, -992.0941f, 30.68958f);
        public static readonly Vector3 EmergencyLockers = new Vector3(268.8305f, -1363.443f, 24.53779f);
        public static readonly Vector3 PaletoLockers = new Vector3(-448.7167f, 6011.534f, 31.71639f);
        public static readonly Vector3 SandyLockers = new Vector3(1852.255f, 3689.962f, 34.26704f);

        public static readonly Vector3 Workshop = new Vector3(-1204.13f, -1489.49f, 4.34967f);
        public static readonly Vector3 Electronics = new Vector3(-1148.98f, -1608.94f, 4.41592f);
        public static readonly Vector3 Police = new Vector3(-1111.952f, -824.9194f, 19.31578f);
        public static readonly Vector3 TownHall = new Vector3(-136.4768f, 6198.505f, 32.38424f);
        public static readonly Vector3 License = new Vector3(-227.5136f, 6321.819f, 31.46245f);
        public static readonly Vector3 Vanilla = new Vector3(120f, -1400f, 30f);
        public static readonly Vector3 Hospital = new Vector3(-242.862f, 6325.652f, 32.42619f);
        public static readonly Vector3 News = new Vector3(-600f, -950f, 25f);
        public static readonly Vector3 Bahama = new Vector3(-1400f, -590f, 30f);
        public static readonly Vector3 Mechanic = new Vector3(492f, -1300f, 30f);
        public static readonly Vector3 Dumper = new Vector3(49.44239f, 6558.004f, 32.18963f);

        public static readonly Vector3 SeaLookingRotation = new Vector3(0.0f, 0.0f, 52.532f);

        public static readonly Vector3 CrateDeliver = new Vector3(-2085.543f, 2600.857f, -0.4712417f);

        public static readonly Vector3 JailOoc = new Vector3(1651.441f, 2569.83f, 45.56486f);

        public static readonly Vector3[] JailSpawns = new Vector3[]
        {
            // Cells
            new Vector3(460.0685f, -993.9847f, 24.91487f),
            new Vector3(459.6115f, -998.0204f, 24.91487f),
            new Vector3(459.8612f, -1001.641f, 24.91487f),

            // IC jail's exit
            new Vector3(463.6655f, -990.8979f, 24.91487f),

            // OOC jail's exit
            new Vector3(-1285.544f, -567.0439f, 31.71239f)
        };

        public static readonly SpawnModel[] CarShopSpawns = new SpawnModel[]
        {
            new SpawnModel(new Vector3(-207.5757f, 6219.714f, 31.49114f), 218.0f),
            new SpawnModel(new Vector3(-205.2744f, 6221.958f, 31.49089f), 218.0f),
            new SpawnModel(new Vector3(-203.0463f, 6224.42f, 31.4899f), 218.0f),
            new SpawnModel(new Vector3(-200.6914f, 6226.81f, 31.49411f), 218.0f),
            new SpawnModel(new Vector3(-198.3211f, 6229.246f, 31.50067f), 218.0f)
        };

        public static readonly SpawnModel[] BikeShopSpawns = new SpawnModel[]
        {
            new SpawnModel(new Vector3(2141.214f, 4824.846f, 41.26408f), 180.0f),
            new SpawnModel(new Vector3(2135.951f, 4822.354f, 41.23187f), 180.0f),
            new SpawnModel(new Vector3(2129.432f, 4819.786f, 41.24192f), 180.0f),
            new SpawnModel(new Vector3(2123.531f, 4817.106f, 41.25154f), 180.0f)
        };

        public static readonly SpawnModel[] BoatShopSpawns = new SpawnModel[]
        {
            new SpawnModel(new Vector3(1477.21f, 3790.257f, 29.74804f), 180.0f),
            new SpawnModel(new Vector3(1456.734f, 3786.735f, 29.78764f), 180.0f)
        };

        public static readonly Vector3[] StolenCarCheckpoints = new Vector3[]
        {
            new Vector3(210.473, -848.802, 29.75367)
        };

        public static readonly Vector3[] PawnShops = new Vector3[]
        {
            new Vector3(-44.59276f, 6447.872f, 31.47821f),
            new Vector3(1929.779f, 3721.547f, 32.8097f)
        };

        public static Vector3[] AtmArray = new Vector3[]
        {
            new Vector3(-846.6537, -341.509, 37.6685),
            new Vector3(1153.747, -326.7634, 69.2050),
            new Vector3(285.6829, 143.4019, 104.169),
            new Vector3(-847.204, -340.4291, 37.6793),
            new Vector3(-1410.736, -98.9279, 51.397),
            new Vector3(-1410.183, -100.6454, 51.3965),
            new Vector3(-2295.853, 357.9348, 173.6014),
            new Vector3(-2295.069, 356.2556, 173.6014),
            new Vector3(-2294.3, 354.6056, 173.6014),
            new Vector3(-282.7141, 6226.43, 30.4965),
            new Vector3(-386.4596, 6046.411, 30.474),
            new Vector3(24.5933, -945.543, 28.333),
            new Vector3(5.686, -919.9551, 28.4809),
            new Vector3(296.1756, -896.2318, 28.2901),
            new Vector3(296.8775, -894.3196, 28.2615),
            new Vector3(-846.6537, -341.509, 37.6685),
            new Vector3(-847.204, -340.4291, 37.6793),
            new Vector3(-1410.736, -98.9279, 51.397),
            new Vector3(-1410.183, -100.6454, 51.3965),
            new Vector3(-2295.853, 357.9348, 173.6014),
            new Vector3(-2295.069, 356.2556, 173.6014),
            new Vector3(-2294.3, 354.6056, 173.6014),
            new Vector3(-282.7141, 6226.43, 30.4965),
            new Vector3(-386.4596, 6046.411, 30.474),
            new Vector3(24.5933, -945.543, 28.333),
            new Vector3(5.686, -919.9551, 28.4809),
            new Vector3(296.1756, -896.2318, 28.2901),
            new Vector3(296.8775, -894.3196, 28.2615),
            new Vector3(-712.9357, -818.4827, 22.7407),
            new Vector3(-710.0828, -818.4756, 22.7363),
            new Vector3(289.53, -1256.788, 28.4406),
            new Vector3(289.2679, -1282.32, 28.6552),
            new Vector3(-1569.84, -547.0309, 33.9322),
            new Vector3(-1570.765, -547.7035, 33.9322),
            new Vector3(-1305.708, -706.6881, 24.3145),
            new Vector3(-2071.928, -317.2862, 12.3181),
            new Vector3(-821.8936, -1081.555, 10.1366),
            new Vector3(-712.9357, -818.4827, 22.7407),
            new Vector3(-710.0828, -818.4756, 22.7363),
            new Vector3(289.53, -1256.788, 28.4406),
            new Vector3(289.2679, -1282.32, 28.6552),
            new Vector3(-1569.84, -547.0309, 33.9322),
            new Vector3(-1570.765, -547.7035, 33.9322),
            new Vector3(-1305.708, -706.6881, 24.3145),
            new Vector3(-2071.928, -317.2862, 12.3181),
            new Vector3(-821.8936, -1081.555, 10.1366),
            new Vector3(-867.013, -187.9928, 36.8822),
            new Vector3(-867.9745, -186.3419, 36.8822),
            new Vector3(-3043.835, 594.1639, 6.7328),
            new Vector3(-3241.455, 997.9085, 11.5484),
            new Vector3(-204.0193, -861.0091, 29.2713),
            new Vector3(118.6416, -883.5695, 30.1395),
            new Vector3(-256.6386, -715.8898, 32.7883),
            new Vector3(-259.2767, -723.2652, 32.7015),
            new Vector3(-254.5219, -692.8869, 32.5783),
            new Vector3(-867.013, -187.9928, 36.8822),
            new Vector3(-867.9745, -186.3419, 36.8822),
            new Vector3(-3043.835, 594.1639, 6.7328),
            new Vector3(-3241.455, 997.9085, 11.5484),
            new Vector3(-204.0193, -861.0091, 29.2713),
            new Vector3(118.6416, -883.5695, 30.1395),
            new Vector3(-256.6386, -715.8898, 32.7883),
            new Vector3(-259.2767, -723.2652, 32.7015),
            new Vector3(-254.5219, -692.8869, 32.5783),
            new Vector3(89.8134, 2.8803, 67.3521),
            new Vector3(-617.8035, -708.8591, 29.0432),
            new Vector3(-617.8035, -706.8521, 29.0432),
            new Vector3(-614.5187, -705.5981, 30.224),
            new Vector3(-611.8581, -705.5981, 30.224),
            new Vector3(-537.8052, -854.9357, 28.2754),
            new Vector3(-526.7791, -1223.374, 17.4527),
            new Vector3(-1315.416, -834.431, 15.9523),
            new Vector3(-1314.466, -835.6913, 15.9523),
            new Vector3(89.8134, 2.8803, 67.3521),
            new Vector3(-617.8035, -708.8591, 29.0432),
            new Vector3(-617.8035, -706.8521, 29.0432),
            new Vector3(-614.5187, -705.5981, 30.224),
            new Vector3(-611.8581, -705.5981, 30.224),
            new Vector3(-537.8052, -854.9357, 28.2754),
            new Vector3(-526.7791, -1223.374, 17.4527),
            new Vector3(-1315.416, -834.431, 15.9523),
            new Vector3(-1314.466, -835.6913, 15.9523),
            new Vector3(-1205.378, -326.5286, 36.851),
            new Vector3(-1206.142, -325.0316, 36.851),
            new Vector3(147.4731, -1036.218, 28.3678),
            new Vector3(145.8392, -1035.625, 28.3678),
            new Vector3(-1205.378, -326.5286, 36.851),
            new Vector3(-1206.142, -325.0316, 36.851),
            new Vector3(147.4731, -1036.218, 28.3678),
            new Vector3(145.8392, -1035.625, 28.3678),
            new Vector3(-1109.797, -1690.808, 4.375014),
            new Vector3(-721.1284, -415.5296, 34.98175),
            new Vector3(130.1186, -1292.669, 29.26953),
            new Vector3(129.7023, -1291.954, 29.26953),
            new Vector3(129.2096, -1291.14, 29.26953),
            new Vector3(288.8256, -1282.364, 29.64128),
            new Vector3(1077.768, -776.4548, 58.23997),
            new Vector3(527.2687, -160.7156, 57.08937),
            new Vector3(-57.64693, -92.66162, 57.77995),
            new Vector3(527.3583, -160.6381, 57.0933),
            new Vector3(-165.1658, 234.8314, 94.92194),
            new Vector3(-165.1503, 232.7887, 94.92194),
            new Vector3(-1091.462, 2708.637, 18.95291),
            new Vector3(1172.492, 2702.492, 38.17477),
            new Vector3(1171.537, 2702.492, 38.17542),
            new Vector3(1822.637, 3683.131, 34.27678),
            new Vector3(1686.753, 4815.806, 42.00874),
            new Vector3(1701.209, 6426.569, 32.76408),
            new Vector3(-1091.42, 2708.629, 18.95568),
            new Vector3(-660.703, -853.971, 24.484),
            new Vector3(-660.703, -853.971, 24.484),
            new Vector3(-1409.782, -100.41, 52.387),
            new Vector3(-1410.279, -98.649, 52.436),
            new Vector3(-2975.014,380.129,14.99909),
            new Vector3(155.9642,6642.763,31.60284),
            new Vector3(174.1721,6637.943,31.57305),
            new Vector3(-97.33363,6455.411,31.46716),
            new Vector3(-95.49733,6457.243,31.46098),
            new Vector3(-303.2701,-829.7642,32.41727),
            new Vector3(-301.6767,-830.1,32.41727),
            new Vector3(-717.6539,-915.6808,19.21559),
            new Vector3(-1391.023, -590.3637, 30.31957),
            new Vector3(1138.311, -468.941, 66.73091),
            new Vector3(1167.086, -456.1151, 66.79015),
            new Vector3(-132.8289f, 6366.315f, 31.4737f),
            new Vector3(1735.206f, 6410.597f, 35.03722f),
            new Vector3(540.5155f, 2671.07f, 42.15653f),
            new Vector3(1968.255f, 3743.691f, 32.34375f),
            new Vector3(1702.953f, 4933.514f, 42.06368f),
            new Vector3(-133.399f, 6366.865f, 31.479f)
        };

        public static Vector3[] CarLicenseCheckpoints = new Vector3[]
        {
            new Vector3(-210.185f, 6332.839f, 30.42618f),
            new Vector3(-292.3593f, 6245.958f, 30.53763f),
            new Vector3(-357.4913f, 6301.83f, 28.99157f),
            new Vector3(-180.3696f, 6465.34f, 29.74923f),
            new Vector3(-126.9398f, 6431.469f, 30.57843f),
            new Vector3(-40.52503f, 6491.655f, 30.51457f),
            new Vector3(68.51f, 6600.582f, 30.50891f),
            new Vector3(136.5284f, 6538.029f, 30.57347f),
            new Vector3(-95.59881f, 6292.953f, 30.46639f),
            new Vector3(-162.9532f, 6351.154f, 30.58549f)
        };

        public static Vector3[] BikeLicenseCheckpoints = new Vector3[]
        {
            new Vector3(-210.185f, 6332.839f, 30.42618f),
            new Vector3(-292.3593f, 6245.958f, 30.53763f),
            new Vector3(-357.4913f, 6301.83f, 28.99157f),
            new Vector3(-180.3696f, 6465.34f, 29.74923f),
            new Vector3(-126.9398f, 6431.469f, 30.57843f),
            new Vector3(-40.52503f, 6491.655f, 30.51457f),
            new Vector3(68.51f, 6600.582f, 30.50891f),
            new Vector3(136.5284f, 6538.029f, 30.57347f),
            new Vector3(-95.59881f, 6292.953f, 30.46639f),
            new Vector3(-162.9532f, 6351.154f, 30.58549f)
        };

        public static Vector3[] FishingPoints = new Vector3[]
        {
            new Vector3(-273.9995f, 6642.273f, 7.39921f),
            new Vector3(-275.8697f, 6640.357f, 7.548759f),
            new Vector3(-278.201f, 6638.141f, 7.552301f),
            new Vector3(-280.1694f, 6636.17f, 7.552289f),
            new Vector3(-282.4903f, 6633.953f, 7.481426f),
            new Vector3(-284.8904f, 6631.555f, 7.339838f),
            new Vector3(-287.5817f, 6629.255f, 7.186343f),
            new Vector3(-287.5817f, 6629.255f, 7.186343f)
        };

        public static Vector3[] EquipmentPoints = new Vector3[]
        {
            new Vector3(-450.0351f, 6016.23f, 31.71639f),
            new Vector3(1856.94f, 3689.781f, 34.26704f)
        };

        public static Vector3[] VehicleDeliveryPlaces = new Vector3[]
        {
            new Vector3(-293.94153f, 6129.4937f, 30.877011f),
            new Vector3(-11.462117f, 6312.465f, 29.607516f)
        };
    }
}