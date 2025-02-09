using Reverie.Common.Items.Components;
using Reverie.Core.Items.Components;

namespace Reverie.Content.Items.Shovels;

public class GoldShovelItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.DamageType = DamageClass.Melee;
        Item.damage = 5;
        Item.knockBack = 5f;

        Item.autoReuse = true;
        Item.useTurn = true;
        
        Item.width = 32;
        Item.height = 32;
        
        Item.UseSound = SoundID.Item18;
        Item.useTime = 17;
        Item.useAnimation = 17;
        Item.useStyle = ItemUseStyleID.Swing;
        
        Item.value = Item.sellPrice(silver: 20);

        if (!Item.TryEnable(out ShovelItemComponent component))
        {
            return;
        }

        component.Power = 55;
    }
    
    public override void AddRecipes()
    {
        base.AddRecipes();
        
        CreateRecipe()
            .AddIngredient(ItemID.Wood, 4)
            .AddIngredient(ItemID.GoldBar, 9)
            .AddTile(TileID.Anvils)
            .Register();
    }
}