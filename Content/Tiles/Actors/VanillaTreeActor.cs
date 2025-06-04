using ReLogic.Content;
using Terraria.Enums;
using Terraria.GameContent;
using Reverie.Content.Tiles.Taiga;

namespace Reverie.Content.Tiles.Actors;

public class SnowTaigaTreeActor : ModTree
{
    public override TreePaintingSettings TreeShaderSettings => new()
    {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };
    public override void SetStaticDefaults() => GrowsOnTileId = [ModContent.TileType<TaigaGrassTile>(), ModContent.TileType<SnowTaigaGrassTile>()];
    public override Asset<Texture2D> GetTexture() => Main.Assets.Request<Texture2D>("Images/Tiles_5_3")/*TextureAssets.Tile[5]*/;

    public override int SaplingGrowthType(ref int style)
    {
        style = 1;
        return TileID.Saplings;
    }

    public override TreeTypes CountsAsTreeType => TreeTypes.Snow;

    public override int TreeLeaf()
    {
        return GoreID.TreeLeaf_Boreal;
    }

    public override Asset<Texture2D> GetBranchTextures() => TextureAssets.TreeBranch[17];
    public override Asset<Texture2D> GetTopTextures() => TextureAssets.TreeTop[17];
    public override int DropWood() => ItemID.BorealWood;

    public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
    {
        //xoffset += 4;
        topTextureFrameWidth = 80;
        topTextureFrameHeight = 80;
    }
}

public class TaigaTreeActor : ModTree
{
    public override TreePaintingSettings TreeShaderSettings => new()
    {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };
    public override void SetStaticDefaults() => GrowsOnTileId = [ModContent.TileType<TaigaGrassTile>()];
    public override Asset<Texture2D> GetTexture() => Main.Assets.Request<Texture2D>("Images/Tiles_5_3")/*TextureAssets.Tile[5]*/;

    public override int SaplingGrowthType(ref int style)
    {
        style = 1;
        return TileID.Saplings;
    }

    public override TreeTypes CountsAsTreeType => TreeTypes.Snow;

    public override int TreeLeaf()
    {
        return GoreID.TreeLeaf_Boreal;
    }

    public override Asset<Texture2D> GetBranchTextures() => TextureAssets.TreeBranch[16];
    public override Asset<Texture2D> GetTopTextures() => TextureAssets.TreeTop[16];
    public override int DropWood() => ItemID.BorealWood;

    public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
    {
        //xoffset += 4;
        topTextureFrameWidth = 80;
        topTextureFrameHeight = 80;
    }
}

public class CrimsonTaigaTreeActor : ModTree
{
    public override TreePaintingSettings TreeShaderSettings => new()
    {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };
    public override void SetStaticDefaults() => GrowsOnTileId = [ModContent.TileType<CrimsonTaigaGrassTile>()];
    public override Asset<Texture2D> GetTexture() => Main.Assets.Request<Texture2D>("Images/Tiles_5_4")/*TextureAssets.Tile[5]*/;

    public override int SaplingGrowthType(ref int style)
    {
        style = 1;
        return TileID.Saplings;
    }

    public override TreeTypes CountsAsTreeType => TreeTypes.Crimson;

    public override int TreeLeaf()
    {
        return GoreID.TreeLeaf_Crimson;
    }

    public override Asset<Texture2D> GetBranchTextures() => ModContent.Request<Texture2D>($"{NAME}/Assets/Textures/Tiles/Taiga/Crimson_Branches");
    public override Asset<Texture2D> GetTopTextures() => ModContent.Request<Texture2D>($"{NAME}/Assets/Textures/Tiles/Taiga/Crimson_Tops");
    public override int DropWood() => ItemID.Shadewood;

    public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
    {
        //xoffset += 4;
        topTextureFrameWidth = 80;
        topTextureFrameHeight = 80;
    }
}

public class CorruptTaigaTreeActor : ModTree
{
    public override TreePaintingSettings TreeShaderSettings => new()
    {
        UseSpecialGroups = true,
        SpecialGroupMinimalHueValue = 11f / 72f,
        SpecialGroupMaximumHueValue = 0.25f,
        SpecialGroupMinimumSaturationValue = 0.88f,
        SpecialGroupMaximumSaturationValue = 1f
    };
    public override void SetStaticDefaults() => GrowsOnTileId = [ModContent.TileType<CorruptTaigaGrassTile>()];
    public override Asset<Texture2D> GetTexture() => Main.Assets.Request<Texture2D>("Images/Tiles_5_0")/*TextureAssets.Tile[5]*/;

    public override int SaplingGrowthType(ref int style)
    {
        style = 1;
        return TileID.Saplings;
    }

    public override TreeTypes CountsAsTreeType => TreeTypes.Corrupt;

    public override int TreeLeaf()
    {
        return GoreID.TreeLeaf_Corruption;
    }

    public override Asset<Texture2D> GetBranchTextures() => ModContent.Request<Texture2D>($"{NAME}/Assets/Textures/Tiles/Taiga/Corrupt_Branches");
    public override Asset<Texture2D> GetTopTextures() => ModContent.Request<Texture2D>($"{NAME}/Assets/Textures/Tiles/Taiga/Corrupt_Tops");
    public override int DropWood() => ItemID.Ebonwood;

    public override void SetTreeFoliageSettings(Tile tile, ref int xoffset, ref int treeFrame, ref int floorY, ref int topTextureFrameWidth, ref int topTextureFrameHeight)
    {
        //xoffset += 4;
        topTextureFrameWidth = 80;
        topTextureFrameHeight = 80;
    }
}
