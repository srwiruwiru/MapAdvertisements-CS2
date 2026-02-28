using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using MenuManager;

using MapAdvertisements.Models;

namespace MapAdvertisements.Menu;

public partial class PluginMenu(MapAdvertisements plugin)
{
    private readonly MapAdvertisements _plugin = plugin;

    public Dictionary<CCSPlayerController, SelectedMaterialModel> _selectedMaterial = new();
    public Dictionary<CCSPlayerController, PropModel> _listenForChat = new();

    private readonly string[] _directions = ["X+", "X-", "Y+", "Y-", "Z+", "Z-"];
    private readonly int[] _decalSize = [16, 32, 64, 128, 256, 512, 1024];

    public void ShowMapAdvertMenu(CCSPlayerController player)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var menu = api.GetMenu(_plugin.Localizer["MapAdvertMenu_Header"]);
        menu.AddMenuOption(_plugin.Localizer["CreatePropMenu"], (p, o) => CreatePropMenu(p));
        menu.AddMenuOption(_plugin.Localizer["CreateDecalMenu"], (p, o) => CreateDecalMenu(p));
        menu.AddMenuOption(_plugin.Localizer["EditPropsMenu"], (p, o) => EditPropsMenu(p));
        menu.AddMenuOption(_plugin.Localizer["EditDecalsMenu"], (p, o) => EditDecalMenu(p));
        menu.AddMenuOption(_plugin.Localizer["RemoveAdvertsMenu"], (p, o) => RemoveAdvertsMenu(p));

        menu.AddMenuOption(_plugin.Localizer["SaveAdvertsMenu"], (p, o) =>
        {
            try
            {
                _plugin.PropManager!.SaveAllAdverts();
                p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["SavedAdverts"]}");
            }
            catch (Exception error)
            {
                p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["SavedAdvertsError"]}");
                _plugin.DebugMode($"{error}");
            }
        });

        menu.AddMenuOption(_plugin.Localizer["ClearCacheMenu"], (p, o) => _plugin.MenuManager!._selectedMaterial.Remove(p));
        menu.Open(player);
    }

    private void RemoveAdvertsMenu(CCSPlayerController player)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var menu = api.GetMenu(_plugin.Localizer["RemoveAdvert_Header"]);
        foreach (var adv in _plugin.PropManager!._props)
        {
            var captured = adv;
            menu.AddMenuOption($"#{captured.Id} — {captured.modelPath}", (p, o) =>
            {
                _plugin.PropManager.RemovePropFromFile(captured.Id);
                p.PrintToChat($"{_plugin.Localizer["SuccessRemove", captured.Id]}");
                Server.NextFrame(() => ShowMapAdvertMenu(p));
            });
        }

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => ShowMapAdvertMenu(p));
        menu.Open(player);
    }

    public void CordsMenu(CCSPlayerController player, PropModel prop, int propId, int type, Action<CCSPlayerController> onBack)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var entity = prop.EntityProp;
        if (entity == null) return;

        var menu = api.GetMenu(_plugin.Localizer[$"CordsFor_{type}", propId]);
        if (type == 0)
        {
            foreach (var v in _plugin.Config.customPositionValues)
            {
                foreach (var dir in _directions)
                {
                    var cv = v; var cd = dir;
                    menu.AddMenuOption($"{cd} {cv}", (p, o) =>
                    {
                        var pos = entity.AbsOrigin!;
                        entity.Teleport(cd switch
                        {
                            "X+" => new Vector(pos.X + cv, pos.Y, pos.Z),
                            "X-" => new Vector(pos.X - cv, pos.Y, pos.Z),
                            "Y+" => new Vector(pos.X, pos.Y + cv, pos.Z),
                            "Y-" => new Vector(pos.X, pos.Y - cv, pos.Z),
                            "Z+" => new Vector(pos.X, pos.Y, pos.Z + cv),
                            "Z-" => new Vector(pos.X, pos.Y, pos.Z - cv),
                            _ => new Vector(pos.X, pos.Y, pos.Z)
                        }, entity.AbsRotation);
                    });
                }
            }
        }
        else if (type == 1)
        {
            foreach (var v in _plugin.Config.customAngleValues)
            {
                foreach (var dir in _directions)
                {
                    var cv = v; var cd = dir;
                    menu.AddMenuOption($"{cd} {cv}", (p, o) =>
                    {
                        var ang = entity.AbsRotation!;
                        entity.Teleport(entity.AbsOrigin, cd switch
                        {
                            "X+" => new QAngle(ang.X + cv, ang.Y, ang.Z),
                            "X-" => new QAngle(ang.X - cv, ang.Y, ang.Z),
                            "Y+" => new QAngle(ang.X, ang.Y + cv, ang.Z),
                            "Y-" => new QAngle(ang.X, ang.Y - cv, ang.Z),
                            "Z+" => new QAngle(ang.X, ang.Y, ang.Z + cv),
                            "Z-" => new QAngle(ang.X, ang.Y, ang.Z - cv),
                            _ => new QAngle(ang.X, ang.Y, ang.Z)
                        });
                    });
                }
            }
        }

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => onBack(p));
        menu.Open(player);
    }
}