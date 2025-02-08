using Reverie.Content.Projectiles.Sylvanwalde.WoodenWarden;

namespace Reverie.Content.Items.Sylvanwalde.WoodenWarden;

public class Bramblebow : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 11;
        Item.width = 32;
        Item.height = 56;
        Item.useTime = Item.useAnimation = 27;
        Item.knockBack = 1.2f;
        Item.crit = 3;
        Item.value = Item.sellPrice(silver: 14);
        Item.rare = ItemRarityID.Blue;

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = SoundID.Item5;
        Item.shootSpeed = 9.75f;
        Item.useAmmo = AmmoID.Arrow;
        Item.DamageType = DamageClass.Ranged;

        Item.shoot = ModContent.ProjectileType<BrambleArrowProj>();
    }
    public override Vector2? HoldoutOffset() => new Vector2(-8, 0);

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        if (type == ProjectileID.WoodenArrowFriendly) type = ModContent.ProjectileType<BrambleArrowProj>();
        damage = damage / 3;
    }
}
