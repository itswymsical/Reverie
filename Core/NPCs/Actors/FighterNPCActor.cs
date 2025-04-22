
namespace Reverie.Core.NPCs.Actors;

public abstract class FighterNPCActor : ModNPC
{
    public virtual float MaxSpeed { get; } = 1.5f;
    public virtual float JumpHeightModifier { get; } = 1f;
    public virtual int MaxStuckTime { get; } = 60;

    public override void AI()
    {
        if (NPC.direction == 0)
            NPC.direction = 1;

        bool canJump = NPC.velocity.X == 0f && !NPC.justHit;
        bool isStuck = NPC.ai[1] >= MaxStuckTime;
        bool lookingAtTarget = NPC.velocity.Y == 0f && Math.Sign(NPC.velocity.X) == NPC.direction;

        HandleStuckState(isStuck, lookingAtTarget);
        UpdateMovement();
        HandleCollision(canJump);
    }

    private void HandleStuckState(bool isStuck, bool lookingAtTarget)
    {
        if (NPC.position.X == NPC.oldPosition.X || isStuck || !lookingAtTarget)
            NPC.ai[1]++;
        else if (Math.Abs(NPC.velocity.X) > 0.9f && NPC.ai[1] > 0f)
            NPC.ai[1]--;

        if (NPC.ai[1] > MaxStuckTime * 5 || NPC.justHit)
            NPC.ai[1] = 0f;

        if (isStuck)
        {
            if (NPC.ai[1] == MaxStuckTime)
                NPC.netUpdate = true;

            if (NPC.velocity.X == 0f && NPC.velocity.Y == 0f)
            {
                if (++NPC.ai[0] >= 2f)
                {
                    NPC.ai[0] = 0f;
                    NPC.direction *= -1;
                    NPC.spriteDirection = NPC.direction;
                }
            }
            else
                NPC.ai[0] = 0f;
        }
        else if (NPC.ai[1] < MaxStuckTime)
            NPC.TargetClosest();
    }

