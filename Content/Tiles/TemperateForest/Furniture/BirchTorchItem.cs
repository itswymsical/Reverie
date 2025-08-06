namespace Reverie.Content.Tiles.TemperateForest.Furniture;

public class BirchTorchItem : ModItem
{
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;

        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.ShimmerTorch;
        ItemID.Sets.SingleUseInGamepad[Type] = true;
        ItemID.Sets.Torches[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToTorch(ModContent.TileType<BirchTorchTile>(), 1, true);
        Item.value = 50;
    }

    public override void HoldItem(Player player)
    {
        if (Main.rand.NextBool(player.itemAnimation > 0 ? 7 : 30))
        {
            var dust = Dust.NewDustDirect(new Vector2(player.itemLocation.X + (player.direction == -1 ? -16f : 6f), player.itemLocation.Y - 14f * player.gravDir), 4, 4, DustID.OrangeTorch, 0f, 0f, 100);
            if (!Main.rand.NextBool(3))
            {
                dust.noGravity = true;
            }

            dust.velocity *= 0.3f;
            dust.velocity.Y -= 1.5f;
            dust.position = player.RotatedRelativePoint(dust.position);
        }

        var position = player.RotatedRelativePoint(new Vector2(player.itemLocation.X + 12f * player.direction + player.velocity.X, player.itemLocation.Y - 14f + player.velocity.Y), true);

        Lighting.AddLight(position, 0.8f, 0.7f, 0.55f);
    }

    public override void PostUpdate()
    {
        Lighting.AddLight(Item.Center, 0.8f, 0.7f, 0.55f);
    }

    public override void AddRecipes()
    {
        CreateRecipe(3)
            .AddIngredient<BirchWoodItem>()
            .AddIngredient(ItemID.Gel)
            .Register();
    }
}