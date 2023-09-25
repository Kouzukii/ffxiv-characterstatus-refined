using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace CharacterPanelRefined;

public class Tooltips : IDisposable {
    private const ushort TitleColor = 8;
    private const ushort HighlightColor = 33;
    private const ushort GreenColor = 43;
    private const ushort OrangeColor = 31;
    private const ushort RedColor = 0x1f4;

    private readonly Dictionary<Entry, IntPtr> allocations = new();

    private readonly Dictionary<Entry, SeString> tooltips = new();

    private readonly Dictionary<(Entry, string), int> keywordIndices = new();

    public enum Entry {
        Crit,
        DirectHit,
        Determination,
        Speed,
        ExpectedDamage,
        ExpectedHeal,
        Tenacity,
        Piety,
        Defense,
        MagicDefense,
        Vitality,
        MainStat,
        ItemLevelSync,
    }

    public Tooltips() {
        foreach (var entry in Enum.GetValues<Entry>()) {
            allocations.Add(entry, Marshal.AllocHGlobal(4096));
        }
    }

    private void LoadLocString(Entry entry, params string[] localization) {
        var str = new SeString();
        tooltips[entry] = str;
        var sb = new StringBuilder();
        var keywords = new HashSet<string>();

        void AddKeyword(string keyword) {
            while (!keywords.Add(keyword))
                keyword += '_';
            keywordIndices[(entry, keyword)] = str.Payloads.Count;
        }

        foreach (var s in localization) {
            int start;
            var end = -1;
            while ((start = s.IndexOf('{', end + 1)) >= 0) {
                if (start - end > 1) {
                    sb.Append(s, end + 1, start - end - 1);
                    str.Append(sb.ToString());
                    sb.Clear();
                }

                end = s.IndexOf('}', start);
                var len = end - start - 1;
                var keyword = s.Substring(start + 1, len);
                if (keyword[0] == '@') {
                    ushort col = 0;
                    switch (keyword) {
                        case "@Title":
                            col = TitleColor;
                            break;
                        case "@Highlight":
                            col = HighlightColor;
                            break;
                        case "@Red":
                            col = RedColor;
                            break;
                        case "@Wasting":
                            AddKeyword(keyword);
                            break;
                    }

                    str.Append(new UIForegroundPayload(col));
                } else {
                    AddKeyword(keyword);
                    str.Append(new TextPayload(""));
                }
            }

            if (end < s.Length - 1)
                sb.Append(s, end + 1, s.Length - end - 1);
        }

        if (sb.Length > 0)
            str.Append(sb.ToString());
    }

    public void Reload() {
        LoadLocString(Entry.Crit, Localization.Tooltips_Crit_Tooltip, "\n", Localization.Tooltips_Wasting, "\n", Localization.Tooltips_Next_Tier);
        LoadLocString(Entry.Determination, Localization.Tooltips_Determination_Tooltip, "\n", Localization.Tooltips_Wasting, "\n",
            Localization.Tooltips_Next_Tier);
        LoadLocString(Entry.DirectHit, Localization.Tooltips_Direct_Hit_Tooltip, "\n", Localization.Tooltips_Wasting, "\n", Localization.Tooltips_Next_Tier);
        LoadLocString(Entry.Tenacity, Localization.Tooltips_Tenacity_Tooltip, "\n", Localization.Tooltips_Wasting, "\n", Localization.Tooltips_Next_Tier);
        LoadLocString(Entry.Piety, Localization.Tooltips_Piety_Tooltip, "\n", Localization.Tooltips_Wasting, "\n", Localization.Tooltips_Next_Tier);
        LoadLocString(Entry.Defense, Localization.Tooltips_Defense_Tooltip, "\n", Localization.Tooltips_Wasting, "\n", Localization.Tooltips_Next_Tier);
        LoadLocString(Entry.MagicDefense, Localization.Tooltips_Magic_Defense_Tooltip, "\n", Localization.Tooltips_Wasting, "\n",
            Localization.Tooltips_Next_Tier);
        LoadLocString(Entry.Vitality, Localization.Tooltips_Vitality_Tooltip);
        LoadLocString(Entry.ExpectedDamage, Localization.Tooltips_Expected_Damage);
        LoadLocString(Entry.ExpectedHeal, Localization.Tooltips_Expected_Heal);
        LoadLocString(Entry.MainStat, Localization.Tooltips_Main_Stat_Tooltip);
        WriteString(Entry.MainStat);
        LoadLocString(Entry.ItemLevelSync, Localization.Tooltips_Item_Level_Sync);
        WriteString(Entry.ItemLevelSync);
    }

    public IntPtr this[Entry entry] => allocations[entry];

