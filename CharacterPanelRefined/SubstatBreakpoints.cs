// File created by CharacterPanelRefined.BreakpointGenerator
// Do not modify manually

namespace CharacterPanelRefined;

public static class SubstatBreakpoints {
    public static ushort? GetBreakpoint(byte equipSlotCategory, ushort itemLevel) {
        return equipSlotCategory switch {
            // Two-Handed
            13 => itemLevel switch {
                <= 10 => 1,
                <= 12 => 2,
                <= 16 => 3,
                <= 21 => 4,
                <= 23 => 5,
                <= 26 => 6,
                <= 28 => 7,
                <= 30 => 8,
                <= 31 => 9,
                <= 32 => 10,
                <= 35 => 11,
                <= 37 => 12,
                <= 38 => 13,
                <= 39 => 14,
                <= 41 => 15,
                43 => 17,
                <= 44 => 18,
                <= 45 => 19,
                <= 46 => 20,
                48 => 22,
                <= 49 => 23,
                <= 52 => 24,
                <= 55 => 25,
                <= 60 => 26,
                70 => 29,
                75 => 31,
                80 => 33,
                90 => 37,
                95 => 39,
                <= 100 => 40,
                110 => 44,
                115 => 46,
                <= 120 => 47,
                125 => 49,
                130 => 51,
                <= 133 => 52,
                135 => 54,
                <= 136 => 55,
                139 => 58,
                142 => 62,
                145 => 67,
                148 => 71,
                150 => 74,
                160 => 76,
                <= 170 => 77,
                >= 175 and <= 180 => 79,
                190 => 81,
                <= 200 => 82,
                <= 205 => 83,
                <= 210 => 84,
                220 => 87,
                <= 235 => 88,
                240 => 90,
                <= 245 => 91,
                <= 250 => 92,
                <= 255 => 93,
                <= 260 => 94,
                <= 265 => 95,
                <= 270 => 96,
                273 => 98,
                >= 275 and <= 276 => 100,
                279 => 103,
                282 => 105,
                285 => 108,
                <= 288 => 109,
                290 => 112,
                300 => 114,
                310 => 117,
                320 => 120,
                330 => 124,
                >= 335 and <= 340 => 126,
                <= 345 => 127,
                350 => 129,
                <= 355 => 130,
                <= 360 => 131,
                >= 365 and <= 370 => 134,
                375 => 136,
                <= 380 => 137,
                385 => 139,
                390 => 141,
                395 => 143,
                <= 400 => 144,
                403 => 146,
                <= 405 => 147,
                406 => 149,
                409 => 151,
                412 => 154,
                415 => 157,
                <= 418 => 158,
                430 => 165,
                440 => 170,
                450 => 174,
                460 => 178,
                465 => 180,
                470 => 182,
                475 => 184,
                480 => 186,
                485 => 188,
                490 => 190,
                <= 495 => 191,
                500 => 194,
                505 => 196,
                510 => 199,
                515 => 201,
                520 => 204,
                525 => 206,
                <= 530 => 207,
                533 => 209,
                <= 536 => 210,
                539 => 212,
                <= 542 => 213,
                <= 545 => 214,
                560 => 232,
                570 => 247,
                580 => 253,
                590 => 260,
                595 => 263,
                600 => 266,
                605 => 269,
                610 => 272,
                615 => 275,
                620 => 277,
                625 => 280,
                630 => 283,
                635 => 287,
                640 => 290,
                645 => 293,
                650 => 297,
                655 => 300,
                660 => 303,
                663 => 305,
                <= 665 => 306,
                <= 666 => 307,
                669 => 309,
                672 => 311,
                675 => 314,
                690 => 339,
                700 => 361,
                710 => 370,
                720 => 380,
                725 => 384,
                730 => 388,
                735 => 393,
                740 => 398,
                745 => 401,
                750 => 406,
                760 => 414,
                765 => 419,
                _ => null
            },
            // Main-Hand
            1 => itemLevel switch {
                <= 10 => 1,
                <= 16 => 2,
                <= 21 => 3,
                <= 24 => 4,
                <= 28 => 5,
                <= 31 => 6,
                <= 33 => 7,
                <= 36 => 8,
                <= 38 => 9,
                <= 40 => 10,
                <= 41 => 11,
                <= 43 => 12,
                >= 45 and <= 46 => 14,
                >= 48 and <= 49 => 16,
                <= 52 => 17,
                <= 55 => 18,
                <= 60 => 19,
                70 => 21,
                <= 75 => 22,
                <= 80 => 23,
                90 => 26,
                95 => 28,
                <= 100 => 29,
                110 => 31,
                115 => 33,
                <= 120 => 34,
                <= 125 => 35,
                <= 130 => 36,
                <= 133 => 37,
                >= 135 and <= 136 => 39,
                139 => 42,
                142 => 45,
                145 => 48,
                148 => 51,
                150 => 53,
                <= 160 => 54,
                <= 170 => 55,
                <= 180 => 56,
                190 => 58,
                <= 205 => 59,
                <= 210 => 60,
                220 => 62,
                <= 235 => 63,
                <= 240 => 64,
                <= 245 => 65,
                <= 255 => 66,
                <= 260 => 67,
                <= 265 => 68,
                <= 270 => 69,
                <= 273 => 70,
                >= 275 and <= 276 => 72,
                <= 279 => 73,
                282 => 75,
                285 => 77,
                <= 288 => 78,
                290 => 80,
                <= 300 => 81,
                310 => 84,
                320 => 86,
                330 => 89,
                <= 340 => 90,
                <= 345 => 91,
                <= 350 => 92,
                <= 355 => 93,
                <= 360 => 94,
                >= 365 and <= 370 => 96,
                <= 375 => 97,
                <= 380 => 98,
                <= 385 => 99,
                390 => 101,
                <= 395 => 102,
                <= 400 => 103,
                <= 403 => 104,
                <= 405 => 105,
                <= 406 => 106,
                409 => 108,
                412 => 110,
                415 => 112,
                <= 418 => 113,
                430 => 118,
                440 => 121,
                450 => 124,
                460 => 127,
                465 => 129,
                <= 470 => 130,
                <= 475 => 131,
                480 => 133,
                <= 485 => 134,
                >= 490 and <= 495 => 136,
                500 => 139,
                <= 505 => 140,
                510 => 142,
                515 => 144,
                520 => 146,
                <= 525 => 147,
                <= 530 => 148,
                <= 533 => 149,
                <= 536 => 150,
                <= 539 => 151,
                <= 542 => 152,
                <= 545 => 153,
                560 => 166,
                570 => 176,
                580 => 181,
                590 => 185,
                595 => 188,
                600 => 190,
                605 => 192,
                610 => 194,
                615 => 196,
                620 => 198,
                625 => 200,
                630 => 202,
                635 => 205,
                640 => 207,
                645 => 209,
                650 => 212,
                655 => 214,
                660 => 217,
                <= 663 => 218,
                <= 666 => 219,
                669 => 221,
                <= 672 => 222,
                675 => 224,
                690 => 242,
                700 => 258,
                710 => 264,
                720 => 271,
                725 => 274,
                730 => 277,
                735 => 281,
                740 => 284,
                745 => 286,
                750 => 290,
                760 => 296,
                765 => 299,
                _ => null
            },
            // Off-Hand
            2 => itemLevel switch {
                <= 23 => 1,
                <= 30 => 2,
                <= 34 => 3,
                <= 39 => 4,
                <= 45 => 5,
                <= 48 => 6,
                <= 55 => 7,
                <= 70 => 8,
                80 => 10,
                <= 100 => 11,
                >= 110 and <= 120 => 13,
                <= 125 => 14,
                <= 135 => 15,
                <= 139 => 16,
                <= 142 => 17,
                145 => 19,
                <= 148 => 20,
                <= 150 => 21,
                <= 170 => 22,
                <= 200 => 23,
                <= 210 => 24,
                <= 235 => 25,
                <= 250 => 26,
                <= 270 => 27,
                <= 276 => 28,
                >= 279 and <= 282 => 30,
                <= 288 => 31,
                <= 290 => 32,
                <= 310 => 33,
                <= 320 => 34,
                <= 330 => 35,
                <= 345 => 36,
                <= 360 => 37,
                <= 370 => 38,
                <= 380 => 39,
                <= 390 => 40,
                <= 400 => 41,
                <= 405 => 42,
                <= 409 => 43,
                <= 412 => 44,
                <= 418 => 45,
                430 => 47,
                440 => 49,
                <= 450 => 50,
                <= 465 => 51,
                <= 470 => 52,
                <= 480 => 53,
                <= 490 => 54,
                <= 500 => 55,
                <= 505 => 56,
                <= 515 => 57,
                <= 520 => 58,
                <= 530 => 59,
                <= 536 => 60,
                <= 545 => 61,
                560 => 66,
                570 => 71,
                <= 580 => 72,
                >= 590 and <= 595 => 75,
                <= 600 => 76,
                <= 605 => 77,
                <= 610 => 78,
                <= 620 => 79,
                <= 625 => 80,
                <= 630 => 81,
                <= 635 => 82,
                <= 640 => 83,
                <= 645 => 84,
                <= 650 => 85,
                <= 660 => 86,
                <= 665 => 87,
                <= 669 => 88,
                <= 672 => 89,
                <= 675 => 90,
                690 => 97,
                700 => 103,
                710 => 106,
                720 => 109,
                <= 725 => 110,
                <= 730 => 111,
                <= 735 => 112,
                740 => 114,
                <= 745 => 115,
                <= 750 => 116,
                760 => 118,
                765 => 120,
                _ => null
            },
            // Head, Hands, Feet
            3 or 5 or 8 => itemLevel switch {
                <= 12 => 1,
                <= 19 => 2,
                <= 24 => 3,
                <= 26 => 4,
                <= 31 => 5,
                <= 34 => 6,
                <= 36 => 7,
                <= 39 => 8,
                <= 41 => 9,
                <= 43 => 10,
                <= 45 => 11,
                <= 46 => 12,
                <= 48 => 13,
                <= 50 => 14,
                <= 55 => 15,
                <= 60 => 16,
                <= 70 => 17,
                80 => 20,
                90 => 22,
                100 => 25,
                110 => 27,
                <= 115 => 28,
                <= 120 => 29,
                <= 125 => 30,
                <= 133 => 31,
                136 => 33,
                139 => 35,
                142 => 38,
                145 => 41,
                148 => 43,
                150 => 45,
                <= 160 => 46,
                <= 170 => 47,
                <= 185 => 48,
                <= 195 => 49,
                <= 205 => 50,
                <= 210 => 51,
                <= 215 => 52,
                <= 230 => 53,
                <= 240 => 54,
                <= 245 => 55,
                <= 255 => 56,
                <= 260 => 57,
                <= 270 => 58,
                273 => 60,
                <= 276 => 61,
                <= 279 => 62,
                282 => 64,
                <= 285 => 65,
                <= 288 => 66,
                290 => 68,
                <= 300 => 69,
                310 => 71,
                <= 315 => 72,
                <= 325 => 73,
                330 => 75,
                <= 335 => 76,
                <= 345 => 77,
                <= 350 => 78,
                <= 355 => 79,
                <= 360 => 80,
                >= 370 and <= 375 => 82,
                <= 380 => 83,
                <= 385 => 84,
                <= 390 => 85,
                >= 395 and <= 400 => 87,
                403 => 89,
                <= 406 => 90,
                409 => 92,
                <= 412 => 93,
                415 => 95,
                <= 418 => 96,
                430 => 100,
                440 => 103,
                <= 445 => 104,
                <= 450 => 105,
                455 => 107,
                <= 460 => 108,
                470 => 111,
                <= 475 => 112,
                <= 480 => 113,
                <= 485 => 114,
                <= 490 => 115,
                <= 495 => 116,
                500 => 118,
                <= 505 => 119,
                510 => 121,
                <= 515 => 122,
                520 => 124,
                <= 525 => 125,
                <= 530 => 126,
                <= 533 => 127,
                <= 539 => 128,
                <= 542 => 129,
                <= 545 => 130,
                560 => 141,
                570 => 150,
                575 => 152,
                580 => 154,
                590 => 158,
                595 => 160,
                600 => 162,
                <= 605 => 163,
                610 => 165,
                620 => 168,
                625 => 170,
                630 => 172,
                635 => 174,
                640 => 176,
                645 => 178,
                650 => 180,
                655 => 182,
                660 => 184,
                <= 663 => 185,
                <= 666 => 186,
                669 => 188,
                <= 672 => 189,
                <= 675 => 190,
                690 => 206,
                700 => 219,
                705 => 222,
                710 => 225,
                720 => 231,
                725 => 233,
                730 => 236,
                740 => 241,
                750 => 246,
                760 => 251,
                _ => null
            },
            // Body, Legs
            4 or 7 => itemLevel switch {
                <= 10 => 1,
                <= 12 => 2,
                <= 17 => 3,
                <= 21 => 4,
                <= 24 => 5,
                <= 26 => 6,
                <= 28 => 7,
                <= 30 => 8,
                <= 32 => 9,
                <= 34 => 10,
                <= 36 => 11,
                <= 37 => 12,
                <= 39 => 13,
                <= 40 => 14,
                <= 41 => 15,
                <= 43 => 16,
                <= 44 => 17,
                46 => 19,
                <= 47 => 20,
                <= 48 => 21,
                <= 49 => 22,
                <= 52 => 23,
                <= 55 => 24,
                <= 60 => 25,
                70 => 28,
                80 => 32,
                90 => 36,
                100 => 39,
                110 => 42,
                115 => 44,
                120 => 46,
                <= 125 => 47,
                130 => 49,
                <= 133 => 50,
                136 => 53,
                139 => 56,
                142 => 60,
                145 => 64,
                148 => 69,
                150 => 71,
                160 => 73,
                170 => 75,
                <= 180 => 76,
                <= 185 => 77,
                <= 190 => 78,
                <= 200 => 79,
                <= 205 => 80,
                <= 210 => 81,
                <= 215 => 82,
                <= 220 => 83,
                <= 225 => 84,
                <= 235 => 85,
                <= 240 => 86,
                <= 245 => 87,
                <= 250 => 88,
                >= 255 and <= 260 => 90,
                <= 265 => 91,
                <= 270 => 92,
                273 => 95,
                276 => 97,
                279 => 99,
                282 => 101,
                285 => 104,
                <= 288 => 105,
                290 => 108,
                300 => 110,
                310 => 113,
                <= 315 => 114,
                320 => 116,
                <= 325 => 117,
                330 => 119,
                335 => 121,
                <= 340 => 122,
                <= 345 => 123,
                <= 350 => 124,
                >= 355 and <= 360 => 126,
                370 => 129,
                375 => 131,
                <= 380 => 132,
                385 => 134,
                390 => 136,
                <= 395 => 137,
                400 => 139,
                403 => 141,
                406 => 143,
                409 => 146,
                412 => 148,
                415 => 151,
                418 => 153,
                430 => 159,
                440 => 164,
                <= 445 => 165,
                450 => 167,
                455 => 169,
                460 => 171,
                470 => 176,
                <= 475 => 177,
                480 => 179,
                485 => 181,
                490 => 183,
                <= 495 => 184,
                500 => 187,
                505 => 189,
                510 => 191,
                515 => 194,
                520 => 196,
                525 => 198,
                530 => 200,
                <= 533 => 201,
                536 => 203,
                <= 539 => 204,
                <= 542 => 205,
                545 => 207,
                560 => 224,
                570 => 238,
                575 => 241,
                580 => 244,
                590 => 250,
                595 => 253,
                600 => 257,
                605 => 259,
                610 => 262,
                620 => 268,
                625 => 270,
                630 => 273,
                635 => 276,
                640 => 280,
                645 => 283,
                650 => 286,
                655 => 289,
                660 => 292,
                663 => 294,
                666 => 296,
                669 => 298,
                672 => 300,
                675 => 302,
                690 => 327,
                700 => 348,
                705 => 352,
                710 => 357,
                720 => 366,
                725 => 370,
                730 => 374,
                740 => 383,
                750 => 391,
                760 => 399,
                _ => null
            },
            // Accessory
            >= 9 and <= 12 => itemLevel switch {
                <= 15 => 1,
                <= 20 => 2,
                <= 27 => 3,
                <= 30 => 4,
                <= 34 => 5,
                <= 38 => 6,
                <= 40 => 7,
                <= 43 => 8,
                >= 46 and <= 48 => 10,
                <= 49 => 11,
                <= 55 => 12,
                <= 60 => 13,
                <= 70 => 14,
                80 => 16,
                90 => 18,
                <= 100 => 19,
                110 => 21,
                <= 115 => 22,
                <= 120 => 23,
                <= 130 => 24,
                <= 133 => 25,
                <= 136 => 26,
                139 => 28,
                142 => 30,
                145 => 32,
                148 => 34,
                <= 150 => 35,
                <= 160 => 36,
                <= 170 => 37,
                <= 185 => 38,
                <= 200 => 39,
                <= 210 => 40,
                <= 225 => 41,
                <= 235 => 42,
                <= 245 => 43,
                <= 255 => 44,
                <= 265 => 45,
                <= 270 => 46,
                <= 273 => 47,
                <= 276 => 48,
                <= 279 => 49,
                <= 282 => 50,
                <= 285 => 51,
                <= 288 => 52,
                <= 290 => 53,
                300 => 55,
                <= 310 => 56,
                <= 320 => 57,
                <= 325 => 58,
                <= 330 => 59,
                <= 340 => 60,
                <= 345 => 61,
                <= 355 => 62,
                <= 360 => 63,
                <= 370 => 64,
                <= 375 => 65,
                <= 380 => 66,
                <= 390 => 67,
                <= 395 => 68,
                <= 400 => 69,
                <= 403 => 70,
                <= 406 => 71,
                <= 409 => 72,
                <= 412 => 73,
                415 => 75,
                <= 418 => 76,
                430 => 79,
                440 => 81,
                <= 445 => 82,
                <= 450 => 83,
                <= 455 => 84,
                <= 460 => 85,
                470 => 87,
                <= 475 => 88,
                <= 480 => 89,
                <= 485 => 90,
                <= 490 => 91,
                500 => 93,
                <= 505 => 94,
                <= 510 => 95,
                <= 515 => 96,
                <= 520 => 97,
                <= 525 => 98,
                <= 530 => 99,
                <= 533 => 100,
                <= 539 => 101,
                <= 542 => 102,
                <= 548 => 103,
                560 => 111,
                570 => 118,
                575 => 120,
                <= 580 => 121,
                590 => 124,
                595 => 126,
                <= 600 => 127,
                605 => 129,
                <= 610 => 130,
                620 => 133,
                <= 625 => 134,
                630 => 136,
                <= 635 => 137,
                640 => 139,
                <= 645 => 140,
                650 => 142,
                655 => 144,
                <= 660 => 145,
                <= 663 => 146,
                <= 666 => 147,
                <= 669 => 148,
                <= 672 => 149,
                <= 675 => 150,
                <= 678 => 151,
                690 => 162,
                700 => 173,
                705 => 175,
                710 => 177,
                720 => 182,
                725 => 184,
                730 => 186,
                740 => 190,
                750 => 194,
                760 => 198,
                _ => null
            },
            _ => null
        };
    }

}
