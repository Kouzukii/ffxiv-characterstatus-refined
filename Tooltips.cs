using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using Dalamud.Game.Text.SeStringHandling;
using T = Dalamud.Game.Text.SeStringHandling.Payloads.TextPayload;
using C = Dalamud.Game.Text.SeStringHandling.Payloads.UIForegroundPayload;

namespace CharacterPanelRefined;

public class Tooltips : IDisposable {
    private const int GreenColor = 43;
    private const int OrangeColor = 31;
    
    private readonly Dictionary<Entry, IntPtr> allocations = new();

    private readonly Dictionary<Entry, SeString> tooltips = new() {
        {
            Entry.Crit,
            new(new C(8), new T(Localization.Tooltips_Critical_Hit), new C(0),
                new T(Localization.Tooltips_Crit_Tooltip),
                new T("0"), new T(Localization.Tooltips_Currently_Wasting), new C(0), new T("0"), new C(0),
                new T(Localization.Tooltips_Next_Tier),
                new C(33), new T("0"), new C(0), new T(Localization.Tooltips_points))
        }, {
            Entry.Determination,
            new(new C(8), new T(Localization.Tooltips_Determination), new C(0),
                new T(Localization.Tooltips_Determination_Tooltip),
                new T("0"), new T(Localization.Tooltips_Currently_Wasting), new C(0), new T("0"), new C(0),
                new T(Localization.Tooltips_Next_Tier),
                new C(33), new T("0"), new C(0), new T(Localization.Tooltips_points))
        }, {
            Entry.DirectHit,
            new(new C(8), new T(Localization.Tooltips_Direct_Hit_Rate), new C(0),
                new T(Localization.Tooltips_Direct_Hit_Tooltip),
                new T("0"), new T(Localization.Tooltips_Currently_Wasting), new C(0), new T("0"), new C(0),
                new T(Localization.Tooltips_Next_Tier),
                new C(33), new T("0"), new C(0), new T(Localization.Tooltips_points))
        }, {
            Entry.Speed,
            new(new C(8), new T(Localization.Tooltips_Skill_Spell_Speed), new C(0),
                new T(Localization.Tooltips_Skill_Spell_Speed_Tooltip),
                new T("0"), new T(Localization.Tooltips_Currently_Wasting), new C(0), new T("0"), new C(0),
                new T(Localization.Tooltips_Next_Tier),
                new C(33), new T("0"), new C(0), new T(Localization.Tooltips_Skill_Spell_Speed_DoT_Increase), new T("0"),
                new T(Localization.Tooltips_Currently_Wasting), new C(0), new T("0"), new C(0), new T(Localization.Tooltips_Next_Tier),
                new C(33),
                new T("0"), new C(0), new T(Localization.Tooltips_points))
        }, {
            Entry.Speed28,
            new(new C(8), new T(Localization.Tooltips_Spell_Speed), new C(0),
                new T(Localization.Tooltips_Skill_Spell_Speed_Tooltip),
                new T("0"), new T(Localization.Tooltips_Currently_Wasting), new C(0), new T("0"), new C(0),
                new T(Localization.Tooltips_Next_Tier),
                new C(33), new T("0"), new C(0), new T(Localization.Tooltips_Spell_Speed_GCD), new T("0"),
                new T(Localization.Tooltips_Currently_Wasting), new C(0), new T("0"), new C(0), new T(Localization.Tooltips_Next_Tier),
                new C(33),
                new T("0"), new C(0), new T(Localization.Tooltips_Skill_Spell_Speed_DoT_Increase), new T("0"),
                new T(Localization.Tooltips_Currently_Wasting), new C(0), new T("0"), new C(0), new T(Localization.Tooltips_Next_Tier),
                new C(33),
                new T("0"), new C(0), new T(Localization.Tooltips_points))
        }, {
            Entry.ExpectedDamage,
            new(new T(Localization.Tooltips_Expected_Damage))
        },
        {
            Entry.Tenacity,
            new(new C(8), new T(Localization.Tooltips_Tenacity), new C(0),
                new T(Localization.Tooltips_Tenacity_Tooltip),
                new T("0"), new T(Localization.Tooltips_Currently_Wasting), new C(0), new T("0"), new C(0),
                new T(Localization.Tooltips_Next_Tier),
                new C(33), new T("0"), new C(0), new T(Localization.Tooltips_points))
        },
        {
            Entry.Piety,
            new(new C(8), new T(Localization.Tooltips_Piety), new C(0),
                new T(Localization.Tooltips_Piety_Tooltip),
                new T("0"), new T(Localization.Tooltips_Currently_Wasting), new C(0), new T("0"), new C(0),
                new T(Localization.Tooltips_Next_Tier),
                new C(33), new T("0"), new C(0), new T(Localization.Tooltips_points))
        },
        {
            Entry.Defense,
            new(new C(8), new T(Localization.Tooltips_Defense), new C(0),
                new T(Localization.Tooltips_Defense_Tooltip),
                new T("0"), new T(Localization.Tooltips_Currently_Wasting), new C(0), new T("0"), new C(0),
                new T(Localization.Tooltips_Next_Tier),
                new C(33), new T("0"), new C(0), new T(Localization.Tooltips_points))
            
        },
        {
            Entry.MagicDefense,
            new(new C(8), new T(Localization.Tooltips_Magic_Defense), new C(0),
                new T(Localization.Tooltips_Magic_Defense_Tooltip),
                new T("0"), new T(Localization.Tooltips_Currently_Wasting), new C(0), new T("0"), new C(0),
                new T(Localization.Tooltips_Next_Tier),
                new C(33), new T("0"), new C(0), new T(Localization.Tooltips_points))
            
        },
        {
            Entry.Vitality,
            new(new C(8), new T(Localization.Tooltips_Vitality), new C(0), new T(Localization.Tooltips_Vitality_Tooltip_1), new T("0"), new T(Localization.Tooltips_Vitality_Tooltip_2), new T(""), 
                new T(Localization.Tooltips_Vitality_Tooltip_3), new T("0"), new T(Localization.Tooltips_Vitality_Tooltip_4))
        },
        {
            Entry.MainStat,            
            new(new C(8), new T(Localization.Tooltips_Main_Stat), new C(0), 
                new T(Localization.Tooltips_Main_Stat_Tooltip))
        }
    };

