using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace Reverie.Content.Sylvanwalde.Tiles
{
    public class WoodgrassSeeds : ModItem
    {
        public override string Texture => Assets.Sylvanwalde.Tiles.Path + Name;
        public override void SetStaticDefaults() => ItemID.Sets.DisableAutomaticPlaceableDrop[Type] = true; 
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Woodgrass>());
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.buyPrice(silver: 2);
        }
    }
}