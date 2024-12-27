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
    [AutoloadEquip(EquipType.Head)]
    public class FrostbarkHelm : ModItem
    {
        public override string Texture => Assets.Terraria.Items.Frostbark + Name;
        public override void SetDefaults()
        {
            Item.defense = 3;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(copper: 35);
            Item.width = Item.height = 34;
        }

        public override void UpdateEquip(Player player)
        {
            base.UpdateEquip(player);
        }
        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<FrostbarkHauberk>() && legs.type == ModContent.ItemType<FrostbarkGreaves>();
        }

        // UpdateArmorSet allows you to give set bonuses to the armor.
        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Increases critical strike and melee speed by 6%";
            player.GetCritChance(DamageClass.Melee) += 6;
            player.GetAttackSpeed(DamageClass.Melee) += 0.06f;
        }
        //AddRecipes adds a crafting recipe to items
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IceBlock, 12);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 6);
            recipe.AddIngredient(ItemID.BorealWood, 8);
            recipe.AddTile(TileID.Anvils); //the tile that is necessary for crafting (can be anything)
            recipe.Register(); //set recipe
        }
    }
}