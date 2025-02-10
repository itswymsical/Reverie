
namespace Reverie.Core.NPCs.Actors;

public abstract class FighterNPCActor : ModNPC
{
    public virtual float MaxSpeed { get; } = 1.5f;
    public virtual float JumpHeightModifier { get; } = 1f;
    public virtual int MaxStuckTime { get; } = 60;

    public override void AI()
    {
        GenericFighterAI();
    }

    public void GenericFighterAI(float maxSpeed = 1.5f, int maxAllowedStuckTime = 60, float jumpHeightModifier = 1f)
    {

        bool canJump = false;
        bool isStuck = NPC.ai[1] >= maxAllowedStuckTime;

        if (NPC.velocity.X == 0f && !NPC.justHit)
        {
            canJump = true;
        }

        bool lookingAtTarget = true;

        if (NPC.velocity.Y == 0f && Math.Sign(NPC.velocity.X) != NPC.direction)
        {
            lookingAtTarget = false;
        }

        if (NPC.position.X == NPC.oldPosition.X || isStuck || !lookingAtTarget)
        {
            NPC.ai[1]++;
        }
        else if (Math.Abs(NPC.velocity.X) > 0.9f && NPC.ai[1] > 0f)
        {
            NPC.ai[1]--;
        }

        if (NPC.ai[1] > maxAllowedStuckTime * 5 || NPC.justHit)
        {
            NPC.ai[1] = 0f;
        }

        if (isStuck)
        {
            // First update being stuck.
            if (NPC.ai[1] == maxAllowedStuckTime)
            {
                NPC.netUpdate = true;
            }

            if (NPC.velocity.X == 0f)
            {
                if (NPC.velocity.Y == 0f)
                {
                    if (++NPC.ai[0] >= 2f)
                    {
                        NPC.ai[0] = 0f;
                        NPC.direction *= -1;
                        NPC.spriteDirection = NPC.direction;
                    }
                }
            }
            else
            {
                NPC.ai[0] = 0f;
            }
            if (NPC.direction == 0)
            {
                NPC.direction = 1;
            }
        }
        else if (NPC.ai[1] < maxAllowedStuckTime)
        {
            NPC.TargetClosest();
        }

        if (NPC.velocity.X < -maxSpeed || NPC.velocity.X > maxSpeed)
        {
            if (NPC.velocity.Y == 0f)
            {
                NPC.velocity *= 0.8f;
            }
        }
        else
        {
            if (NPC.velocity.X < maxSpeed && NPC.direction == 1)
            {
                NPC.velocity.X += 0.07f;
            }
            if (NPC.velocity.X > -maxSpeed && NPC.direction == -1)
            {
                NPC.velocity.X -= 0.07f;
            }
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -maxSpeed, maxSpeed);
        }

        bool collisionBottom = false;
        if (NPC.velocity.Y == 0f)
        {
            int tileY = (int)(NPC.position.Y + NPC.height + 7f) / 16;
            int minTileX = (int)(NPC.position.X / 16);
            int maxTileX = (int)(NPC.position.X + NPC.width) / 16;

            for (int tileX = minTileX; tileX <= maxTileX; tileX++)
            {
                if (Main.tile[tileX, tileY] == null)
                {
                    return;
                }
                if (Main.tile[tileX, tileY].HasUnactuatedTile && Main.tileSolid[Main.tile[tileX, tileY].TileType])
                {
                    collisionBottom = true;
                    break;
                }
            }
        }

        if (NPC.velocity.Y >= 0f)
        {
            SlopedCollision(NPC);
        }

        if (collisionBottom)
        {
           HandleJump(NPC, canJump, jumpHeightModifier);
        }
    }

    private static void SlopedCollision(NPC npc)
    {
        int velocityDirection = Math.Sign(npc.velocity.X);
        Vector2 targetPosition = npc.position + new Vector2(npc.velocity.X, 0);

        int tileX = (int)((targetPosition.X + (npc.width / 2) + ((npc.width / 2 + 1) * velocityDirection)) / 16f);
        int tileY = (int)((targetPosition.Y + npc.height - 1f) / 16f);

        Tile tile1 = Framing.GetTileSafely(tileX, tileY);
        Tile tile2 = Framing.GetTileSafely(tileX, tileY - 1);
        Tile tile3 = Framing.GetTileSafely(tileX, tileY - 2);
        Tile tile4 = Framing.GetTileSafely(tileX, tileY - 3);
        Tile tile5 = Framing.GetTileSafely(tileX, tileY - 4);
        Tile tile6 = Framing.GetTileSafely(tileX - velocityDirection, tileY - 3);

        if (tileX * 16 < targetPosition.X + npc.width && tileX * 16 + 16 > targetPosition.X &&
            ((tile1.HasUnactuatedTile && !tile1.TopSlope && !tile2.TopSlope && Main.tileSolid[tile1.TileType] && !Main.tileSolidTop[tile1.TileType]) ||
            (tile2.IsHalfBlock && tile2.HasUnactuatedTile)) && (!tile2.HasUnactuatedTile || !Main.tileSolid[tile2.TileType] || Main.tileSolidTop[tile2.TileType] ||
            (tile2.IsHalfBlock &&
            (!tile5.HasUnactuatedTile || !Main.tileSolid[tile5.TileType] || Main.tileSolidTop[tile5.TileType]))) &&
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
                float targetYPosition = targetPosition.Y + npc.height - tileYPosition;
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

    private static void HandleJump(NPC npc, bool canJump, float jumpHeightModifier)
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
            if (tile2.HasUnactuatedTile && Main.tileSolid[tile2.TileType])
            {
                npc.netUpdate = true;
                npc.velocity.Y = -6f;
            }
            else if (npc.position.Y + npc.height - (tileY * 16) > 20f && tile1.HasUnactuatedTile && !tile1.TopSlope && Main.tileSolid[tile1.TileType])
            {
                npc.netUpdate = true;
                npc.velocity.Y = -5f;
            }
            else if (npc.directionY < 0 &&
                (!tile3.HasUnactuatedTile || !Main.tileSolid[tile3.TileType]) &&
                (!tile4.HasUnactuatedTile || !Main.tileSolid[tile4.TileType]))
            {
                npc.netUpdate = true;
                npc.velocity.Y = -8f;
                npc.velocity.X *= 1.5f;
            }

            if (npc.velocity.Y == 0f && canJump && npc.ai[1] == 1f)
            {
                npc.velocity.Y = -5f;
            }

            if (npc.velocity.Y < 0)
            {
                npc.velocity.Y *= jumpHeightModifier;
            }
        }
    }
}