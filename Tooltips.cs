using System;
using System.Collections.Generic;
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
            new(new C(8), new T("Critical Hit"), new C(0),
                new T(" affects the rate at which your attacks and heals can critically hit as well as the damage dealt and HP restored by a critical hit.\n\nCritical Hit Rate and Damage increase by 0.1% approx. every "),
                new T("0"), new T(" points.\nYou are currently wasting "), new C(0), new T("0"), new C(0),
                new T(" point(s).\nTo reach the next tier you need "),
                new C(33), new T("0"), new C(0), new T(" point(s)."))
        }, {
            Entry.Determination,
            new(new C(8), new T("Determination"), new C(0),
                new T(" increases damage dealt by attacks as well as HP restored by healing actions.\n\nDetermination increases damage by 0.1% approx. every "),
                new T("0"), new T(" points.\nYou are currently wasting "), new C(0), new T("0"), new C(0),
                new T(" point(s).\nTo reach the next tier you need "),
                new C(33), new T("0"), new C(0), new T(" point(s)."))
        }, {
            Entry.DirectHit,
            new(new C(8), new T("Direct Hit Rate"), new C(0),
                new T(" affects the rate at which your attacks can direct hit.\nA direct hit increases damage done by 25%.\n\nDirect Hit Rate increases by 0.1% approx. every "),
                new T("0"), new T(" points.\nYou are currently wasting "), new C(0), new T("0"), new C(0),
                new T(" point(s).\nTo reach the next tier you need "),
                new C(33), new T("0"), new C(0), new T(" point(s)."))
        }, {
            Entry.Speed,
            new(new C(8), new T("Skill/Spell Speed"), new C(0),
                new T(" shortens cast times and recast timers as well as increase damage done by damage over time effects.\n\nA 2.5s GCD is sped up by 0.01s approx. every "),
                new T("0"), new T(" points.\nYour are currently wasting "), new C(0), new T("0"), new C(0),
                new T(" point(s).\nTo reach the next tier you need "),
                new C(33), new T("0"), new C(0), new T(" point(s).\n\nDoT damage increases by 0.1% approx. every "), new T("0"),
                new T(" points.\nYou are currently wasting "), new C(0), new T("0"), new C(0), new T(" point(s).\nTo reach the next tier you need "),
                new C(33),
                new T("0"), new C(0), new T(" point(s)."))
        }, {
            Entry.Speed28,
            new(new C(8), new T("Spell Speed"), new C(0),
                new T(" shortens cast times and recast timers as well as increase damage done by damage over time effects.\n\nA 2.5s GCD is sped up by 0.01s approx. every "),
                new T("0"), new T(" points.\nYour are currently wasting "), new C(0), new T("0"), new C(0),
                new T(" point(s).\nTo reach the next tier you need "),
                new C(33), new T("0"), new C(0), new T(" point(s).\n\nA 2.8s GCD is sped up by 0.01s approx. every "), new T("0"),
                new T(" points.\nYou are currently wasting "), new C(0), new T("0"), new C(0), new T(" point(s).\nTo reach the next tier you need "),
                new C(33),
                new T("0"), new C(0), new T(" point(s).\n\nDoT damage increases by 0.1% approx. every "), new T("0"),
                new T(" points.\nYou are currently wasting "), new C(0), new T("0"), new C(0), new T(" point(s).\nTo reach the next tier you need "),
                new C(33),
                new T("0"), new C(0), new T(" point(s)."))
        }, {
            Entry.ExpectedDamage,
            new(new T("Average damage of a 100 potency skill, including critical and direct hits.\nDoes not include non-permanent buffs and traits (such as Enochian or Dance Partner).\n\n" +
                      "Damage is multiplied by Potency, Weapon Damage, Main Stat (Str/Int/Dex/Mnd) and all other damage modifiers (such as Direct Hit +25%, Brotherhood +5%) and always has a variance of ±5%."))
        },
        {
            Entry.Tenacity,
            new(new C(8), new T("Tenacity"), new C(0),
                new T(" increases damage dealt and HP restored by your own actions as well as reducing damage taken.\n\nTenacity's effect increases by 0.1% approx. every "),
                new T("0"), new T(" points.\nYou are currently wasting "), new C(0), new T("0"), new C(0),
                new T(" point(s).\nTo reach the next tier you need "),
                new C(33), new T("0"), new C(0), new T(" point(s)."))
        },
        {
            Entry.Piety,
            new(new C(8), new T("Piety"), new C(0),
                new T(" increases the amount of MP gained per server tick (every 3s).\nBy default you will gain 200 MP per tick.\n\nPiety gives 1 extra MP approx. every "),
                new T("0"), new T(" points.\nYou are currently wasting "), new C(0), new T("0"), new C(0),
                new T(" point(s).\nTo reach the next tier you need "),
                new C(33), new T("0"), new C(0), new T(" point(s)."))
        },
        {
            Entry.Defense,
            new(new C(8), new T("Defense"), new C(0),
                new T(" reduces physical damage taken by 1% approx. every "),
                new T("0"), new T(" points.\nYou are currently wasting "), new C(0), new T("0"), new C(0),
                new T(" point(s).\nTo reach the next tier you need "),
                new C(33), new T("0"), new C(0), new T(" point(s)."))
            
        },
        {
            Entry.MagicDefense,
            new(new C(8), new T("Magic Defense"), new C(0),
                new T(" reduces magic damage taken by 1% approx. every "),
                new T("0"), new T(" points.\nYou are currently wasting "), new C(0), new T("0"), new C(0),
                new T(" point(s).\nTo reach the next tier you need "),
                new C(33), new T("0"), new C(0), new T(" point(s)."))
            
        },
        {
            Entry.Vitality,
            new(new C(8), new T("Vitality"), new C(0), new T(" increases max HP by "), new T("0"), new T(" every point.\nAs a "), new T(""), 
                new T(" you have "), new T("0"), new T(" base HP."))
        },
        {
            Entry.MainStat,            
            new(new C(8), new T("Main Stat"), new C(0), 
                new T(" increases damage dealt as well as HP restored by your actions.\nAn increase of 10% main stat constitutes a 10% increase in damage and healing.\n\n" +
                      "SMNs physick is the only exception since it scales with MND instead of INT."))
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
