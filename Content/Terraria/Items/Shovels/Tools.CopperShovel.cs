using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace Reverie.Content.Terraria.Items.Shovels
{
    public class CopperShovel : ShovelItem
    {
        public override string Texture => Assets.Terraria.Items.Shovels + Name;
        public override void SetDefaults()
        {
            DiggingPower(30);
            Item.DamageType = DamageClass.Melee;
            Item.damage = 2;
            Item.useTime = Item.useAnimation = 20;
            Item.width = Item.height = 32;

            Item.autoReuse = Item.useTurn = true;

            Item.value = Item.sellPrice(silver: 1);

            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item18;
            Item.tileBoost -= 1;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Wood, 4);
            recipe.AddIngredient(ItemID.CopperBar, 9);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}