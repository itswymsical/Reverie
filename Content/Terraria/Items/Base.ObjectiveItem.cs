using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Terraria.Items
{
    public abstract class ObjectiveItem : ModItem
    {
        public override string Texture => Assets.PlaceholderTexture;
        public override void SetDefaults()
        {
            Item.rare = ItemRarityID.Quest;
            Item.value = Item.sellPrice(0);
            Item.width = Item.height = 28;
            Item.maxStack = 999;
        }
    }
}
