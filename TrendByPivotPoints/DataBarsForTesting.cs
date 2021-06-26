using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrendByPivotPointsStrategy;
using TSLab.DataSource;

namespace TrendByPivotPointsStrategy.Tests
{
    class DataBarsForTesting
    {
        public List<Bar> GetBars()
        {
            var result = new List<Bar>();

            result.Add(new Bar() { Open = 76005, High = 76081, Low = 75906, Close = 76074 });//0
            result.Add(new Bar() { Open = 76076, High = 76099, Low = 76038, Close = 76086 });//1
            result.Add(new Bar() { Open = 76089, High = 76098, Low = 76041, Close = 76052 });//2
            result.Add(new Bar() { Open = 76052, High = 76078, Low = 76052, Close = 76067 });//3
            result.Add(new Bar() { Open = 76062, High = 76068, Low = 76000, Close = 76008 });//4
            result.Add(new Bar() { Open = 76007, High = 76010, Low = 75960, Close = 75965 });//5
            result.Add(new Bar() { Open = 75967, High = 76000, Low = 75952, Close = 75994 });//6L
            result.Add(new Bar() { Open = 75989, High = 76000, Low = 75982, Close = 75993 });//7
            result.Add(new Bar() { Open = 75992, High = 76036, Low = 75960, Close = 76026 });//8
            result.Add(new Bar() { Open = 76026, High = 76026, Low = 75984, Close = 76016 });//9
            result.Add(new Bar() { Open = 76016, High = 76050, Low = 76005, Close = 76049 });//10
            result.Add(new Bar() { Open = 76049, High = 76072, Low = 76040, Close = 76070 });//11
            result.Add(new Bar() { Open = 76067, High = 76070, Low = 76015, Close = 76030 });//12
            result.Add(new Bar() { Open = 76030, High = 76052, Low = 76028, Close = 76045 });//13
            result.Add(new Bar() { Open = 76046, High = 76063, Low = 76032, Close = 76032 });//14
            result.Add(new Bar() { Open = 76034, High = 76088, Low = 76016, Close = 76078 });//15
            result.Add(new Bar() { Open = 76075, High = 76084, Low = 76058, Close = 76078 });//16
            result.Add(new Bar() { Open = 76077, High = 76087, Low = 76074, Close = 76083 });//17
            result.Add(new Bar() { Open = 76086, High = 76088, Low = 76064, Close = 76067 });//18
            result.Add(new Bar() { Open = 76067, High = 76082, Low = 76027, Close = 76067 });//19
            result.Add(new Bar() { Open = 76063, High = 76064, Low = 76003, Close = 76014 });//20
            result.Add(new Bar() { Open = 76014, High = 76014, Low = 75955, Close = 75962 });//21
            result.Add(new Bar() { Open = 75962, High = 75991, Low = 75930, Close = 75982 });//22
            result.Add(new Bar() { Open = 75982, High = 76020, Low = 75974, Close = 75986 });//23
            result.Add(new Bar() { Open = 75984, High = 76020, Low = 75952, Close = 75959 });//24
            result.Add(new Bar() { Open = 75955, High = 75966, Low = 75820, Close = 75885 });//25
            result.Add(new Bar() { Open = 75884, High = 75885, Low = 75800, Close = 75800 });//26
            result.Add(new Bar() { Open = 75801, High = 75895, Low = 75775, Close = 75889 });//27
            result.Add(new Bar() { Open = 75889, High = 75895, Low = 75862, Close = 75874 });//28
            result.Add(new Bar() { Open = 75872, High = 75885, Low = 75630, Close = 75694 });//29
            result.Add(new Bar() { Open = 75691, High = 75735, Low = 75664, Close = 75724 });//30
            result.Add(new Bar() { Open = 75725, High = 75740, Low = 75673, Close = 75691 });//31
            result.Add(new Bar() { Open = 75689, High = 75711, Low = 75649, Close = 75689 });//32
            result.Add(new Bar() { Open = 75685, High = 75725, Low = 75677, Close = 75703 });//33
            result.Add(new Bar() { Open = 75704, High = 75732, Low = 75674, Close = 75691 });//34
            result.Add(new Bar() { Open = 75690, High = 75699, Low = 75610, Close = 75640 });//35
            result.Add(new Bar() { Open = 75637, High = 75701, Low = 75613, Close = 75662 });//36
            result.Add(new Bar() { Open = 75666, High = 75720, Low = 75610, Close = 75630 });//37
            result.Add(new Bar() { Open = 75630, High = 75651, Low = 75558, Close = 75562 });//38
            result.Add(new Bar() { Open = 75562, High = 75619, Low = 75531, Close = 75561 });//39
            result.Add(new Bar() { Open = 75563, High = 75592, Low = 75510, Close = 75581 });//40
            result.Add(new Bar() { Open = 75581, High = 75615, Low = 75570, Close = 75580 });//41
            result.Add(new Bar() { Open = 75577, High = 75657, Low = 75570, Close = 75652 });//42
            result.Add(new Bar() { Open = 75650, High = 75675, Low = 75609, Close = 75652 });//43
            result.Add(new Bar() { Open = 75655, High = 75689, Low = 75634, Close = 75666 });//44
            result.Add(new Bar() { Open = 75668, High = 75698, Low = 75629, Close = 75630 });//45
            result.Add(new Bar() { Open = 75633, High = 75645, Low = 75618, Close = 75619 });//46
            result.Add(new Bar() { Open = 75622, High = 75641, Low = 75609, Close = 75635 });//47
            result.Add(new Bar() { Open = 75634, High = 75675, Low = 75628, Close = 75653 });//48
            result.Add(new Bar() { Open = 75652, High = 75669, Low = 75633, Close = 75642 });//49
            result.Add(new Bar() { Open = 75640, High = 75657, Low = 75625, Close = 75652 });//50
            result.Add(new Bar() { Open = 75652, High = 75672, Low = 75635, Close = 75639 });//51
            result.Add(new Bar() { Open = 75638, High = 75671, Low = 75629, Close = 75651 });//52
            result.Add(new Bar() { Open = 75652, High = 75687, Low = 75627, Close = 75640 });//53
            result.Add(new Bar() { Open = 75640, High = 75646, Low = 75601, Close = 75630 });//54
            result.Add(new Bar() { Open = 75630, High = 75648, Low = 75591, Close = 75599 });//55
            result.Add(new Bar() { Open = 75595, High = 75618, Low = 75582, Close = 75587 });//56
            result.Add(new Bar() { Open = 75588, High = 75638, Low = 75583, Close = 75611 });//57
            result.Add(new Bar() { Open = 75611, High = 75655, Low = 75590, Close = 75637 });//58
            result.Add(new Bar() { Open = 75635, High = 75666, Low = 75630, Close = 75645 });//59
            result.Add(new Bar() { Open = 75646, High = 75652, Low = 75621, Close = 75633 });//60
            result.Add(new Bar() { Open = 75633, High = 75653, Low = 75611, Close = 75621 });//61
            result.Add(new Bar() { Open = 75620, High = 75659, Low = 75616, Close = 75646 });//62
            result.Add(new Bar() { Open = 75645, High = 75652, Low = 75582, Close = 75603 });//63
            result.Add(new Bar() { Open = 75603, High = 75640, Low = 75576, Close = 75640 });//64
            result.Add(new Bar() { Open = 75640, High = 75778, Low = 75635, Close = 75710 });//65
            result.Add(new Bar() { Open = 75712, High = 75772, Low = 75706, Close = 75770 });//66
            result.Add(new Bar() { Open = 75772, High = 75777, Low = 75701, Close = 75707 });//67
            result.Add(new Bar() { Open = 75710, High = 75765, Low = 75689, Close = 75739 });//68
            result.Add(new Bar() { Open = 75736, High = 75766, Low = 75719, Close = 75734 });//69
            result.Add(new Bar() { Open = 75736, High = 75808, Low = 75727, Close = 75734 });//70
            result.Add(new Bar() { Open = 75732, High = 75748, Low = 75673, Close = 75704 });//71
            result.Add(new Bar() { Open = 75707, High = 75718, Low = 75645, Close = 75654 });//72
            result.Add(new Bar() { Open = 75655, High = 75664, Low = 75625, Close = 75632 });//73
            result.Add(new Bar() { Open = 75633, High = 75661, Low = 75580, Close = 75596 });//74
            result.Add(new Bar() { Open = 75596, High = 75617, Low = 75523, Close = 75540 });//75
            result.Add(new Bar() { Open = 75538, High = 75610, Low = 75528, Close = 75585 });//76
            result.Add(new Bar() { Open = 75588, High = 75642, Low = 75468, Close = 75510 });//77
            result.Add(new Bar() { Open = 75510, High = 75511, Low = 75340, Close = 75419 });//78
            result.Add(new Bar() { Open = 75417, High = 75510, Low = 75404, Close = 75476 });//79
            result.Add(new Bar() { Open = 75474, High = 75559, Low = 75433, Close = 75500 });//80
            result.Add(new Bar() { Open = 75499, High = 75528, Low = 75415, Close = 75447 });//81
            result.Add(new Bar() { Open = 75447, High = 75450, Low = 75315, Close = 75368 });//82
            result.Add(new Bar() { Open = 75368, High = 75398, Low = 75323, Close = 75385 });//83
            result.Add(new Bar() { Open = 75385, High = 75439, Low = 75384, Close = 75395 });//84
            result.Add(new Bar() { Open = 75396, High = 75488, Low = 75392, Close = 75470 });//85
            result.Add(new Bar() { Open = 75470, High = 75479, Low = 75371, Close = 75458 });//86
            result.Add(new Bar() { Open = 75460, High = 75479, Low = 75424, Close = 75432 });//87
            result.Add(new Bar() { Open = 75435, High = 75466, Low = 75423, Close = 75431 });//88
            result.Add(new Bar() { Open = 75433, High = 75440, Low = 75410, Close = 75420 });//89
            result.Add(new Bar() { Open = 75421, High = 75466, Low = 75382, Close = 75397 });//90
            result.Add(new Bar() { Open = 75397, High = 75413, Low = 75354, Close = 75396 });//91
            result.Add(new Bar() { Open = 75395, High = 75426, Low = 75305, Close = 75341 });//92
            result.Add(new Bar() { Open = 75341, High = 75344, Low = 75221, Close = 75226 });//93
            result.Add(new Bar() { Open = 75224, High = 75275, Low = 75200, Close = 75250 });//94
            result.Add(new Bar() { Open = 75250, High = 75283, Low = 75211, Close = 75237 });//95
            result.Add(new Bar() { Open = 75239, High = 75319, Low = 75216, Close = 75298 });//96
            result.Add(new Bar() { Open = 75297, High = 75394, Low = 75293, Close = 75386 });//97
            result.Add(new Bar() { Open = 75386, High = 75430, Low = 75347, Close = 75416 });//98
            result.Add(new Bar() { Open = 75410, High = 75415, Low = 75341, Close = 75361 });//99
            result.Add(new Bar() { Open = 75360, High = 75373, Low = 75288, Close = 75304 });//100
            result.Add(new Bar() { Open = 75306, High = 75341, Low = 75292, Close = 75332 });//101
            result.Add(new Bar() { Open = 75332, High = 75334, Low = 75274, Close = 75319 });//102
            result.Add(new Bar() { Open = 75318, High = 75385, Low = 75306, Close = 75360 });
            result.Add(new Bar() { Open = 75358, High = 75473, Low = 75344, Close = 75446 });
            result.Add(new Bar() { Open = 75446, High = 75478, Low = 75418, Close = 75438 });
            result.Add(new Bar() { Open = 75440, High = 75479, Low = 75427, Close = 75468 });
            result.Add(new Bar() { Open = 75468, High = 75476, Low = 75336, Close = 75350 });
            result.Add(new Bar() { Open = 75354, High = 75438, Low = 75344, Close = 75431 });
            result.Add(new Bar() { Open = 75431, High = 75459, Low = 75383, Close = 75410 });
            result.Add(new Bar() { Open = 75412, High = 75438, Low = 75349, Close = 75386 });
            result.Add(new Bar() { Open = 75386, High = 75414, Low = 75381, Close = 75392 });
            result.Add(new Bar() { Open = 75392, High = 75404, Low = 75363, Close = 75376 });
            result.Add(new Bar() { Open = 75370, High = 75388, Low = 75328, Close = 75356 });
            result.Add(new Bar() { Open = 75352, High = 75415, Low = 75322, Close = 75381 });
            result.Add(new Bar() { Open = 75381, High = 75392, Low = 75360, Close = 75377 });
            result.Add(new Bar() { Open = 75382, High = 75453, Low = 75372, Close = 75418 });
            result.Add(new Bar() { Open = 75417, High = 75527, Low = 75406, Close = 75519 });
            result.Add(new Bar() { Open = 75518, High = 75538, Low = 75452, Close = 75458 });
            result.Add(new Bar() { Open = 75461, High = 75466, Low = 75370, Close = 75386 });
            result.Add(new Bar() { Open = 75390, High = 75438, Low = 75382, Close = 75437 });
            result.Add(new Bar() { Open = 75437, High = 75442, Low = 75359, Close = 75427 });
            result.Add(new Bar() { Open = 75427, High = 75484, Low = 75421, Close = 75446 });
            result.Add(new Bar() { Open = 75447, High = 75461, Low = 75339, Close = 75343 });
            result.Add(new Bar() { Open = 75342, High = 75405, Low = 75336, Close = 75369 });
            result.Add(new Bar() { Open = 75370, High = 75412, Low = 75346, Close = 75377 });
            result.Add(new Bar() { Open = 75373, High = 75412, Low = 75331, Close = 75410 });
            result.Add(new Bar() { Open = 75411, High = 75447, Low = 75375, Close = 75431 });
            result.Add(new Bar() { Open = 75434, High = 75466, Low = 75404, Close = 75443 });
            result.Add(new Bar() { Open = 75443, High = 75500, Low = 75431, Close = 75486 });
            result.Add(new Bar() { Open = 75489, High = 75511, Low = 75464, Close = 75479 });
            result.Add(new Bar() { Open = 75480, High = 75497, Low = 75413, Close = 75420 });
            result.Add(new Bar() { Open = 75420, High = 75441, Low = 75412, Close = 75433 });
            result.Add(new Bar() { Open = 75433, High = 75460, Low = 75423, Close = 75432 });
            result.Add(new Bar() { Open = 75430, High = 75436, Low = 75354, Close = 75382 });
            result.Add(new Bar() { Open = 75382, High = 75425, Low = 75374, Close = 75420 });
            result.Add(new Bar() { Open = 75420, High = 75468, Low = 75410, Close = 75447 });
            result.Add(new Bar() { Open = 75446, High = 75493, Low = 75441, Close = 75462 });
            result.Add(new Bar() { Open = 75465, High = 75479, Low = 75450, Close = 75464 });
            result.Add(new Bar() { Open = 75464, High = 75560, Low = 75462, Close = 75522 });
            result.Add(new Bar() { Open = 75540, High = 75555, Low = 75500, Close = 75549 });
            result.Add(new Bar() { Open = 75546, High = 75577, Low = 75521, Close = 75532 });
            result.Add(new Bar() { Open = 75528, High = 75573, Low = 75525, Close = 75570 });
            result.Add(new Bar() { Open = 75570, High = 75574, Low = 75550, Close = 75552 });
            result.Add(new Bar() { Open = 75555, High = 75558, Low = 75529, Close = 75543 });
            result.Add(new Bar() { Open = 75543, High = 75547, Low = 75497, Close = 75498 });
            result.Add(new Bar() { Open = 75499, High = 75509, Low = 75466, Close = 75498 });
            result.Add(new Bar() { Open = 75498, High = 75519, Low = 75496, Close = 75511 });
            result.Add(new Bar() { Open = 75510, High = 75514, Low = 75490, Close = 75505 });
            result.Add(new Bar() { Open = 75505, High = 75511, Low = 75477, Close = 75492 });
            result.Add(new Bar() { Open = 75492, High = 75496, Low = 75481, Close = 75487 });
            result.Add(new Bar() { Open = 75486, High = 75513, Low = 75486, Close = 75506 });
            result.Add(new Bar() { Open = 75504, High = 75547, Low = 75503, Close = 75533 });
            result.Add(new Bar() { Open = 75533, High = 75557, Low = 75525, Close = 75548 });
            result.Add(new Bar() { Open = 75547, High = 75553, Low = 75533, Close = 75541 });
            result.Add(new Bar() { Open = 75541, High = 75546, Low = 75519, Close = 75533 });
            result.Add(new Bar() { Open = 75533, High = 75547, Low = 75522, Close = 75531 });
            result.Add(new Bar() { Open = 75531, High = 75532, Low = 75494, Close = 75520 });
            result.Add(new Bar() { Open = 75519, High = 75539, Low = 75506, Close = 75524 });
            result.Add(new Bar() { Open = 75520, High = 75557, Low = 75514, Close = 75555 });
            result.Add(new Bar() { Open = 75552, High = 75556, Low = 75523, Close = 75537 });
            result.Add(new Bar() { Open = 75540, High = 75557, Low = 75526, Close = 75549 });
            result.Add(new Bar() { Open = 75548, High = 75555, Low = 75524, Close = 75541 });
            result.Add(new Bar() { Open = 75538, High = 75541, Low = 75511, Close = 75514 });
            result.Add(new Bar() { Open = 75512, High = 75514, Low = 75469, Close = 75508 });
            result.Add(new Bar() { Open = 75508, High = 75508, Low = 75479, Close = 75503 });
            result.Add(new Bar() { Open = 75503, High = 75513, Low = 75482, Close = 75487 });
            result.Add(new Bar() { Open = 75486, High = 75494, Low = 75456, Close = 75490 });
            result.Add(new Bar() { Open = 75489, High = 75490, Low = 75474, Close = 75490 });
            result.Add(new Bar() { Open = 75486, High = 75504, Low = 75461, Close = 75474 });
            result.Add(new Bar() { Open = 75474, High = 75481, Low = 75457, Close = 75457 });
            result.Add(new Bar() { Open = 75460, High = 75486, Low = 75456, Close = 75483 });
            result.Add(new Bar() { Open = 75484, High = 75487, Low = 75455, Close = 75473 });
            result.Add(new Bar() { Open = 75473, High = 75494, Low = 75464, Close = 75466 });
            result.Add(new Bar() { Open = 75467, High = 75490, Low = 75461, Close = 75468 });
            result.Add(new Bar() { Open = 75466, High = 75480, Low = 75459, Close = 75461 });
            result.Add(new Bar() { Open = 75461, High = 75495, Low = 75460, Close = 75481 });
            result.Add(new Bar() { Open = 75485, High = 75487, Low = 75470, Close = 75483 });
            result.Add(new Bar() { Open = 75480, High = 75486, Low = 75466, Close = 75475 });
            result.Add(new Bar() { Open = 75475, High = 75488, Low = 75475, Close = 75485 });
            result.Add(new Bar() { Open = 75484, High = 75488, Low = 75469, Close = 75473 });
            result.Add(new Bar() { Open = 75473, High = 75505, Low = 75463, Close = 75500 });
            result.Add(new Bar() { Open = 75497, High = 75504, Low = 75481, Close = 75487 });
            result.Add(new Bar() { Open = 75488, High = 75494, Low = 75474, Close = 75490 });
            result.Add(new Bar() { Open = 75493, High = 75513, Low = 75476, Close = 75509 });
            result.Add(new Bar() { Open = 75509, High = 75526, Low = 75502, Close = 75509 });
            result.Add(new Bar() { Open = 75508, High = 75520, Low = 75498, Close = 75501 });
            result.Add(new Bar() { Open = 75501, High = 75509, Low = 75477, Close = 75494 });
            result.Add(new Bar() { Open = 75494, High = 75507, Low = 75490, Close = 75498 });
            result.Add(new Bar() { Open = 75498, High = 75505, Low = 75490, Close = 75496 });
            result.Add(new Bar() { Open = 75496, High = 75506, Low = 75488, Close = 75495 });
            result.Add(new Bar() { Open = 75495, High = 75505, Low = 75492, Close = 75499 });
            result.Add(new Bar() { Open = 75498, High = 75514, Low = 75486, Close = 75496 });
            result.Add(new Bar() { Open = 75496, High = 75517, Low = 75490, Close = 75498 });
            result.Add(new Bar() { Open = 75498, High = 75518, Low = 75498, Close = 75501 });
            result.Add(new Bar() { Open = 75510, High = 75511, Low = 75490, Close = 75495 });
            result.Add(new Bar() { Open = 75500, High = 75523, Low = 75491, Close = 75516 });
            result.Add(new Bar() { Open = 75513, High = 75539, Low = 75486, Close = 75515 });
            
            return result;
        }

