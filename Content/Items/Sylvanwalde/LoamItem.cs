namespace Reverie.Content.Items.Sylvanwalde;

public class LoamItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        //Item.DefaultToPlaceableTile(ModContent.TileType<LoamTile>());

        Item.rare = ItemRarityID.Blue;
    }
}