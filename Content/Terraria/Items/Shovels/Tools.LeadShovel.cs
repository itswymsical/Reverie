using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Items.Shovels
{
    public class LeadShovel : ShovelItem
    {
        public override string Texture => Assets.Terraria.Items.Shovels + Name;
        public override void SetDefaults()
        {
            DiggingPower(40);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 5;
            Item.useTime = Item.useAnimation = 18;
            Item.width = Item.height = 32;
            Item.knockBack = 5;

            Item.autoReuse = Item.useTurn = true;

            Item.value = Item.sellPrice(silver: 6);

            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item18;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Wood, 4);
            recipe.AddIngredient(ItemID.LeadBar, 9);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}