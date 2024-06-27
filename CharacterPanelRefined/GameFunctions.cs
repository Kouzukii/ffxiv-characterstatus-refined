using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace CharacterPanelRefined;

public sealed class GameFunctions {

    [Signature("E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? 83 FE 71")]
    public readonly unsafe delegate*unmanaged[Thiscall]<AtkTooltipManager*, AtkResNode*, void> AtkTooltipManagerShowNodeTooltip = null!;

    public GameFunctions() {
        Service.GameInteropProvider.InitializeFromAttributes(this);
    }
}
