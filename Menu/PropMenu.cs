using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using MenuManager;

using MapAdvertisements.Models;

namespace MapAdvertisements.Menu;

public partial class PluginMenu
{
    public void CreatePropMenu(CCSPlayerController player)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        if (!_selectedMaterial.TryGetValue(player, out var data))
        {
            data = new SelectedMaterialModel();
            _selectedMaterial[player] = data;
        }

        var menu = api.GetMenu(_plugin.Localizer["Prop_Header"]);
        menu.AddMenuOption(
            data.material ?? _plugin.Localizer["ChooseMaterial"],
            (p, o) => { }, disabled: true);

        menu.AddMenuOption(_plugin.Localizer["Material_Header"],
            (p, o) => PropMaterialsMenu(p));

        menu.AddMenuOption(_plugin.Localizer["SpawnOnPing", data.onPing], (p, o) =>
        {
            data.onPing = !data.onPing;
            if (data.onPing)
                p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["PingModeEnabled", data.material!]}");
            Server.NextFrame(() => CreatePropMenu(p));
        }, disabled: data.material == null);

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => ShowMapAdvertMenu(p));
        menu.Open(player);
    }

    private void PropMaterialsMenu(CCSPlayerController player)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var menu = api.GetMenu(_plugin.Localizer["Material_Header"]);
        foreach (var material in _plugin.Config.Props)
        {
            if (!_plugin.PluginUtils!.CheckMaterial(material)) continue;
            var mat = material;
            menu.AddMenuOption(mat, (p, o) =>
            {
                if (!_selectedMaterial.ContainsKey(p))
                    _selectedMaterial[p] = new SelectedMaterialModel { material = mat };
                else
                    _selectedMaterial[p].material = mat;

                Server.NextFrame(() => CreatePropMenu(p));
            });
        }

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => CreatePropMenu(p));
        menu.Open(player);
    }

    private void EditPropsMenu(CCSPlayerController player)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var menu = api.GetMenu(_plugin.Localizer["ListOfProps_Header"]);
        foreach (var prop in _plugin.PropManager!._props)
        {
            if (!_plugin.PluginUtils!.CheckMaterial(prop.modelPath!)) continue;
            var p2 = prop;
            menu.AddMenuOption($"#{p2.Id} — {p2.modelPath}", (p, o) =>
                EditSpecificProp(p, p2));
        }

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => ShowMapAdvertMenu(p));
        menu.Open(player);
    }

    private void EditSpecificProp(CCSPlayerController player, PropModel prop)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return;

        var entity = prop.EntityProp;
        if (entity == null) return;

        var menu = api.GetMenu(_plugin.Localizer["EditProp_Header", prop.Id]);
        menu.AddMenuOption(_plugin.Localizer["TeleportToAdv"], (p, o) =>
            pawn.Teleport(new Vector(prop.posX, prop.posY, prop.posZ)));

        menu.AddMenuOption(_plugin.Localizer["ChooseMaterial"], (p, o) =>
            PropMaterialEdit(p, prop));

        menu.AddMenuOption($"{_plugin.Localizer["SelectPropSkin"]} {prop.ModelGroupIndex}", (p, o) =>
        {
            _listenForChat[p] = prop;
            p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["NotificationSkin"]}");
        });

        menu.AddMenuOption(_plugin.Localizer["ChangePositionAdvert"], (p, o) =>
            CordsMenu(p, prop, prop.Id, 0, pl => EditSpecificProp(pl, prop)));

        menu.AddMenuOption(_plugin.Localizer["ChangeAnglesAdvert"], (p, o) =>
            CordsMenu(p, prop, prop.Id, 1, pl => EditSpecificProp(pl, prop)));

        menu.AddMenuOption(_plugin.Localizer["SavePropConfig"], (p, o) =>
        {
            _plugin.PropManager!.SavePropConfiguration(entity.As<CPhysicsPropOverride>(), prop);
            p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["SavedProp", prop.Id]}");
            Server.NextFrame(() => EditPropsMenu(p));
        });

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => EditPropsMenu(p));
        menu.Open(player);
    }

    private void PropMaterialEdit(CCSPlayerController player, PropModel prop)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var entity = prop.EntityProp;
        if (entity == null) return;

        var menu = api.GetMenu(_plugin.Localizer["Material_Header"]);
        foreach (var material in _plugin.Config.Props)
        {
            if (!_plugin.PluginUtils!.CheckMaterial(material)) continue;
            var mat = material;
            menu.AddMenuOption(mat, (p, o) =>
            {
                var old = entity.As<CPhysicsPropOverride>();
                var pos = old.AbsOrigin;
                var ang = old.AbsRotation;
                old.Remove();

                prop.modelPath = mat;
                prop.EntityProp = _plugin.PluginUtils!.CreatePropModel(pos!, ang!, mat, prop.isOnGround, prop.ModelGroupIndex, prop.Id);
                Server.NextFrame(() => EditSpecificProp(p, prop));
            });
        }

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => EditSpecificProp(p, prop));
        menu.Open(player);
    }
}