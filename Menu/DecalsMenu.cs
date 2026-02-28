using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using MenuManager;

using MapAdvertisements.Models;

namespace MapAdvertisements.Menu;

public partial class PluginMenu
{
    public void CreateDecalMenu(CCSPlayerController player)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        if (!_selectedMaterial.TryGetValue(player, out var data))
        {
            data = new SelectedMaterialModel { depth = 14 };
            _selectedMaterial[player] = data;
        }

        var menu = api.GetMenu(_plugin.Localizer["CreateDecalMenu"]);
        menu.AddMenuOption(
            data.material ?? _plugin.Localizer["ChooseMaterial"],
            (p, o) => { }, disabled: true);

        menu.AddMenuOption(_plugin.Localizer["Material_Header"],
            (p, o) => DecalMaterialsMenu(p));

        menu.AddMenuOption(
            data.width != 0
                ? string.Format(_plugin.Localizer["Decal_Width"], data.width)
                : _plugin.Localizer["SelectFirst_Width"],
            (p, o) => DecalSizeMenu(p, "Width"));

        menu.AddMenuOption(
            data.height != 0
                ? string.Format(_plugin.Localizer["Decal_Height"], data.height)
                : _plugin.Localizer["SelectFirst_Height"],
            (p, o) => DecalSizeMenu(p, "Height"));

        menu.AddMenuOption(string.Format(_plugin.Localizer["Decal_Depth"], data.depth),
            (p, o) => DecalDepthMenu(p, data));

