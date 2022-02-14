using System.Collections.Generic;

namespace CharacterPanelRefined {
    public class LevelModifiers {
        public static readonly Dictionary<int, (int Main, int Sub, int Div)> LevelTable = new() {
            { 1, (Main: 20, Sub: 56, Div: 56) },
            { 2, (Main: 21, Sub: 57, Div: 57) },
            { 3, (Main: 22, Sub: 60, Div: 60) },
            { 4, (Main: 24, Sub: 62, Div: 62) },
            { 5, (Main: 26, Sub: 65, Div: 65) },
            { 6, (Main: 27, Sub: 68, Div: 68) },
            { 7, (Main: 29, Sub: 70, Div: 70) },
            { 8, (Main: 31, Sub: 73, Div: 73) },
            { 9, (Main: 33, Sub: 76, Div: 76) },
            { 10, (Main: 35, Sub: 78, Div: 78) },
            { 11, (Main: 36, Sub: 82, Div: 82) },
            { 12, (Main: 38, Sub: 85, Div: 85) },
            { 13, (Main: 41, Sub: 89, Div: 89) },
            { 14, (Main: 44, Sub: 93, Div: 93) },
            { 15, (Main: 46, Sub: 96, Div: 96) },
            { 16, (Main: 49, Sub: 100, Div: 100) },
            { 17, (Main: 52, Sub: 104, Div: 104) },
            { 18, (Main: 54, Sub: 109, Div: 109) },
            { 19, (Main: 57, Sub: 113, Div: 113) },
            { 20, (Main: 60, Sub: 116, Div: 116) },
            { 21, (Main: 63, Sub: 122, Div: 122) },
            { 22, (Main: 67, Sub: 127, Div: 127) },
            { 23, (Main: 71, Sub: 133, Div: 133) },
            { 24, (Main: 74, Sub: 138, Div: 138) },
            { 25, (Main: 78, Sub: 144, Div: 144) },
            { 26, (Main: 81, Sub: 150, Div: 150) },
            { 27, (Main: 85, Sub: 155, Div: 155) },
            { 28, (Main: 89, Sub: 162, Div: 162) },
            { 29, (Main: 92, Sub: 168, Div: 168) },
            { 30, (Main: 97, Sub: 173, Div: 173) },
            { 31, (Main: 101, Sub: 181, Div: 181) },
            { 32, (Main: 106, Sub: 188, Div: 188) },
            { 33, (Main: 110, Sub: 194, Div: 194) },
            { 34, (Main: 115, Sub: 202, Div: 202) },
            { 35, (Main: 119, Sub: 209, Div: 209) },
            { 36, (Main: 124, Sub: 215, Div: 215) },
            { 37, (Main: 128, Sub: 223, Div: 223) },
            { 38, (Main: 134, Sub: 229, Div: 229) },
            { 39, (Main: 139, Sub: 236, Div: 236) },
            { 40, (Main: 144, Sub: 244, Div: 244) },
            { 41, (Main: 150, Sub: 253, Div: 253) },
            { 42, (Main: 155, Sub: 263, Div: 263) },
            { 43, (Main: 161, Sub: 272, Div: 272) },
            { 44, (Main: 166, Sub: 283, Div: 283) },
            { 45, (Main: 171, Sub: 292, Div: 292) },
            { 46, (Main: 177, Sub: 302, Div: 302) },
            { 47, (Main: 183, Sub: 311, Div: 311) },
            { 48, (Main: 189, Sub: 322, Div: 322) },
            { 49, (Main: 196, Sub: 331, Div: 331) },
            { 50, (Main: 202, Sub: 341, Div: 341) },
            { 51, (Main: 204, Sub: 342, Div: 366) },
            { 52, (Main: 205, Sub: 344, Div: 392) },
            { 53, (Main: 207, Sub: 345, Div: 418) },
            { 54, (Main: 209, Sub: 346, Div: 444) },
            { 55, (Main: 210, Sub: 347, Div: 470) },
            { 56, (Main: 212, Sub: 349, Div: 496) },
            { 57, (Main: 214, Sub: 350, Div: 522) },
            { 58, (Main: 215, Sub: 351, Div: 548) },
            { 59, (Main: 217, Sub: 352, Div: 574) },
            { 60, (Main: 218, Sub: 354, Div: 600) },
            { 61, (Main: 224, Sub: 355, Div: 630) },
            { 62, (Main: 228, Sub: 356, Div: 660) },
            { 63, (Main: 236, Sub: 357, Div: 690) },
            { 64, (Main: 244, Sub: 358, Div: 720) },
            { 65, (Main: 252, Sub: 359, Div: 750) },
            { 66, (Main: 260, Sub: 360, Div: 780) },
            { 67, (Main: 268, Sub: 361, Div: 810) },
            { 68, (Main: 276, Sub: 362, Div: 840) },
            { 69, (Main: 284, Sub: 363, Div: 870) },
            { 70, (Main: 292, Sub: 364, Div: 900) },
            { 71, (Main: 296, Sub: 365, Div: 940) },
            { 72, (Main: 300, Sub: 366, Div: 980) },
            { 73, (Main: 305, Sub: 367, Div: 1020) },
            { 74, (Main: 310, Sub: 368, Div: 1060) },
            { 75, (Main: 315, Sub: 370, Div: 1100) },
            { 76, (Main: 320, Sub: 372, Div: 1140) },
            { 77, (Main: 325, Sub: 374, Div: 1180) },
            { 78, (Main: 330, Sub: 376, Div: 1220) },
            { 79, (Main: 335, Sub: 378, Div: 1260) },
            { 80, (Main: 340, Sub: 380, Div: 1300) },
            { 81, (Main: 345, Sub: 382, Div: 1360) },
            { 82, (Main: 350, Sub: 384, Div: 1420) },
            { 83, (Main: 355, Sub: 386, Div: 1480) },
            { 84, (Main: 360, Sub: 388, Div: 1540) },
            { 85, (Main: 365, Sub: 390, Div: 1600) },
            { 86, (Main: 370, Sub: 392, Div: 1660) },
            { 87, (Main: 375, Sub: 394, Div: 1720) },
            { 88, (Main: 380, Sub: 396, Div: 1780) },
            { 89, (Main: 385, Sub: 398, Div: 1840) },
            { 90, (Main: 390, Sub: 400, Div: 1900) }
        };

        // this seems to be the modifiers after some testing..
        // it is yet missing the adjustment for tanks however
        public static double AttackModifier(int level) {
            if (level <= 50) {
                return 75;
            }

            if (level <= 70) {
                return (level - 50) * 2.5 + 75;
            }

            if (level <= 80) {
                return (level - 70) * 4 + 125;
            }

            return (level - 80) * 3 + 165;
        }
    }
}