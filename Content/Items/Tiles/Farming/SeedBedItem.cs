using Reverie.Content.Tiles.Farming;

namespace Reverie.Content.Items.Tiles.Farming;

public class SeedBedItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<SeedBedTile>());
        Item.value = Item.sellPrice(copper: 78);
    }
}