    public enum Entry {
        Crit,
        DirectHit,
        Determination,
        Speed,
        Speed28,
        ExpectedDamage,
        Tenacity,
        Piety,
        Defense,
        MagicDefense,
        Vitality,
        MainStat
    }

    public Tooltips() {
        foreach (var entry in Enum.GetValues<Entry>()) {
            allocations.Add(entry, Marshal.AllocHGlobal(4096));
            WriteString(entry);
        }
    }

    public IntPtr this[Entry entry] => allocations[entry];

    public void UpdateVitality(string job, double hpPerVitality, double jobModifer) {
        var tooltip = tooltips[Entry.Vitality];
        ((T)tooltip.Payloads[4]).Text = hpPerVitality.ToString("N1");
        ((T)tooltip.Payloads[6]).Text = job;
        ((T)tooltip.Payloads[8]).Text = jobModifer.ToString("P0");
        WriteString(Entry.Vitality);
    }
    
    public void UpdateSpeed(ref StatInfo statInfo, ref StatInfo gcd25, ref StatInfo gcd28, bool show28) {
        var tooltip = tooltips[show28 ? Entry.Speed28 : Entry.Speed];
        var b = 10;
        if (show28) {
            ((T)tooltip.Payloads[b + 4]).Text = (gcd28.NextTier - gcd28.PrevTier).ToString();
            ((C)tooltip.Payloads[b + 6]).ColorKey = (ushort)(gcd28.CurrentValue - gcd28.PrevTier == 0 ? GreenColor : OrangeColor);
            ((T)tooltip.Payloads[b + 11]).Text = (gcd28.NextTier - gcd28.CurrentValue).ToString();
            ((T)tooltip.Payloads[b + 7]).Text = (gcd28.CurrentValue - gcd28.PrevTier).ToString();
            b += 10;
        }

        ((T)tooltip.Payloads[b + 4]).Text = (statInfo.NextTier - statInfo.PrevTier).ToString();
        ((C)tooltip.Payloads[b + 6]).ColorKey = (ushort)(statInfo.CurrentValue - statInfo.PrevTier == 0 ? GreenColor : OrangeColor);
        ((T)tooltip.Payloads[b + 11]).Text = (statInfo.NextTier - statInfo.CurrentValue).ToString();
        ((T)tooltip.Payloads[b + 7]).Text = (statInfo.CurrentValue - statInfo.PrevTier).ToString();
        Update(show28 ? Entry.Speed28 : Entry.Speed, ref gcd25);
    }

    public void Update(Entry entry, ref StatInfo statInfo) {
        var tooltip = tooltips[entry];
        ((T)tooltip.Payloads[4]).Text = (statInfo.NextTier - statInfo.PrevTier).ToString();
        ((C)tooltip.Payloads[6]).ColorKey = (ushort)(statInfo.CurrentValue - statInfo.PrevTier == 0 ? GreenColor : OrangeColor);
        ((T)tooltip.Payloads[11]).Text = (statInfo.NextTier - statInfo.CurrentValue).ToString();
        ((T)tooltip.Payloads[7]).Text = (statInfo.CurrentValue - statInfo.PrevTier).ToString();
        WriteString(entry);
        
    }

    private void WriteString(Entry entry) {
        var target = allocations[entry];
        var encoded = tooltips[entry].Encode();

        Marshal.Copy(encoded, 0, target, encoded.Length);
        Marshal.WriteByte(target, encoded.Length, 0);
    }

    private void ReleaseUnmanagedResources() {
        foreach (var (_, alloc) in allocations) {
            Marshal.FreeHGlobal(alloc);
        }

        allocations.Clear();
    }

    public void Dispose() {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~Tooltips() {
        ReleaseUnmanagedResources();
    }
}
