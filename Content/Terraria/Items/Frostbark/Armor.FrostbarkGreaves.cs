using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Items.Frostbark
{
    [AutoloadEquip(EquipType.Legs)]
    public class FrostbarkGreaves : ModItem
    {
        public override string Texture => Assets.Terraria.Items.Frostbark + Name;
        public override void SetDefaults()
        {
            Item.defense = 1;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(copper: 30);
            Item.width = Item.height = 34;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 4);
            recipe.AddIngredient(ItemID.BorealWood, 16);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}