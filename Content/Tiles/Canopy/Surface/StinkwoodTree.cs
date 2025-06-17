using ReLogic.Content;
using Terraria.Enums;
using Terraria.GameContent;

namespace Reverie.Content.Tiles.Canopy.Surface;

public class StinkwoodTree : ModPalmTree
{
    private Asset<Texture2D> texture;
    private Asset<Texture2D> oasisTopsTexture;
    private Asset<Texture2D> topsTexture;

    public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings
    {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };

    public override void SetStaticDefaults()
    {
        GrowsOnTileId = [ModContent.TileType<WoodgrassTile>(), ModContent.TileType<CanopyGrassTile>()];

        texture = ModContent.Request<Texture2D>($"{NAME}/Assets/Textures/Tiles/Canopy/Surface/StinkwoodTree");
        oasisTopsTexture = ModContent.Request<Texture2D>($"{NAME}/Assets/Textures/Tiles/Canopy/Surface/StinkwoodTree_Tops");
        topsTexture = ModContent.Request<Texture2D>($"{NAME}/Assets/Textures/Tiles/Canopy/Surface/StinkwoodTree_Tops");
    }

    public override Asset<Texture2D> GetTexture() => texture;

    public override int SaplingGrowthType(ref int style)
    {
        style = 0; // Make sure this matches your sapling's style
        return ModContent.TileType<StinkwoodSapling>();
    }

    public override Asset<Texture2D> GetOasisTopTextures() => oasisTopsTexture;

    public override Asset<Texture2D> GetTopTextures() => topsTexture;

    public override int DropWood()
    {
        return ItemID.Wood;
    }

    public override TreeTypes CountsAsTreeType => TreeTypes.Palm;

    public override int CreateDust()
    {
        return DustID.RichMahogany;
    }

    public override int TreeLeaf()
    {
        return GoreID.TreeLeaf_Palm;
    }

    public override bool Shake(int x, int y, ref bool createLeaves)
    {
        if (WorldGen.genRand.NextBool(10))
        {
            Item.NewItem(null, new Rectangle(x * 16, y * 16, 16, 16), ItemID.Acorn);
        }

        createLeaves = true;
        return true;
    }
}