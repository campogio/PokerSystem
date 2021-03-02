-- phpMyAdmin SQL Dump
-- version 5.0.3
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Creato il: Mar 01, 2021 alle 15:02
-- Versione del server: 10.4.14-MariaDB
-- Versione PHP: 7.4.11

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `gtav`
--

-- --------------------------------------------------------

--
-- Struttura della tabella `accounts`
--

CREATE TABLE `accounts` (
  `socialName` varchar(32) NOT NULL,
  `forumName` varchar(32) NOT NULL DEFAULT '',
  `password` varchar(64) NOT NULL,
  `status` int(11) NOT NULL DEFAULT 0,
  `lastCharacter` int(11) NOT NULL DEFAULT -1,
  `lastIp` varchar(16) NOT NULL DEFAULT '',
  `updated` timestamp NOT NULL DEFAULT current_timestamp(),
  `retries` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dump dei dati per la tabella `accounts`
--

INSERT INTO `accounts` (`socialName`, `forumName`, `password`, `status`, `lastCharacter`, `lastIp`, `updated`, `retries`) VALUES
('vaisseau_spatial', '', 'b133a0c0e9bee3be20163d2ad31d6248db292aa6dcb1ee087a2aa50e0fc75ae2', 1, 1, '', '2021-02-21 12:54:11', 0);

-- --------------------------------------------------------

--
-- Struttura della tabella `admin`
--

CREATE TABLE `admin` (
  `source` varchar(24) NOT NULL DEFAULT '',
  `target` varchar(24) NOT NULL DEFAULT '',
  `action` varchar(32) NOT NULL DEFAULT '',
  `time` int(11) NOT NULL DEFAULT 0,
  `reason` varchar(150) NOT NULL DEFAULT '',
  `date` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `animations`
--

CREATE TABLE `animations` (
  `id` int(11) NOT NULL,
  `category` int(11) DEFAULT NULL,
  `description` varchar(32) NOT NULL,
  `library` varchar(64) NOT NULL,
  `name` varchar(64) NOT NULL,
  `flag` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Server animations';

--
-- Dump dei dati per la tabella `animations`
--

INSERT INTO `animations` (`id`, `category`, `description`, `library`, `name`, `flag`) VALUES
(1, 1, 'Facepalm', 'anim@mp_player_intcelebrationmale@face_palm', 'face_palm', 32),
(2, 1, 'Loco', 'anim@mp_player_intcelebrationmale@you_loco', 'you_loco', 32),
(3, 1, 'Flipar', 'anim@mp_player_intcelebrationmale@freakout', 'freakout', 32),
(4, 1, 'Burla', 'anim@mp_player_intcelebrationmale@thumb_on_ears', 'thumb_on_ears', 32),
(5, 1, 'Victoria', 'anim@mp_player_intcelebrationmale@v_sign', 'v_sign', 32),
(6, 1, 'Llorar', 'mp_bank_heist_1', 'f_cower_01', 33),
(7, 1, 'Apresurar', 'missfam4', 'say_hurry_up_a_trevor', 33),
(8, 1, 'Beso', 'mini@hookers_sp', 'idle_a', 32),
(9, 1, 'Que te jodan', 'anim@mp_player_intselfiethe_bird', 'idle_a', 49),
(10, 1, 'Nudillos', 'anim@mp_player_intupperknuckle_crunch', 'idle_a', 49),
(11, 1, 'Aplauso', 'amb@world_human_strip_watch_stand@male_a@idle_a', 'idle_a', 33),
(12, 1, 'Encogerse de hombros', 'gestures@f@standing@casual', 'gesture_shrug_hard', 48),
(13, 1, 'Desesperado', 'misscarsteal4@toilet', 'desperate_toilet_idle_a', 33),
(14, 1, 'Pensativo', 'misscarsteal4@aliens', 'rehearsal_base_idle_director', 49),
(15, 2, 'Recoger', 'anim@heists@money_grab@duffel', 'loop', 33),
(16, 2, 'Agacharse', 'misscarstealfinalecar_5_ig_3', 'crouchloop', 33),
(17, 2, 'Arrodillarse', 'amb@medic@standing@kneel@base', 'base', 33),
(18, 2, 'Hablar', 'amb@code_human_police_crowd_control@idle_a', 'idle_b', 33),
(19, 2, 'Excavar', 'missmic1leadinoutmic_1_mcs_2', '_leadin_trevor', 33),
(20, 2, 'Teclear', 'mp_fbi_heist', 'loop', 33),
(21, 2, 'Tocar la puerta', 'timetable@jimmy@doorknock@', 'knockdoor_idle', 33),
(22, 2, 'Graffiti', 'switch@franklin@lamar_tagging_wall', 'lamar_tagging_exit_loop_lamar', 33),
(23, 2, 'Beber', 'amb@world_human_drinking_fat@beer@male@idle_a', 'idle_a', 33),
(24, 2, 'Comer', 'mp_player_inteat@burger', 'mp_player_int_eat_burger', 49),
(25, 2, 'Apuntar', 'combat@chg_stance', 'aima_loop', 33),
(26, 2, 'Vomitar', 'missheistpaletoscore1leadinout', 'trv_puking_leadout', 32),
(27, 2, 'Mear', 'missbigscore1switch_trevor_piss', 'piss_loop', 33),
(28, 2, 'Calentar manos', 'amb@world_human_stand_fire@male@base', 'base', 33),
(29, 2, 'Fumar 1', 'amb@world_human_smoking@male@male_a@idle_a', 'idle_c', 33),
(30, 2, 'Fumar 2', 'amb@world_human_smoking@female@idle_a', 'idle_b', 49),
(31, 2, 'Fumar 3', 'mini@hookers_spfrench', 'idle_wait', 33),
(32, 3, 'Limpiar coche', 'switch@franklin@cleaning_car', '001946_01_gc_fras_v2_ig_5_base', 33),
(33, 3, 'Limpiar ventanas', 'timetable@maid@cleaning_window@base', 'base', 33),
(34, 3, 'Limpiar 1', 'missheistdocks2bleadinoutlsdh_2b_int', 'leg_massage_b_floyd', 33),
(35, 3, 'Limpiar 2', 'missfbi_s4mop', 'idle_scrub', 33),
(36, 3, 'Limpiar 3', 'amb@world_human_bum_wash@male@low@idle_a', 'idle_c', 33),
(37, 3, 'Ducha 1', 'anim@mp_yacht@shower@male@', 'male_shower_idle_d', 33),
(38, 3, 'Ducha 2', 'anim@mp_yacht@shower@female@', 'shower_idle_a', 33),
(39, 4, 'Mecánico 1', 'amb@world_human_vehicle_mechanic@male@idle_a', 'idle_a', 33),
(40, 4, 'Mecánico 2', 'mini@repair', 'fixing_a_ped', 1),
(41, 4, 'Mecánico 3', 'missheistdockssetup1ig_10@laugh', 'laugh_pipe_worker1', 33),
(42, 4, 'RCP 1', 'mini@cpr@char_a@cpr_def', 'cpr_intro', 34),
(43, 4, 'RCP 2', 'mini@cpr@char_a@cpr_str', 'cpr_kol', 33),
(44, 4, 'RCP 3', 'mini@cpr@char_a@cpr_def', 'cpr_pumpchest_idle', 33),
(45, 4, 'RCP 4', 'mini@cpr@char_a@cpr_str', 'cpr_success', 32),
(46, 5, 'Meditar', 'rcmcollect_paperleadinout@', 'meditiate_idle', 33),
(47, 5, 'Deporte 1', 'timetable@reunited@ig_2', 'jimmy_masterbation', 33),
(48, 5, 'Deporte 2', 'timetable@reunited@ig_2', 'jimmy_base', 33),
(49, 5, 'Running 1', 'move_m@jog@', 'run', 33),
(50, 5, 'Running 2', 'move_f@jogger', 'jogging', 33),
(51, 5, 'Running 3', 'amb@world_human_jog@female@base', 'base', 33),
(52, 5, 'Triatlón 1', 'mini@triathlon', 'idle_a', 33),
(53, 5, 'Triatlón 2', 'mini@triathlon', 'idle_d', 33),
(54, 5, 'Triatlón 3', 'mini@triathlon', 'idle_e', 33),
(55, 5, 'Triatlón 4', 'mini@triathlon', 'idle_f', 33),
(56, 5, 'Yoga 1', 'amb@world_human_yoga@female@base', 'base_a', 33),
(57, 5, 'Yoga 2', 'amb@world_human_yoga@female@base', 'base_c', 33),
(58, 5, 'Yoga 3', 'amb@world_human_yoga@male@base', 'base_b', 33),
(59, 5, 'Flexiones', 'amb@world_human_push_ups@male@idle_a', 'idle_d', 33),
(60, 5, 'Sentadillas', 'amb@world_human_sit_ups@male@base', 'base', 33),
(61, 6, 'Caminar 1', 'move_f@heels@c', 'walk', 33),
(62, 6, 'Caminar 2', 'move_f@arrogant@a', 'walk', 33),
(63, 6, 'Caminar 3', 'move_f@sad@a', 'walk', 33),
(64, 6, 'Caminar 4', 'move_m@drunk@moderatedrunk', 'walk', 33),
(65, 6, 'Caminar 5', 'move_m@shadyped@a', 'walk', 33),
(66, 6, 'Caminar 6', 'move_f@gangster@ng', 'walk', 33),
(67, 6, 'Caminar 7', 'move_f@generic', 'walk', 33),
(68, 6, 'Caminar 8', 'move_f@heels@d', 'walk', 33),
(69, 6, 'Caminar 9', 'move_f@posh@', 'walk', 33),
(70, 6, 'Caminar 10', 'move_m@brave@b', 'walk', 33),
(71, 6, 'Caminar 11', 'move_m@confident', 'walk', 33),
(72, 6, 'Caminar 12', 'move_m@depressed@d', 'walk', 33),
(73, 6, 'Caminar 13', 'move_m@favor_right_foot', 'walk', 33),
(74, 6, 'Caminar 14', 'move_m@generic', 'walk', 33),
(75, 6, 'Caminar 15', 'move_m@generic_variations@walk', 'walk_a', 33),
(76, 6, 'Caminar 16', 'move_m@generic_variations@walk', 'walk_f', 33),
(77, 6, 'Caminar 17', 'move_m@golfer@', 'golf_walk', 33),
(78, 6, 'Caminar 18', 'move_m@money', 'walk', 33),
(79, 6, 'Caminar 19', 'move_m@shadyped@a', 'walk', 33),
(80, 6, 'Caminar 20', 'move_m@swagger@b', 'walk', 33),
(81, 6, 'Caminar 21', 'switch@franklin@dispensary', 'exit_dispensary_outro_ped_f_a', 33),
(82, 7, 'Sentarse 1', 'amb@world_human_stupor@male@base', 'base', 33),
(83, 7, 'Sentarse 2', 'amb@world_human_stupor@male_looking_left@base', 'base', 33),
(84, 7, 'Sentarse 3', 'anim@heists@fleeca_bank@ig_7_jetski_owner', 'owner_idle', 33),
(85, 7, 'Sentarse 4', 'mp_army_contact', 'positive_a', 33),
(86, 7, 'Sentarse 5', 'timetable@reunited@ig_10', 'base_amanda', 33),
(87, 7, 'Sentarse 6', 'anim@heists@prison_heistunfinished_biztarget_idle', 'target_idle', 33),
(88, 7, 'Sentarse 7', 'switch@michael@sitting', 'idle', 33),
(89, 7, 'Sentarse 8', 'timetable@michael@on_sofaidle_c', 'sit_sofa_g', 33),
(90, 7, 'Sentarse 9', 'timetable@michael@on_sofaidle_b', 'sit_sofa_d', 33),
(91, 7, 'Sentarse 10', 'timetable@michael@on_sofaidle_a', 'sit_sofa_a', 33),
(92, 7, 'Sentarse 11', 'rcm_barry3', 'barry_3_sit_loop', 33),
(93, 7, 'Tumbarse 1', 'amb@world_human_sunbathe@male@back@idle_a', 'idle_a', 33),
(94, 7, 'Tumbarse 2', 'amb@world_human_sunbathe@female@back@idle_a', 'idle_a', 33),
(95, 7, 'Tumbarse 3', 'amb@world_human_sunbathe@female@front@base', 'base', 33),
(96, 7, 'Tumbarse 4', 'amb@world_human_picnic@male@base', 'base', 33),
(97, 7, 'Tumbarse 5', 'amb@world_human_picnic@female@base', 'base', 33),
(98, 7, 'Tumbarse 6', 'missfra0_chop_fchase', 'ballasog_rollthroughtraincar_ig6_loop', 33),
(99, 7, 'Apoyarse 1', 'amb@prop_human_bum_shopping_cart@male@idle_a', 'idle_a', 33),
(100, 7, 'Apoyarse 2', 'anim@mp_ferris_wheel', 'idle_a_player_one', 33),
(101, 7, 'Apoyarse 3', 'amb@prop_human_bum_shopping_cart@male@base', 'base', 33),
(102, 7, 'Apoyarse 4', 'amb@world_human_leaning@male@wall@back@legs_crossed@idle_b', 'idle_d', 33),
(103, 7, 'Apoyarse 5', 'amb@world_human_leaning@male@wall@back@hands_together@idle_a', 'idle_c', 33),
(104, 7, 'Apoyarse 6', 'amb@world_human_leaning@male@wall@back@mobile@base', 'base', 33),
(105, 7, 'Apoyarse 7', 'amb@world_human_leaning@male@wall@back@texting@base', 'base', 33),
(106, 7, 'Apoyarse 8', 'amb@world_human_leaning@female@wall@back@mobile@base', 'base', 33),
(107, 7, 'Apoyarse 9', 'amb@world_human_leaning@female@wall@back@texting@base', 'base', 33),
(108, 7, 'Apoyarse 10', 'amb@world_human_leaning@female@smoke@idle_a', 'idle_a', 33),
(109, 7, 'Apoyarse 11', 'amb@world_human_leaning@male@wall@back@foot_up@idle_b', 'idle_d', 33),
(110, 7, 'Apoyarse 12', 'misscarsteal1car_1_ext_leadin', 'base_driver1', 33),
(111, 8, 'DJ', 'anim@mp_player_intupperdj', 'enter', 32),
(112, 8, 'Borracho', 'mini@hookers_spcrackhead', 'idle_c', 33),
(113, 8, 'Rock', 'anim@mp_player_intincarrockbodhi@ps@', 'enter', 48),
(114, 8, 'Contoneo', 'misscarsteal4@actor', 'stumble', 33),
(115, 8, 'Animar 1', 'amb@world_human_cheering@female_a', 'base', 33),
(116, 8, 'Animar 2', 'amb@world_human_cheering@female_c', 'base', 33),
(117, 8, 'Animar 3', 'amb@world_human_cheering@female_d', 'base', 33),
(118, 8, 'Animar 4', 'amb@world_human_cheering@male_a', 'base', 33),
(119, 8, 'Animar 5', 'amb@world_human_cheering@male_b', 'base', 33),
(120, 8, 'Animar 6', 'amb@world_human_cheering@male_d', 'base', 33),
(121, 8, 'Animar 7', 'amb@world_human_cheering@male_e', 'base', 33),
(122, 8, 'Bailar 1', 'amb@world_human_jog_standing@female@base', 'base', 33),
(123, 8, 'Bailar 2', 'amb@world_human_jog_standing@female@idle_a', 'idle_a', 33),
(124, 8, 'Bailar 3', 'amb@world_human_power_walker@female@static', 'static', 33),
(125, 8, 'Bailar 4', 'amb@world_human_partying@female@partying_beer@base', 'base', 33),
(126, 8, 'Bailar 5', 'amb@world_human_partying@female@partying_cellphone@idle_a', 'idle_a', 33),
(127, 8, 'Bailar 6', 'amb@world_human_partying@female@partying_beer@idle_a', 'idle_a', 33),
(128, 9, 'Saludo 1', 'mp_player_int_uppersalute', 'mp_player_int_salute', 33),
(129, 9, 'Saludo 2', 'mp_ped_interaction', 'hugs_guy_a', 32),
(130, 9, 'Saludo 3', 'mp_player_intsalute', 'mp_player_int_salute', 32),
(131, 9, 'Saludo 4', 'missmic4premiere', 'crowd_a_idle_01', 33),
(132, 9, 'Saludo 5', 'missexile2', 'franklinwavetohelicopter', 33),
(133, 9, 'Saludo 6', 'anim@mp_player_intcelebrationmale@wave', 'wave', 32),
(134, 9, 'Rendirse 1', 'mp_am_hold_up', 'handsup_base', 49),
(135, 9, 'Rendirse 2', 'anim@mp_player_intuppersurrender', 'idle_a_fp', 49),
(136, 9, 'Rendirse 3', 'amb@code_human_cower@female@react_cowering', 'base_back_left', 49),
(137, 9, 'Rendirse 4', 'amb@code_human_cower@female@react_cowering', 'base_right', 49),
(138, 9, 'Rendirse 5', 'missfbi5ig_0', 'lyinginpain_loop_steve', 33),
(139, 9, 'Rendirse 6', 'missfbi5ig_10', 'lift_holdup_loop_labped', 33),
(140, 9, 'Rendirse 7', 'missfbi5ig_17', 'walk_in_aim_loop_scientista', 33),
(141, 9, 'Rendirse 8', 'mp_am_hold_up', 'cower_loop', 33),
(142, 9, 'Rendirse 9', 'mp_arrest_paired', 'crook_p1_idle', 33),
(143, 9, 'Rendirse 10', 'mp_bank_heist_1', 'm_cower_02', 33),
(144, 9, 'Rendirse 11', 'misstrevor1', 'threaten_ortega_endloop_ort', 33),
(145, 9, 'Brazos 1', 'amb@world_human_hang_out_street@male_c@base', 'base', 33),
(146, 9, 'Brazos 2', 'amb@world_human_hang_out_street@female_arms_crossed@idle_a', 'idle_a', 33),
(147, 9, 'Brazos 3', 'mini@hookers_sp', 'idle_reject_loop_c', 49),
(148, 9, 'Brazos 4', 'mini@hookers_sp', 'idle_reject', 48),
(149, 9, 'Guardia 1', 'missbigscore1', 'idle_a', 33),
(150, 9, 'Guardia 2', 'missbigscore1', 'idle_base', 33),
(151, 9, 'Guardia 3', 'missbigscore1', 'idle_c', 33),
(152, 9, 'Guardia 4', 'missbigscore1', 'idle_e', 33),
(153, 10, 'Striptease 1', 'mini@strip_club@pole_dance@pole_a_2_stage', 'pole_a_2_stage', 34),
(154, 10, 'Striptease 2', 'mini@strip_club@pole_dance@pole_b_2_stage', 'pole_b_2_stage', 34),
(155, 10, 'Striptease 3', 'mini@strip_club@pole_dance@pole_c_2_prvd_a', 'pole_c_2_prvd_a', 34),
(156, 10, 'Striptease 4', 'mini@strip_club@pole_dance@pole_c_2_prvd_b', 'pole_c_2_prvd_b', 34),
(157, 10, 'Striptease 5', 'mini@hookers_spcrackhead', 'idle_b', 33),
(158, 10, 'Striptease 6', 'mini@strip_club@pole_dance@pole_c_2_prvd_c', 'pole_c_2_prvd_c', 34),
(159, 10, 'Striptease 7', 'mini@strip_club@pole_dance@pole_dance1', 'pd_dance_01', 33),
(160, 10, 'Striptease 8', 'mini@strip_club@pole_dance@pole_dance2', 'pd_dance_02', 33),
(161, 10, 'Striptease 9', 'mini@strip_club@pole_dance@pole_dance3', 'pd_dance_03', 33),
(162, 10, 'Striptease 10', 'mini@strip_club@pole_dance@pole_enter', 'pd_enter', 33),
(163, 10, 'Striptease 11', 'mini@strip_club@pole_dance@pole_exit', 'pd_exit', 33),
(164, 10, 'Striptease 12', 'mini@strip_club@private_dance@exit', 'priv_dance_exit', 33),
(165, 10, 'Striptease 13', 'mini@strip_club@private_dance@idle', 'priv_dance_idle', 33),
(166, 10, 'Striptease 14', 'mp_am_stripper', 'lap_dance_girl', 33),
(167, 10, 'Baile 1', 'amb@world_human_prostitute@cokehead@idle_a', 'idle_b', 33),
(168, 10, 'Baile 2', 'mini@hookers_sp', 'idle_c', 32),
(169, 10, 'Baile 3', 'mini@hookers_spcokehead', 'idle_a', 32),
(170, 10, 'Baile 4', 'mini@hookers_spcokehead', 'idle_c', 32),
(171, 10, 'Baile 5', 'mini@hookers_spcrackhead', 'idle_b', 33),
(172, 10, 'Baile 6', 'mini@hookers_spvanilla', 'idle_b', 32),
(173, 10, 'Coche 1', 'mini@prostitutes@sexnorm_veh', 'bj_loop_male', 33),
(174, 10, 'Coche 2', 'mini@prostitutes@sexnorm_veh', 'sex_loop_male', 33),
(175, 10, 'Coche 3', 'mini@prostitutes@sexnorm_veh', 'sex_loop_prostitute', 33),
(176, 10, 'Coche 4', 'mini@prostitutes@sexlow_veh', 'low_car_bj_loop_player', 33),
(177, 10, 'Coche 5', 'mini@prostitutes@sexlow_veh', 'low_car_bj_loop_female', 33),
(178, 10, 'Coche 6', 'mini@prostitutes@sexlow_veh', 'low_car_sex_loop_female', 33),
(179, 11, 'Escribir mensaje', 'amb@world_human_stand_mobile@male@text@base', 'base', 49),
(180, 11, 'Leer email', 'cellphone@', 'cellphone_email_read_base', 49),
(181, 11, 'Cámara', 'cellphone@', 'cellphone_photo_idle', 49),
(182, 11, 'Llamada 1', 'amb@world_human_stand_mobile@female@standing@call@idle_a', 'idle_a', 49),
(183, 11, 'Llamada 2', 'amb@world_human_mobile_film_shocking@female@idle_a', 'idle_a', 33),
(184, 12, 'Herido', 'combat@damage@injured_pistol@to_writhe', 'variation_b', 34),
(185, 12, 'Muerto 1', 'missarmenian2', 'corpse_search_exit_ped', 33),
(186, 12, 'Muerto 2', 'missarmenian2', 'drunk_loop', 33),
(187, 12, 'Muerto 3', 'missfinale_c1@', 'lying_dead_player0', 33),
(188, 12, 'Muerto 4', 'mp_bank_heist_1', 'prone_l_loop', 33),
(189, 12, 'Muerto 5', 'missfra2', 'lamar_base_idle', 33),
(190, 12, 'Pose 1', 'amb@world_human_hang_out_street@female_hold_arm@base', 'base', 49),
(191, 12, 'Pose 2', 'mini@hookers_sp', 'idle_wait', 49),
(192, 12, 'Pose 3', 'amb@world_human_stand_impatient@female@no_sign@base', 'base', 33),
(193, 12, 'Pose 4', 'mini@hookers_spfrench', 'idle_wait', 33),
(194, 12, 'Pose 5', 'amb@world_human_hang_out_street@female_arm_side@idle_a', 'idle_a', 33),
(195, 12, 'Pose 6', 'amb@world_human_muscle_flex@arms_in_front@idle_a', 'idle_b', 33),
(196, 12, 'Pose 7', 'missfbi5leadinout', 'leadin_2_fra', 33),
(197, 12, 'Pose 8', 'amb@world_human_cop_idles@female@idle_a', 'idle_d', 33),
(198, 12, 'Pose 9', 'amb@world_human_cop_idles@male@idle_b', 'idle_a', 33);

-- --------------------------------------------------------

--
-- Struttura della tabella `answers`
--

CREATE TABLE `answers` (
  `id` int(10) NOT NULL,
  `question` int(11) DEFAULT NULL,
  `answer` text NOT NULL,
  `correct` bit(1) NOT NULL DEFAULT b'0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `applications`
--

CREATE TABLE `applications` (
  `account` varchar(32) NOT NULL DEFAULT '',
  `mistakes` int(11) NOT NULL DEFAULT 0,
  `submission` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `blood`
--

CREATE TABLE `blood` (
  `id` int(11) NOT NULL,
  `doctor` int(11) NOT NULL,
  `patient` int(11) NOT NULL,
  `bloodtype` varchar(8) NOT NULL,
  `used` bit(1) NOT NULL DEFAULT b'0',
  `date` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `business`
--

CREATE TABLE `business` (
  `id` int(10) NOT NULL,
  `type` int(10) NOT NULL DEFAULT 0,
  `inner` bit(1) NOT NULL,
  `posX` float NOT NULL DEFAULT 0,
  `posY` float NOT NULL DEFAULT 0,
  `posZ` float NOT NULL DEFAULT 0,
  `dimension` int(11) NOT NULL DEFAULT 0,
  `name` varchar(64) NOT NULL DEFAULT 'Negocio',
  `owner` varchar(32) NOT NULL DEFAULT '',
  `funds` int(11) NOT NULL DEFAULT 0,
  `products` int(11) NOT NULL DEFAULT 0,
  `multiplier` float NOT NULL DEFAULT 3,
  `locked` bit(1) NOT NULL DEFAULT b'0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `calls`
--

CREATE TABLE `calls` (
  `phone` int(10) NOT NULL,
  `target` int(10) NOT NULL,
  `time` int(10) NOT NULL,
  `date` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `categories`
--

CREATE TABLE `categories` (
  `id` int(11) NOT NULL,
  `name` varchar(32) NOT NULL DEFAULT ''
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='Animation categories';

--
-- Dump dei dati per la tabella `categories`
--

INSERT INTO `categories` (`id`, `name`) VALUES
(1, 'Reacciones'),
(2, 'Acciones'),
(3, 'Higiene'),
(4, 'Trabajos'),
(5, 'Deporte'),
(6, 'Caminar'),
(7, 'Descanso'),
(8, 'Fiesta'),
(9, 'Brazos'),
(10, 'Sexo'),
(11, 'Teléfono'),
(12, 'Otros');

-- --------------------------------------------------------

--
-- Struttura della tabella `channels`
--

CREATE TABLE `channels` (
  `id` int(10) NOT NULL,
  `owner` int(10) NOT NULL DEFAULT 0,
  `password` varchar(32) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `clothes`
--

CREATE TABLE `clothes` (
  `id` int(10) NOT NULL,
  `player` int(10) NOT NULL DEFAULT 0,
  `type` int(10) NOT NULL DEFAULT 0,
  `slot` int(10) NOT NULL DEFAULT 0,
  `drawable` int(10) NOT NULL DEFAULT 0,
  `texture` int(10) NOT NULL DEFAULT 0,
  `dressed` bit(1) NOT NULL DEFAULT b'0',
  `stored` bit(1) NOT NULL DEFAULT b'0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `contacts`
--

CREATE TABLE `contacts` (
  `id` int(11) NOT NULL,
  `owner` int(6) NOT NULL,
  `contactNumber` int(6) NOT NULL,
  `contactName` varchar(20) NOT NULL DEFAULT ''
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `controls`
--

CREATE TABLE `controls` (
  `id` int(10) NOT NULL,
  `name` varchar(24) NOT NULL DEFAULT '',
  `item` int(10) NOT NULL DEFAULT 0,
  `posX` float NOT NULL DEFAULT 0,
  `posY` float NOT NULL DEFAULT 0,
  `posZ` float NOT NULL DEFAULT 0,
  `rotation` float NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `crimes`
--

CREATE TABLE `crimes` (
  `id` int(11) NOT NULL,
  `description` varchar(256) NOT NULL DEFAULT '',
  `jail` int(11) NOT NULL DEFAULT 0,
  `fine` int(11) NOT NULL DEFAULT 0,
  `reminder` varchar(128) NOT NULL DEFAULT ''
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `dealers`
--

CREATE TABLE `dealers` (
  `vehicleHash` varchar(24) NOT NULL,
  `dealerId` int(11) NOT NULL,
  `vehicleType` int(11) NOT NULL,
  `price` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dump dei dati per la tabella `dealers`
--

INSERT INTO `dealers` (`vehicleHash`, `dealerId`, `vehicleType`, `price`) VALUES
('adder', 0, 5, 310000),
('akuma', 1, 8, 71000),
('alpha', 0, 5, 35600),
('asea', 0, 1, 7800),
('asterope', 0, 1, 15000),
('avarus', 1, 8, 55000),
('bagger', 1, 8, 25000),
('baller', 0, 2, 35000),
('baller2', 0, 2, 39000),
('baller3', 0, 2, 41000),
('banshee', 0, 5, 89000),
('banshee2', 0, 5, 122000),
('bati', 1, 8, 57000),
('bati2', 1, 8, 59000),
('benson', 0, 2, 76000),
('bestiagts', 0, 5, 96000),
('bf400', 1, 8, 47000),
('bfinjection', 0, 9, 36000),
('bison', 0, 2, 31000),
('bjxl', 0, 2, 18000),
('blade', 0, 4, 8000),
('blazer', 1, 9, 23000),
('blazer2', 1, 9, 16000),
('blazer3', 1, 9, 21000),
('blazer4', 1, 8, 27000),
('blista', 0, 0, 10000),
('blista2', 0, 0, 6000),
('blista3', 0, 0, 6300),
('bmx', 1, 8, 600),
('bodhi2', 0, 9, 23000),
('brawler', 0, 9, 52600),
('brioso', 0, 0, 15000),
('buccaneer', 0, 4, 14000),
('buccaneer2', 0, 4, 35000),
('buffalo', 0, 5, 45000),
('buffalo2', 0, 5, 49000),
('bullet', 0, 5, 140000),
('burrito3', 0, 2, 35000),
('camper', 0, 2, 32000),
('carbonizzare', 0, 5, 115000),
('carbonrs', 1, 8, 46000),
('casco', 0, 5, 152000),
('cavalcade', 0, 2, 16200),
('cavalcade2', 0, 2, 15900),
('cheetah', 0, 5, 260000),
('cheetah2', 0, 5, 197000),
('chimera', 1, 8, 31800),
('chino', 0, 4, 16000),
('chino2', 0, 4, 38000),
('cliffhanger', 1, 8, 38000),
('cog55', 0, 1, 55000),
('cogcabrio', 0, 3, 39000),
('cognoscenti', 0, 1, 65000),
('comet2', 0, 5, 96500),
('comet3', 0, 5, 120000),
('contender', 0, 2, 43200),
('coquette', 0, 5, 115000),
('coquette2', 0, 5, 139000),
('coquette3', 0, 5, 108500),
('cruiser', 1, 8, 350),
('cyclone', 0, 5, 375000),
('daemon', 1, 8, 10000),
('daemon2', 1, 8, 12500),
('defiler', 1, 8, 61900),
('diablous', 1, 8, 45000),
('diablous2', 1, 8, 40000),
('dilettante', 0, 0, 7000),
('Dinghy', 2, 14, 25000),
('Dinghy2', 2, 14, 32000),
('Dinghy3', 2, 14, 40000),
('Dinghy4', 2, 14, 55000),
('dloader', 0, 9, 40000),
('dominator', 0, 4, 30000),
('dominator2', 0, 4, 33000),
('double', 1, 8, 48000),
('dubsta', 0, 2, 37600),
('dubsta2', 0, 2, 46000),
('dubsta3', 0, 2, 59000),
('dukes', 0, 4, 15000),
('elegy', 0, 5, 79900),
('elegy2', 0, 5, 97000),
('emperor', 0, 1, 6400),
('emperor2', 0, 1, 3100),
('enduro', 1, 8, 7000),
('entityxf', 0, 5, 325000),
('esskey', 1, 8, 22400),
('exemplar', 0, 3, 45000),
('f620', 0, 3, 46300),
('faction', 0, 4, 13000),
('faction2', 0, 4, 35200),
('faction3', 0, 4, 42500),
('faggio', 1, 8, 2100),
('faggio2', 1, 8, 1500),
('faggio3', 1, 8, 2000),
('fcr', 1, 8, 27000),
('fcr2', 1, 8, 32000),
('felon', 0, 3, 42000),
('felon2', 0, 3, 42500),
('feltzer2', 0, 5, 104000),
('feltzer3', 0, 5, 147000),
('fixter', 1, 8, 620),
('fmj', 0, 5, 347000),
('fq2', 0, 2, 38800),
('fugitive', 0, 1, 14400),
('furoregt', 0, 5, 93000),
('fusilade', 0, 5, 41000),
('futo', 0, 5, 18700),
('gargoyle', 1, 8, 46700),
('gauntlet', 0, 4, 28000),
('gauntlet2', 0, 4, 36000),
('gburrito', 0, 2, 36000),
('gburrito2', 0, 2, 32000),
('glendale', 0, 1, 12000),
('gp1', 0, 5, 245000),
('granger', 0, 2, 25000),
('gresley', 0, 2, 26000),
('guardian', 0, 2, 84000),
('habanero', 0, 2, 13900),
('hakuchou', 1, 8, 72000),
('hakuchou2', 1, 8, 120000),
('hexer', 1, 8, 12500),
('hotknife', 0, 4, 32000),
('huntley', 0, 2, 43100),
('infernus', 0, 5, 135000),
('infernus2', 0, 5, 169000),
('ingot', 0, 1, 5600),
('innovation', 1, 8, 16000),
('intruder', 0, 1, 7600),
('issi2', 0, 0, 8000),
('italigtb', 0, 5, 330000),
('italigtb2', 0, 5, 355000),
('jackal', 0, 3, 43200),
('jester', 0, 5, 107000),
('Jetmax', 2, 14, 80000),
('journey', 0, 2, 21000),
('kalahari', 0, 9, 16200),
('khamelion', 0, 5, 86000),
('kuruma', 0, 5, 65000),
('landstalker', 0, 2, 22000),
('lectro', 1, 8, 31800),
('lynx', 0, 5, 109000),
('mamba', 0, 5, 146000),
('manchez', 1, 8, 19000),
('Marquis', 2, 14, 60000),
('massacro', 0, 5, 106500),
('mesa', 0, 9, 17200),
('mesa3', 0, 9, 29500),
('monroe', 0, 5, 145000),
('moonbeam', 0, 4, 9000),
('moonbeam2', 0, 4, 36000),
('mule3', 0, 2, 62000),
('nemesis', 1, 8, 29700),
('nero', 0, 5, 380000),
('nero2', 0, 5, 392000),
('nightblade', 1, 8, 35700),
('nightshade', 0, 4, 33000),
('ninef', 0, 5, 112500),
('ninef2', 0, 5, 110000),
('omnis', 0, 5, 89000),
('oracle', 0, 3, 28000),
('oracle2', 0, 3, 38000),
('osiris', 0, 5, 341000),
('panto', 0, 0, 4500),
('patriot', 0, 2, 26700),
('pcj', 1, 8, 33000),
('penetrator', 0, 5, 192000),
('penumbra', 0, 5, 19700),
('peyote', 0, 1, 30000),
('pfister811', 0, 5, 335000),
('phoenix', 0, 4, 20900),
('picador', 0, 4, 23000),
('pony', 0, 2, 21000),
('prairie', 0, 0, 11000),
('premier', 0, 1, 11100),
('primo', 0, 1, 13600),
('primo2', 0, 1, 46400),
('prototipo', 0, 5, 800000),
('radi', 0, 2, 22000),
('rancherxl', 0, 9, 15200),
('rapidgt', 0, 5, 34600),
('rapidgt2', 0, 5, 35100),
('rapidgt3', 0, 5, 57000),
('ratbike', 1, 8, 2500),
('ratloader', 0, 4, 3000),
('ratloader2', 0, 4, 5000),
('reaper', 0, 5, 337000),
('rebel', 0, 9, 2700),
('rebel2', 0, 9, 10000),
('regina', 0, 1, 4600),
('retinue', 0, 5, 38000),
('rhapsody', 0, 0, 1500),
('rocoto', 0, 2, 31000),
('ruffian', 1, 8, 32300),
('ruiner', 0, 4, 23000),
('rumpo', 0, 2, 35000),
('rumpo3', 0, 2, 90000),
('ruston', 0, 5, 69600),
('sabregt', 0, 4, 15500),
('sabregt2', 0, 4, 29500),
('sadler', 0, 4, 10000),
('sanchez', 1, 8, 13100),
('sanchez2', 1, 8, 23000),
('sanctus', 1, 8, 95000),
('sandking', 0, 9, 21000),
('sandking2', 0, 9, 20100),
('schafter2', 0, 5, 42200),
('schafter3', 0, 5, 53200),
('schwarzer', 0, 5, 48000),
('scorcher', 1, 8, 500),
('Seashark', 2, 14, 15000),
('Seashark3', 2, 14, 35000),
('seminole', 0, 2, 23700),
('sentinel', 0, 3, 25000),
('sentinel2', 0, 3, 30000),
('serrano', 0, 2, 22600),
('seven70', 0, 5, 122000),
('sheava', 0, 5, 368000),
('slamvan', 0, 4, 11000),
('slamvan2', 0, 4, 15000),
('slamvan3', 0, 4, 35300),
('sovereign', 1, 8, 42300),
('specter', 0, 5, 106000),
('specter2', 0, 5, 129000),
('Speeder', 2, 14, 72500),
('Speeder2', 2, 14, 90000),
('speedo', 0, 2, 23000),
('Squalo', 2, 14, 55000),
('stalion', 0, 4, 8700),
('stalion2', 0, 4, 38000),
('stanier', 0, 1, 9700),
('stinger', 0, 5, 149000),
('stingergt', 0, 5, 151000),
('stratum', 0, 1, 13000),
('stretch', 0, 1, 95000),
('sultan', 0, 5, 45000),
('sultanrs', 0, 5, 110000),
('Suntrap', 2, 14, 30000),
('superd', 0, 1, 55000),
('surano', 0, 5, 63000),
('surfer', 0, 2, 15000),
('surfer2', 0, 2, 10000),
('surge', 0, 1, 13700),
('t20', 0, 5, 385000),
('tailgater', 0, 1, 33000),
('tampa', 0, 4, 19000),
('tempesta', 0, 5, 340000),
('thrust', 1, 8, 24500),
('Toro', 2, 14, 90000),
('Toro2', 2, 14, 120000),
('tribike', 1, 8, 900),
('Tropic', 2, 14, 50000),
('Tropic2', 2, 14, 60000),
('tropos', 0, 5, 56000),
('Tug', 2, 14, 175000),
('turismo2', 0, 5, 260000),
('turismor', 0, 5, 270000),
('vacca', 0, 5, 198000),
('vader', 1, 8, 28100),
('vagner', 0, 5, 395000),
('verlierer2', 0, 5, 95000),
('vigero', 0, 4, 10600),
('vindicator', 1, 8, 39000),
('virgo', 0, 4, 14700),
('virgo2', 0, 4, 37900),
('virgo3', 0, 4, 45000),
('visione', 0, 5, 390000),
('voltic', 0, 5, 132000),
('voodoo', 0, 4, 32600),
('voodoo2', 0, 4, 25000),
('vortex', 1, 8, 55560),
('warrener', 0, 1, 7200),
('washington', 0, 1, 6800),
('windsor', 0, 3, 55000),
('windsor2', 0, 3, 63000),
('wolfsbane', 1, 8, 27600),
('xa21', 0, 5, 389000),
('xls', 0, 2, 42500),
('zentorno', 0, 5, 295000),
('zion', 0, 3, 25000),
('zion2', 0, 3, 32000),
('zombiea', 1, 8, 24900),
('zombieb', 1, 8, 26100),
('ztype', 0, 5, 55000);

-- --------------------------------------------------------

--
-- Struttura della tabella `fines`
--

CREATE TABLE `fines` (
  `officer` varchar(32) NOT NULL DEFAULT '',
  `target` varchar(32) NOT NULL DEFAULT '',
  `amount` int(10) NOT NULL DEFAULT 0,
  `reason` varchar(128) NOT NULL DEFAULT '',
  `date` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `furniture`
--

CREATE TABLE `furniture` (
  `id` int(10) NOT NULL,
  `hash` int(10) NOT NULL DEFAULT 0,
  `house` int(10) NOT NULL DEFAULT 0,
  `posX` float NOT NULL DEFAULT 0,
  `posY` float NOT NULL DEFAULT 0,
  `posZ` float NOT NULL DEFAULT 0,
  `rotation` float NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `hotwires`
--

CREATE TABLE `hotwires` (
  `vehicle` int(10) NOT NULL,
  `player` varchar(24) NOT NULL DEFAULT '',
  `posX` float NOT NULL,
  `posY` float NOT NULL,
  `posZ` float NOT NULL,
  `date` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `houses`
--

CREATE TABLE `houses` (
  `id` int(10) NOT NULL,
  `type` int(11) NOT NULL DEFAULT 0,
  `posX` float NOT NULL DEFAULT 0,
  `posY` float NOT NULL DEFAULT 0,
  `posZ` float NOT NULL DEFAULT 0,
  `dimension` int(11) NOT NULL DEFAULT 0,
  `name` varchar(32) NOT NULL DEFAULT 'Casa',
  `price` int(11) NOT NULL DEFAULT 10000,
  `owner` varchar(32) NOT NULL DEFAULT '',
  `status` int(1) NOT NULL DEFAULT 2,
  `tenants` int(1) NOT NULL DEFAULT 0,
  `rental` int(10) NOT NULL DEFAULT 0,
  `locked` bit(1) NOT NULL DEFAULT b'1'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `interiors`
--

CREATE TABLE `interiors` (
  `id` int(11) NOT NULL,
  `name` varchar(64) NOT NULL DEFAULT '',
  `posX` float NOT NULL,
  `posY` float NOT NULL,
  `posZ` float NOT NULL,
  `dimension` int(11) NOT NULL DEFAULT 0,
  `type` int(11) NOT NULL DEFAULT 0,
  `blip` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `items`
--

CREATE TABLE `items` (
  `id` int(11) NOT NULL,
  `hash` varchar(32) NOT NULL DEFAULT '',
  `ownerEntity` varchar(16) NOT NULL DEFAULT '',
  `ownerIdentifier` int(11) NOT NULL DEFAULT 0,
  `amount` int(11) NOT NULL DEFAULT 0,
  `posX` float NOT NULL DEFAULT 0,
  `posY` float NOT NULL DEFAULT 0,
  `posZ` float NOT NULL DEFAULT 0,
  `dimension` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `licensed`
--

CREATE TABLE `licensed` (
  `item` int(11) NOT NULL DEFAULT 0,
  `buyer` varchar(24) NOT NULL DEFAULT '',
  `date` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `messages`
--

CREATE TABLE `messages` (
  `id` int(11) NOT NULL,
  `senderNumber` int(6) NOT NULL DEFAULT 0,
  `receiverNumber` int(6) NOT NULL DEFAULT 0,
  `message` text NOT NULL,
  `deleted` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `money`
--

CREATE TABLE `money` (
  `source` varchar(32) NOT NULL,
  `receiver` varchar(32) NOT NULL,
  `type` varchar(32) NOT NULL,
  `amount` int(11) NOT NULL DEFAULT 0,
  `date` date NOT NULL,
  `hour` time NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dump dei dati per la tabella `money`
--

INSERT INTO `money` (`source`, `receiver`, `type`, `amount`, `date`, `hour`) VALUES
('Payday', 'Ciao Tizio', 'Payday', 579, '2021-02-21', '15:51:23'),
('Payday', 'Ciao Tizio', 'Payday', 579, '2021-02-23', '12:19:15');

-- --------------------------------------------------------

--
-- Struttura della tabella `news`
--

CREATE TABLE `news` (
  `id` int(11) NOT NULL,
  `winner` int(11) NOT NULL DEFAULT 0,
  `journalist` int(11) NOT NULL DEFAULT 0,
  `amount` int(11) NOT NULL DEFAULT 0,
  `annoucement` varchar(150) NOT NULL DEFAULT '0',
  `date` datetime NOT NULL,
  `given` bit(1) NOT NULL DEFAULT b'0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `parkings`
--

CREATE TABLE `parkings` (
  `id` int(10) NOT NULL,
  `type` int(1) NOT NULL DEFAULT 0,
  `house` int(11) NOT NULL DEFAULT 0,
  `posX` float NOT NULL DEFAULT 0,
  `posY` float NOT NULL DEFAULT 0,
  `posZ` float NOT NULL DEFAULT 0,
  `capacity` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `permissions`
--

CREATE TABLE `permissions` (
  `playerId` int(10) NOT NULL DEFAULT 0,
  `command` varchar(16) NOT NULL DEFAULT '',
  `option` varchar(16) NOT NULL DEFAULT ''
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `phones`
--

CREATE TABLE `phones` (
  `itemId` int(11) NOT NULL,
  `owner` varchar(32) NOT NULL,
  `number` int(6) NOT NULL,
  `activation` datetime NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `plants`
--

CREATE TABLE `plants` (
  `id` int(11) NOT NULL,
  `posX` float NOT NULL,
  `posY` float NOT NULL,
  `posZ` float NOT NULL,
  `dimension` int(11) NOT NULL DEFAULT 0,
  `growth` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `poker_sits`
--

CREATE TABLE `poker_sits` (
  `id` int(11) NOT NULL,
  `x` float NOT NULL,
  `y` float NOT NULL,
  `z` float NOT NULL,
  `rotX` float NOT NULL,
  `rotY` float NOT NULL,
  `rotZ` float NOT NULL,
  `pokerTable` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dump dei dati per la tabella `poker_sits`
--

INSERT INTO `poker_sits` (`id`, `x`, `y`, `z`, `rotX`, `rotY`, `rotZ`, `pokerTable`) VALUES
(7, -237.359, 6190.85, 31.4899, 0, 0, -131.075, 9),
(8, -230.988, 6182.34, 31.4898, 0, 0, -115.92, 10),
(9, -239.679, 6178.93, 31.4845, 0, 0, 70.7722, 11);

-- --------------------------------------------------------

--
-- Struttura della tabella `poker_tables`
--

CREATE TABLE `poker_tables` (
  `id` int(11) NOT NULL,
  `x` float NOT NULL,
  `y` float NOT NULL,
  `z` float NOT NULL,
  `dimension` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dump dei dati per la tabella `poker_tables`
--

INSERT INTO `poker_tables` (`id`, `x`, `y`, `z`, `dimension`) VALUES
(8, -235.852, 6183.98, 31.4903, 0),
(9, -236.332, 6189.89, 31.4904, 0),
(10, -230.988, 6182.34, 31.4898, 0),
(11, -239.679, 6178.93, 31.4845, 0);

-- --------------------------------------------------------

--
-- Struttura della tabella `questions`
--

CREATE TABLE `questions` (
  `id` int(10) NOT NULL,
  `question` text NOT NULL,
  `license` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `skins`
--

CREATE TABLE `skins` (
  `characterId` int(11) NOT NULL,
  `firstHeadShape` int(11) NOT NULL,
  `secondHeadShape` int(11) NOT NULL,
  `firstSkinTone` int(11) NOT NULL,
  `secondSkinTone` int(11) NOT NULL,
  `headMix` float NOT NULL,
  `skinMix` float NOT NULL,
  `hairModel` int(10) NOT NULL,
  `firstHairColor` int(10) NOT NULL,
  `secondHairColor` int(10) NOT NULL,
  `beardModel` int(10) NOT NULL,
  `beardColor` int(10) NOT NULL,
  `chestModel` int(10) NOT NULL,
  `chestColor` int(10) NOT NULL,
  `blemishesModel` int(10) NOT NULL,
  `ageingModel` int(10) NOT NULL,
  `complexionModel` int(10) NOT NULL,
  `sundamageModel` int(10) NOT NULL,
  `frecklesModel` int(10) NOT NULL,
  `noseWidth` float NOT NULL,
  `noseHeight` float NOT NULL,
  `noseLength` float NOT NULL,
  `noseBridge` float NOT NULL,
  `noseTip` float NOT NULL,
  `noseShift` float NOT NULL,
  `browHeight` float NOT NULL,
  `browWidth` float NOT NULL,
  `cheekboneHeight` float NOT NULL,
  `cheekboneWidth` float NOT NULL,
  `cheeksWidth` float NOT NULL,
  `eyes` float NOT NULL,
  `lips` float NOT NULL,
  `jawWidth` float NOT NULL,
  `jawHeight` float NOT NULL,
  `chinLength` float NOT NULL,
  `chinPosition` float NOT NULL,
  `chinWidth` float NOT NULL,
  `chinShape` float NOT NULL,
  `neckWidth` float NOT NULL,
  `eyesColor` int(11) NOT NULL,
  `eyebrowsModel` int(11) NOT NULL,
  `eyebrowsColor` int(11) NOT NULL,
  `makeupModel` int(11) NOT NULL,
  `blushModel` int(11) NOT NULL,
  `blushColor` int(11) NOT NULL,
  `lipstickModel` int(11) NOT NULL,
  `lipstickColor` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dump dei dati per la tabella `skins`
--

INSERT INTO `skins` (`characterId`, `firstHeadShape`, `secondHeadShape`, `firstSkinTone`, `secondSkinTone`, `headMix`, `skinMix`, `hairModel`, `firstHairColor`, `secondHairColor`, `beardModel`, `beardColor`, `chestModel`, `chestColor`, `blemishesModel`, `ageingModel`, `complexionModel`, `sundamageModel`, `frecklesModel`, `noseWidth`, `noseHeight`, `noseLength`, `noseBridge`, `noseTip`, `noseShift`, `browHeight`, `browWidth`, `cheekboneHeight`, `cheekboneWidth`, `cheeksWidth`, `eyes`, `lips`, `jawWidth`, `jawHeight`, `chinLength`, `chinPosition`, `chinWidth`, `chinShape`, `neckWidth`, `eyesColor`, `eyebrowsModel`, `eyebrowsColor`, `makeupModel`, `blushModel`, `blushColor`, `lipstickModel`, `lipstickColor`) VALUES
(1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

-- --------------------------------------------------------

--
-- Struttura della tabella `sms`
--

CREATE TABLE `sms` (
  `phone` int(10) NOT NULL,
  `target` int(10) NOT NULL,
  `message` text NOT NULL,
  `date` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `tattoos`
--

CREATE TABLE `tattoos` (
  `player` int(10) NOT NULL DEFAULT 0,
  `zone` int(10) NOT NULL DEFAULT 0,
  `library` varchar(32) NOT NULL DEFAULT '',
  `hash` varchar(32) NOT NULL DEFAULT ''
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `tunning`
--

CREATE TABLE `tunning` (
  `id` int(11) NOT NULL,
  `vehicle` int(11) NOT NULL DEFAULT 0,
  `slot` int(11) NOT NULL DEFAULT 0,
  `component` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Struttura della tabella `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `name` varchar(32) NOT NULL,
  `posX` float(10,0) NOT NULL DEFAULT -136,
  `posY` float NOT NULL DEFAULT 6198.95,
  `posZ` float NOT NULL DEFAULT 32.3845,
  `rotation` float NOT NULL DEFAULT 0,
  `money` int(11) NOT NULL DEFAULT 0,
  `bank` int(11) NOT NULL DEFAULT 3500,
  `health` int(11) NOT NULL DEFAULT 100,
  `armor` int(11) NOT NULL DEFAULT 0,
  `age` int(11) NOT NULL DEFAULT 14,
  `sex` int(11) NOT NULL DEFAULT 0,
  `model` varchar(32) NOT NULL,
  `faction` int(11) NOT NULL DEFAULT 0,
  `job` int(11) NOT NULL DEFAULT 0,
  `rank` int(11) NOT NULL DEFAULT 0,
  `radio` int(11) NOT NULL DEFAULT 0,
  `jailed` varchar(8) NOT NULL DEFAULT '-1,-1',
  `carKeys` varchar(32) NOT NULL DEFAULT '0,0,0,0,0',
  `documentation` int(11) NOT NULL DEFAULT 0,
  `licenses` varchar(32) NOT NULL DEFAULT '-1,-1,-1',
  `insurance` int(11) NOT NULL DEFAULT 0,
  `weaponLicense` int(11) NOT NULL DEFAULT 0,
  `houseRent` int(11) NOT NULL DEFAULT 0,
  `buildingEntered` varchar(8) NOT NULL DEFAULT '0,0',
  `jobDeliver` int(11) NOT NULL DEFAULT 0,
  `jobCooldown` int(11) NOT NULL DEFAULT 0,
  `played` int(11) NOT NULL DEFAULT 0,
  `status` int(11) NOT NULL DEFAULT 1,
  `socialName` varchar(32) NOT NULL,
  `adminRank` int(11) NOT NULL DEFAULT 0,
  `adminname` varchar(24) NOT NULL DEFAULT '',
  `employeeCooldown` int(11) NOT NULL DEFAULT 0,
  `duty` int(11) NOT NULL DEFAULT 0,
  `killed` int(11) NOT NULL DEFAULT 0,
  `jobPoints` varchar(64) NOT NULL DEFAULT '0,0,0,0,0,0,0',
  `rolePoints` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dump dei dati per la tabella `users`
--

INSERT INTO `users` (`id`, `name`, `posX`, `posY`, `posZ`, `rotation`, `money`, `bank`, `health`, `armor`, `age`, `sex`, `model`, `faction`, `job`, `rank`, `radio`, `jailed`, `carKeys`, `documentation`, `licenses`, `insurance`, `weaponLicense`, `houseRent`, `buildingEntered`, `jobDeliver`, `jobCooldown`, `played`, `status`, `socialName`, `adminRank`, `adminname`, `employeeCooldown`, `duty`, `killed`, `jobPoints`, `rolePoints`) VALUES
(1, 'Ciao Tizio', -202, 6215.55, 31.4896, 114.838, 0, 4658, 100, 0, 19, 0, 's_m_m_strperf_01', 0, 0, 0, 0, '-1,-1', '0,0,0,0,0', 0, '-1,-1,-1', 0, 0, 0, '0,0', 0, 0, 124, 1, 'vaisseau_spatial', 0, '', 0, 0, 0, '0,0,0,0,0,0,0', 0);

-- --------------------------------------------------------

--
-- Struttura della tabella `vehicles`
--

CREATE TABLE `vehicles` (
  `id` int(11) NOT NULL,
  `model` varchar(32) NOT NULL,
  `posX` float NOT NULL,
  `posY` float NOT NULL,
  `posZ` float NOT NULL,
  `rotation` float NOT NULL,
  `firstColor` varchar(12) NOT NULL DEFAULT '0,0,0',
  `secondColor` varchar(12) NOT NULL DEFAULT '0,0,0',
  `dimension` int(11) NOT NULL DEFAULT 0,
  `engine` int(11) NOT NULL DEFAULT 0,
  `locked` int(11) NOT NULL DEFAULT 0,
  `faction` int(11) NOT NULL DEFAULT 0,
  `owner` varchar(32) NOT NULL,
  `plate` varchar(8) NOT NULL,
  `price` int(11) NOT NULL DEFAULT 0,
  `parking` int(11) NOT NULL DEFAULT 0,
  `parkedTime` int(11) NOT NULL DEFAULT 0,
  `gas` float NOT NULL DEFAULT 0,
  `kms` float NOT NULL DEFAULT 0,
  `colorType` int(11) NOT NULL DEFAULT 1,
  `pearlescent` int(11) NOT NULL DEFAULT 0
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Indici per le tabelle scaricate
--

--
-- Indici per le tabelle `accounts`
--
ALTER TABLE `accounts`
  ADD PRIMARY KEY (`socialName`);

--
-- Indici per le tabelle `admin`
--
ALTER TABLE `admin`
  ADD PRIMARY KEY (`source`,`target`,`date`);

--
-- Indici per le tabelle `animations`
--
ALTER TABLE `animations`
  ADD PRIMARY KEY (`id`),
  ADD KEY `Animation_Category` (`category`);

--
-- Indici per le tabelle `answers`
--
ALTER TABLE `answers`
  ADD PRIMARY KEY (`id`),
  ADD KEY `Answer_Question` (`question`);

--
-- Indici per le tabelle `applications`
--
ALTER TABLE `applications`
  ADD PRIMARY KEY (`account`,`submission`);

--
-- Indici per le tabelle `blood`
--
ALTER TABLE `blood`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `business`
--
ALTER TABLE `business`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `calls`
--
ALTER TABLE `calls`
  ADD PRIMARY KEY (`phone`,`target`,`date`);

--
-- Indici per le tabelle `categories`
--
ALTER TABLE `categories`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `channels`
--
ALTER TABLE `channels`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `clothes`
--
ALTER TABLE `clothes`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `contacts`
--
ALTER TABLE `contacts`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `controls`
--
ALTER TABLE `controls`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `crimes`
--
ALTER TABLE `crimes`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `dealers`
--
ALTER TABLE `dealers`
  ADD PRIMARY KEY (`vehicleHash`);

--
-- Indici per le tabelle `fines`
--
ALTER TABLE `fines`
  ADD PRIMARY KEY (`officer`,`target`,`date`);

--
-- Indici per le tabelle `furniture`
--
ALTER TABLE `furniture`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `hotwires`
--
ALTER TABLE `hotwires`
  ADD PRIMARY KEY (`player`,`vehicle`,`date`);

--
-- Indici per le tabelle `houses`
--
ALTER TABLE `houses`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `interiors`
--
ALTER TABLE `interiors`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `items`
--
ALTER TABLE `items`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `licensed`
--
ALTER TABLE `licensed`
  ADD PRIMARY KEY (`item`);

--
-- Indici per le tabelle `messages`
--
ALTER TABLE `messages`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `news`
--
ALTER TABLE `news`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `parkings`
--
ALTER TABLE `parkings`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `permissions`
--
ALTER TABLE `permissions`
  ADD PRIMARY KEY (`playerId`,`command`,`option`);

--
-- Indici per le tabelle `phones`
--
ALTER TABLE `phones`
  ADD PRIMARY KEY (`itemId`),
  ADD UNIQUE KEY `number` (`number`);

--
-- Indici per le tabelle `plants`
--
ALTER TABLE `plants`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `poker_sits`
--
ALTER TABLE `poker_sits`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `poker_tables`
--
ALTER TABLE `poker_tables`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `questions`
--
ALTER TABLE `questions`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `skins`
--
ALTER TABLE `skins`
  ADD PRIMARY KEY (`characterId`);

--
-- Indici per le tabelle `sms`
--
ALTER TABLE `sms`
  ADD PRIMARY KEY (`phone`,`target`,`date`);

--
-- Indici per le tabelle `tattoos`
--
ALTER TABLE `tattoos`
  ADD PRIMARY KEY (`player`,`hash`);

--
-- Indici per le tabelle `tunning`
--
ALTER TABLE `tunning`
  ADD PRIMARY KEY (`id`);

--
-- Indici per le tabelle `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `name` (`name`);

--
-- Indici per le tabelle `vehicles`
--
ALTER TABLE `vehicles`
  ADD PRIMARY KEY (`id`);

--
-- AUTO_INCREMENT per le tabelle scaricate
--

--
-- AUTO_INCREMENT per la tabella `animations`
--
ALTER TABLE `animations`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=199;

--
-- AUTO_INCREMENT per la tabella `answers`
--
ALTER TABLE `answers`
  MODIFY `id` int(10) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `blood`
--
ALTER TABLE `blood`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `business`
--
ALTER TABLE `business`
  MODIFY `id` int(10) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `categories`
--
ALTER TABLE `categories`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=13;

--
-- AUTO_INCREMENT per la tabella `channels`
--
ALTER TABLE `channels`
  MODIFY `id` int(10) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `clothes`
--
ALTER TABLE `clothes`
  MODIFY `id` int(10) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `contacts`
--
ALTER TABLE `contacts`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `controls`
--
ALTER TABLE `controls`
  MODIFY `id` int(10) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `crimes`
--
ALTER TABLE `crimes`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `furniture`
--
ALTER TABLE `furniture`
  MODIFY `id` int(10) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `houses`
--
ALTER TABLE `houses`
  MODIFY `id` int(10) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `interiors`
--
ALTER TABLE `interiors`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `items`
--
ALTER TABLE `items`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `messages`
--
ALTER TABLE `messages`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `news`
--
ALTER TABLE `news`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `parkings`
--
ALTER TABLE `parkings`
  MODIFY `id` int(10) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `plants`
--
ALTER TABLE `plants`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `poker_sits`
--
ALTER TABLE `poker_sits`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT per la tabella `poker_tables`
--
ALTER TABLE `poker_tables`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=12;

--
-- AUTO_INCREMENT per la tabella `questions`
--
ALTER TABLE `questions`
  MODIFY `id` int(10) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `tunning`
--
ALTER TABLE `tunning`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT per la tabella `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT per la tabella `vehicles`
--
ALTER TABLE `vehicles`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT;

--
-- Limiti per le tabelle scaricate
--

--
-- Limiti per la tabella `animations`
--
ALTER TABLE `animations`
  ADD CONSTRAINT `Animation_Category` FOREIGN KEY (`category`) REFERENCES `categories` (`id`) ON DELETE CASCADE;

--
-- Limiti per la tabella `answers`
--
ALTER TABLE `answers`
  ADD CONSTRAINT `Answer_Question` FOREIGN KEY (`question`) REFERENCES `questions` (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
