namespace CharacterPanelRefined.Jobs; 

public static class JobInfo {
    public static bool IsCrafter(this JobId id) =>
        id switch {
            JobId.CRP => true,
            JobId.BSM => true,
            JobId.ARM => true,
            JobId.GSM => true,
            JobId.LTW => true,
            JobId.WVR => true,
            JobId.ALC => true,
            JobId.CUL => true,
            _ => false
        };
        
    public static bool IsGatherer(this JobId id) =>
        id switch {
            JobId.MIN => true,
            JobId.BTN => true,
            JobId.FSH => true,
            _ => false
        };

    public static bool IsCaster(this JobId id) =>
        id switch {
            JobId.THM => true,
            JobId.BLM => true,
            JobId.CNJ => true,
            JobId.WHM => true,
            JobId.ACN => true,
            JobId.SCH => true,
            JobId.AST => true,
            JobId.SMN => true,
            JobId.RDM => true,
            JobId.BLU => true,
            JobId.SGE => true,
            _ => false
        };

    public static bool UsesDexterity(this JobId id) =>
        id switch {
            JobId.ARC => true,
            JobId.BRD => true,
            JobId.ROG => true,
            JobId.NIN => true,
            JobId.MCH => true,
            JobId.DNC => true,
            _ => false
        };

    public static bool UsesMind(this JobId id) =>
        id switch {
            JobId.CNJ => true,
            JobId.WHM => true,
            JobId.SCH => true,
            JobId.AST => true,
            JobId.SGE => true,
            _ => false
        };

    public static bool UsesTenacity(this JobId id) =>
        id switch {
            JobId.GLA => true,
            JobId.PLD => true,
            JobId.MRD => true,
            JobId.WAR => true,
            JobId.DRK => true,
            JobId.GNB => true,
            _ => false
        };

    // https://github.com/xivapi/ffxiv-datamining/blob/master/csv/ClassJob.csv
    public static int AttackModifier(this JobId id) =>
        id switch {
            JobId.GLA => 95,
            JobId.PGL => 100,
            JobId.MRD => 100,
            JobId.LNC => 105,
            JobId.ARC => 105,
            JobId.CNJ => 105,
            JobId.THM => 105,
            JobId.PLD => 100,
            JobId.MNK => 110,
            JobId.WAR => 105,
            JobId.DRG => 115,
            JobId.BRD => 115,
            JobId.WHM => 115,
            JobId.BLM => 115,
            JobId.ACN => 105,
            JobId.SMN => 115,
            JobId.SCH => 115,
            JobId.ROG => 100,
            JobId.NIN => 110,
            JobId.MCH => 115,
            JobId.DRK => 105,
            JobId.AST => 115,
            JobId.SAM => 112,
            JobId.RDM => 115,
            JobId.BLU => 115,
            JobId.GNB => 100,
            JobId.DNC => 115,
            JobId.RPR => 115,
            JobId.SGE => 115,
            _ => 0
        };

    public static int HpModifier(this JobId id) =>
        id switch {
            JobId.GLA => 130,
            JobId.PGL => 105,
            JobId.MRD => 135,
            JobId.LNC => 110,
            JobId.ARC => 100,
            JobId.CNJ => 100,
            JobId.THM => 100,
            JobId.CRP => 100,
            JobId.BSM => 100,
            JobId.ARM => 100,
            JobId.GSM => 100,
            JobId.LTW => 100,
            JobId.WVR => 100,
            JobId.ALC => 100,
            JobId.CUL => 100,
            JobId.MIN => 100,
            JobId.BTN => 100,
            JobId.FSH => 100,
            JobId.PLD => 140,
            JobId.MNK => 110,
            JobId.WAR => 145,
            JobId.DRG => 115,
            JobId.BRD => 105,
            JobId.WHM => 105,
            JobId.BLM => 105,
            JobId.ACN => 100,
            JobId.SMN => 105,
            JobId.SCH => 105,
            JobId.ROG => 103,
            JobId.NIN => 108,
            JobId.MCH => 105,
            JobId.DRK => 140,
            JobId.AST => 105,
            JobId.SAM => 109,
            JobId.RDM => 105,
            JobId.BLU => 105,
            JobId.GNB => 140,
            JobId.DNC => 105,
            JobId.RPR => 115,
            JobId.SGE => 105,
            _ => 0
        };

    public static double TraitModifiers(this JobId id, int level) {
        if (id is JobId.BLU) {
            return level switch {
                >= 50 => 1.5,
                >= 40 => 1.4,
                >= 30 => 1.3,
                >= 20 => 1.2,
                >= 10 => 1.1,
                _ => 1
            };
        }
        // maim and mend trait
        if (id.IsCaster()) {
            return level switch {
                >= 40 => 1.3,
                >= 20 => 1.1,
                _ => 1
            };
        }
        // increased action damage trait
        return (id, level) switch {
            (JobId.ARC or JobId.BRD or JobId.MCH, >=40) => 1.2,
            (JobId.ARC or JobId.BRD or JobId.MCH, >=20) => 1.1,
            (JobId.DNC, >=60) => 1.2,
            (JobId.DNC, >=50) => 1.1,
            _ => 1
        };
    }
}