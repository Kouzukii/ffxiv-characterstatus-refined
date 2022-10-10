using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace CharacterPanelRefined;

public class Tooltips : IDisposable {
    private const int TitleColor = 8;
    private const int HighlightColor = 33;
    private const int GreenColor = 43;
    private const int OrangeColor = 31;

    private readonly Dictionary<Entry, IntPtr> allocations = new();

    private readonly Dictionary<Entry, SeString> tooltips = new();

    private readonly Dictionary<(Entry, string), int> keywordIndices = new();

    public enum Entry {
        Crit,
        DirectHit,
        Determination,
        Speed,
        Speed28,
        ExpectedDamage,
        ExpectedHeal,
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
        LoadLocString(Entry.Speed, Localization.Tooltips_Skill_Spell_Speed_Tooltip, "\n", Localization.Tooltips_Wasting, "\n", Localization.Tooltips_Next_Tier,
            "\n\n", Localization.Tooltips_Skill_Spell_Speed_DoT_Increase, "\n", Localization.Tooltips_Wasting, "\n", Localization.Tooltips_Next_Tier);
        LoadLocString(Entry.Speed28, Localization.Tooltips_Skill_Spell_Speed_Tooltip, "\n", Localization.Tooltips_Wasting, "\n",
            Localization.Tooltips_Next_Tier, "\n\n", Localization.Tooltips_Spell_Speed_28_GCD, "\n", Localization.Tooltips_Wasting, "\n",
            Localization.Tooltips_Next_Tier, "\n\n", Localization.Tooltips_Skill_Spell_Speed_DoT_Increase, "\n", Localization.Tooltips_Wasting, "\n",
            Localization.Tooltips_Next_Tier);
        LoadLocString(Entry.Vitality, Localization.Tooltips_Vitality_Tooltip);
        LoadLocString(Entry.ExpectedDamage, Localization.Tooltips_Expected_Damage);
        LoadLocString(Entry.ExpectedHeal, Localization.Tooltips_Expected_Heal);
        LoadLocString(Entry.MainStat, Localization.Tooltips_Main_Stat_Tooltip);
        WriteString(Entry.MainStat);
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

    public void UpdateSpeed(ref StatInfo statInfo, ref StatInfo gcd25, ref StatInfo gcd28, bool show28) {
        if (show28) {
            Update(Entry.Speed28, ref gcd25);
            Update(Entry.Speed28, ref gcd28, "_");
            Update(Entry.Speed28, ref statInfo, "__");
        } else {
            Update(Entry.Speed, ref gcd25);
            Update(Entry.Speed28, ref statInfo, "_");
        }
    }

    public void Update(Entry entry, ref StatInfo statInfo, string suffix = "") {
        var tooltip = tooltips[entry];
        ((TextPayload)tooltip.Payloads[keywordIndices[(entry, "PointsPerTier" + suffix)]]).Text = statInfo.PointsPerTier.ToString("N1");
        var wasting = statInfo.CurrentValue - statInfo.PrevTier;
        ((UIForegroundPayload)tooltip.Payloads[keywordIndices[(entry, "@Wasting" + suffix)]]).ColorKey = (ushort)(wasting == 0 ? GreenColor : OrangeColor);
        ((TextPayload)tooltip.Payloads[keywordIndices[(entry, "Wasting" + suffix)]]).Text = wasting.ToString();
        ((TextPayload)tooltip.Payloads[keywordIndices[(entry, "Points" + suffix)]]).Text =
            wasting == 1 ? Localization.Tooltips_Wasting_Points_Singular : Localization.Tooltips_Wasting_Points_Plural;
        ((TextPayload)tooltip.Payloads[keywordIndices[(entry, "NextTier" + suffix)]]).Text = (statInfo.NextTier - statInfo.CurrentValue).ToString();
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
