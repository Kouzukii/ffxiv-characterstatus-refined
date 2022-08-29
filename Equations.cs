using System;
using System.Linq;
using CharacterPanelRefined.Jobs;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;

namespace CharacterPanelRefined; 

public class Equations {
    public static unsafe double CalcDh(UIState* uiState, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        var dh = statInfo.CurrentValue = uiState->PlayerState.Attributes[(int)Attributes.DirectHit];
        statInfo.DisplayValue = Math.Floor(550d * (dh - lvlModifier.Sub) / lvlModifier.Div) / 1000d;
        statInfo.PrevTier = (int)Math.Ceiling(statInfo.DisplayValue * 1000d * lvlModifier.Div / 550d + lvlModifier.Sub);
        statInfo.NextTier = (int)Math.Ceiling((statInfo.DisplayValue + 0.001) * 1000d * lvlModifier.Div / 550d + lvlModifier.Sub);
        return statInfo.DisplayValue;
    }

    public static unsafe double CalcDet(UIState* uiState, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        var det = statInfo.CurrentValue = uiState->PlayerState.Attributes[(int)Attributes.Determination];
        var cVal = statInfo.DisplayValue = Math.Floor(140d * (det - lvlModifier.Main) / lvlModifier.Div) / 1000d;
        statInfo.PrevTier = (int)Math.Ceiling(cVal * 1000d * lvlModifier.Div / 140d + lvlModifier.Main);
        statInfo.NextTier = (int)Math.Ceiling((cVal + 0.001) * 1000d * lvlModifier.Div / 140d + lvlModifier.Main);
        return cVal;
    }

    public static unsafe double CalcCritRate(UIState* uiState, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        var crit = statInfo.CurrentValue = uiState->PlayerState.Attributes[(int)Attributes.CriticalHit];
        statInfo.DisplayValue = Math.Floor(200d * (crit - lvlModifier.Sub) / lvlModifier.Div + 50) / 1000d;
        statInfo.PrevTier = (int)Math.Ceiling((statInfo.DisplayValue * 1000d - 50) * lvlModifier.Div / 200d + lvlModifier.Sub);
        statInfo.NextTier = (int)Math.Ceiling(((statInfo.DisplayValue + 0.001) * 1000d - 50) * lvlModifier.Div / 200d + lvlModifier.Sub);
        return statInfo.DisplayValue;
    }

    public static unsafe double CalcCritDmg(UIState* uiState, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        var crit = statInfo.CurrentValue = uiState->PlayerState.Attributes[(int)Attributes.CriticalHit];
        var curVal = statInfo.DisplayValue = Math.Floor(200d * (crit - lvlModifier.Sub) / lvlModifier.Div + 1400) / 1000d;
        statInfo.PrevTier = (int)Math.Ceiling((curVal * 1000d - 1400) * lvlModifier.Div / 200d + lvlModifier.Sub);
        statInfo.NextTier = (int)Math.Ceiling(((curVal + 0.001) * 1000d - 1400) * lvlModifier.Div / 200d + lvlModifier.Sub);
        return curVal;
    }

    public static unsafe void CalcDef(UIState* uiState, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        var def = statInfo.CurrentValue = uiState->PlayerState.Attributes[(int)Attributes.Defense];
        statInfo.DisplayValue = Math.Floor(15d * def / lvlModifier.Div) / 100d;
        statInfo.PrevTier = (int)Math.Ceiling(statInfo.DisplayValue * 100d * lvlModifier.Div / 15d);
        statInfo.NextTier = (int)Math.Ceiling((statInfo.DisplayValue + 0.01) * 100d * lvlModifier.Div / 15d);
    }

    public static unsafe void CalcMagicDef(UIState* uiState, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        var def = statInfo.CurrentValue = uiState->PlayerState.Attributes[(int)Attributes.MagicDefense];
        statInfo.DisplayValue = Math.Floor(15d * def / lvlModifier.Div) / 100d;
        statInfo.PrevTier = (int)Math.Ceiling(statInfo.DisplayValue * 100d * lvlModifier.Div / 15d);
        statInfo.NextTier = (int)Math.Ceiling((statInfo.DisplayValue + 0.01) * 100d * lvlModifier.Div / 15d);
    }

    public static unsafe double CalcTenacity(UIState* uiState, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        var ten = statInfo.CurrentValue = uiState->PlayerState.Attributes[(int)Attributes.Tenacity];
        var cVal = statInfo.DisplayValue = Math.Floor(100d * (ten - lvlModifier.Sub) / lvlModifier.Div) / 1000d;
        statInfo.PrevTier = (int)Math.Ceiling(cVal * 1000d * lvlModifier.Div / 100d + lvlModifier.Sub);
        statInfo.NextTier = (int)Math.Ceiling((cVal + 0.001) * 1000d * lvlModifier.Div / 100d + lvlModifier.Sub);
        return cVal;
    }

    public static unsafe void CalcPiety(UIState* uiState, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        var pie= statInfo.CurrentValue = uiState->PlayerState.Attributes[(int)Attributes.Piety];
        var cVal = Math.Floor(150d * (pie - lvlModifier.Main) / lvlModifier.Div);
        statInfo.DisplayValue = cVal + 200;
        statInfo.PrevTier = (int)Math.Ceiling(cVal * lvlModifier.Div / 150d + lvlModifier.Main);
        statInfo.NextTier = (int)Math.Ceiling((cVal + 1) * lvlModifier.Div / 150d + lvlModifier.Main);
    }

