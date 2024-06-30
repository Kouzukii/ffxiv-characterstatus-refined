using System.Collections.Generic;

namespace CharacterPanelRefined;

public record LevelModifier(int Main, int Sub, int Div);

public static class LevelModifiers {

    public static readonly Dictionary<int, LevelModifier> LevelTable = new() {
        [1] = new(Main: 20, Sub: 56, Div: 56),
        [2] = new(Main: 21, Sub: 57, Div: 57),
        [3] = new(Main: 22, Sub: 60, Div: 60),
        [4] = new(Main: 24, Sub: 62, Div: 62),
        [5] = new(Main: 26, Sub: 65, Div: 65),
        [6] = new(Main: 27, Sub: 68, Div: 68),
        [7] = new(Main: 29, Sub: 70, Div: 70),
        [8] = new(Main: 31, Sub: 73, Div: 73),
        [9] = new(Main: 33, Sub: 76, Div: 76),
        [10] = new(Main: 35, Sub: 78, Div: 78),
        [11] = new(Main: 36, Sub: 82, Div: 82),
        [12] = new(Main: 38, Sub: 85, Div: 85),
        [13] = new(Main: 41, Sub: 89, Div: 89),
        [14] = new(Main: 44, Sub: 93, Div: 93),
        [15] = new(Main: 46, Sub: 96, Div: 96),
        [16] = new(Main: 49, Sub: 100, Div: 100),
        [17] = new(Main: 52, Sub: 104, Div: 104),
        [18] = new(Main: 54, Sub: 109, Div: 109),
        [19] = new(Main: 57, Sub: 113, Div: 113),
        [20] = new(Main: 60, Sub: 116, Div: 116),
        [21] = new(Main: 63, Sub: 122, Div: 122),
        [22] = new(Main: 67, Sub: 127, Div: 127),
        [23] = new(Main: 71, Sub: 133, Div: 133),
        [24] = new(Main: 74, Sub: 138, Div: 138),
        [25] = new(Main: 78, Sub: 144, Div: 144),
        [26] = new(Main: 81, Sub: 150, Div: 150),
        [27] = new(Main: 85, Sub: 155, Div: 155),
        [28] = new(Main: 89, Sub: 162, Div: 162),
        [29] = new(Main: 92, Sub: 168, Div: 168),
        [30] = new(Main: 97, Sub: 173, Div: 173),
        [31] = new(Main: 101, Sub: 181, Div: 181),
        [32] = new(Main: 106, Sub: 188, Div: 188),
        [33] = new(Main: 110, Sub: 194, Div: 194),
        [34] = new(Main: 115, Sub: 202, Div: 202),
        [35] = new(Main: 119, Sub: 209, Div: 209),
        [36] = new(Main: 124, Sub: 215, Div: 215),
        [37] = new(Main: 128, Sub: 223, Div: 223),
        [38] = new(Main: 134, Sub: 229, Div: 229),
        [39] = new(Main: 139, Sub: 236, Div: 236),
        [40] = new(Main: 144, Sub: 244, Div: 244),
        [41] = new(Main: 150, Sub: 253, Div: 253),
        [42] = new(Main: 155, Sub: 263, Div: 263),
        [43] = new(Main: 161, Sub: 272, Div: 272),
        [44] = new(Main: 166, Sub: 283, Div: 283),
        [45] = new(Main: 171, Sub: 292, Div: 292),
        [46] = new(Main: 177, Sub: 302, Div: 302),
        [47] = new(Main: 183, Sub: 311, Div: 311),
        [48] = new(Main: 189, Sub: 322, Div: 322),
        [49] = new(Main: 196, Sub: 331, Div: 331),
        [50] = new(Main: 202, Sub: 341, Div: 341),
        [51] = new(Main: 204, Sub: 342, Div: 366),
        [52] = new(Main: 205, Sub: 344, Div: 392),
        [53] = new(Main: 207, Sub: 345, Div: 418),
        [54] = new(Main: 209, Sub: 346, Div: 444),
        [55] = new(Main: 210, Sub: 347, Div: 470),
        [56] = new(Main: 212, Sub: 349, Div: 496),
        [57] = new(Main: 214, Sub: 350, Div: 522),
        [58] = new(Main: 215, Sub: 351, Div: 548),
        [59] = new(Main: 217, Sub: 352, Div: 574),
        [60] = new(Main: 218, Sub: 354, Div: 600),
        [61] = new(Main: 224, Sub: 355, Div: 630),
        [62] = new(Main: 228, Sub: 356, Div: 660),
        [63] = new(Main: 236, Sub: 357, Div: 690),
        [64] = new(Main: 244, Sub: 358, Div: 720),
        [65] = new(Main: 252, Sub: 359, Div: 750),
        [66] = new(Main: 260, Sub: 360, Div: 780),
        [67] = new(Main: 268, Sub: 361, Div: 810),
        [68] = new(Main: 276, Sub: 362, Div: 840),
        [69] = new(Main: 284, Sub: 363, Div: 870),
        [70] = new(Main: 292, Sub: 364, Div: 900),
        [71] = new(Main: 296, Sub: 365, Div: 940),
        [72] = new(Main: 300, Sub: 366, Div: 980),
        [73] = new(Main: 305, Sub: 367, Div: 1020),
        [74] = new(Main: 310, Sub: 368, Div: 1060),
        [75] = new(Main: 315, Sub: 370, Div: 1100),
        [76] = new(Main: 320, Sub: 372, Div: 1140),
        [77] = new(Main: 325, Sub: 374, Div: 1180),
        [78] = new(Main: 330, Sub: 376, Div: 1220),
        [79] = new(Main: 335, Sub: 378, Div: 1260),
        [80] = new(Main: 340, Sub: 380, Div: 1300),
        [81] = new(Main: 345, Sub: 382, Div: 1360),
        [82] = new(Main: 350, Sub: 384, Div: 1420),
        [83] = new(Main: 355, Sub: 386, Div: 1480),
        [84] = new(Main: 360, Sub: 388, Div: 1540),
        [85] = new(Main: 365, Sub: 390, Div: 1600),
        [86] = new(Main: 370, Sub: 392, Div: 1660),
        [87] = new(Main: 375, Sub: 394, Div: 1720),
        [88] = new(Main: 380, Sub: 396, Div: 1780),
        [89] = new(Main: 385, Sub: 398, Div: 1840),
        [90] = new(Main: 390, Sub: 400, Div: 1900),
        [91] = new(Main: 395, Sub: 402, Div: 1960),
        [92] = new(Main: 400, Sub: 404, Div: 2020),
        [93] = new(Main: 405, Sub: 406, Div: 2080),
        [94] = new(Main: 410, Sub: 408, Div: 2140),
        [95] = new(Main: 415, Sub: 410, Div: 2200),
        [96] = new(Main: 420, Sub: 412, Div: 2260),
        [97] = new(Main: 425, Sub: 414, Div: 2320),
        [98] = new(Main: 430, Sub: 416, Div: 2380),
        [99] = new(Main: 435, Sub: 418, Div: 2440),
        [100] = new(Main: 440, Sub: 420, Div: 2500)
    };

    // this seems to be the modifiers after some testing..
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

    public static double HealModifier(int level) {
        if (level < 60) {
            return level * 1.5 + 10;
        }
        if (level < 70) {
            return (level - 60) * 2 + 100;
        }
        if (level < 80) {
            return 120;
        }
        return (level - 80) * 2.5 + 120.8;
    }

    public static double TankAttackModifier(int level) {
        if (level <= 80) {
            return level + 35;
        }

        return (level - 80) * 4.1 + 115; // goes from 115 at 80 to 156 at 90 ?
    }

    public static double HpModifier(int level) {
        return 4.5 + 0.22 * level;
    }

    public static double TankHpModifier(int level) {
        return 6.7 + 0.31 * level;
    }
}