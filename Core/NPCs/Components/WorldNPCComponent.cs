
namespace Reverie.Core.NPCs.Components;

public class WorldNPCComponent : NPCComponent
{
    #region States
    private const float STATE_NONE = 0f;
    private const float STATE_FOLLOW = 1f;
    private const float STATE_STAY = 2f;
    private const float STATE_WANDER = 3f;
    private const float STATE_ATTACK = 10f;

    #endregion

    #region Properties
    public float FollowDistance { get; set; } = 100f;
    public int FollowingPlayer { get; private set; } = -1;
    private Point HomePosition { get; set; }
    private float DefaultMovementSpeed => 2.25f;
    public float AttackDistance { get; set; } = 200f;
    private int JumpCooldown { get; set; } = 0;
    private const int JUMP_COOLDOWN_TICKS = 40;
    #endregion

    public override void SetDefaults(NPC npc)
    {
        Enabled = npc.townNPC;
        if (Enabled)
        {
            npc.homeless = true;
            HomePosition = npc.Center.ToTileCoordinates();
        }
    }

    #region Command Methods
    public void Follow(NPC npc, int playerIndex)
    {
        if (!Enabled) return;

        npc.ai[3] = STATE_FOLLOW;
        FollowingPlayer = playerIndex;
        npc.aiStyle = -1; // Take control of AI
        npc.netUpdate = true;
    }

    public void Stay(NPC npc)
    {
        if (!Enabled) return;

        npc.ai[3] = STATE_STAY;
        FollowingPlayer = -1;
        HomePosition = npc.Center.ToTileCoordinates();
        npc.aiStyle = -1; // Take control of AI
        npc.netUpdate = true;
    }

    public void Wander(NPC npc)
    {
        if (!Enabled) return;

        npc.ai[3] = STATE_WANDER;
        FollowingPlayer = -1;
        npc.aiStyle = 7; // Return to town NPC AI
        npc.netUpdate = true;
    }

    private void ReturnToDefaultAI(NPC npc)
    {
        npc.ai[3] = STATE_NONE;
        FollowingPlayer = -1;
        npc.aiStyle = 7;
        npc.netUpdate = true;
    }
    #endregion

