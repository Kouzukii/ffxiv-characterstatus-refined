using System;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using ImGuiNET;

namespace CharacterPanelRefined; 

public sealed class ConfigWindow : IDisposable {
    private readonly CharacterPanelRefinedPlugin plugin;
    private bool showConfig;

    public ConfigWindow(CharacterPanelRefinedPlugin plugin, DalamudPluginInterface pluginInterface) {
        this.plugin = plugin;
        pluginInterface.UiBuilder.Draw += Draw;
        pluginInterface.UiBuilder.OpenConfigUi += () => showConfig = true;
        Service.CommandManager.AddHandler("/cprconfig",
            new CommandInfo((_, _) => showConfig ^= true) { HelpMessage = "Open the Character Panel Refined configuration." });
    }

    private void Draw() {
        if (!showConfig)
            return;
        var conf = plugin.Configuration;
        if (ImGui.Begin(Localization.Config_Character_Panel_Refined_Config, ref showConfig, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize)) {
            
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
            if (ImGui.Checkbox(Localization.Config_Show_item_level_information, ref bShowGearProps)) {
                conf.ShowGearProperties = bShowGearProps;
                conf.Save();
            }
            
            var bShowSyncedStats = conf.ShowSyncedStatsOnTooltip;
            if (ImGui.Checkbox(Localization.Config_Show_synced_stats, ref bShowSyncedStats)) {
                conf.ShowSyncedStatsOnTooltip = bShowSyncedStats;
                conf.Save();
            }
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(Localization.Tooltip_Show_synced_stats);

            ImGui.End();
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

    public void Dispose() {
        Service.CommandManager.RemoveHandler("/cprconfig");
    }
}