        public List<Indicator> GetExpectedLows_lefLocal3_rightLocal3()
        {
            var result = new List<Indicator>();

            result.Add(new Indicator() { Value = 75952, BarNumber = 6 });
            result.Add(new Indicator() { Value = 75630, BarNumber = 29 });
            result.Add(new Indicator() { Value = 75510, BarNumber = 40 });
            result.Add(new Indicator() { Value = 75609, BarNumber = 47 });
            result.Add(new Indicator() { Value = 75582, BarNumber = 56 });
            result.Add(new Indicator() { Value = 75576, BarNumber = 64 });
            result.Add(new Indicator() { Value = 75340, BarNumber = 78 });
            result.Add(new Indicator() { Value = 75315, BarNumber = 82 });
            result.Add(new Indicator() { Value = 75200, BarNumber = 94 });//94
            result.Add(new Indicator() { Value = 75274, BarNumber = 102 });
            result.Add(new Indicator() { Value = 75336, BarNumber = 107 });
            result.Add(new Indicator() { Value = 75322, BarNumber = 114 });
            result.Add(new Indicator() { Value = 75331, BarNumber = 126 });
            result.Add(new Indicator() { Value = 75354, BarNumber = 134 });
            result.Add(new Indicator() { Value = 75466, BarNumber = 146 });
            result.Add(new Indicator() { Value = 75494, BarNumber = 157 });
            result.Add(new Indicator() { Value = 75456, BarNumber = 167 });
            result.Add(new Indicator() { Value = 75455, BarNumber = 172 });
            result.Add(new Indicator() { Value = 75463, BarNumber = 181 });
            result.Add(new Indicator() { Value = 75486, BarNumber = 192 });

            return result;
        }

