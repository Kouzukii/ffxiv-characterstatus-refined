using System;
using System.Linq;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.GeneratedSheets;

namespace CharacterPanelRefined;

public class Equations {
    public static double CalcDh(int dh, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        statInfo.CurrentValue = dh;
        var cVal = statInfo.DisplayValue = Math.Floor(550d * (dh - lvlModifier.Sub) / lvlModifier.Div) / 1000d;
        statInfo.PrevTier = (int)Math.Ceiling(cVal * 1000d * lvlModifier.Div / 550d + lvlModifier.Sub);
        statInfo.NextTier = (int)Math.Ceiling((cVal + 0.001) * 1000d * lvlModifier.Div / 550d + lvlModifier.Sub);
        statInfo.PointsPerTier = lvlModifier.Div / 550d;
        return cVal;
    }

    public static double CalcDet(int det, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        statInfo.CurrentValue = det;
        var cVal = statInfo.DisplayValue = Math.Floor(140d * (det - lvlModifier.Main) / lvlModifier.Div) / 1000d;
        statInfo.PrevTier = (int)Math.Ceiling(cVal * 1000d * lvlModifier.Div / 140d + lvlModifier.Main);
        statInfo.NextTier = (int)Math.Ceiling((cVal + 0.001) * 1000d * lvlModifier.Div / 140d + lvlModifier.Main);
        statInfo.PointsPerTier = lvlModifier.Div / 140d;
        return cVal;
    }

    public static double CalcCritRate(int crit, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        statInfo.CurrentValue = crit;
        var cVal = statInfo.DisplayValue = Math.Floor(200d * (crit - lvlModifier.Sub) / lvlModifier.Div + 50) / 1000d;
        statInfo.PrevTier = (int)Math.Ceiling((cVal * 1000d - 50.0000001) * lvlModifier.Div / 200d + lvlModifier.Sub);
        statInfo.NextTier = (int)Math.Ceiling(((cVal + 0.001) * 1000d - 50.0000001) * lvlModifier.Div / 200d + lvlModifier.Sub);
        statInfo.PointsPerTier = lvlModifier.Div / 200d;
        return cVal;
    }

    public static double CalcCritDmg(int crit, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        statInfo.CurrentValue = crit;
        var cVal = statInfo.DisplayValue = Math.Floor(200d * (crit - lvlModifier.Sub) / lvlModifier.Div + 1400) / 1000d;
        statInfo.PrevTier = (int)Math.Ceiling((cVal * 1000d - 1400.0000001) * lvlModifier.Div / 200d + lvlModifier.Sub);
        statInfo.NextTier = (int)Math.Ceiling(((cVal + 0.001) * 1000d - 1400.0000001) * lvlModifier.Div / 200d + lvlModifier.Sub);
        statInfo.PointsPerTier = lvlModifier.Div / 200d;
        return cVal;
    }

    public static void CalcDef(int def, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        statInfo.CurrentValue = def;
        var cVal = statInfo.DisplayValue = Math.Floor(15d * def / lvlModifier.Div) / 100d;
        statInfo.PrevTier = (int)Math.Ceiling(cVal * 100d * lvlModifier.Div / 15d);
        statInfo.NextTier = (int)Math.Ceiling((cVal + 0.01) * 100d * lvlModifier.Div / 15d);
        statInfo.PointsPerTier = lvlModifier.Div / 15d;
    }

    public static void CalcMagicDef(int def, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        statInfo.CurrentValue = def;
        var cVal = statInfo.DisplayValue = Math.Floor(15d * def / lvlModifier.Div) / 100d;
        statInfo.PrevTier = (int)Math.Ceiling(cVal * 100d * lvlModifier.Div / 15d);
        statInfo.NextTier = (int)Math.Ceiling((cVal + 0.01) * 100d * lvlModifier.Div / 15d);
        statInfo.PointsPerTier = lvlModifier.Div / 15d;
    }

    public static double CalcTenacity(int ten, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        statInfo.CurrentValue = ten;
        var cVal = statInfo.DisplayValue = Math.Floor(100d * (ten - lvlModifier.Sub) / lvlModifier.Div) / 1000d;
        statInfo.PrevTier = (int)Math.Ceiling(cVal * 1000d * lvlModifier.Div / 100d + lvlModifier.Sub);
        statInfo.NextTier = (int)Math.Ceiling((cVal + 0.001) * 1000d * lvlModifier.Div / 100d + lvlModifier.Sub);
        statInfo.PointsPerTier = lvlModifier.Div / 100d;
        return cVal;
    }

    public static void CalcPiety(int pie, ref StatInfo statInfo, ref LevelModifier lvlModifier) {
        statInfo.CurrentValue = pie;
        var cVal = Math.Floor(150d * (pie - lvlModifier.Main) / lvlModifier.Div);
        statInfo.DisplayValue = cVal + 200;
        statInfo.PrevTier = (int)Math.Ceiling(cVal * lvlModifier.Div / 150d + lvlModifier.Main);
        statInfo.NextTier = (int)Math.Ceiling((cVal + 1) * lvlModifier.Div / 150d + lvlModifier.Main);
        statInfo.PointsPerTier = lvlModifier.Div / 150d;
    }

