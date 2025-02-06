using Reverie.Content.Tiles.Sylvanwalde;

namespace Reverie.Content.Items.Sylvanwalde.Canopy;

public class LoamItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        
        Item.DefaultToPlaceableTile(ModContent.TileType<LoamTile>());
        
        Item.rare = ItemRarityID.Blue;
    }
}