    public void UpdateExpectedOutput(Entry entry, double normalOutput, double critOutput) {
        var tooltip = tooltips[entry];
        ((TextPayload)tooltip.Payloads[keywordIndices[(entry, "NormalValue")]]).Text = normalOutput.ToString("N0");
        ((TextPayload)tooltip.Payloads[keywordIndices[(entry, "CritValue")]]).Text = critOutput.ToString("N0");
        WriteString(entry);
    }

    public void UpdateVitality(string job, double hpPerVitality, double jobModifer) {
        var tooltip = tooltips[Entry.Vitality];
        ((TextPayload)tooltip.Payloads[keywordIndices[(Entry.Vitality, "HpPerPoint")]]).Text = hpPerVitality.ToString("N0");
        ((TextPayload)tooltip.Payloads[keywordIndices[(Entry.Vitality, "Job")]]).Text = job;
        ((TextPayload)tooltip.Payloads[keywordIndices[(Entry.Vitality, "BaseHp")]]).Text = jobModifer.ToString("P0");
        WriteString(Entry.Vitality);
    }

    public void UpdateSpeed(in StatInfo statInfo, in StatInfo gcdMain, in StatInfo gcdAlt, int baseGcd, int? altBaseGcd, in GcdModifier? mod) {
        var sb = new StringBuilder();
        sb.Append(Localization.Tooltips_Skill_Spell_Speed_Tooltip)
            .Append("\n\n");
        if (mod != null) {
            sb.Append(mod.Passive ? Localization.Tooltips_Skill_Spell_Speed_With_Mod : Localization.Tooltips_Skill_Spell_Speed_Without_Mod)
                .Append("\n\n");
        }

        sb.Append(Localization.Tooltips_Skill_Spell_Speed_GCD)
            .Append('\n')
            .Append(Localization.Tooltips_Wasting)
            .Append('\n')
            .Append(Localization.Tooltips_Next_Tier)
            .Append("\n\n");
        if (altBaseGcd != null) {
            sb.Append(Localization.Tooltips_Skill_Spell_Speed_GCD)
                .Append('\n')
                .Append(Localization.Tooltips_Wasting)
                .Append('\n')
                .Append(Localization.Tooltips_Next_Tier)
                .Append("\n\n");
        }
        sb.Append(Localization.Tooltips_Skill_Spell_Speed_DoT_Increase)
            .Append('\n')
            .Append(Localization.Tooltips_Wasting)
            .Append('\n')
            .Append(Localization.Tooltips_Next_Tier);
        
        LoadLocString(Entry.Speed, sb.ToString());
        
        var tooltip = tooltips[Entry.Speed];
        
        ((TextPayload)tooltip.Payloads[keywordIndices[(Entry.Speed, "GCD")]]).Text = (baseGcd / 100.0).ToString("N2");
        if (mod != null) {
            ((TextPayload)tooltip.Payloads[keywordIndices[(Entry.Speed, "ModName")]]).Text = mod.Name;
            if (mod.Passive)
                ((TextPayload)tooltip.Payloads[keywordIndices[(Entry.Speed, "ModName_")]]).Text = mod.Name;
        }
        Update(Entry.Speed, gcdMain);
        if (altBaseGcd != null) {
            ((TextPayload)tooltip.Payloads[keywordIndices[(Entry.Speed, "GCD_")]]).Text = (altBaseGcd.Value / 100.0).ToString("N2");
            Update(Entry.Speed, gcdAlt, "_");
            Update(Entry.Speed, statInfo, "__");
        } else {
            Update(Entry.Speed, statInfo, "_");
        }

        WriteString(Entry.Speed);
    }

    public void Update(Entry entry, in StatInfo statInfo, string suffix = "") {
        var tooltip = tooltips[entry];
        ((TextPayload)tooltip.Payloads[keywordIndices[(entry, "PointsPerTier" + suffix)]]).Text = statInfo.PointsPerTier.ToString("N1");
        var wasting = statInfo.CurrentValue - statInfo.PrevTier;
        ((UIForegroundPayload)tooltip.Payloads[keywordIndices[(entry, "@Wasting" + suffix)]]).ColorKey = wasting == 0 ? GreenColor : OrangeColor;
        ((TextPayload)tooltip.Payloads[keywordIndices[(entry, "Wasting" + suffix)]]).Text = wasting.ToString();
        ((TextPayload)tooltip.Payloads[keywordIndices[(entry, $"Points{suffix}{suffix}")]]).Text =
            wasting == 1 ? Localization.Tooltips_Wasting_Points_Singular : Localization.Tooltips_Wasting_Points_Plural;
        var nextTier = statInfo.NextTier - statInfo.CurrentValue;
        ((TextPayload)tooltip.Payloads[keywordIndices[(entry, "NextTier" + suffix)]]).Text = nextTier.ToString();
        ((TextPayload)tooltip.Payloads[keywordIndices[(entry, $"Points{suffix}{suffix}_")]]).Text =
            nextTier == 1 ? Localization.Tooltips_Wasting_Points_Singular : Localization.Tooltips_Wasting_Points_Plural;
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
