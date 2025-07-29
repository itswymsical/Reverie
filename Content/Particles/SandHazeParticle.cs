using Microsoft.Xna.Framework.Graphics.PackedVector;
using Reverie.Common.Systems.Particles;
using Reverie.Content.Tiles.Archaea;
using System;

namespace Reverie.Content.Particles;

public class SandHazeParticle : Particle
{
    public Rectangle frame;
    public float yVelInfluence; // Track Y velocity changes for quicker fade

    private const float FADE_IN_DURATION = 25f;
    private const float FADE_OUT_DURATION = 70f;
    private const float TARGET_ALPHA = 200f; // Brighter than before
    private const float GRAVITY_CHANCE = 1f / 15f;
    private const float GRAVITY_STRENGTH = 0.008f;
    private const float VEL_INFLUENCE_FADE_FACTOR = 15f; // How much Y influence affects fade

    public override void Initialize(Vector2 startPos)
    {
        position = startPos;
        velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-0.2f, 0.2f));
        velocity *= 0.05f;
        velocity.Y *= 0.5f;
        scale = 0.47f;
        frame = new Rectangle((Main.rand.NextBool() ? 0 : 250), 0, 250, 115);
        alpha = 0f;
        lifetime = 0f;
        maxLifetime = 280f;
        yVelInfluence = 0f;
        active = true;
    }

    public override void Update()
    {
        if (!active) return;

        float oldVelY = velocity.Y;

        lifetime++;

        UpdateAlpha();
        HandlePlayerCollision();
        HandleTileCollision();

        velocity *= VELOCITY_DAMPING;
        if (Main.rand.NextFloat() < GRAVITY_CHANCE)
            velocity.Y += GRAVITY_STRENGTH;

        // Track Y velocity influence for fade acceleration
        float velYChange = Math.Abs(velocity.Y - oldVelY);
        yVelInfluence += velYChange;

        // Move opposite to velocity (sand drift effect)
        position -= velocity;

        if (alpha <= 0f || lifetime > GetEffectiveMaxLifetime())
            active = false;
    }

    private bool IsConnectedToSandTile()
    {
        int checkRadius = 2; // Check 2 tiles around particle
        int centerX = (int)(position.X / 16f);
        int centerY = (int)(position.Y / 16f);

        for (int x = centerX - checkRadius; x <= centerX + checkRadius; x++)
        {
            for (int y = centerY - checkRadius; y <= centerY + checkRadius; y++)
            {
                if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                    continue;

                Tile tile = Main.tile[x, y];
                if (IsSandTile(tile))
                    return true;
            }
        }
        return false;
    }

    private bool IsSandTile(Tile tile) => tile.HasTile && (
        tile.TileType == TileID.Sand ||
        tile.TileType == TileID.Ebonsand ||
        tile.TileType == TileID.Crimsand ||
        tile.TileType == TileID.Pearlsand ||
        tile.TileType == ModContent.TileType<PrimordialSandTile>()
    );

    private float GetEffectiveMaxLifetime()
    {
        // Reduce lifetime based on how much Y velocity has been influenced
        float reduction = yVelInfluence * VEL_INFLUENCE_FADE_FACTOR;
        return Math.Max(maxLifetime - reduction, 100f); // Don't go below 100 frames
    }

    protected override void ApplyPhysics() { }
    protected override void UpdateBehavior() { }

    private void UpdateAlpha()
    {
        bool connectedToSand = IsConnectedToSandTile();
        float effectiveMaxLifetime = GetEffectiveMaxLifetime();

        // If not connected to sand, reduce effective lifetime dramatically
        if (!connectedToSand)
        {
            effectiveMaxLifetime = Math.Min(effectiveMaxLifetime, lifetime + 60f); // Start fading within 60 frames
        }

        float fadeOutStart = effectiveMaxLifetime - FADE_OUT_DURATION;

        if (lifetime < FADE_IN_DURATION)
        {
            // Fade in from 0 to target alpha
            float progress = lifetime / FADE_IN_DURATION;
            float easedProgress = progress * progress * progress; // Cubic easing
            alpha = TARGET_ALPHA * easedProgress;
        }
        else if (lifetime >= fadeOutStart || !connectedToSand)
        {
            // Fade out from target alpha to 0, accelerated by Y velocity influence
            float fadeProgress = (lifetime - fadeOutStart) / FADE_OUT_DURATION;

            // If not connected to sand, fade much faster
            if (!connectedToSand)
            {
                fadeProgress += 0.05f; // Extra fade speed when disconnected
            }

            // Accelerate fade based on Y velocity influence
            float acceleratedFade = fadeProgress + (yVelInfluence * 0.1f);
            acceleratedFade = MathHelper.Clamp(acceleratedFade, 0f, 1f);

            alpha = TARGET_ALPHA * (1f - acceleratedFade);
        }
        else
        {
            // Maintain target alpha with slight random variation
            if (Main.rand.NextBool(8))
            {
                alpha += Main.rand.NextFloat(-2f, 2f);
                alpha = MathHelper.Clamp(alpha, TARGET_ALPHA - 10f, TARGET_ALPHA + 10f);
            }
        }
    }

    private void HandlePlayerCollision()
    {
        Player player = Main.LocalPlayer;
        if (!player.active || player.dead) return;

        float playerSpeed = player.velocity.Length();
        const float MIN_SPEED = 4f;

        if (playerSpeed <= MIN_SPEED) return;

        float distance = Vector2.Distance(position, player.Center);
        const float COLLISION_RANGE = 80f;

        if (distance < COLLISION_RANGE && distance > 0)
        {
            Vector2 pushDir = Vector2.Normalize(position - player.Center);
            float pushStrength = (COLLISION_RANGE - distance) / COLLISION_RANGE;

            float velMultiplier = MathHelper.Clamp((playerSpeed - MIN_SPEED) / 8f, 0f, 1f);
            pushStrength *= velMultiplier;

            Vector2 oldVel = velocity;
            velocity += pushDir * pushStrength * 0.3f;
            velocity += player.velocity * 0.03f * velMultiplier;

            // Track the Y velocity change from player interaction
            yVelInfluence += Math.Abs(velocity.Y - oldVel.Y) * 2f; // Double weight for player interaction
        }
    }

    private void HandleTileCollision()
    {
        int tileX = (int)(position.X / 16f);
        int tileY = (int)(position.Y / 16f);

        if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
            return;

        Tile tile = Main.tile[tileX, tileY];
        if (tile.HasTile && Main.tileSolid[tile.TileType])
        {
            // Sand haze passes through tiles but moves slower
            const float TILE_SLOWDOWN = 0.7f; // Slow down to 70% speed in tiles
            velocity *= TILE_SLOWDOWN;

            // Only add upward velocity if this is a horizontal collision
            // Check if particle is moving horizontally into a tile wall
            bool horizontalCollision = false;

            if (Math.Abs(velocity.X) > 0.001f) // Has horizontal movement
            {
                int checkX = velocity.X > 0 ? tileX - 1 : tileX + 1; // Check opposite direction
                if (checkX >= 0 && checkX < Main.maxTilesX)
                {
                    Tile adjacentTile = Main.tile[checkX, tileY];
                    // If coming from air into solid tile horizontally
                    if (!adjacentTile.HasTile || !Main.tileSolid[adjacentTile.TileType])
                    {
                        horizontalCollision = true;
                    }
                }
            }

            if (horizontalCollision)
            {
                // Add upward drift when hitting tiles horizontally
                velocity.Y -= 0.002f;
                // Track this as a significant velocity change
                yVelInfluence += 0.003f;
            }
        }
    }

    public override void Reset()
    {
        base.Reset();
        frame = Rectangle.Empty;
        yVelInfluence = 0f;
    }
}