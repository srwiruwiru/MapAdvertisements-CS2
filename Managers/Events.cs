using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace MapAdvertisements.Managers;

public class EventManager(MapAdvertisements plugin)
{
    private readonly MapAdvertisements _plugin = plugin;

    public void RegisterEvents()
    {
        _plugin.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        _plugin.RegisterEventHandler<EventPlayerPing>(OnPlayerPing);

        _plugin.RegisterListener<Listeners.OnServerPrecacheResources>((ResourceManifest manifest) =>
        {
            foreach (var prop in _plugin.Config.Props)
            {
                manifest.AddResource(prop);
            }
        });
        _plugin.RegisterListener<Listeners.OnMapStart>(OnMapStart);
        _plugin.RegisterListener<Listeners.OnTick>(OnTick);
        _plugin.AddCommandListener("say", OnPlayerChatListener);
        _plugin.AddCommandListener("say_team", OnPlayerChatListener);
    }

    private void OnTick()
    {
        foreach (var player in _plugin.MenuManager!._selectedMaterial)
        {
            if (player.Value.onPing)
            {
                player.Key.PrintToCenterHtml($"{_plugin.Localizer["OnTickNotification", player.Value.material!]}");
            }
        }
    }

    private HookResult OnPlayerChatListener(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (player == null) return HookResult.Continue;
        if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.AdminFlag) || !_plugin.MenuManager!._listenForChat.ContainsKey(player))
            return HookResult.Continue;

        var msg = commandInfo.GetArg(1);
        if (string.IsNullOrWhiteSpace(msg)) return HookResult.Continue;
        if (!int.TryParse(msg, out int value))
        {
            player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["NoArg"]}");
            return HookResult.Continue;
        }

        _plugin.MenuManager._listenForChat[player].ModelGroupIndex = value;
        player.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["PlayerSelectedSkin", value]}");
        _plugin.MenuManager._listenForChat[player].EntityProp!.AcceptInput("Skin", _plugin.MenuManager._listenForChat[player].EntityProp, _plugin.MenuManager._listenForChat[player].EntityProp, value.ToString());

        Server.NextFrame(() => _plugin.MenuManager._listenForChat.Remove(player));

        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        _plugin.PropManager!.SpawnProps();
        return HookResult.Continue;
    }

    private HookResult OnPlayerPing(EventPlayerPing @event, GameEventInfo info)
    {
        var ping = @event;
        var player = ping.Userid;
        if (player == null) return HookResult.Continue;

        if (!AdminManager.PlayerHasPermissions(player, _plugin.Config.AdminFlag) || !_plugin.MenuManager!._selectedMaterial.TryGetValue(player, out var selected))
            return HookResult.Continue;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return HookResult.Continue;
        if (!selected!.onPing) return HookResult.Continue;

        if (_plugin.PluginUtils!.CheckMaterial(selected.material!))
        {
            _plugin.PluginUtils!.CreatePropModelOnClick(
                new CounterStrikeSharp.API.Modules.Utils.Vector(ping.X, ping.Y, ping.Z),
                new CounterStrikeSharp.API.Modules.Utils.QAngle(pawn.EyeAngles.X, pawn.EyeAngles.Y, pawn.EyeAngles.Z),
                selected.material!, selected.isOnGround, selected.materialIndex);
        }
        else
        {
            _plugin.PluginUtils!.CreateDecalOnClick(player, new CounterStrikeSharp.API.Modules.Utils.Vector(ping.X, ping.Y, ping.Z));
        }

        return HookResult.Continue;
    }

    private void OnMapStart(string mapName)
    {
        _plugin.PropManager!._props.Clear();
        Server.NextFrame(() =>
        {
            _plugin.PropManager._mapName = mapName;
            _plugin.PropManager._mapFilePath = Path.Combine(_plugin.ModuleDirectory, "maps", $"{mapName}.json");
            _plugin.PropManager.GenerateJsonFile();
            Server.NextFrame(() => _plugin.PropManager.LoadPropsFromMap());
        });
    }
}