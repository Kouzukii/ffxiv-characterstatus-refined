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
        if (ImGui.Begin("Character Panel Refined Config", ref bShowConfig, ImGuiWindowFlags.NoCollapse)) {
            var bShowTooltips = conf.ShowTooltips;
            if (ImGui.Checkbox("Override default tooltips", ref bShowTooltips)) {
                conf.ShowTooltips = bShowTooltips;
                conf.Save();
            }
            ShowTooltipsTooltip();

            ImGui.End();

            if (!bShowConfig)
                ShowConfig = false;
        }
    }
    private void ShowTooltipsTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted("Override the default tooltips for all stats. Currently only english language available.");
            ImGui.EndTooltip();
        }
    }
}
