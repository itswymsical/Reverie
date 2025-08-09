namespace Reverie.Content.Items.Accessories;

[AutoloadEquip(EquipType.Neck)]
public class AmethystPendant : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.accessory = true;
        Item.width = Item.height = 30;
        Item.rare = ItemRarityID.White;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        var reqMana = 60;
        var currentMana = (float)player.statMana / player.statManaMax2;
        if (player.statManaMax2 >= reqMana)
        {
            var damageReduction = 0.02f + (currentMana * 0.08f);
            player.endurance += damageReduction;
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Amethyst, 2)
            .AddIngredient(ItemID.CopperBar, 5)
            .AddIngredient(ItemID.Chain, 2)
            .AddTile(TileID.Anvils)
            .Register();
    }
}