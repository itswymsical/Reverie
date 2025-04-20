using Reverie.Content.Tiles.Canopy;

namespace Reverie.Content.Items.Tiles.Canopy;

public class AlluviumOreItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AlluviumOreTile>());
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.buyPrice(silver: 2);
    }
}