    public static unsafe void CalcSkillSpeed(UIState* uiState, ref StatInfo statInfo, ref StatInfo gcd25, ref StatInfo gcd28, ref LevelModifier lvlModifier) {
        CalcSpeed(uiState->PlayerState.Attributes[(int)Attributes.SkillSpeed], ref statInfo, ref gcd25, ref gcd28, ref lvlModifier, false);
    }

    public static unsafe void CalcSpellSpeed(UIState* uiState, ref StatInfo statInfo, ref StatInfo gcd25, ref StatInfo gcd28, ref LevelModifier lvlModifier, bool show28) {
        CalcSpeed(uiState->PlayerState.Attributes[(int)Attributes.SpellSpeed], ref statInfo, ref gcd25, ref gcd28, ref lvlModifier, show28);
    }

    private static void CalcSpeed(int speed, ref StatInfo statInfo, ref StatInfo gcd25, ref StatInfo gcd28, ref LevelModifier lvlModifier, bool show28) {
        statInfo.CurrentValue = speed;
        var cVal = statInfo.DisplayValue = Math.Floor(130d * (speed - lvlModifier.Sub) / lvlModifier.Div) / 1000d;
        statInfo.PrevTier = (int)Math.Ceiling(cVal * 1000d * lvlModifier.Div / 130d + lvlModifier.Sub);
        statInfo.NextTier = (int)Math.Ceiling((cVal + 0.001) * 1000d * lvlModifier.Div / 130d + lvlModifier.Sub);
        gcd25.CurrentValue = speed;
        cVal = gcd25.DisplayValue = Math.Floor(2500d * (1000d + Math.Ceiling(130d * (lvlModifier.Sub - speed) / lvlModifier.Div)) / 10000d) / 100d;
        gcd25.PrevTier = -(int)Math.Floor(Math.Ceiling((cVal + 0.01) * 100d * 10000d / 2500d - 1001d) * lvlModifier.Div / 130d - lvlModifier.Sub);
        gcd25.NextTier = -(int)Math.Floor(Math.Ceiling(cVal * 100d * 10000d / 2500d - 1001d) * lvlModifier.Div / 130d - lvlModifier.Sub);
        if (show28) {
            gcd28.CurrentValue = speed;
            cVal = gcd28.DisplayValue = Math.Floor(2800d * (1000d + Math.Ceiling(130d * (lvlModifier.Sub - speed) / lvlModifier.Div)) / 10000d) / 100d;
            gcd28.PrevTier = -(int)Math.Floor(Math.Ceiling((cVal + 0.01) * 100d * 10000d / 2800d - 1001d) * lvlModifier.Div / 130d - lvlModifier.Sub);
            gcd28.NextTier = -(int)Math.Floor(Math.Ceiling(cVal * 100d * 10000d / 2800d - 1001d) * lvlModifier.Div / 130d - lvlModifier.Sub);
        }
    }

    public static unsafe double CalcRawDamage(UIState* uiState, JobId jobId, double det, double critDmg, double critRate, double dh, double ten,
        ref LevelModifier lvlModifier) {
        try {
            var lvl = uiState->PlayerState.CurrentLevel;
            var ap = uiState->PlayerState.Attributes[(int)(jobId.IsCaster() ? Attributes.AttackMagicPotency : Attributes.AttackPower)];
            var equippedWeapon = InventoryManager.Instance()->GetInventoryContainer(InventoryType.EquippedItems)->Items[0];
            var weaponItem = Service.DataManager.GetExcelSheet<Item>()?.GetRow(equippedWeapon.ItemID);
            var weaponBaseDamage = (jobId.IsCaster() ? weaponItem?.DamageMag : weaponItem?.DamagePhys) ?? 0;
            if (equippedWeapon.Flags.HasFlag(InventoryItem.ItemFlags.HQ)) {
                weaponBaseDamage += (ushort)(weaponItem?.UnkData73.FirstOrDefault(d => d.BaseParamSpecial == 12)?.BaseParamValueSpecial ?? 0);
            }

            var weaponDamage = Math.Floor(lvlModifier.Main * jobId.AttackModifier() / 1000.0 + weaponBaseDamage) / 100.0;
            var atk = Math.Floor(LevelModifiers.AttackModifier(lvl) * (ap - lvlModifier.Main) / lvlModifier.Main + 100) / 100.0;
            var rawDamage = Math.Floor(100 * atk * weaponDamage * (1 + det) * jobId.TraitModifiers(lvl) * (1 + (critDmg - 1) * critRate) * (1 + dh * 0.25) *
                                       (1 + ten));
            return rawDamage;
        } catch (Exception e) {
            PluginLog.Warning(e, "Failed to calculate raw damage");
            return 0;
        }
    }

    public static void CalcHp(JobId jobId, out double hpPerVitality, out double hpModifier) {
        hpPerVitality = jobId.UsesTenacity() ? 31.5 : 22.1;
        hpModifier = jobId.HpModifier() / 100d;
    }
}
