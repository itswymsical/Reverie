using Reverie.Common.Items.Components;
using Reverie.Core.Items.Components;

namespace Reverie.Content.Items.Shovels;

public class CopperShovelItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.DamageType = DamageClass.Melee;
        Item.damage = 2;

        Item.autoReuse = true;
        Item.useTurn = true;
        
        Item.width = 32;
        Item.height = 32;

        Item.tileBoost = -1;
        
        Item.UseSound = SoundID.Item18;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Swing;
        
        Item.value = Item.sellPrice(silver: 1);

        if (!Item.TryEnable(out ShovelItemComponent component))
        {
            return;
        }

        component.Power = 30;
    }
    
    public override void AddRecipes()
    {
        base.AddRecipes();
        
        CreateRecipe()
            .AddIngredient(ItemID.Wood, 4)
            .AddIngredient(ItemID.CopperBar, 9)
            .AddTile(TileID.Anvils)
            .Register();
    }
}