using Reverie.Common.Items.Components;
using Reverie.Core.Items.Components;

namespace Reverie.Content.Items.Shovels;

public class PlatinumShovelItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.DamageType = DamageClass.Melee;
        Item.damage = 6;
        Item.knockBack = 5f;

        Item.autoReuse = true;
        Item.useTurn = true;
        
        Item.width = 32;
        Item.height = 32;
        
        Item.UseSound = SoundID.Item18;
        Item.useTime = 16;
        Item.useAnimation = 16;
        Item.useStyle = ItemUseStyleID.Swing;
        
        Item.value = Item.sellPrice(silver: 30);

        if (!Item.TryEnable(out ShovelItemComponent component))
        {
            return;
        }

        component.Power = 59;
    }
    
    public override void AddRecipes()
    {
        base.AddRecipes();
        
        CreateRecipe()
            .AddIngredient(ItemID.Wood, 4)
            .AddIngredient(ItemID.PlatinumBar, 9)
            .AddTile(TileID.Anvils)
            .Register();
    }
}