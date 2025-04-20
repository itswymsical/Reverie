
using Terraria.GameContent;

namespace Reverie.Utilities;

/// <summary>
/// Utility class for modifying NPCs.
/// </summary>
public static class NPCUtils
{
    /// <summary>
    /// Checks if a player is below the NPC while it's on a platform.
    /// Credits: https://github.com/PhoenixBladez/SpiritMod/tree/master
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="player"></param>
    public static void CheckPlatform(NPC npc, Player player) 
    {
        bool onplatform = true;
        for (int i = (int)npc.position.X; i < npc.position.X + npc.width; i += npc.height / 2)
        {
            Tile tile = Framing.GetTileSafely(new Point((int)npc.position.X / 16, (int)(npc.position.Y + npc.height + 8) / 16));
            if (!TileID.Sets.Platforms[tile.TileType])
                onplatform = false;
        }
        if (onplatform && (npc.Center.Y < player.position.Y - 20))
            npc.noTileCollide = true;
        else
            npc.noTileCollide = false;
    }

    public static void SlopedCollision(NPC npc)
    {
        var velocityDirection = Math.Sign(npc.velocity.X);
        var targetPosition = npc.position + new Vector2(npc.velocity.X, 0);

        var tileX = (int)((targetPosition.X + npc.width / 2 + (npc.width / 2 + 1) * velocityDirection) / 16f);
        var tileY = (int)((targetPosition.Y + npc.height - 1f) / 16f);

        var tile1 = Framing.GetTileSafely(tileX, tileY);
        var tile2 = Framing.GetTileSafely(tileX, tileY - 1);
        var tile3 = Framing.GetTileSafely(tileX, tileY - 2);
        var tile4 = Framing.GetTileSafely(tileX, tileY - 3);
        var tile5 = Framing.GetTileSafely(tileX, tileY - 4);
        var tile6 = Framing.GetTileSafely(tileX - velocityDirection, tileY - 3);

        if (tileX * 16 < targetPosition.X + npc.width && tileX * 16 + 16 > targetPosition.X &&
            (tile1.HasUnactuatedTile && !tile1.TopSlope && !tile2.TopSlope && Main.tileSolid[tile1.TileType] && !Main.tileSolidTop[tile1.TileType] ||
            tile2.IsHalfBlock && tile2.HasUnactuatedTile) && (!tile2.HasUnactuatedTile || !Main.tileSolid[tile2.TileType] || Main.tileSolidTop[tile2.TileType] ||
            tile2.IsHalfBlock &&
            (!tile5.HasUnactuatedTile || !Main.tileSolid[tile5.TileType] || Main.tileSolidTop[tile5.TileType])) &&
            (!tile3.HasUnactuatedTile || !Main.tileSolid[tile3.TileType] || Main.tileSolidTop[tile3.TileType]) &&
            (!tile4.HasUnactuatedTile || !Main.tileSolid[tile4.TileType] || Main.tileSolidTop[tile4.TileType]) &&
            (!tile6.HasUnactuatedTile || !Main.tileSolid[tile6.TileType]))
        {
            float tileYPosition = tileY * 16;
            if (Main.tile[tileX, tileY].IsHalfBlock)
            {
                tileYPosition += 8f;
            }
            if (Main.tile[tileX, tileY - 1].IsHalfBlock)
            {
                tileYPosition -= 8f;
            }

            if (tileYPosition < targetPosition.Y + npc.height)
            {
                var targetYPosition = targetPosition.Y + npc.height - tileYPosition;
                if (targetYPosition <= 16.1f)
                {
                    npc.gfxOffY += npc.position.Y + npc.height - tileYPosition;
                    npc.position.Y = tileYPosition - npc.height;

                    if (targetYPosition < 9f)
                    {
                        npc.stepSpeed = 1f;
                    }
                    else
                    {
                        npc.stepSpeed = 2f;
                    }
                }
            }
        }
    }

