using Reverie.Common.Global;
using Reverie.Content.Terraria.Items.Lodestone;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Achievements;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Items.Mission
{
    public abstract class MissionItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            Item.favorited = true;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            base.ModifyTooltips(tooltips);

            foreach (TooltipLine line in tooltips.Where(line => line.Name == "Quest"))
            {
                line.Text = "Mission Item";
                line.OverrideColor = new(95, 205, 228);
            }
        }
        public override void UpdateInventory(Player player)
        {
            base.UpdateInventory(player);
            Item.favorited = true;
        }
    }

    public class MagnetizedCoil : MissionItem
    {
        public override string Texture => Assets.Terraria.Items.Lodestone + Name;
        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(silver: 3);
            Item.width = Item.height = 28;
            Item.maxStack = 999;
        }

        public override void AddRecipes()
        {

            Recipe recipe = CreateRecipe(2);
            recipe.AddIngredient(ItemID.Chain, 2);
            recipe.AddRecipeGroup(nameof(ItemID.CopperBar), 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

    public class CoilArray : MissionItem
    {
        public override string Texture => Assets.Terraria.Items.Lodestone + "MagnetizedCoil";
        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Quest;
            Item.value = Item.sellPrice(silver: 10);
            Item.width = Item.height = 28;
            Item.maxStack = 999;
        }

        public override void AddRecipes()
        {

            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<MagnetizedCoil>(), 4);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

    public class DimensionalTuningFork : MissionItem
    {
        public override string Texture => Assets.PlaceholderTexture;
        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Quest;
            Item.width = Item.height = 28;
            Item.maxStack = 999;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 3);
            recipe.AddIngredient(ModContent.ItemType<Lodestone.Lodestone>(), 5);
            recipe.AddIngredient(ItemID.Sapphire, 4);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }

    public class RealmCrystal : MissionItem
    {
        public override string Texture => Assets.Terraria.Items.Mission + Name;
        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(silver: 10);
            Item.width = Item.height = 28;
            Item.maxStack = 999;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Amethyst, 8);
            recipe.AddIngredient(ModContent.ItemType<Lodestone.Lodestone>(), 2);
            recipe.AddIngredient(ItemID.SandBlock, 4);
            recipe.AddTile(TileID.Furnaces);
            recipe.Register();
        }
    }
}