    public static void CalcSpeed(int speed, ref StatInfo statInfo, ref StatInfo gcd25, ref StatInfo gcd28, ref LevelModifier lvlModifier, bool show28) {
        statInfo.CurrentValue = speed;
        var cVal = statInfo.DisplayValue = Math.Floor(130d * (speed - lvlModifier.Sub) / lvlModifier.Div) / 1000d;
        statInfo.PrevTier = (int)Math.Ceiling(cVal * 1000d * lvlModifier.Div / 130d + lvlModifier.Sub);
        statInfo.NextTier = (int)Math.Ceiling((cVal + 0.001) * 1000d * lvlModifier.Div / 130d + lvlModifier.Sub);
        statInfo.PointsPerTier = lvlModifier.Div / 130d;
        gcd25.CurrentValue = speed;
        cVal = gcd25.DisplayValue = Math.Floor(2500d * (1000d + Math.Ceiling(130d * (lvlModifier.Sub - speed) / lvlModifier.Div)) / 10000d) / 100d;
        if (cVal >= 2.50)
            gcd25.PrevTier = lvlModifier.Sub;
        else
            gcd25.PrevTier = -(int)Math.Floor(Math.Floor((cVal + 0.01) * 100d * 10000d / 2500d - 1000.01d) * lvlModifier.Div / 130d - lvlModifier.Sub);
        gcd25.NextTier = -(int)Math.Floor(Math.Floor(cVal * 100d * 10000d / 2500d - 1000.01d) * lvlModifier.Div / 130d - lvlModifier.Sub);
        gcd25.PointsPerTier = lvlModifier.Div / 130d / 0.25;
        if (show28) {
            gcd28.CurrentValue = speed;
            cVal = gcd28.DisplayValue = Math.Floor(2800d * (1000d + Math.Ceiling(130d * (lvlModifier.Sub - speed) / lvlModifier.Div)) / 10000d) / 100d;
            if (cVal >= 2.80)
                gcd28.PrevTier = lvlModifier.Sub;
            else
                gcd28.PrevTier = -(int)Math.Floor(Math.Floor((cVal + 0.01) * 100d * 10000d / 2800d - 1000.01d) * lvlModifier.Div / 130d - lvlModifier.Sub);
            gcd28.NextTier = -(int)Math.Floor(Math.Floor(cVal * 100d * 10000d / 2800d - 1000.01d) * lvlModifier.Div / 130d - lvlModifier.Sub);
            gcd28.PointsPerTier = lvlModifier.Div / 130d / 0.28;
        }
    }

    public static unsafe (double AvgDamage, double NormalDamage, double CritDamage, double AvgHeal, double NormalHeal, double CritHeal) CalcExpectedOutput(UIState* uiState, JobId jobId, double det, double critMult, double critRate, double dh, double ten, ref LevelModifier lvlModifier) {
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
            var lvlAttackModifier = jobId.UsesTenacity() ? LevelModifiers.TankAttackModifier(lvl) : LevelModifiers.AttackModifier(lvl);
            var atk = Math.Floor(lvlAttackModifier * (ap - lvlModifier.Main) / lvlModifier.Main + 100) / 100.0;
            var baseDamage = Math.Floor(Math.Floor(Math.Floor(100 * atk * weaponDamage) * (1 + det)) * (1 + ten)) * jobId.TraitModifiers(lvl);
            var avgDamage = Math.Floor(baseDamage * (1 + (critMult - 1) * critRate) * (1 + dh * 0.25));
            var critDamage = Math.Floor(baseDamage * critMult);
            var normalDamage = Math.Floor(baseDamage);
            
            var healPot = Math.Floor(569.0 * (ap - lvlModifier.Main) / 1522.0 + 100) / 100.0;
            var normalHeal = Math.Floor(Math.Floor(Math.Floor(Math.Floor(100 * healPot * weaponDamage) * (1 + det)) * (1 + ten)) * (jobId.IsCaster() ? jobId.TraitModifiers(lvl) : 1));
            var avgHeal = Math.Floor(normalHeal * (1 + (critMult - 1) * critRate));
            var critHeal = Math.Floor(normalHeal * critMult);
            return (avgDamage, normalDamage, critDamage, avgHeal, normalHeal, critHeal);
        } catch (Exception e) {
            PluginLog.Warning(e, "Failed to calculate raw damage");
            return (0, 0, 0, 0, 0, 0);
        }
    }

    public static unsafe void CalcHp(UIState* uiState, JobId jobId, out double hpPerVitality, out double hpModifier) {
        var lvl = uiState->PlayerState.CurrentLevel;
        hpPerVitality = jobId.UsesTenacity() ? LevelModifiers.TankHpModifier(lvl) : LevelModifiers.HpModifier(lvl);
        hpModifier = jobId.HpModifier() / 100d;
    }
}
