using ReLogic.Content;
using Terraria.Enums;
using Terraria.GameContent;

namespace Reverie.Content.Tiles.Sylvanwalde.Canopy;

public class TeakWoodTree : ModTree
{
    public override TreePaintingSettings TreeShaderSettings { get; } = new()
    {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };

    public override TreeTypes CountsAsTreeType { get; } = TreeTypes.Forest;

    public override void SetStaticDefaults()
    {
        GrowsOnTileId = [ModContent.TileType<LoamGrassTile>()];
    }

    public override Asset<Texture2D> GetTexture()
    {
        return ModContent.Request<Texture2D>($"{nameof(Reverie)}/Assets/Textures/Tiles/Canopy/TeakWoodTree");
    }

    public override Asset<Texture2D> GetTopTextures()
    {
        return ModContent.Request<Texture2D>($"{nameof(Reverie)}/Assets/Textures/Tiles/Canopy/TeakWoodTree_Tops");
    }

    public override Asset<Texture2D> GetBranchTextures()
    {
        return ModContent.Request<Texture2D>($"{nameof(Reverie)}/Assets/Textures/Tiles/Canopy/TeakWoodTree_Branches");
    }

    public override int DropWood()
    {
        return ItemID.Wood;
    }

    public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
    {
        topTextureFrameWidth = 124;
        topTextureFrameHeight = 98;
    }

    public override int SaplingGrowthType(ref int style)
    {
        style = 0;

        return ModContent.TileType<TeakWoodSaplingTile>();
    }
}