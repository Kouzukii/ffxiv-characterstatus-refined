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
        if (ImGui.Begin(Localization.Config_Character_Panel_Refined_Config, ref bShowConfig, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize)) {
            
            ImGui.TextUnformatted(Localization.Config_Help_Apply_Changes);
            ImGui.Spacing();
            
            var bShowTooltips = conf.ShowTooltips;
            if (ImGui.Checkbox(Localization.Config_Override_default_tooltips, ref bShowTooltips)) {
                conf.ShowTooltips = bShowTooltips;
                conf.Save();
            }
            ShowTooltipsTooltip();
            
            
            var bUseGameLanguage = conf.UseGameLanguage;
            if (ImGui.Checkbox(Localization.Config_Use_Game_Language_if_available, ref bUseGameLanguage)) {
                conf.UseGameLanguage = bUseGameLanguage;
                plugin.UpdateLanguage();
                conf.Save();
            }
            UseGameLanguageTooltip();
            
            var bShowAvgDamage = conf.ShowAvgDamage;
            if (ImGui.Checkbox(Localization.Config_Show_average_damage, ref bShowAvgDamage)) {
                conf.ShowAvgDamage = bShowAvgDamage;
                conf.Save();
            }
            
            var bShowAvgHealing = conf.ShowAvgHealing;
            if (ImGui.Checkbox(Localization.Config_Show_average_healing, ref bShowAvgHealing)) {
                conf.ShowAvgHealing = bShowAvgHealing;
                conf.Save();
            }
            
            var bShowCritDamageIncrease = conf.ShowCritDamageIncrease;
            if (ImGui.Checkbox(Localization.Config_Show_crit_damage_increase, ref bShowCritDamageIncrease)) {
                conf.ShowCritDamageIncrease = bShowCritDamageIncrease;
                conf.Save();
            }
            
            var bShowDhDamageIncrease = conf.ShowDhDamageIncrease;
            if (ImGui.Checkbox(Localization.Config_Show_direct_hit_damage_increase, ref bShowDhDamageIncrease)) {
                conf.ShowDhDamageIncrease = bShowDhDamageIncrease;
                conf.Save();
            }
            
            var bShowStatsWithoutFood = conf.ShowDoHDoLStatsWithoutFood;
            if (ImGui.Checkbox(Localization.Config_Show_stats_without_consumables, ref bShowStatsWithoutFood)) {
                conf.ShowDoHDoLStatsWithoutFood = bShowStatsWithoutFood;
                conf.Save();
            }
            
            var bShowGearProps = conf.ShowGearProperties;
            if (ImGui.Checkbox("Show item level information", ref bShowGearProps)) {
                conf.ShowGearProperties = bShowGearProps;
                conf.Save();
            }

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
