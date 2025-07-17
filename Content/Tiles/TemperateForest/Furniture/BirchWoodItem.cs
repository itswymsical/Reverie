namespace Reverie.Content.Tiles.TemperateForest.Furniture;

public class BirchWoodItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = Item.sellPrice(0);
        Item.DefaultToPlaceableTile(ModContent.TileType<BirchWoodTile>());
    }
}
