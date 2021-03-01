using RAGE;
using RAGE.Game;

namespace WiredPlayers_Client.globals
{
    class Doors : Events.Script
    {
        public Doors()
        {
            // Lock police front doors
            Object.DoorControl(Misc.GetHashKey("v_ilev_ph_door01"), 434.7479f, -983.2151f, 30.83926f, true, 0, 0, 0);
            Object.DoorControl(Misc.GetHashKey("v_ilev_ph_door01"), 434.7479f, -980.6184f, 30.83926f, true, 0, 0, 0);

            // Lock police back doors
            Object.DoorControl(Misc.GetHashKey("v_ilev_rc_door2"), 469.9679f, -1014.452f, 26.53623f, true, 0, 0, 0);
            Object.DoorControl(Misc.GetHashKey("v_ilev_rc_door2"), 467.3716f, -1014.452f, 26.53623f, true, 0, 0, 0);

            // Lock LSPD cells
            Object.DoorControl(Misc.GetHashKey("v_ilev_ph_cellgate"), 461.8065f, -994.4086f, 25.06443f, true, 0, 0, 0);
            Object.DoorControl(Misc.GetHashKey("v_ilev_ph_cellgate"), 461.8065f, -997.6583f, 25.06443f, true, 0, 0, 0);
            Object.DoorControl(Misc.GetHashKey("v_ilev_ph_cellgate"), 461.8065f, -1001.302f, 25.06443f, true, 0, 0, 0);

            // Open Paleto Sheriff's doors
            Object.DoorControl(Misc.GetHashKey("v_ilev_shrf2door"), -442.66f, 6015.222f, 31.86633f, false, 0, 0, 0);
            Object.DoorControl(Misc.GetHashKey("v_ilev_shrf2door"), -444.4985f, 6017.06f, 31.86633f, false, 0, 0, 0);

            // Open Sandy Sheriff's doors
            Object.DoorControl(Misc.GetHashKey("v_ilev_shrfdoor"), 1855.685f, 3683.93f, 34.59282f, false, 0, 0, 0);

            // Open Simeon's front doors
            Object.DoorControl(Misc.GetHashKey("v_ilev_csr_door_l"), -59.89302f, -1092.952f, 26.88362f, false, 0, 0, 0);
            Object.DoorControl(Misc.GetHashKey("v_ilev_csr_door_r"), -60.54582f, -1094.749f, 26.88872f, false, 0, 0, 0);

            // Open Simeon's parking doors
            Object.DoorControl(Misc.GetHashKey("v_ilev_csr_door_l"), -39.13366f, -1108.218f, 26.7198f, false, 0, 0, 0);
            Object.DoorControl(Misc.GetHashKey("v_ilev_csr_door_r"), -37.33113f, -1108.873f, 26.7198f, false, 0, 0, 0);

            // Lock The Lost Clubhouse's door
            Object.DoorControl(Misc.GetHashKey("v_ilev_lostdoor"), 981.7533f, -102.7987f, 74.84873f, true, 0, 0, 0);
        }

        public static void LockInteriorDoors()
        {
            // Lock supermarket doors
            Object.DoorControl(Misc.GetHashKey("v_ilev_gasdoor"), -713.0732f, -916.5409f, 19.36553f, true, 0, 0, 0);
            Object.DoorControl(Misc.GetHashKey("v_ilev_gasdoor_r"), -710.4722f, -916.5372f, 19.36553f, true, 0, 0, 0);

            // Lock clothes shop
            Object.DoorControl(Misc.GetHashKey("v_ilev_clothmiddoor"), 127.8201f, -211.8274f, 55.22751f, true, 0, 0, 0);

            // Lock barbershop
            Object.DoorControl(Misc.GetHashKey("v_ilev_bs_door"), 132.5569f, -1710.996f, 29.44157f, true, 0, 0, 0);

            // Unlock tattoo shop
            Object.DoorControl(Misc.GetHashKey("v_ilev_ta_door"), -1155.454f, -1424.008f, 5.046147f, true, 0, 0, 0);

            // Lock the Ammunation
            Object.DoorControl(Misc.GetHashKey("v_ilev_gc_door03"), 1699.937f, 3753.42f, 34.85526f, true, 0, 0, 0);
            Object.DoorControl(Misc.GetHashKey("v_ilev_gc_door04"), 1698.176f, 3751.506f, 34.85526f, true, 0, 0, 0);
        }

        public static void UnlockInteriorDoors()
        {
            // Unlock supermarket doors
            Object.DoorControl(Misc.GetHashKey("v_ilev_gasdoor"), -713.0732f, -916.5409f, 19.36553f, false, 0, 0, 0);
            Object.DoorControl(Misc.GetHashKey("v_ilev_gasdoor_r"), -710.4722f, -916.5372f, 19.36553f, false, 0, 0, 0);

            // Unlock clothes shop
            Object.DoorControl(Misc.GetHashKey("v_ilev_clothmiddoor"), 127.8201f, -211.8274f, 55.22751f, false, 0, 0, 0);

            // Unlock barbershop
            Object.DoorControl(Misc.GetHashKey("v_ilev_bs_door"), 132.5569f, -1710.996f, 29.44157f, false, 0, 0, 0);

            // Unlock tattoo shop
            Object.DoorControl(Misc.GetHashKey("v_ilev_ta_door"), -1155.454f, -1424.008f, 5.046147f, false, 0, 0, 0);

            // Unlock the Ammunation
            Object.DoorControl(Misc.GetHashKey("v_ilev_gc_door03"), 1699.937f, 3753.42f, 34.85526f, false, 0, 0, 0);
            Object.DoorControl(Misc.GetHashKey("v_ilev_gc_door04"), 1698.176f, 3751.506f, 34.85526f, false, 0, 0, 0);
        }
    }
}