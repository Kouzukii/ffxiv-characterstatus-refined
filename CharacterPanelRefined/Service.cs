using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

#pragma warning disable 8618
namespace CharacterPanelRefined;

// ReSharper disable UnusedAutoPropertyAccessor.Local
internal class Service {
    internal static void Initialize(IDalamudPluginInterface pluginInterface) => pluginInterface.Create<Service>();

    [PluginService]
    public static IClientState ClientState { get; private set; }

    [PluginService]
    internal static IKeyState KeyState { get; private set; }

    [PluginService]
    internal static IFramework Framework { get; private set; }

    [PluginService]
    internal static IDataManager DataManager { get; private set; }

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; }

    [PluginService]
    internal static IGameGui GameGui { get; private set; }

    [PluginService]
    internal static IGameInteropProvider GameInteropProvider { get; private set; }

    [PluginService]
    internal static IPluginLog PluginLog { get; private set; }

    [PluginService]
    internal static IAddonLifecycle AddonLifecycle { get; private set; }
}
#pragma warning restore 8618