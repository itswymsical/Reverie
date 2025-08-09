namespace Reverie.Content.Items.Accessories;

[AutoloadEquip(EquipType.Neck)]
public class DiamondPendant : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.accessory = true;
        Item.width = Item.height = 30;
        Item.rare = ItemRarityID.Green;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetCritChance(DamageClass.Generic) += 10;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Diamond, 2)
            .AddIngredient(ItemID.PlatinumBar, 5)
            .AddIngredient(ItemID.Chain, 2)
            .AddTile(TileID.Anvils)
            .Register();
    }
}