using ReLogic.Content;
using Reverie.Content.Dusts;
using Reverie.Content.Items.Tiles.TemperateForest;
using Terraria.Enums;
using Terraria.GameContent;

namespace Reverie.Content.Tiles.TemperateForest;
public class BirchTree : ModTree
{
    public override TreePaintingSettings TreeShaderSettings => new()
    {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };
    public string texture = ($"{NAME}/Assets/Textures/Tiles/TemperateForest/BirchTree");
    public override void SetStaticDefaults()
    {
        GrowsOnTileId = [ModContent.TileType<TemperateGrassTile>()];
    }
    public override int SaplingGrowthType(ref int style)
    {
        style = 0;
        return ModContent.TileType<BirchSapling>();
    }

    public override bool Shake(int x, int y, ref bool createLeaves)
    {
        if (Main.rand.NextBool(7))
            Item.NewItem(WorldGen.GetItemSource_FromTreeShake(x, y), new Vector2(x, y) * 16, ItemID.Peach);

        return false;
    }

    public override int TreeLeaf() => ModContent.GoreType<BirchLeafFX>();  

    public override int DropWood() => ModContent.ItemType<BirchWoodItem>();
    public override int CreateDust() => ModContent.DustType<BirchDust>();
    
    public override TreeTypes CountsAsTreeType => TreeTypes.Custom;

    public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>(texture);
    public override Asset<Texture2D> GetBranchTextures() => ModContent.Request<Texture2D>($"{texture}_Branches");
    public override Asset<Texture2D> GetTopTextures() => ModContent.Request<Texture2D>($"{texture}_Tops");

    public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
    {
        topTextureFrameWidth = 112;
        topTextureFrameHeight = 158;
    }
}