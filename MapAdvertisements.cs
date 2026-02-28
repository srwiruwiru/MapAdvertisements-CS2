using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using Microsoft.Extensions.Logging;
using MenuManager;

using MapAdvertisements.Menu;
using MapAdvertisements.Utils;
using MapAdvertisements.Config;
using MapAdvertisements.Managers;

namespace MapAdvertisements;

[MinimumApiVersion(363)]
public class MapAdvertisements : BasePlugin, IPluginConfig<PluginConfig>
{
    public override string ModuleName => "MapAdvertisements";
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "Letaryat (fork by luca.uy)";
    public override string ModuleDescription => "Creates map advertisements.";

    public required PluginConfig Config { get; set; }
    public static MapAdvertisements? Instance { get; private set; }

    public IMenuApi? MenuApi { get; private set; }
    private readonly PluginCapability<IMenuApi?> _menuCapability = new("menu:nfcore");

    public EventManager? EventManager { get; private set; }
    public PropManager? PropManager { get; private set; }
    public PluginUtils? PluginUtils { get; private set; }
    public CommandsManager? CommandsManager { get; private set; }
    public PluginMenu? MenuManager { get; private set; }

    public override void Load(bool hotReload)
    {
        Console.WriteLine("Loaded MapAdvertisements");
        Instance = this;

        EventManager = new EventManager(this);
        PluginUtils = new PluginUtils(this);
        CommandsManager = new CommandsManager(this);
        PropManager = new PropManager(this);
        MenuManager = new PluginMenu(this);

        EventManager.RegisterEvents();
        CommandsManager.RegisterCommands();
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        MenuApi = _menuCapability.Get();

        if (MenuApi == null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[MapAdvertisements] CRITICAL ERROR: MenuManager API not found!");
            Console.WriteLine("[MapAdvertisements] MenuManager is a required dependency for this plugin to function.");
            Console.WriteLine("[MapAdvertisements] Please install MenuManagerCS2 from: https://github.com/NickFox007/MenuManagerCS2");
            Console.WriteLine("[MapAdvertisements] Plugin will now unload automatically.");
            Console.ResetColor();

            Server.NextFrame(() =>
            {
                try { Server.ExecuteCommand($"css_plugins unload {ModuleName}"); }
                catch (Exception ex) { Console.WriteLine($"[MapAdvertisements] Error during auto-unload: {ex.Message}"); }
            });
        }
    }

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;
    }

    public override void Unload(bool hotReload)
    {
        if (MenuApi != null)
        {
            foreach (var player in Utilities.GetPlayers().Where(p => p.IsValid))
                MenuApi.CloseMenu(player);
        }
        Console.WriteLine("Unloaded MapAdvertisements");
    }

    public void DebugMode(string message)
    {
        if (Config.Debug)
            Logger.LogInformation(message);
    }
}