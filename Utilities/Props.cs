using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MapAdvertisements.Utils;

public partial class PluginUtils
{
    public CDynamicProp? CreatePropModel(Vector cords, QAngle angle, string material, bool onGround, int materialIndex, int propId)
    {
        var entity = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        if (entity == null) return null;

        entity.Entity!.Name = $"advert_prop{propId}";
        entity.SetModel(material);
        entity.Teleport(new Vector(cords.X, cords.Y, cords.Z), angle);

        if (onGround)
            entity.AbsRotation!.X = -90;

        if (materialIndex != 0)
            entity.AcceptInput("Skin", entity, entity, materialIndex.ToString());

        entity.DispatchSpawn();
        return entity;
    }

    public CDynamicProp? CreatePropModelOnClick(Vector cords, QAngle angle, string material, bool onGround, int materialIndex)
    {
        var entity = Utilities.CreateEntityByName<CDynamicProp>("prop_dynamic");
        if (entity == null) return null;

        entity.Entity!.Name = "advert_prop";
        entity.SetModel(material);
        entity.Teleport(new Vector(cords.X, cords.Y, cords.Z), angle);

        if (onGround)
            entity.AbsRotation!.X = -90;

        if (materialIndex != 0)
            entity.AcceptInput("Skin", entity, entity, materialIndex.ToString());

        entity.DispatchSpawn();

        var model = _plugin.PropManager!.PushCordsToFile(cords, angle, material, 0, 0, 0, onGround, materialIndex, entity);
        if (model != null)
            model.EntityProp = entity;

        return entity;
    }
}