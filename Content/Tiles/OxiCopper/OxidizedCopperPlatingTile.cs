using Reverie.Common.Systems;

namespace Reverie.Content.Tiles.OxiCopper;
public class OxidizedCopperPlatingItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<OxidizedCopperPlatingTile>());
        Item.width = 8;
        Item.height = 10;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
        .AddIngredient(ItemID.CopperPlating, 2)
        .AddCondition(Condition.NearWater)
        .Register();
    }
}

public class OxidizedCopperPlatingTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        this.Merge(Type, TileID.CopperBrick, TileID.CopperPlating);
        Main.tileBlockLight[Type] = true;
        DustType = DustID.Copper;
        MineResist = 2f;
        HitSound = SoundID.NPCHit42;
        AddMapEntry(new Color(183, 94, 35));
    }
}

