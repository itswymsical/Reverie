namespace Reverie.Content.Items.Botany;

public class CactusPotItem : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 16;
        Item.height = 26;
        Item.accessory = true;

        Item.rare = ItemRarityID.White;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        base.UpdateAccessory(player, hideVisual);
        player.thorns += .22f;
    }
    public override void AddRecipes()
    {
       CreateRecipe()
            .AddIngredient(ItemID.Cactus, 20)
            .AddIngredient(ItemID.ClayPot, 1)
            .AddTile(TileID.WorkBenches)
            .Register();
    }
}
