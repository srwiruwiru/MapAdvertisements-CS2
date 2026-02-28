using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using Microsoft.Extensions.Logging;

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

    public EventManager? EventManager { get; private set; }
    public PropManager? PropManager { get; private set; }

    public PluginUtils? PluginUtils { get; private set; }
    public CommandsManager? CommandsManager { get; private set; }

    public PluginMenu? MenuManager {get; private set;}
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

    public void OnConfigParsed(PluginConfig config)
    {
        Config = config;
    }
    public override void Unload(bool hotReload)
    {
        Console.WriteLine("Unloaded MapAdvertisements");
    }

    public void DebugMode(string message)
    {
        if (Config.Debug)
        {
            Logger.LogInformation(message);
        }
    }

}