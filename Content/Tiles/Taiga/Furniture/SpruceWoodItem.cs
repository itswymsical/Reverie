namespace Reverie.Content.Tiles.Taiga.Furniture;

public class SpruceWoodItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.value = Item.sellPrice(0);
        Item.DefaultToPlaceableTile(ModContent.TileType<SpruceWoodTile>());
    }
}
