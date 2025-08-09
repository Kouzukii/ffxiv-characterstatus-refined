using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace CharacterPanelRefined;

public unsafe class ItemTooltipAugments(CharacterPanelRefinedPlugin plugin) {
    private const int AlreadyProcessed = 0x40000000;

    public void RequestedUpdate(AddonEvent type, AddonArgs args) {
        if (plugin.CtrlHeld) return;
        if (!plugin.Configuration.ShowSyncedStatsOnTooltip) return;
        if (args is not AddonRequestedUpdateArgs requestedUpdateArgs) return;
        var numberArrayData = ((NumberArrayData**)requestedUpdateArgs.NumberArrayData)[30];
        var stringArrayData = ((StringArrayData**)requestedUpdateArgs.StringArrayData)[27];
        if ((numberArrayData->IntArray[5] & 16) == 0) return;
        if ((numberArrayData->IntArray[5] & AlreadyProcessed) != 0) return; // already processed
        if (IlvlSync.GetCurrentIlvlSync() is not ({ } ilvlSync, var ilvlSyncType)) return;

        var itemId = Service.GameGui.HoveredItem;
        if (itemId is >=500000 and <1000000 or >=2000000)
            return; // collectibles & key items

        if (Service.DataManager.GetExcelSheet<Item>().GetRowOrDefault((uint)(itemId % 500000)) is not { EquipSlotCategory.RowId: > 0 } item)
            return;

        if (item.LevelItem.RowId <= ilvlSync)
            return;

        if (ilvlSyncType == IlvlSyncType.LevelBased && item.LevelEquip <= UIState.Instance()->PlayerState.CurrentLevel)
            return;

        var ilvl = item.LevelItem.Value;
        var sync = Service.DataManager.GetExcelSheet<ItemLevel>().GetRow(ilvlSync);
        var cult = CultureInfo.InvariantCulture;
        // weapon
        if ((numberArrayData->IntArray[5] & 0x2000) != 0) {
            if (ParseString(stringArrayData->StringArray[9], out _)) {
                var encoded = EncodeHighlighted(sync.PhysicalDamage.ToString(cult));
                stringArrayData->SetValue(9, encoded, false, true, true);
            }

            if (ParseString(stringArrayData->StringArray[8], out var autoAtk) && double.TryParse(autoAtk.Text, cult, out var autoAtkVal)) {
                var encoded = EncodeHighlighted((autoAtkVal * sync.PhysicalDamage / ilvl.PhysicalDamage - 0.001).ToString("F2", cult));
                stringArrayData->SetValue(8, encoded, false, true, true);
            }
        } else {
            if (ParseString(stringArrayData->StringArray[8], out var def) && double.TryParse(def.Text, cult, out var defVal)) {
                if (defVal > 1) {
                    var encoded = EncodeHighlighted((defVal * sync.Defense / ilvl.Defense - 0.1).ToString("F0", cult));
                    stringArrayData->SetValue(8, encoded, false, true, true);
                } else {
                    stringArrayData->SetValue(8, def.Encode(), false, true, true);
                }
            }

            if (ParseString(stringArrayData->StringArray[7], out var magicDef) && double.TryParse(magicDef.Text, cult, out var mDefVal)) {
                if (mDefVal > 1) {
                    var encoded = EncodeHighlighted((mDefVal * sync.MagicDefense / ilvl.MagicDefense - 0.1).ToString("F0", cult));
                    stringArrayData->SetValue(7, encoded, false, true, true);
                } else {
                    stringArrayData->SetValue(7, magicDef.Encode(), false, true, true);
                }
            }
        }

        var mainIdx = 37;
        var vitIdx = 38;
        // MND and INT are after VIT
        if (item.BaseParam[0].RowId > (uint) Attributes.Vitality)
            (vitIdx, mainIdx) = (mainIdx, vitIdx);

        if (ParseString(stringArrayData->StringArray[mainIdx], out var main)) {
            var s = main.Text!.Split('+');
            if (s.Length == 2 && double.TryParse(s[1], cult, out var mainVal)) {
                var encoded = EncodeHighlighted($"{s[0]}+{(mainVal * sync.Strength / ilvl.Strength - 0.1).ToString("F0", cult)}");
                stringArrayData->SetValue(mainIdx, encoded, false, true, true);
            }
        }

        if (ParseString(stringArrayData->StringArray[vitIdx], out var vit)) {
            var s = vit.Text!.Split('+');
            if (s.Length == 2 && double.TryParse(s[1], cult, out var vitVal)) {
                var encoded = EncodeHighlighted($"{s[0]}+{(vitVal * sync.Vitality / ilvl.Vitality - 0.1).ToString("F0", cult)}");
                stringArrayData->SetValue(vitIdx, encoded, false, true, true);
            }
        }

        var subStrs = new TextPayload?[4];
        var split = new string[4][];
        var parse = new double[4];
        for (var i = 0; i < 4; i++) {
            if (!ParseString(stringArrayData->StringArray[39 + i], out subStrs[i])) continue;
            split[i] = subStrs[i]!.Text!.Split('+');
            if (split[i].Length != 2 || !double.TryParse(split[i][1], cult, out parse[i]))
                subStrs[i] = null;
        }

        var subMax = SubstatBreakpoints.GetBreakpoint((byte)item.EquipSlotCategory.RowId, (ushort)ilvlSync) ?? parse.Max() * sync.CriticalHit / ilvl.CriticalHit - 0.1;
        for (var i = 0; i < 4; i++) {
            if (subStrs[i] is not { } sub) continue;
            if (parse[i] > subMax) {
                var encoded = EncodeHighlighted($"{split[i][0]}+{Math.Min(parse[i], subMax).ToString("F0", cult)}");
                stringArrayData->SetValue(39 + i, encoded, false, true, true);
            } else {
                // write back the original value without highlighting, since it's not actually down-synced
                stringArrayData->SetValue(39 + i, sub.Encode(), false, true, true);
            }
        }

        for (var i = 0; i < 5; i++) {
            if (ParseString(stringArrayData->StringArray[58 + i], out var mat)) {
                var encoded = EncodeHighlighted($"{mat.Text!.Split('+')[0]}+0");
                stringArrayData->SetValue(58 + i, encoded, false, true, true);
            }
        }

        numberArrayData->IntArray[5] |= AlreadyProcessed;
    }

    private bool ParseString(byte* ptr, [NotNullWhen(true)] out TextPayload? text) {
        text = null;
        if (ptr == null) return false;
        text = MemoryHelper.ReadSeStringNullTerminated((IntPtr)ptr).Payloads switch {
            [_, _, _, TextPayload p, _, _, _] => p,
            [TextPayload p] => p,
            _ => null
        };
        return text != null;
    }

    private static readonly byte[] FgPayload = new UIForegroundPayload(573).Encode();
    private static readonly byte[] GlPayload = new UIGlowPayload(574).Encode();
    private static readonly byte[] EmPayload = EmphasisItalicPayload.ItalicsOn.Encode();
    private static readonly byte[] FgClPayload = UIForegroundPayload.UIForegroundOff.Encode();
    private static readonly byte[] GlClPayload = UIGlowPayload.UIGlowOff.Encode();
    private static readonly byte[] EmClPayload = EmphasisItalicPayload.ItalicsOff.Encode();

    private byte[] EncodeHighlighted(string newText) {
        return new[] {
            FgPayload, GlPayload, EmPayload,
            new TextPayload(newText).Encode(),
            EmClPayload, FgClPayload, GlClPayload
        }.SelectMany(i => i).ToArray();
    }
}
