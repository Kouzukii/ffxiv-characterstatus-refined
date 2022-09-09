using System;
using System.Globalization;
using Dalamud;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace CharacterPanelRefined;

[Serializable]
public class Configuration : IPluginConfiguration {
    [NonSerialized] private DalamudPluginInterface pluginInterface = null!;

    public bool ShowTooltips { get; set; } = true;
    public bool UseGameLanguage { get; set; } = true;
    public int Version { get; set; } = 0;

    public static Configuration Get(DalamudPluginInterface pluginInterface) {
        var config = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        config.pluginInterface = pluginInterface;
        return config;
    }

    public void Save() {
        pluginInterface.SavePluginConfig(this);
    }
}
