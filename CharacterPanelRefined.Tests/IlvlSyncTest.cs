using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CharacterPanelRefined.Tests; 

public class IlvlSyncTest {
    public List<(int, int)> Breakpoints = new() {
        (635, 126),
        (630, 125),
        (625, 124),
        (620, 123),
        (615, 122),
        (610, 121),
        (605, 120),
        (600, 119),
        (595, 118),
        (590, 117),
        (580, 115),
        (570, 113),
        (560, 111),
        (545, 108),
        (535, 106),
        (530, 105),
        (525, 105),
        (520, 104),
        (520, 104),
        (515, 104),
        (510, 103),
        (505, 103),
        (500, 102),
        (495, 102),
        (490, 101),
        (485, 101),
        (480, 100),
        (475, 100),
        (470, 99),
        (465, 99),
        (460, 98),
        (450, 97),
        (440, 96),
        (430, 95),
        (415, 94),
        (405, 93),
        (400, 92),
        (395, 92),
        (390, 91),
        (385, 91),
        (380, 90),
        (375, 90),
        (370, 89),
        (365, 89),
        (360, 88),
        (355, 88),
        (350, 87),
        (345, 87),
        (340, 86),
        (335, 86),
        (330, 85),
        (320, 84),
        (310, 83)
    };
    
    [Test]
    public void TestIlvlBreakpoints() {
        foreach (var (ilvl, expected) in Breakpoints) {
            var actual = IlvlSync.IlvlSyncToWeaponDamage(ilvl);
            Assert.AreEqual(expected, actual, "Weapon damage off for for {0}", ilvl);
        }
    }
}
