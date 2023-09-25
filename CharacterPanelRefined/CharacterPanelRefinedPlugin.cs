using System.Globalization;
using Dalamud;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace CharacterPanelRefined;

public class CharacterPanelRefinedPlugin : IDalamudPlugin {
    internal Configuration Configuration { get; }
    internal GameFunctions GameFunctions { get; }
    internal bool CtrlHeld { get; private set; }

    private readonly CharacterStatusAugments characterStatusAugments;
    private readonly ItemTooltipAugments itemTooltipAugments;
    private readonly ConfigWindow configWindow;

    public CharacterPanelRefinedPlugin(DalamudPluginInterface pluginInterface) {
        Service.Initialize(pluginInterface);

        Configuration = Configuration.Get(pluginInterface);
        configWindow = new ConfigWindow(this, pluginInterface);
        GameFunctions = new GameFunctions();
        characterStatusAugments = new CharacterStatusAugments(this);
        itemTooltipAugments = new ItemTooltipAugments(this);
        
        UpdateLanguage();
        
        Service.Framework.Update += FrameworkOnUpdate;

        Service.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, "CharacterStatus", characterStatusAugments.OnSetup);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "CharacterStatus", characterStatusAugments.RequestedUpdate);
        Service.AddonLifecycle.RegisterListener(AddonEvent.PreRequestedUpdate, "ItemDetail", itemTooltipAugments.RequestedUpdate);
    }

    internal void UpdateLanguage() {
        var lang = "";

        if (Configuration.UseGameLanguage) {
            lang = Service.ClientState.ClientLanguage switch {
                ClientLanguage.English => "",
                ClientLanguage.French => "fr",
                ClientLanguage.German => "de",
                ClientLanguage.Japanese => "ja",
                _ => ""
            };
        }

        Localization.Culture = new CultureInfo(lang);

        characterStatusAugments.ReloadLocs();
    }

    private void FrameworkOnUpdate(IFramework framework) {
        var ctrlState = Service.KeyState[VirtualKey.CONTROL];
        if (ctrlState == CtrlHeld)
            return;
        CtrlHeld = ctrlState;
        characterStatusAugments.Update();
    }

    public void Dispose() {
        configWindow.Dispose();
        characterStatusAugments.Dispose();
    }
}
