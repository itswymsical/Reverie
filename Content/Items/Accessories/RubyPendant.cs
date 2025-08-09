namespace Reverie.Content.Items.Accessories;

public class RubyPendant : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.accessory = true;
        Item.width = Item.height = 30;
        Item.rare = ItemRarityID.Blue;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        var reqMana = 80;
        var currentMana = (float)player.statMana / player.statManaMax2;
        var manaReq = Math.Min(1f, (float)player.statManaMax2 / reqMana);

        if (player.statManaMax2 >= reqMana)
        {
            var lifeRegen = 0.03f + (currentMana * 0.082f);
            player.lifeRegen += (int)lifeRegen;
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Ruby, 2)
            .AddIngredient(ItemID.GoldBar, 5)
            .AddIngredient(ItemID.Chain, 2)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