        menu.AddMenuOption(_plugin.Localizer["SpawnOnPing", data.onPing], (p, o) =>
        {
            data.onPing = !data.onPing;
            Server.NextFrame(() => CreateDecalMenu(p));
        }, disabled: data.material == null);

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => ShowMapAdvertMenu(p));
        menu.Open(player);
    }

    private void DecalMaterialsMenu(CCSPlayerController player)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var menu = api.GetMenu(_plugin.Localizer["Material_Header"]);
        foreach (var material in _plugin.Config.Props)
        {
            if (_plugin.PluginUtils!.CheckMaterial(material)) continue;
            var mat = material;
            menu.AddMenuOption(mat, (p, o) =>
            {
                if (!_selectedMaterial.ContainsKey(p))
                    _selectedMaterial[p] = new SelectedMaterialModel { material = mat, depth = 14 };
                else
                    _selectedMaterial[p].material = mat;

                Server.NextFrame(() => CreateDecalMenu(p));
            });
        }

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => CreateDecalMenu(p));
        menu.Open(player);
    }

    private void DecalSizeMenu(CCSPlayerController player, string type)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var menu = api.GetMenu($"Set {type}");
        foreach (var size in _decalSize)
        {
            var s = size;
            menu.AddMenuOption($"{s}", (p, o) =>
            {
                var d = _selectedMaterial[p];
                if (type == "Height") d.height = s;
                else d.width = s;
                Server.NextFrame(() => CreateDecalMenu(p));
            });
        }

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => CreateDecalMenu(p));
        menu.Open(player);
    }

    private void DecalDepthMenu(CCSPlayerController player, SelectedMaterialModel data)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var menu = api.GetMenu(_plugin.Localizer["DecalDepth_Header", data.depth]);
        menu.AddMenuOption(_plugin.Localizer["DecalDepth_ItemPlus"], (p, o) =>
        {
            data.depth++;
            Server.NextFrame(() => DecalDepthMenu(p, data));
        });

        menu.AddMenuOption(_plugin.Localizer["DecalDepth_ItemMinus"], (p, o) =>
        {
            data.depth--;
            Server.NextFrame(() => DecalDepthMenu(p, data));
        });

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => CreateDecalMenu(p));
        menu.Open(player);
    }

    private void EditDecalMenu(CCSPlayerController player)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var menu = api.GetMenu(_plugin.Localizer["ListOfDecals_Header"]);
        foreach (var prop in _plugin.PropManager!._props)
        {
            if (_plugin.PluginUtils!.CheckMaterial(prop.modelPath!)) continue;
            var p2 = prop;
            menu.AddMenuOption($"#{p2.Id} — {p2.modelPath}", (p, o) =>
                EditSpecificDecal(p, p2));
        }

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => ShowMapAdvertMenu(p));
        menu.Open(player);
    }

    private void EditSpecificDecal(CCSPlayerController player, PropModel prop)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var pawn = player.PlayerPawn.Value;
        if (pawn == null || !pawn.IsValid) return;

        var entity = prop.EntityProp;
        if (entity == null) return;

        var menu = api.GetMenu($"{_plugin.Localizer["EditDecal_Header"]} #{prop.Id}");
        menu.AddMenuOption(_plugin.Localizer["TeleportToAdv"], (p, o) =>
            pawn.Teleport(new Vector(prop.posX, prop.posY, prop.posZ)));

        menu.AddMenuOption(_plugin.Localizer["ChooseMaterial"], (p, o) =>
            DecalMaterialEdit(p, prop));

        menu.AddMenuOption(_plugin.Localizer["Decal_Width", prop.width], (p, o) =>
            EditDecalSizeMenu(p, prop, 0));

        menu.AddMenuOption(_plugin.Localizer["Decal_Height", prop.height], (p, o) =>
            EditDecalSizeMenu(p, prop, 1));

        menu.AddMenuOption(_plugin.Localizer["Decal_Depth", prop.depth], (p, o) =>
            EditDecalDepthMenu(p, prop));

        menu.AddMenuOption(_plugin.Localizer["ChangePositionAdvert"], (p, o) =>
            CordsMenu(p, prop, prop.Id, 0, pl => EditSpecificDecal(pl, prop)));

        menu.AddMenuOption(_plugin.Localizer["ChangeAnglesAdvert"], (p, o) =>
            CordsMenu(p, prop, prop.Id, 1, pl => EditSpecificDecal(pl, prop)));

        menu.AddMenuOption(_plugin.Localizer["SavePropConfig"], (p, o) =>
        {
            _plugin.PropManager!.SavePropConfiguration(entity, prop);
            p.PrintToChat($"{_plugin.Localizer["Prefix"]}{_plugin.Localizer["SavedProp", prop.Id]}");
            Server.NextFrame(() => EditDecalMenu(p));
        });

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => EditDecalMenu(p));
        menu.Open(player);
    }

    private void EditDecalSizeMenu(CCSPlayerController player, PropModel prop, int type)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var entity = prop.EntityProp;
        if (entity == null) return;

        var menu = api.GetMenu($"{_plugin.Localizer[$"ChangeHeader_{type}"]} #{prop.Id}");
        foreach (var size in _decalSize)
        {
            var s = size;
            menu.AddMenuOption($"{s}", (p, o) =>
            {
                var old = entity.As<CEnvDecal>();
                var pos = old.AbsOrigin;
                var ang = old.AbsRotation;
                var width  = type == 0 ? s : old.Width;
                var height = type == 1 ? s : old.Height;
                old.Remove();

                if (type == 0) prop.width  = s;
                else           prop.height = s;

                prop.EntityProp = _plugin.PluginUtils!.CreateDecal(pos!, ang!, prop.modelPath!, width, height, prop.depth);
                Server.NextFrame(() => EditSpecificDecal(p, prop));
            });
        }

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => EditSpecificDecal(p, prop));
        menu.Open(player);
    }

    private void EditDecalDepthMenu(CCSPlayerController player, PropModel prop)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var menu = api.GetMenu(_plugin.Localizer["DecalDepth_Header", prop.depth]);
        menu.AddMenuOption(_plugin.Localizer["DecalDepth_ItemPlus"], (p, o) =>
        {
            prop.depth++;
            RecreateDecal(prop);
            Server.NextFrame(() => EditDecalDepthMenu(p, prop));
        });

        menu.AddMenuOption(_plugin.Localizer["DecalDepth_ItemMinus"], (p, o) =>
        {
            prop.depth--;
            RecreateDecal(prop);
            Server.NextFrame(() => EditDecalDepthMenu(p, prop));
        });

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => EditSpecificDecal(p, prop));
        menu.Open(player);
    }

    private void DecalMaterialEdit(CCSPlayerController player, PropModel prop)
    {
        var api = _plugin.MenuApi;
        if (api == null) return;

        var entity = prop.EntityProp;
        if (entity == null) return;

        var menu = api.GetMenu(_plugin.Localizer["Material_Header"]);
        foreach (var material in _plugin.Config.Props)
        {
            if (_plugin.PluginUtils!.CheckMaterial(material)) continue;
            var mat = material;
            menu.AddMenuOption(mat, (p, o) =>
            {
                var old = entity.As<CEnvDecal>();
                var pos = old.AbsOrigin;
                var ang = old.AbsRotation;
                var w = old.Width; var h = old.Height;
                old.Remove();

                prop.modelPath = mat;
                prop.EntityProp = _plugin.PluginUtils!.CreateDecal(pos!, ang!, mat, w, h, prop.depth);
                Server.NextFrame(() => EditSpecificDecal(p, prop));
            });
        }

        menu.AddMenuOption($"← {_plugin.Localizer["Menu_Back"]}", (p, o) => EditSpecificDecal(p, prop));
        menu.Open(player);
    }

    private void RecreateDecal(PropModel prop)
    {
        var entity = prop.EntityProp;
        if (entity == null) return;

        var old = entity.As<CEnvDecal>();
        var pos = old.AbsOrigin;
        var ang = old.AbsRotation;
        var w = old.Width; var h = old.Height;
        old.Remove();

        prop.EntityProp = _plugin.PluginUtils!.CreateDecal(pos!, ang!, prop.modelPath!, w, h, prop.depth);
    }
}