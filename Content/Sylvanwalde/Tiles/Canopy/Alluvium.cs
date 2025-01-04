using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Content.Sylvanwalde.Tiles.Canopy
{
    public class Alluvium : ModItem
    {
        public override string Texture => $"{Assets.Sylvanwalde.Tiles.Canopy}AlluviumOre";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<AlluviumOreTile>());
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 2);
        }
    }
}
