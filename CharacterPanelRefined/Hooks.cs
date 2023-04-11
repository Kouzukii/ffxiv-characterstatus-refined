using System;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace CharacterPanelRefined; 

public class Hooks : IDisposable {

    [Signature("4C 8B DC 55 53 41 56 49 8D 6B A1 48 81 EC F0 00 00 00 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 45 07", DetourName = nameof(CharacterStatusOnSetupDetour))]
    private readonly Hook<AddonOnSetup> characterStatusOnSetup = null!;
    
    [Signature("48 89 5c 24 08 48 89 6c 24 10 48 89 74 24 18 57 48 83 ec 50 48 8b 7a 18 48 8b f1 48 8b aa ?? ?? ?? ?? 48 8b 47 20", DetourName = nameof(CharacterStatusRequestUpdateDetour))]
    private readonly Hook<RequestUpdate> characterStatusRequestUpdate = null!;

    [Signature("E9 ?? ?? ?? ?? 83 FF 71")]
    public readonly unsafe delegate*unmanaged[Thiscall]<AtkTooltipManager*, AtkResNode*, void> AtkTooltipManagerShowNodeTooltip = null!;
    
    private unsafe delegate void* AddonOnSetup(AtkUnitBase* atkUnitBase, int a2, void* a3);

    private unsafe delegate void* RequestUpdate(AtkUnitBase* atkUnitBase, void* a2);

    private readonly CharacterPanelRefinedPlugin plugin;

    public Hooks(CharacterPanelRefinedPlugin plugin) {
        this.plugin = plugin;
        SignatureHelper.Initialise(this);
        characterStatusOnSetup.Enable();
        characterStatusRequestUpdate.Enable();
    }

    private unsafe void* CharacterStatusOnSetupDetour(AtkUnitBase* atkUnitBase, int a2, void* a3) {
        var val = characterStatusOnSetup.Original(atkUnitBase, a2, a3);
        try {
            plugin.CharacterStatusOnSetup(atkUnitBase);
        } catch (Exception e) {
            PluginLog.Log(e, "Failed to setup Character Panel");
        }
        return val;
    }


    private unsafe void* CharacterStatusRequestUpdateDetour(AtkUnitBase* atkUnitBase, void* a2) {
        try {
            plugin.CharacterStatusRequestUpdate(atkUnitBase);
        } catch (Exception e) {
            PluginLog.Log(e, "Failed to update Character Panel");
        }
        return characterStatusRequestUpdate.Original(atkUnitBase, a2);
    }

    public void Dispose() {
        characterStatusOnSetup.Dispose();
        characterStatusRequestUpdate.Dispose();
    }
}