    private static void ApplyJump(NPC npc, bool canJump, float jumpHeightModifier)
    {
        int tileX = (int)((npc.Center.X + 15 * npc.direction) / 16f);
        int tileY = (int)((npc.position.Y + npc.height - 15f) / 16f);

        Tile tile1 = Framing.GetTileSafely(tileX, tileY);
        Tile tile2 = Framing.GetTileSafely(tileX, tileY - 1);
        Tile tile3 = Framing.GetTileSafely(tileX, tileY + 1);
        Tile tile4 = Framing.GetTileSafely(tileX + npc.direction, tileY + 1);

        tile3.IsHalfBlock = true;

        if (npc.spriteDirection == Math.Sign(npc.velocity.X))
        {
            if (tile2.HasTile && Main.tileSolid[tile2.TileType])
            {
                npc.netUpdate = true;
                npc.velocity.Y = -6f;
            }
            else if (npc.position.Y + npc.height - (tileY * 16) > 20f && tile1.HasTile && !tile1.TopSlope && Main.tileSolid[tile1.TileType])
            {
                npc.netUpdate = true;
                npc.velocity.Y = -5f;
            }
            else if (npc.directionY < 0 &&
                (!tile3.HasTile || !Main.tileSolid[tile3.TileType]) &&
                (!tile4.HasTile || !Main.tileSolid[tile4.TileType]))
            {
                npc.netUpdate = true;
                npc.velocity.Y = -8f;
                npc.velocity.X *= 1.5f;
            }

            if (npc.velocity.Y == 0f && canJump && npc.ai[1] == 1f)
                npc.velocity.Y = -5f;

            if (npc.velocity.Y < 0f)
                npc.velocity.Y *= jumpHeightModifier;
        }
    }

    public static bool HoleBelow(this NPC npc)
    {
        int tileWidth = (int)Math.Round(npc.width / 16f);

        int tileX = (int)(npc.Center.X / 16f) - tileWidth;

        if (npc.velocity.X > 0f)
            tileX += tileWidth;

        int tileY = (int)((npc.position.Y + npc.height) / 16f);

        for (int j = tileY; j < tileY + 2; j++)
        {
            for (int i = tileX; i < tileX + tileWidth; i++)
            {
                if (!WorldGen.InWorld(i, j))
                    continue;

                Tile tile = Framing.GetTileSafely(i, j);

                if (tile.HasTile)
                    return false;
            }
        }

        return true;
    }

    public static bool DrawNPCCenteredWithTexture(this NPC npc, Texture2D texture, SpriteBatch spriteBatch, Color color)
    {
        Vector2 origin = npc.frame.Size() / 2f + new Vector2(0f, npc.ModNPC.DrawOffsetY);

        SpriteEffects effects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        Vector2 drawPosition = npc.Center.ToDrawPosition() + new Vector2(0f, npc.gfxOffY);

        spriteBatch.Draw(texture, drawPosition, npc.frame, color, npc.rotation, origin, npc.scale, effects, 0f);

        return false;
    }
    public static bool DrawNPCCentered(this NPC npc, Texture2D texture, SpriteBatch spriteBatch, Color color)
    {
        return npc.DrawNPCCenteredWithTexture(texture, spriteBatch, npc.GetAlpha(color));
    }
    public static void DrawNPCTrailCenteredWithTexture(this NPC npc, Texture2D texture, SpriteBatch spriteBatch, Color color, float initialOpacity = 0.8f, float opacityDegrade = 0.2f, int stepSize = 1)
    {
        Vector2 origin = npc.frame.Size() / 2f;

        SpriteEffects effects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        for (int i = 0; i < NPCID.Sets.TrailCacheLength[npc.type]; i += stepSize)
        {
            float opacity = initialOpacity - opacityDegrade * i;

            Vector2 position = npc.oldPos[i].ToDrawPosition() + npc.Hitbox.Size() / 2f + new Vector2(0f, npc.gfxOffY);

            spriteBatch.Draw(texture, position, npc.frame, color * opacity, npc.oldRot[i], origin, npc.scale, effects, 0f);
        }
    }
    public static void DrawNPCTrailCentered(this NPC npc, SpriteBatch spriteBatch, Color color, float initialOpacity = 0.8f, float opacityDegrade = 0.2f, int stepSize = 1)
    {
        Texture2D texture = TextureAssets.Npc[npc.type].Value;

        npc.DrawNPCTrailCenteredWithTexture(texture, spriteBatch, npc.GetAlpha(color), initialOpacity, opacityDegrade, stepSize);
    }

}
