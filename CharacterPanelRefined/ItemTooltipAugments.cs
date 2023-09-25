using System;
using System.Globalization;
using System.Linq;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

namespace CharacterPanelRefined; 

public unsafe class ItemTooltipAugments {
    private readonly CharacterPanelRefinedPlugin plugin;
    
    public ItemTooltipAugments(CharacterPanelRefinedPlugin plugin) => this.plugin = plugin;

    public void RequestedUpdate(AddonEvent type, AddonArgs args) {
        if (plugin.CtrlHeld) return;
        if (!plugin.Configuration.ShowSyncedStatsOnTooltip) return;
        if (args is not AddonRequestedUpdateArgs requestedUpdateArgs) return;
        var numberArrayData = ((NumberArrayData**)requestedUpdateArgs.NumberArrayData)[29];
        var stringArrayData = ((StringArrayData**)requestedUpdateArgs.StringArrayData)[26];
        if ((numberArrayData->IntArray[2] & 1) == 0) return;
        if ((numberArrayData->IntArray[4] & (1 << 30)) != 0) return; // already processed
        if (IlvlSync.GetCurrentIlvlSync() is not ({ } ilvlSync, var ilvlSyncType)) return;
        
        var itemId = Service.GameGui.HoveredItem;
        if (itemId is >=500000 and <1000000 or >=2000000)
            return; // collectibles & key items
        
        var item = Service.DataManager.GetExcelSheet<Item>()!.GetRow((uint)(itemId % 500000));
        
        if (item == null || item.EquipSlotCategory.Row == 0)
            return;
        
        if (item.LevelItem.Row <= ilvlSync)
            return;

        if (ilvlSyncType == IlvlSyncType.LevelBased && item.LevelEquip <= UIState.Instance()->PlayerState.CurrentLevel)
            return;

        var ilvl = item.LevelItem.Value!;
        var sync = Service.DataManager.GetExcelSheet<ItemLevel>()!.GetRow(ilvlSync)!;
        var cult = CultureInfo.InvariantCulture;
        // weapon
        if ((numberArrayData->IntArray[4] & 0x2000) != 0) {
            if (ParseUnknownLength(stringArrayData->StringArray[9]) is not null) {
                var encoded = EncodeHighlighted(sync.PhysicalDamage.ToString(cult));
                stringArrayData->SetValue(9, encoded, false, true, true);
            }

            if (ParseUnknownLength(stringArrayData->StringArray[8]) is { } autoAtk) {
                var encoded = EncodeHighlighted((double.Parse(autoAtk.Text!, cult) * sync.PhysicalDamage / ilvl.PhysicalDamage - 0.001).ToString("F2", cult));
                stringArrayData->SetValue(8, encoded, false, true, true);
            }
        } else {
            if (ParseUnknownLength(stringArrayData->StringArray[8]) is { } def) {
                var defVal = double.Parse(def.Text!, cult);
                if (defVal > 1) {
                    var encoded = EncodeHighlighted((defVal * sync.Defense / ilvl.Defense - 0.1).ToString("F0", cult));
                    stringArrayData->SetValue(8, encoded, false, true, true);
                } else {
                    stringArrayData->SetValue(8, def.Encode(), false, true, true);
                }
            }

            if (ParseUnknownLength(stringArrayData->StringArray[7]) is { } magicDef) {
                var mDefVal = double.Parse(magicDef.Text!, cult);
                if (mDefVal > 1) {
                    var encoded = EncodeHighlighted((mDefVal * sync.MagicDefense / ilvl.MagicDefense - 0.1).ToString("F0", cult));
                    stringArrayData->SetValue(7, encoded, false, true, true);
                } else {
                    stringArrayData->SetValue(7, magicDef.Encode(), false, true, true);
                }
            }
        }

        if (ParseUnknownLength(stringArrayData->StringArray[37]) is { } vit) {
            var s = vit.Text!.Split('+');
            var encoded = EncodeHighlighted($"{s[0]}+{(double.Parse(s[1], cult) * sync.Vitality / ilvl.Vitality - 0.1).ToString("F0", cult)}");
            stringArrayData->SetValue(37, encoded, false, true, true);
        }

        if (ParseUnknownLength(stringArrayData->StringArray[38]) is { } main) {
            var s = main.Text!.Split('+');
            var encoded = EncodeHighlighted($"{s[0]}+{(double.Parse(s[1], cult) * sync.Strength / ilvl.Strength - 0.1).ToString("F0", cult)}");
            stringArrayData->SetValue(38, encoded, false, true, true);
        }

        var subStrs = new TextPayload?[4];
        var split = new string[4][];
        var parse = new double[4];
        for (var i = 0; i < 4; i++) {
            if ((subStrs[i] = ParseUnknownLength(stringArrayData->StringArray[39 + i])) is not { } sub) continue;
            split[i] = sub.Text!.Split('+');
            parse[i] = double.Parse(split[i][1], cult);
        }

        var subMax = SubstatBreakpoints.GetBreakpoint((byte)item.EquipSlotCategory.Row, (ushort)ilvlSync) ?? parse.Max() * sync.CriticalHit / ilvl.CriticalHit - 0.1;
        for (var i = 0; i < 4; i++) {
            if (subStrs[i] is not { } sub) continue;
            if (parse[i] > subMax) {
                var encoded = EncodeHighlighted($"{split[i][0]}+{Math.Min(parse[i], subMax).ToString("F0", cult)}");
                stringArrayData->SetValue(39 + i, encoded, false, true, true);
            } else {
                stringArrayData->SetValue(39 + i, sub.Encode(), false, true, true);
            }
        }

        for (var i = 0; i < 5; i++) {
            if (ParseUnknownLength(stringArrayData->StringArray[58 + i]) is { } mat) {
                var encoded = EncodeHighlighted($"{mat.Text!.Split('+')[0]}+0");
                stringArrayData->SetValue(58 + i, encoded, false, true, true);
            }
        }
        
        numberArrayData->IntArray[4] |= 1 << 30;
    }

    private TextPayload? ParseUnknownLength(byte* ptr) {
        if (ptr == null) return null;
        var end = 0;
        for (; ptr[end] != 0; end++)
            if (end >= 4096) return null;
        return SeString.Parse(ptr, end).Payloads switch {
            [_, _, _, TextPayload p, _, _, _] => p,
            [TextPayload p] => p,
            _ => null
        };
    }

    private static readonly byte[] FgPayload = new UIForegroundPayload(573).Encode();
    private static readonly byte[] GlPayload = new UIGlowPayload(574).Encode();
    private static readonly byte[] EmPayload = new EmphasisItalicPayload(true).Encode();
    private static readonly byte[] FgClPayload = new UIForegroundPayload(0).Encode();
    private static readonly byte[] GlClPayload = new UIGlowPayload(0).Encode();
    private static readonly byte[] EmClPayload = new EmphasisItalicPayload(false).Encode();

    private byte[] EncodeHighlighted(string newText) {
        return new[] {
            FgPayload, GlPayload, EmPayload,
            new TextPayload(newText).Encode(),
            EmClPayload, FgClPayload, GlClPayload
        }.SelectMany(i => i).ToArray();
    }
}
