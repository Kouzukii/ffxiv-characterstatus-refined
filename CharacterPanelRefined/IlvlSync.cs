using System;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace CharacterPanelRefined;

public enum IlvlSyncType {
    Strict,
    // a lvl 80 535 weapon will not be synced in a level 80 duty
    // a higher level weapon will be synced to 530 however
    LevelBased
}

public static class IlvlSync {
    public static unsafe (uint?, IlvlSyncType) GetCurrentIlvlSync() {
        if (EventFramework.Instance() != null && EventFramework.Instance()->GetInstanceContentDirector() != null) {
            var icd = (IntPtr)EventFramework.Instance()->GetInstanceContentDirector();
            if (*(byte*)(icd + 3284) != 8 && (*(byte*)(icd + 828) & 1) == 0) {
                // min ilvl
                if (*(byte*)(icd + 7324) >= 0x80 && *(ushort*)(icd + 1316) > 0) {
                    PluginLog.LogDebug($"Using min ilvl {*(ushort*)(icd + 1316)}");
                    return (*(ushort*)(icd + 1316), IlvlSyncType.Strict);
                }

                // duty is sync'd
                if (((*(byte*)(icd + 7324) & 0x40) == 0 || (UIState.Instance()->PlayerState.IsLevelSynced & 1) != 0) && *(ushort*)(icd + 1318) > 0) {
                    PluginLog.LogDebug($"Using duty ilvl sync {*(ushort*)(icd + 1318)}");
                    return (*(ushort*)(icd + 1318), IlvlSyncType.Strict);
                }
            }
        }

        if (UIState.Instance()->PlayerState.IsLevelSynced == 1) {
            var syncedLevel = UIState.Instance()->PlayerState.CurrentLevel;
            var ilvl = (uint)(syncedLevel switch {
                90 => 660,
                >=83 => 530 + (syncedLevel - 83) * 3,
                >=81 => 520 + (syncedLevel - 81) * 5,
                80 => 530,
                >=73 => 400 + (syncedLevel - 73) * 3,
                >=71 => 390 + (syncedLevel - 71) * 5,
                70 => 400,
                >=63 => 270 + (syncedLevel - 63) * 3,
                >=61 => 260 + (syncedLevel - 61) * 5,
                60 => 270,
                >=53 => 130 + (syncedLevel - 53) * 3,
                >=51 => 120 + (syncedLevel - 51) * 5,
                50 => 130,
                _ => syncedLevel
            });
            PluginLog.LogDebug($"Using level based ilvl {ilvl}");
            return (ilvl, IlvlSyncType.LevelBased);
        }

        return (null, IlvlSyncType.Strict);
    }
}
