using Reverie.Common.Items.Components;
using Reverie.Core.Items.Components;

namespace Reverie.Content.Items.Shovels;

public class IronShovelItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.DamageType = DamageClass.Melee;
        Item.damage = 4;
        Item.knockBack = 5f;

        Item.autoReuse = true;
        Item.useTurn = true;
        
        Item.width = 32;
        Item.height = 32;
        
        Item.UseSound = SoundID.Item18;
        Item.useTime = 19;
        Item.useAnimation = 19;
        Item.useStyle = ItemUseStyleID.Swing;
        
        Item.value = Item.sellPrice(silver: 4);

        if (!Item.TryEnable(out ShovelItemComponent component))
        {
            return;
        }

        component.Power = 38;
    }
    
    public override void AddRecipes()
    {
        base.AddRecipes();
        
        CreateRecipe()
            .AddIngredient(ItemID.Wood, 4)
            .AddIngredient(ItemID.IronBar, 9)
            .AddTile(TileID.Anvils)
            .Register();
    }
}