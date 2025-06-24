using Terraria.GameContent.Drawing;
using Terraria.GameContent;
using Terraria.Utilities;
using Terraria;

namespace Reverie.Content.Tiles.Canopy.Trees;

public class MediumTanglewoodTree : TanglewoodTree
{
    #region Tree Properties

    public override int FrameWidth => 28;
    public override int FrameHeight => 28;
    public override int TreeWidth => 1;
    public override int MaxHeight => 35;
    public override int MinHeight => 15;
    public override int TrunkTextureCount => 6;

    #endregion

    /// <summary>
    /// Frames 0-1: Stump textures (1-2) - should always connect
    /// Frames 2-4: Trunk textures (3-5) - should connect with each other
    /// Frames 3-4: Have special leaf foliage rendering (frames 4-5 in 1-indexed)
    /// </summary>
    protected override Point GetTrunkFrame(int heightFromBase, int totalHeight)
    {
        int frameX, frameY = 0;

        if (heightFromBase == 0)
        {
            frameX = WorldGen.genRand.Next(2);
        }
        else if (heightFromBase == totalHeight - 1)
        {
            frameX = 4;
        }
        else
        {
            int workingHeight = heightFromBase - 1;
            int stripGroup = workingHeight / 3;
            int positionInStrip = workingHeight % 3;

            if (positionInStrip == 1)
            {
                frameX = WorldGen.genRand.NextFloat() < 0.3f ? 3 : 5;
            }
            else
            {
                frameX = 2 + positionInStrip;
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

    public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
    {
        var tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || tile.TileType != Type)
            return;
        // Base implementation - draw tree top if this is actually the top
        if (IsTreeTop(i, j))
        {
            DrawTreeTop(i, j, spriteBatch);
            return;
        }
        // Draw special leaf foliage on frame 3 (texture 4) only - not on frame 5 alt
        int frameIndex = tile.TileFrameX / FrameWidth;
        if (frameIndex == 3)
        {
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
            float rotation = GetSway(i, j);
            var position = new Vector2(i + 11.54f, j + 10.99f) * 16 - Main.screenPosition;
            var samplerState = Main.graphics.GraphicsDevice.SamplerStates[0];
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, Main.Rasterizer, null);
            var foliageSource = new Rectangle(84, 48, 56, 48);
            var foliageOrigin = new Vector2(foliageSource.Width / 2, foliageSource.Height / 2);
            var foliageDrawPosition = position + new Vector2(16, 16);
            float foliageRotation = rotation * -0.6f;
            spriteBatch.Draw(texture, foliageDrawPosition, foliageSource,
                color, foliageRotation, foliageOrigin, 1f, SpriteEffects.None, 0f);
            // DEBUG: Draw hitbox for foliage
            var hitboxRect = new Rectangle(
                (int)(foliageDrawPosition.X - foliageOrigin.X),
                (int)(foliageDrawPosition.Y - foliageOrigin.Y),
                foliageSource.Width,
                foliageSource.Height
            );
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState,
                DepthStencilState.None, Main.Rasterizer, null);
        }
    }
    protected override void DrawTreeTop(int i, int j, SpriteBatch spriteBatch)
    {
        var tile = Framing.GetTileSafely(i, j);
        if (!tile.HasTile || tile.TileType != Type)
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
        float rotation = GetSway(i, j);
        var position = new Vector2(i + 11.54f, j + 10.99f) * 16 - Main.screenPosition;
        // Store current sampler state and switch to PointClamp
        var samplerState = Main.graphics.GraphicsDevice.SamplerStates[0];
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
            DepthStencilState.None, Main.Rasterizer, null);
        // Draw the tree top sprite if this is actually the top
        if (IsTreeTop(i, j))
        {
            var topSource = new Rectangle(0, 48, 80, 120); // Tree top coordinates
            var topOrigin = new Vector2(topSource.Width / 2, topSource.Height); // Center-bottom origin
            var topDrawPosition = position + new Vector2(14, 16); // Center horizontally, align to tile bottom
            spriteBatch.Draw(texture, topDrawPosition, topSource,
                color, rotation, topOrigin, 1f, SpriteEffects.None, 0f);
        }
        // Restore previous sampler state
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState,
            DepthStencilState.None, Main.Rasterizer, null);
    }

    protected override void GrowEffects(int i, int j)
    {
        var center = new Vector2(i, j) * 16f + new Vector2(14); // Adjusted for larger tree

        // More particles for medium tree
        for (var g = 0; g < 12; g++)
        {
            var leaf = Dust.NewDustDirect(center + Main.rand.NextVector2Unit() * 35f, 0, 0,
                DustID.GrassBlades, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-1.5f, 1.5f));
            leaf.fadeIn = 1.1f;
            leaf.scale = 1.0f;
        }
    }

    public static bool GrowTanglewoodTree(int i, int j)
    {
        var instance = ModContent.GetInstance<MediumTanglewoodTree>();
        return instance.GrowTree(i, j);
    }
}