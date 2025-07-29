using Terraria.GameContent.Drawing;
using Terraria.GameContent;
using Terraria.Utilities;
using Reverie.Core.Tiles;

namespace Reverie.Content.Tiles.Canopy.Trees;

public class SmallTanglewoodTree : CustomTree
{
    #region Tree Properties

    public override int FrameWidth => 16;
    public override int FrameHeight => 22;
    public override int TreeWidth => 1;
    public override int MaxHeight => 24;
    public override int MinHeight => 9;
    public override int TrunkTextureCount => 9;

    #endregion

    /// <summary>
    /// Frames 0-1: Stump textures (1-2)
    /// Frames 2-4: Strip 1 (textures 3-5) - connects in order
    /// Frames 5-7: Strip 2 (textures 6-8) - connects in order  
    /// Frame 8: End texture (texture 9) - tree top
    /// Every 3 trunk segments, choose a strip variant and use all 3 textures in order
    /// </summary>
    protected override Point GetTrunkFrame(int heightFromBase, int totalHeight)
    {
        int frameX, frameY = 0;

        if (heightFromBase == 0)
        {
            // Base stump - random from frames 0-1 (textures 1-2)
            frameX = WorldGen.genRand.Next(2);
        }
        else if (heightFromBase == totalHeight - 1) // Changed from >= to ==
        {
            // Tree top - use frame 8 (texture 9) ONLY for the exact top tile
            frameX = 8;
        }
        else
        {
            // Trunk segments - choose strip every 3 segments
            int workingHeight = heightFromBase - 1; // Exclude base stump
            int stripGroup = workingHeight / 3; // Which group of 3 we're in
            int positionInStrip = workingHeight % 3; // Position within the current strip (0, 1, or 2)

            // Deterministically choose strip based on tree position and group
            bool useStrip1 = ((stripGroup + (totalHeight % 3)) % 2) == 0;

            if (useStrip1)
            {
                frameX = 2 + positionInStrip; // Frames 2-4 (strip 3-5)
            }
            else
            {
                frameX = 5 + positionInStrip; // Frames 5-7 (strip 6-8)
            }
        }

        return new Point(frameX, frameY);
    }

    protected override WeightedRandom<int> GetTreeDrops()
    {
        var drop = new WeightedRandom<int>();
        drop.Add(ItemID.Acorn, 0.075f);
        drop.Add(ItemID.Apple, 0.02f);
        return drop;
    }

    protected override void DrawTreeTop(int i, int j, SpriteBatch spriteBatch)
    {
        var tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || tile.TileType != Type)
            return;
        // Only draw if this is ACTUALLY the tree top, not just a tile with frame 8
        if (!IsTreeTop(i, j))
            return;
        // Get visual info
        Color color = tile.IsTileFullbright ? Color.White : Lighting.GetColor(i, j);
        Texture2D texture = TextureAssets.Tile[tile.TileType].Value;
        if (!TileDrawing.IsVisible(tile))
            return;
        if (tile.TileColor != PaintID.None)
        {
            var painted = Main.instance.TilePaintSystem.TryGetTileAndRequestIfNotReady(tile.TileType, 0, tile.TileColor);
            texture = painted ?? texture;
        }
        // Draw the tree top sprite
        var position = new Vector2(i + 11.74f, j + 11.2f) * 16 - Main.screenPosition;
        float rotation = GetSway(i, j);
        var source = new Rectangle(0, 32, 64, 96);
        var origin = new Vector2(source.Width / 2, source.Height); // Center-bottom origin
        var drawPosition = position + new Vector2(8, 16); // Center horizontally, align to tile bottom

        // Store current sampler state and switch to PointClamp
        var samplerState = Main.graphics.GraphicsDevice.SamplerStates[0];
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
            DepthStencilState.None, Main.Rasterizer, null);

        // Draw with PointClamp
        spriteBatch.Draw(texture, drawPosition, source,
            color, rotation, origin, 1f, SpriteEffects.None, 0f);

        // Restore previous sampler state
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState,
            DepthStencilState.None, Main.Rasterizer, null);
    }

    protected override void GrowEffects(int i, int j)
    {
        var center = new Vector2(i, j) * 16f + new Vector2(8);

        // Fewer particles for smaller tree
        for (var g = 0; g < 6; g++)
        {
            var leaf = Dust.NewDustDirect(center + Main.rand.NextVector2Unit() * 20f, 0, 0,
                DustID.GrassBlades, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1f, 1f));
            leaf.fadeIn = 1.0f;
            leaf.scale = 0.9f;
        }
    }

    public static bool GrowTanglewoodTree(int i, int j)
    {
        var instance = ModContent.GetInstance<SmallTanglewoodTree>();
        return instance.GrowTree(i, j);
    }
}