    public override bool PreAI(NPC npc)
    {
        if (!Enabled) return true;
        if (npc.aiStyle == 7) return true; // Let default AI handle it

        // Tick down jump cooldown
        if (JumpCooldown > 0)
            JumpCooldown--;

        // Check if there are any hostile NPCs nearby
        bool hostileNearby = false;
        foreach (NPC otherNPC in Main.npc)
        {
            if (otherNPC.active && !otherNPC.friendly && !otherNPC.CountsAsACritter && otherNPC.DistanceSQ(npc.Center) <= AttackDistance * AttackDistance)
            {
                hostileNearby = true;
                break;
            }
        }

        // Enter or exit the attack state based on hostile NPC presence
        if (hostileNearby && npc.ai[3] != STATE_ATTACK)
        {
            npc.ai[3] = STATE_ATTACK; // Set the state to the attack state
            npc.aiStyle = 7;
            npc.netUpdate = true;
        }
        if (!hostileNearby && npc.ai[3] == STATE_ATTACK)
        {
            npc.ai[3] = STATE_FOLLOW; // Resume the following state
            npc.aiStyle = -1; // Reset the AI style to the default value
            npc.netUpdate = true;
        }

        // Check which state to handle
        switch (npc.ai[3])
        {
            case STATE_FOLLOW:
                return HandleFollowAI(npc);
            case STATE_STAY:
                return HandleStayAI(npc);
            case STATE_ATTACK:
                // Do nothing, let the vanilla AI handle the attack state
                return true;
            default:
                return true;
        }
    }
    #region Custom AI Hooks
    private bool HandleFollowAI(NPC npc)
    {
        if (FollowingPlayer == -1 || !Main.player[FollowingPlayer].active)
        {
            ReturnToDefaultAI(npc);
            return true;
        }

        var target = Main.player[FollowingPlayer];

        // Calculate NPC center in tile coordinates
        var npcTileX = (int)(npc.Center.X / 16f);
        var npcTileY = (int)(npc.Center.Y / 16f);

        bool shouldJump = false;
        int npcWidth = (int)Math.Ceiling(npc.width / 16f);
        int npcHeight = (int)Math.Ceiling(npc.height / 16f);

        var xDiff = target.Center.X - npc.Center.X;
        npc.direction = Math.Sign(xDiff);

        // Check for gaps in ground or obstacles ahead
        int scanStartX = npcTileX + (npc.direction > 0 ? npcWidth : -2);

        // Check for deep gaps that need jumping
        bool hasDeepGap = false;
        for (int x = 0; x < 2; x++)
        {
            int checkX = scanStartX + (x * npc.direction);

            // Check if there's a gap deeper than 3 tiles
            bool foundGround = false;
            for (int y = 0; y < 3; y++)
            {
                if (WorldGen.SolidTile(checkX, npcTileY + npcHeight + y))
                {
                    foundGround = true;
                    break;
                }
            }

            if (!foundGround)
            {
                hasDeepGap = true;
                break;
            }
        }

        if (hasDeepGap)
        {
            shouldJump = true;
        }

        // Check for tall obstacles
        int consecutiveBlocksUp = 0;
        for (int x = 0; x < 1; x++)
        {
            int checkX = scanStartX + (x * npc.direction);
            for (int y = 0; y < 3; y++)
            {
                Tile tile = Main.tile[checkX, npcTileY - y];
                if (tile.HasUnactuatedTile && (Main.tileSolid[tile.TileType] || tile.IsHalfBlock || tile.TopSlope || tile.BottomSlope || tile.LeftSlope || tile.RightSlope))
                { 
                    consecutiveBlocksUp++;
                    if (consecutiveBlocksUp >= 2)
                    {
                        shouldJump = true;
                        break;
                    }
                }
                else
                {
                    consecutiveBlocksUp = 0;
                }
            }
        }

        // Handle movement
        if (Math.Abs(xDiff) > FollowDistance)
        {
            var acceleration = 0.046f;
            if (npc.velocity.X < DefaultMovementSpeed && npc.direction == 1)
            {
                npc.velocity.X += acceleration;
                if (npc.velocity.X > DefaultMovementSpeed)
                    npc.velocity.X = DefaultMovementSpeed;
            }
            else if (npc.velocity.X > -DefaultMovementSpeed && npc.direction == -1)
            {
                npc.velocity.X -= acceleration;
                if (npc.velocity.X < -DefaultMovementSpeed)
                    npc.velocity.X = -DefaultMovementSpeed;
            }
        }
        else
        {
            npc.velocity.X *= 0.9f;
        }

        // Check if NPC is on solid ground
        bool onGround = false;
        for (int x = 0; x < npcWidth; x++)
        {
            if (WorldGen.SolidTile(npcTileX + x, npcTileY + npcHeight))
            {
                onGround = true;
                break;
            }
        }

        // Handle jumping with cooldown
        if (onGround && shouldJump && JumpCooldown <= 0)
        {
            npc.velocity.Y = -7f; // Jump height
            JumpCooldown = JUMP_COOLDOWN_TICKS;
        }
        else if (!onGround)
        {
            // Apply gravity
            npc.velocity.Y += 0.029f;
            if (npc.velocity.Y > 6.725f)
                npc.velocity.Y = 6.725f;
        }

        // Simple collision
        SlopedCollision(npc);

        return false; // Don't run default AI
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

    private bool HandleStayAI(NPC npc)
    {
        var currentPos = npc.Center.ToTileCoordinates();

        // If too far from stay position, walk back
        if (Math.Abs(currentPos.X - HomePosition.X) > 2)
        {
            npc.direction = Math.Sign(HomePosition.X - currentPos.X);
            var acceleration = 0.1f;

            if (npc.velocity.X < DefaultMovementSpeed && npc.direction == 1)
            {
                npc.velocity.X += acceleration;
            }
            else if (npc.velocity.X > -DefaultMovementSpeed && npc.direction == -1)
            {
                npc.velocity.X -= acceleration;
            }
        }
        else
        {
            npc.velocity.X *= 0.9f;
            if (Math.Abs(npc.velocity.X) < 0.1f)
            {
                npc.velocity.X = 0f;
            }
        }

        // Basic gravity and collision
        if (!Collision.SolidCollision(npc.position, npc.width, npc.height))
        {
            npc.velocity.Y += 0.029f;
            if (npc.velocity.Y > 6.725f)
                npc.velocity.Y = 6.725f;
        }

        Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

        return false; // Don't run default AI
    }
    #endregion
}