namespace Reverie.Content.Items.Sylvanwalde;

public class CobblestoneItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        //Item.DefaultToPlaceableTile(ModContent.TileType<CobblestoneTile>());

        Item.rare = ItemRarityID.Blue;
    }
}