        public List<Indicator> GetExpectedHighs_lefLocal3_rightLocal3()
        {
            var result = new List<Indicator>();          

            result.Add(new Indicator() { Value = 76072, BarNumber = 11 });
            result.Add(new Indicator() { Value = 76088, BarNumber = 15 });//!
            result.Add(new Indicator() { Value = 75698, BarNumber = 45 });
            result.Add(new Indicator() { Value = 75687, BarNumber = 53 });
            result.Add(new Indicator() { Value = 75666, BarNumber = 59 });
            result.Add(new Indicator() { Value = 75778, BarNumber = 65 });
            result.Add(new Indicator() { Value = 75808, BarNumber = 70 });
            result.Add(new Indicator() { Value = 75488, BarNumber = 85 });
            result.Add(new Indicator() { Value = 75430, BarNumber = 98 });//94
            result.Add(new Indicator() { Value = 75479, BarNumber = 106 });
            result.Add(new Indicator() { Value = 75538, BarNumber = 118 });
            result.Add(new Indicator() { Value = 75484, BarNumber = 122 });
            result.Add(new Indicator() { Value = 75511, BarNumber = 130 });
            result.Add(new Indicator() { Value = 75577, BarNumber = 141 });
            result.Add(new Indicator() { Value = 75557, BarNumber = 153 });
            result.Add(new Indicator() { Value = 75557, BarNumber = 159 });
            result.Add(new Indicator() { Value = 75495, BarNumber = 176 });
            result.Add(new Indicator() { Value = 75526, BarNumber = 185 });            

            return result;
        }
    }    
}