    private void UpdateMovement()
    {
        if (Math.Abs(NPC.velocity.X) > MaxSpeed)
        {
            if (NPC.velocity.Y == 0f)
                NPC.velocity *= 0.8f;
        }
        else
        {
            if (NPC.velocity.X < MaxSpeed && NPC.direction == 1)
                NPC.velocity.X += 0.07f;
            if (NPC.velocity.X > -MaxSpeed && NPC.direction == -1)
                NPC.velocity.X -= 0.07f;

            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -MaxSpeed, MaxSpeed);
        }
    }

    private void HandleCollision(bool canJump)
    {
        if (NPC.velocity.Y >= 0f)
            HandleSlopedCollision();

        if (CheckBottomCollision())
            HandleJump(canJump);
    }

    private void HandleSlopedCollision()
    {
        int velocityDirection = Math.Sign(NPC.velocity.X);
        Vector2 targetPosition = NPC.position + new Vector2(NPC.velocity.X, 0);
        int tileX = (int)((targetPosition.X + (NPC.width / 2) + ((NPC.width / 2 + 1) * velocityDirection)) / 16f);
        int tileY = (int)((targetPosition.Y + NPC.height - 1f) / 16f);

        var tiles = new[]
        {
           Framing.GetTileSafely(tileX, tileY),     // tile1
           Framing.GetTileSafely(tileX, tileY - 1), // tile2
           Framing.GetTileSafely(tileX, tileY - 2), // tile3
           Framing.GetTileSafely(tileX, tileY - 3), // tile4
           Framing.GetTileSafely(tileX, tileY - 4), // tile5
           Framing.GetTileSafely(tileX - velocityDirection, tileY - 3) // tile6
       };

        if (tileX * 16 < targetPosition.X + NPC.width && tileX * 16 + 16 > targetPosition.X &&
            ((tiles[0].HasUnactuatedTile && !tiles[0].TopSlope && !tiles[1].TopSlope && Main.tileSolid[tiles[0].TileType] && !Main.tileSolidTop[tiles[0].TileType]) ||
            (tiles[1].IsHalfBlock && tiles[1].HasUnactuatedTile)) && (!tiles[1].HasUnactuatedTile || !Main.tileSolid[tiles[1].TileType] || Main.tileSolidTop[tiles[1].TileType] ||
            (tiles[1].IsHalfBlock &&
            (!tiles[4].HasUnactuatedTile || !Main.tileSolid[tiles[4].TileType] || Main.tileSolidTop[tiles[4].TileType]))) &&
            (!tiles[2].HasUnactuatedTile || !Main.tileSolid[tiles[2].TileType] || Main.tileSolidTop[tiles[2].TileType]) &&
            (!tiles[3].HasUnactuatedTile || !Main.tileSolid[tiles[3].TileType] || Main.tileSolidTop[tiles[3].TileType]) &&
            (!tiles[5].HasUnactuatedTile || !Main.tileSolid[tiles[5].TileType]))
        {
            float tileYPosition = tileY * 16;
            if (tiles[0].IsHalfBlock)
                tileYPosition += 8f;
            if (tiles[1].IsHalfBlock)
                tileYPosition -= 8f;

            if (tileYPosition < targetPosition.Y + NPC.height)
            {
                float targetYPosition = targetPosition.Y + NPC.height - tileYPosition;
                if (targetYPosition <= 16.1f)
                {
                    NPC.gfxOffY += NPC.position.Y + NPC.height - tileYPosition;
                    NPC.position.Y = tileYPosition - NPC.height;
                    NPC.stepSpeed = targetYPosition < 9f ? 1f : 2f;
                }
            }
        }
    }

    private bool CheckBottomCollision()
    {
        if (NPC.velocity.Y != 0f) return false;

        int tileY = (int)(NPC.position.Y + NPC.height + 7f) / 16;
        int minTileX = (int)(NPC.position.X / 16);
        int maxTileX = (int)(NPC.position.X + NPC.width) / 16;

        for (int tileX = minTileX; tileX <= maxTileX; tileX++)
        {
            var tile = Main.tile[tileX, tileY];
            if (tile == null) return false;
            if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType])
                return true;
        }
        return false;
    }

    private void HandleJump(bool canJump)
    {
        int tileX = (int)((NPC.Center.X + 15 * NPC.direction) / 16f);
        int tileY = (int)((NPC.position.Y + NPC.height - 15f) / 16f);

        var tiles = new[]
        {
           Framing.GetTileSafely(tileX, tileY),     // tile1
           Framing.GetTileSafely(tileX, tileY - 1), // tile2
           Framing.GetTileSafely(tileX, tileY + 1), // tile3
           Framing.GetTileSafely(tileX + NPC.direction, tileY + 1) // tile4
       };

        if (NPC.spriteDirection == Math.Sign(NPC.velocity.X))
        {
            if (tiles[1].HasUnactuatedTile && Main.tileSolid[tiles[1].TileType])
            {
                NPC.netUpdate = true;
                NPC.velocity.Y = -6f;
            }
            else if (NPC.position.Y + NPC.height - (tileY * 16) > 20f && tiles[0].HasUnactuatedTile && !tiles[0].TopSlope && Main.tileSolid[tiles[0].TileType])
            {
                NPC.netUpdate = true;
                NPC.velocity.Y = -5f;
            }
            else if (NPC.directionY < 0 &&
                (!tiles[2].HasUnactuatedTile || !Main.tileSolid[tiles[2].TileType]) &&
                (!tiles[3].HasUnactuatedTile || !Main.tileSolid[tiles[3].TileType]))
            {
                NPC.netUpdate = true;
                NPC.velocity.Y = -8f;
                NPC.velocity.X *= 1.5f;
            }

            if (NPC.velocity.Y == 0f && canJump && NPC.ai[1] == 1f)
                NPC.velocity.Y = -5f;

            if (NPC.velocity.Y < 0)
                NPC.velocity.Y *= JumpHeightModifier;
        }
    }
}