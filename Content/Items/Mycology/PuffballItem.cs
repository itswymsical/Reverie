namespace Reverie.Content.Items.Mycology;

public class PuffballItem : ModItem
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.width = Item.height = 36;
        Item.rare = ItemRarityID.Quest;
        Item.maxStack = Item.CommonMaxStack;
    }
}
