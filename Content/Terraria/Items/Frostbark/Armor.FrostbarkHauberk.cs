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
    [AutoloadEquip(EquipType.Body)]
    public class FrostbarkHauberk : ModItem
    {
        public override string Texture => Assets.Terraria.Items.Frostbark + Name;
        public override void SetDefaults()
        {
            Item.defense = 3;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(copper: 45);
            Item.width = Item.height = 34;
        }
        public override void UpdateEquip(Player player)
        {
            base.UpdateEquip(player);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IceBlock, 8);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 5);
            recipe.AddIngredient(ItemID.BorealWood, 16);
            recipe.AddTile(TileID.Anvils); //the tile that is necessary for crafting (can be anything)
            recipe.Register(); //set recipe
        }
    }
}