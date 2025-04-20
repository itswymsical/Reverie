using ReLogic.Content;
using Terraria.Enums;
using Terraria.GameContent;

namespace Reverie.Content.Tiles.Canopy;

public class CanopyTree : ModTree
{
    public override TreePaintingSettings TreeShaderSettings => new TreePaintingSettings
    {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };
    public override void SetStaticDefaults() => GrowsOnTileId = [ModContent.TileType<WoodgrassTile>()];

    public override Asset<Texture2D> GetTexture() => ModContent.Request<Texture2D>($"Reverie/Assets/Textures/Tiles/Canopy/CanopyTree");
    public override TreeTypes CountsAsTreeType => TreeTypes.Forest;
    public override int SaplingGrowthType(ref int style)
    {
        style = 0;
        return ModContent.TileType<TeakWoodSaplingTile>();
    }
    public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
    {
        topTextureFrameWidth = 124;
        topTextureFrameHeight = 98;

    }
    public override Asset<Texture2D> GetBranchTextures() => ModContent.Request<Texture2D>($"Reverie/Assets/Textures/Tiles/Canopy/CanopyTree_Branches");
    public override Asset<Texture2D> GetTopTextures() => ModContent.Request<Texture2D>($"Reverie/Assets/Textures/Tiles/Canopy/CanopyTree_Tops");
    public override int DropWood() => ItemID.Wood;

}