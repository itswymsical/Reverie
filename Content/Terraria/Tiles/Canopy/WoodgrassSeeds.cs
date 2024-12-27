using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Reverie.Content.Tiles.Canopy;

namespace Reverie.Content.Terraria.Tiles.Canopy
{
    public class WoodgrassSeeds : ModItem
    {
        public override string Texture => Assets.Terraria.Tiles.Canopy + Name;
        public override void SetStaticDefaults() => ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true; 
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Woodgrass>());
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 2);
        }
    }
}