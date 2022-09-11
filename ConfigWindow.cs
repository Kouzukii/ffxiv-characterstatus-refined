using System.Globalization;
using Dalamud;
using Dalamud.Interface;
using ImGuiNET;

namespace CharacterPanelRefined; 

public class ConfigWindow {
    private readonly CharacterPanelRefinedPlugin plugin;

    public ConfigWindow(CharacterPanelRefinedPlugin plugin) {
        this.plugin = plugin;
    }

    public bool ShowConfig { get; internal set; }

    public void Draw() {
        if (!ShowConfig)
            return;
        var conf = plugin.Configuration;
        var bShowConfig = ShowConfig;
        ImGui.SetNextWindowSize(ImGuiHelpers.ScaledVector2(300, 100));
        if (ImGui.Begin(Localization.Config_Character_Panel_Refined_Config, ref bShowConfig, ImGuiWindowFlags.NoCollapse)) {
            
            
            var bShowTooltips = conf.ShowTooltips;
            if (ImGui.Checkbox(Localization.Config_Override_default_tooltips, ref bShowTooltips)) {
                conf.ShowTooltips = bShowTooltips;
                conf.Save();
            }
            ShowTooltipsTooltip();
            
            
            var bUseGameLanguage = conf.UseGameLanguage;
            if (ImGui.Checkbox(Localization.Config_Use_Game_Language_if_available, ref bUseGameLanguage)) {
                conf.UseGameLanguage = bUseGameLanguage;
                plugin.LoadLocalization();
                conf.Save();
            }
            UseGameLanguageTooltip();
            

            ImGui.End();

            if (!bShowConfig)
                ShowConfig = false;
        }
    }
    private void ShowTooltipsTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(Localization.Config_Override_default_tooltips_tooltip);
            ImGui.EndTooltip();
        }
    }
    
    private void UseGameLanguageTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(Localization.Config_Window_Use_Game_Language_Tooltip);
            ImGui.EndTooltip();
        }
    }
}
