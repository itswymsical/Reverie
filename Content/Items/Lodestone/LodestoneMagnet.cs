﻿using Reverie.Content.Projectiles.Lodestone;
using Terraria.DataStructures;

namespace Reverie.Content.Items.Lodestone;

public class LodestoneMagnet : ModItem
{
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 20;
        Item.width = Item.height = 32;

        Item.autoReuse = Item.useTurn = true;

        Item.value = Item.sellPrice(gold: 1);

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = SoundID.Item18;
        Item.rare = ItemRarityID.Green;
        Item.autoReuse = false;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.channel = true;
        Item.shootSpeed = 7f;
        Item.shoot = ModContent.ProjectileType<LodestoneMagnetProj>();
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        var muzzleOffset = Vector2.Normalize(new Vector2(velocity.X, velocity.Y)) * 25f;
        if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
        {
            position += muzzleOffset;
        }
        Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<LodestoneMagnetProj>(), damage, knockback, player.whoAmI);
        return false;
    }
    public override bool CanUseItem(Player player)
        => player.ownedProjectileCounts[ModContent.ProjectileType<LodestoneMagnetProj>()] <= 0;

    public override void AddRecipes()
    {
        var recipe = CreateRecipe();
        recipe.AddIngredient<Lodestone>(25);
        recipe.AddIngredient<MagnetizedCoil>(15);
        recipe.AddRecipeGroup(nameof(ItemID.SilverBar), 10);
        recipe.AddTile(TileID.Anvils);
        recipe.Register();
    }
}