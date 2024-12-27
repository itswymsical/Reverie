using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Items
{
    public class Apple : ModItem
    {
        public override string Texture => Assets.Terraria.Items.Food + Name;
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 22;
            Item.consumable = true;
            Item.healLife = 25;
            Item.useTime = Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.EatFood;
            Item.value = Item.buyPrice(copper: 80);
            Item.maxStack = 999;
            Item.UseSound = SoundID.Item2;
            Item.potion = true;
        }
    }
}
