namespace Reverie.Common.Items.Types;

public sealed class AcornGlobalItem : GlobalItem
{
    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type == ItemID.Acorn;
    }

    public override void SetDefaults(Item entity)
    {
        base.SetDefaults(entity);
        
        entity.DamageType = DamageClass.Ranged;
        entity.damage = 6;
        entity.knockBack = 1f;
        entity.noMelee = true;
        entity.shootSpeed = 7f;
        
        entity.ammo = entity.type;
    }
}
