using Reverie.Content.Tiles.Sylvanwalde;

namespace Reverie.Content.Items.Sylvanwalde;

public class CobblestoneItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.DefaultToPlaceableTile(ModContent.TileType<CobblestoneTile>());
        Item.maxStack = 999;
        Item.rare = ItemRarityID.Blue;
    }
}