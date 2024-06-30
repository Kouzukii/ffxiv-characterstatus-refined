namespace CharacterPanelRefined;

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
            JobId.PCT => true,
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
            JobId.VPR => true,
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
            JobId.VPR => 110,
            JobId.PCT => 115,
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
            JobId.VPR => 111,
            JobId.PCT => 105,
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

    public static GcdModifier? GcdMod(this JobId id, int level) =>
        (id, level) switch {
            (JobId.SAM, >= 78) => GcdModifier.EnhancedShifu,
            (JobId.SAM, >= 18) => GcdModifier.Shifu,
            (JobId.NIN, >= 45) => GcdModifier.Huton,
            (JobId.MNK, >= 76) => GcdModifier.EnhancedGreasedLightning3,
            (JobId.MNK, >= 40) => GcdModifier.EnhancedGreasedLightning2,
            (JobId.MNK or JobId.PGL, >= 20) => GcdModifier.EnhancedGreasedLightning1,
            (JobId.MNK or JobId.PGL, >=  1) => GcdModifier.GreasedLightning,
            (JobId.WHM, >= 30) => GcdModifier.PresenceOfMind,
            (JobId.BRD, >= 40) => GcdModifier.ArmysPaeon,
            (JobId.BLM, >= 52) => GcdModifier.LeyLines,
            (JobId.VPR, >= 65) => GcdModifier.HuntersInstinct,
            (JobId.PCT, >= 82) => GcdModifier.Hyperphantasia,
            _ => null
        };

    public static AlternateGcd? AltGcd(this JobId id, int level) =>
        (id, level) switch {
            (JobId.BLM, >= 60) => AlternateGcd.FireIV,
            (JobId.SMN or JobId.ARC, >= 6) => AlternateGcd.RubyRite,
            (JobId.PCT, >= 60) => AlternateGcd.BlizzardInCyan,
            _ => null
        };
}

public record AlternateGcd(int Gcd, string Name) {
    public static readonly AlternateGcd FireIV = new(280, Localization.Panel_Fire_IV_GCD);
    public static readonly AlternateGcd RubyRite = new(300, Localization.Panel_RubyRite_GCD);
    public static readonly AlternateGcd BlizzardInCyan = new(330, Localization.Panel_BlizzardInCyan_GCD);
}

public record GcdModifier(int Mod, string Name, string Abbrev, bool Passive) {
    public static readonly GcdModifier EnhancedShifu = new(13, Localization.Buff_Shifu, Localization.Buff_Shifu, true);
    public static readonly GcdModifier Shifu = new(10, Localization.Buff_Shifu, Localization.Buff_Shifu, true);
    public static readonly GcdModifier Huton = new(15, Localization.Buff_Huton, Localization.Buff_Huton, true);
    public static readonly GcdModifier EnhancedGreasedLightning3 = new(20, Localization.Buff_GreasedLightning, Localization.Buff_GreasedLightning_Abbrev, true);
    public static readonly GcdModifier EnhancedGreasedLightning2 = new(15, Localization.Buff_GreasedLightning, Localization.Buff_GreasedLightning_Abbrev, true);
    public static readonly GcdModifier EnhancedGreasedLightning1 = new(10, Localization.Buff_GreasedLightning, Localization.Buff_GreasedLightning_Abbrev, true);
    public static readonly GcdModifier GreasedLightning = new(5, Localization.Buff_GreasedLightning, Localization.Buff_GreasedLightning_Abbrev, true);
    public static readonly GcdModifier ArmysPaeon = new (16, Localization.Buff_ArmysPaeon, Localization.Buff_ArmysPaeon_Abbrev, false);
    public static readonly GcdModifier LeyLines = new (15, Localization.Buff_LeyLines, Localization.Buff_LeyLines_Abbrev, false);
    public static readonly GcdModifier PresenceOfMind = new (20, Localization.Buff_PresenceOfMind, Localization.Buff_PresenceOfMind_Abbrev, false);
    public static readonly GcdModifier HuntersInstinct = new(15, Localization.Buff_HuntersInstinct, Localization.Buff_HuntersInstinct_Abbrev, true);
    public static readonly GcdModifier Hyperphantasia = new(25, Localization.Buff_Hyperphantasia, Localization.Buff_Hyperphantasia_Abbrev, false);
}