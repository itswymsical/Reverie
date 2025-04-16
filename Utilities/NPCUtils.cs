
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
}
