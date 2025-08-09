namespace Reverie.Content.Items.Accessories;

[AutoloadEquip(EquipType.Neck)]
public class EmeraldPendant : ModItem
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
        var reqMana = 60;
        var currentMana = (float)player.statMana / player.statManaMax2;

        if (player.statManaMax2 >= reqMana)
        {
            var defenseBonus = (int)((0.02f + (currentMana * 0.18f)) * 50);
            player.statDefense += defenseBonus;
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Emerald, 2)
            .AddIngredient(ItemID.TungstenBar, 5)
            .AddIngredient(ItemID.Chain, 2)
            .AddTile(TileID.Anvils)
            .Register();
    }
}