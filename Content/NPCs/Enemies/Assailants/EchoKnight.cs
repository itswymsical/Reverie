using Reverie.Core.Graphics;
using Reverie.Core.Graphics.Interfaces;
using Reverie.Core.Loaders;
using Reverie.Core.NPCs.Actors;
using Reverie.Utilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;

namespace Reverie.Content.NPCs.Enemies.Assailants;

public class EchoKnight : FighterNPCActor, IDrawPrimitive
{
    public override float MaxSpeed => 1.46f;
    private float TeleportCooldown = Main.rand.NextFloat(120f, 180f);
    private readonly int MaxTeleportDistance = 140;
    private readonly int MinTeleportDistance = 60;
    private readonly int ChargeDuration = 45;
    private readonly int SlashDuration = 40;
    private readonly int ProximityThreshold = 44;
    private readonly float DamageMultiplier = 1.5f;
    private readonly int HitboxExtension = 44;

    private readonly int RetreatDuration = 60;
    private readonly float RetreatMoveSpeedMultiplier = 0.6f;
    private readonly int FastTeleportChargeDuration = 15;
    private readonly float RetreatThreshold = 75f;
    private readonly float RetreatProbability = 0.6f;

    private readonly int SlashHitboxDuration = 5;

    private Rectangle originalHitbox;
    private bool hitboxExtended = false;
    private int hitboxExtensionCounter = 0;

    private const int STATE_NORMAL = 0;
    private const int STATE_CHARGING = 1;
    private const int STATE_TELEPORTING = 2;
    private const int STATE_SLASHING = 3;
    private const int STATE_RETREAT = 4;
    private const int STATE_FAST_CHARGING = 5;

    public override void SetStaticDefaults()
    {
        NPCID.Sets.TrailingMode[NPC.type] = 0;
        NPCID.Sets.TrailCacheLength[NPC.type] = 7;

        Main.npcFrameCount[NPC.type] = 1;

        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
        {
            Velocity = 0f,
            Direction = 1
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange([BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheCorruption,
            new FlavorTextBestiaryInfoElement("Mods.Reverie.NPCs.EchoKnight.Bestiary")
        ]);
    }

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.damage = 8;
        NPC.defense = 8;
        NPC.lifeMax = 88;
        NPC.width = 30;
        NPC.height = 48;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.22f;
        NPC.value = Item.buyPrice(copper: Main.rand.Next(92, 170));
    }
    private List<Vector2> cache;
    private Trail trail;
    private Trail trail2;
    public override void AI()
    {
        ManageCaches();
        ManageTrail();
        var target = Main.player[NPC.target];
        NPC.velocity.Y = NPC.velocity.Y * 0.97f;
        if (target.active && !target.dead)
        {
            NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
        }

        if ((int)NPC.ai[3] == STATE_NORMAL)
        {
            base.AI();
        }

        switch ((int)NPC.ai[3])
        {
            case STATE_NORMAL:
                TrySlash();

                NPC.ai[2]++;
                if (NPC.ai[2] >= TeleportCooldown)
                {
                    NPC.ai[3] = STATE_CHARGING;
                    NPC.ai[2] = 0;
                    NPC.velocity.X = 0f;
                    NPC.netUpdate = true;
                }
                break;

            case STATE_CHARGING:
                NPC.velocity.X = 0f;
                DoChargingFX();
                NPC.ai[2]++;

                if (NPC.ai[2] >= ChargeDuration)
                {
                    Teleport();
                    NPC.ai[3] = STATE_TELEPORTING;
                    NPC.ai[2] = 0;
                    NPC.netUpdate = true;
                }
                break;

            case STATE_TELEPORTING:
                NPC.ai[2]++;
                if (NPC.ai[2] >= 5)
                {
                    NPC.ai[3] = STATE_NORMAL;
                    NPC.ai[2] = 0;
                    NPC.netUpdate = true;
                }
                break;

            case STATE_SLASHING:
                NPC.velocity.X = 0f;

                PerformSlash();
                ManageHitboxExtension();
                NPC.ai[2]++;

                if (NPC.ai[2] >= SlashDuration ||
                    Vector2.Distance(NPC.Center, target.Center) > ProximityThreshold * 1.5f)
                {
                    // Chance to enter retreat state after slashing
                    if (Main.rand.NextFloat() < RetreatProbability)
                    {
                        NPC.ai[3] = STATE_RETREAT;
                        NPC.ai[2] = 0;
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        NPC.ai[3] = STATE_NORMAL;
                        NPC.ai[2] = 0;
                        NPC.netUpdate = true;
                    }
                    ResetSlash();
                }
                break;

            case STATE_RETREAT:
                PerformRetreat();
                NPC.ai[2]++;

                if (NPC.ai[2] >= RetreatDuration)
                {
                    // Transition to fast charging teleport
                    NPC.ai[3] = STATE_FAST_CHARGING;
                    NPC.ai[2] = 0;
                    NPC.velocity.X = 0f;
                    NPC.netUpdate = true;
                }
                break;

            case STATE_FAST_CHARGING:
                NPC.velocity.X = 0f;
                DoFastChargingFX();
                NPC.ai[2]++;

                if (NPC.ai[2] >= FastTeleportChargeDuration)
                {
                    // Faster, more aggressive teleport
                    FastTeleport();
                    NPC.ai[3] = STATE_TELEPORTING;
                    NPC.ai[2] = 0;
                    NPC.netUpdate = true;
                }
                break;
        }
    }
    private void ManageCaches()
    {
        // Get the actual projectile position without random offsets
        var pos = NPC.Top;

        if (cache == null)
        {
            cache = [];
            for (var i = 0; i < 15; i++)
            {
                cache.Add(pos);
            }
        }

        cache.Add(pos);
        while (cache.Count > 15)
        {
            cache.RemoveAt(0);
        }
    }

    private void ManageTrail()
    {
        var pos = NPC.Center;

        var purple = new Color(73, 48, 137);

        trail ??= new Trail(Main.instance.GraphicsDevice, 15, new RoundedTip(12), factor => factor * 74, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.MediumPurple * 0;
            return purple * 0.6f * (float)Math.Pow(factor.X, 2);
        });
        trail.Positions = [.. cache];

        trail2 ??= new Trail(Main.instance.GraphicsDevice, 15, new RoundedTip(12), factor => factor * 46, factor =>
        {
            if (factor.X >= 0.98f)
                return Color.White * 0;
            return Color.Lerp(purple, Color.DimGray, 0.6f) * 0.7f * (float)Math.Pow(factor.X, 2);
        });
        trail2.Positions = [.. cache];

        trail.NextPosition = pos;
        trail2.NextPosition = pos;
    }

    public void DrawPrimitives()
    {
        var effect = ShaderLoader.GetShader("pixelTrail").Value;

        if (effect != null)
        {
            var world = Matrix.CreateTranslation(-Main.screenPosition.ToVector3());
            var view = Main.GameViewMatrix.TransformationMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"]?.SetValue(Main.GameUpdateCount * 0.02f);
            effect.Parameters["repeats"]?.SetValue(8f);
            effect.Parameters["pixelation"]?.SetValue(4f);
            effect.Parameters["resolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["transformMatrix"]?.SetValue(world * view * projection);
            effect.Parameters["sampleTexture"]?.SetValue(ModContent.Request<Texture2D>($"{VFX_DIRECTORY}EnergyTrail").Value);

            trail?.Render(effect);

            effect.Parameters["pixelation"]?.SetValue(6f);
            trail2?.Render(effect);
        }
    }
    private void PerformRetreat()
    {
        var target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest();
            target = Main.player[NPC.target];
        }

        // Move away from player
        var retreatDirection = NPC.Center - target.Center;

        // If too close to a wall, try to move more horizontally
        if (Collision.SolidCollision(NPC.position + new Vector2(retreatDirection.X, retreatDirection.Y).SafeNormalize(Vector2.Zero) * 20f, NPC.width, NPC.height))
        {
            retreatDirection.Y *= 0.2f;
        }

        retreatDirection.Normalize();

        // Apply reduced speed during retreat
        var retreatSpeed = MaxSpeed * RetreatMoveSpeedMultiplier;
        NPC.velocity = retreatDirection * retreatSpeed;

        // Create dust trail while retreating
        if (NPC.ai[2] % 4 == 0)
        {
            var dustIndex = Dust.NewDust(
                NPC.position,
                NPC.width,
                NPC.height,
                DustID.Shadowflame,
                0f,
                0f
            );
            Main.dust[dustIndex].noGravity = true;
            Main.dust[dustIndex].scale = Main.rand.NextFloat(0.8f, 1.2f);
            Main.dust[dustIndex].velocity = NPC.velocity * -0.5f;
        }
    }

    private void DoFastChargingFX()
    {
        // Play fast teleport charging sound at the beginning
        if (NPC.ai[2] == 0)
        {
            SoundEngine.PlaySound(SoundID.Item9, NPC.position);
        }

        // More intense particle effects than normal charging
        for (var i = 0; i < 4; i++)
        {
            var angle = Main.rand.NextFloat(MathHelper.TwoPi);
            var distance = Main.rand.NextFloat(15f, 25f);

            var offset = new Vector2(
                (float)Math.Cos(angle) * distance,
                (float)Math.Sin(angle) * distance
            );

            var dustPos = NPC.Center + offset;
            var velocity = -offset * 0.1f;

            var dustIndex = Dust.NewDust(dustPos, 4, 4, DustID.CorruptTorch, velocity.X, velocity.Y);
            Main.dust[dustIndex].noGravity = true;
            Main.dust[dustIndex].scale = Main.rand.NextFloat(1.2f, 2.0f);
        }

        // Add bright light
        Lighting.AddLight(NPC.Center, 0.7f, 0f, 1.0f);

        // Occasionally play charging pulses
        if (NPC.ai[2] % 5 == 0)
        {
            SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.4f, Pitch = 0.2f }, NPC.position);
        }
    }

    private void FastTeleport()
    {
        var target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest();
            target = Main.player[NPC.target];
        }

        // Always teleport behind for fast teleport
        Vector2 teleportDirection;
        var playerFacingDirection = target.direction;

        if (playerFacingDirection != 0)
        {
            teleportDirection = new Vector2(-playerFacingDirection, 0);
        }
        else
        {
            var mouseWorld = Main.MouseWorld;
            var playerToMouse = mouseWorld - target.Center;

            if (playerToMouse.Length() > 5f)
            {
                playerToMouse.Normalize();
                teleportDirection = -playerToMouse;
            }
            else
            {
                teleportDirection = -(target.Center - NPC.Center);
                teleportDirection.Normalize();
            }
        }

        // Apply a slight Y adjustment with less downward bias
        teleportDirection.Y -= 0.1f;
        teleportDirection.Normalize();

        // Shorter teleport distance for more aggressive positioning
        var teleportDistance = MinTeleportDistance * 0.8f;
        var newPosition = target.Center + teleportDirection * teleportDistance;

        var isValidPosition = !Collision.SolidCollision(
            newPosition - new Vector2(NPC.width / 2, NPC.height / 2),
            NPC.width,
            NPC.height
        );

        if (isValidPosition)
        {
            // More dramatic effect for fast teleport
            SpawnInverseDust(NPC.Center);

            SoundEngine.PlaySound(SoundID.Item8, NPC.position);
            SoundEngine.PlaySound(SoundID.DD2_BetsysWrathImpact, NPC.position);
            NPC.Center = newPosition;

            NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            SpawnInverseDust(NPC.Center);

            // More aggressive velocity after fast teleport
            NPC.velocity.X = target.Center.X > NPC.Center.X ? 4.8f : -4.8f;

            // Reset teleport cooldown
            TeleportCooldown = Main.rand.NextFloat(80f, 140f);
        }
        else
        {
            // Fallback to normal teleport if position is invalid
            Teleport();
        }
    }

    private static void SpawnInverseDust(Vector2 position)
    {
        for (var i = 0; i < 30; i++)
        {
            var velocity = Main.rand.NextVector2Circular(4.0f, 3.0f);
            var dustIndex = Dust.NewDust(position, 24, 24, DustID.Shadowflame, velocity.X, velocity.Y);
            Main.dust[dustIndex].noGravity = true;
            Main.dust[dustIndex].scale = Main.rand.NextFloat(1.2f, 2.0f);
        }
    }

    private void ManageHitboxExtension()
    {
        // If hitbox is extended, count down timer
        if (hitboxExtended)
        {
            hitboxExtensionCounter--;

            // Reset hitbox when timer expires
            if (hitboxExtensionCounter <= 0)
            {
                ResetExtendedHitbox();
            }
        }
    }

    private void ApplyExtendedHitbox()
    {
        // Only apply if not already extended
        if (!hitboxExtended)
        {
            // Store original hitbox if this is first time
            if (originalHitbox.Width == 0)
            {
                originalHitbox = NPC.Hitbox;
                NPC.ai[0] = NPC.damage;
            }

            // Apply damage boost
            NPC.damage = (int)(NPC.damage * DamageMultiplier);

            // Extend hitbox
            var extendedHitbox = NPC.Hitbox;
            extendedHitbox.X -= HitboxExtension / 2;
            extendedHitbox.Width += HitboxExtension;
            NPC.Hitbox = extendedHitbox;

            // Mark as extended and set timer
            hitboxExtended = true;
            hitboxExtensionCounter = SlashHitboxDuration;
        }
    }

    private void ResetExtendedHitbox()
    {
        if (hitboxExtended)
        {
            // Reset damage
            NPC.damage = (int)NPC.ai[0];

            // Reset hitbox
            NPC.Hitbox = originalHitbox;

            // Mark as not extended
            hitboxExtended = false;
        }
    }

    private void TrySlash()
    {
        var target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest();
            target = Main.player[NPC.target];
        }

        if (Vector2.Distance(NPC.Center, target.Center) <= ProximityThreshold)
        {
            NPC.ai[3] = STATE_SLASHING;
            NPC.ai[2] = 0;
            ApplySlash();
            NPC.netUpdate = true;
        }
        // Sometimes decide to retreat if player gets too close
        else if (Vector2.Distance(NPC.Center, target.Center) <= RetreatThreshold && Main.rand.NextFloat() < 0.01f)
        {
            NPC.ai[3] = STATE_RETREAT;
            NPC.ai[2] = 0;
            NPC.netUpdate = true;
        }
    }

    private void ApplySlash()
    {
        // Store original hitbox for reference
        originalHitbox = NPC.Hitbox;
        NPC.ai[0] = NPC.damage;

        SoundEngine.PlaySound(SoundID.Item1, NPC.position);
    }

    private void ResetSlash()
    {
        ResetExtendedHitbox();
    }

    private void PerformSlash()
    {
        if (NPC.ai[2] % 5 == 0)
        {
            if (NPC.ai[2] % 15 == 0)
            {
                var target = Main.player[NPC.target];
                var direction = target.Center - NPC.Center;
                direction.Normalize();

                for (var i = 0; i < 15; i++)
                {
                    var dustVelocity = direction.RotatedBy(
                        Main.rand.NextFloat(-0.3f, 0.3f)) * Main.rand.NextFloat(3f, 7f);
                    var dustPosition = NPC.Center + direction * 20f;
                    var dustIndex = Dust.NewDust(
                        dustPosition,
                        10,
                        10,
                        DustID.CorruptTorch,
                        dustVelocity.X,
                        dustVelocity.Y
                    );
                    Main.dust[dustIndex].noGravity = true;
                    Main.dust[dustIndex].scale = Main.rand.NextFloat(1.2f, 2.0f);
                }
                SoundEngine.PlaySound(SoundID.Item71, NPC.position);
                ApplyExtendedHitbox();
            }
        }
    }

    private void DoChargingFX()
    {
        // Play teleport charging sound at the beginning of charging
        if (NPC.ai[2] == 0)
        {
            SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal, NPC.position); // Initial charging sound
        }

        for (var i = 0; i < 2; i++)
        {
            var angle = Main.rand.NextFloat(MathHelper.TwoPi);
            var distance = Main.rand.NextFloat(20f, 30f);

            var offset = new Vector2(
                (float)Math.Cos(angle) * distance,
                (float)Math.Sin(angle) * distance
            );

            var dustPos = NPC.Center + offset;

            var velocity = -offset * 0.05f;

            var dustIndex = Dust.NewDust(dustPos, 4, 4, DustID.CorruptTorch, velocity.X, velocity.Y);
            Main.dust[dustIndex].noGravity = true;
            Main.dust[dustIndex].scale = Main.rand.NextFloat(1.0f, 1.8f);

            if (NPC.ai[2] > ChargeDuration * 0.5f)
            {
                //Lighting.AddLight(NPC.Center, 0.02f, 0f, 0.04f);

                if (NPC.ai[2] % 10 == 0)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BookStaffCast with { Volume = 0.5f }, NPC.position);
                }
            }
        }
    }

    private void Teleport()
    {
        var oldPosition = NPC.Center;
        var oldDirection = NPC.direction;

        var target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest();
            target = Main.player[NPC.target];
        }

        var distanceToPlayer = Vector2.Distance(NPC.Center, target.Center);
        var longRangeMode = distanceToPlayer > 250f;

        Vector2 teleportDirection;

        if (longRangeMode)
        {
            // When far away, teleport in front of player to cut off escape
            var playerVelocity = target.velocity;

            // If player is moving significantly
            if (playerVelocity.Length() > 2f)
            {
                // Teleport in the direction they're moving
                playerVelocity.Normalize();
                teleportDirection = playerVelocity;
                teleportDirection.Y -= 0.1f; // Slight upward adjustment
            }
            else
            {
                // If player isn't moving much, teleport to their front side
                teleportDirection = new Vector2(target.direction, 0);

                // Fallback if player direction is 0
                if (teleportDirection.X == 0)
                {
                    teleportDirection.X = target.Center.X > NPC.Center.X ? 1 : -1;
                }
            }
        }
        else
        {
            // Close range behavior - teleport behind player as before
            var playerFacingDirection = target.direction;

            if (playerFacingDirection != 0)
            {
                teleportDirection = new Vector2(-playerFacingDirection, 0);
            }
            else
            {
                var mouseWorld = Main.MouseWorld;
                var playerToMouse = mouseWorld - target.Center;

                if (playerToMouse.Length() > 5f)
                {
                    playerToMouse.Normalize();
                    teleportDirection = -playerToMouse;
                }
                else
                {
                    teleportDirection = -(target.Center - NPC.Center);
                    teleportDirection.Normalize();
                }
            }
        }

        // Apply a slight Y adjustment to avoid getting stuck on platforms
        teleportDirection.Y -= 0.2f;
        teleportDirection.Normalize();

        // Adjust teleport distance based on mode
        float teleportDistance;
        if (longRangeMode)
        {
            // Teleport further ahead when cutting off the player
            teleportDistance = Main.rand.NextFloat(MaxTeleportDistance * 0.8f, MaxTeleportDistance * 1.2f);
        }
        else
        {
            teleportDistance = Main.rand.NextFloat(MinTeleportDistance, MaxTeleportDistance);
        }

        var newPosition = target.Center + teleportDirection * teleportDistance;

        var isValidPosition = !Collision.SolidCollision(
            newPosition - new Vector2(NPC.width / 2, NPC.height / 2),
            NPC.width,
            NPC.height
        );

        if (isValidPosition)
        {
            SpawnDust(NPC.Center);

            SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot, NPC.position);
            NPC.Center = newPosition;

            NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;

            SpawnDust(new(NPC.Center.X, NPC.Center.Y / 1.5f));

            // More aggressive velocity when teleporting in front to cut player off
            if (longRangeMode)
            {
                NPC.velocity.X = target.Center.X > NPC.Center.X ? 4f : -4f;
            }
            else
            {
                NPC.velocity.X = target.Center.X > NPC.Center.X ? 3f : -3f;
            }

            TeleportCooldown = Main.rand.NextFloat(120f, 180f);
        }
        else
        {
            // Fallback teleport logic remains the same
            teleportDirection = target.Center - NPC.Center;
            teleportDirection.Normalize();

            var randomAngle = Main.rand.NextFloat(-MathHelper.Pi / 6, MathHelper.Pi / 6);
            teleportDirection = teleportDirection.RotatedBy(randomAngle);

            newPosition = NPC.Center + teleportDirection * teleportDistance;

            isValidPosition = !Collision.SolidCollision(
                newPosition - new Vector2(NPC.width / 2, NPC.height / 2),
                NPC.width,
                NPC.height
            );

            if (isValidPosition)
            {
                SpawnDust(NPC.Center);

                SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot, NPC.position);
                NPC.Center = newPosition;
                NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
                NPC.spriteDirection = NPC.direction;

                SpawnDust(new(NPC.Center.X, NPC.Center.Y / 1.5f));
            }
            else
            {
                NPC.ai[3] = STATE_NORMAL;
            }
        }
    }

    private static void SpawnDust(Vector2 position)
    {
        for (var i = 0; i < 24; i++)
        {
            var velocity = Main.rand.NextVector2Circular(2.3f, 1.4f);
            var dustIndex = Dust.NewDust(position, 20, 20, DustID.CorruptTorch, velocity.X, velocity.Y);
            Main.dust[dustIndex].noGravity = true;
            Main.dust[dustIndex].scale = Main.rand.NextFloat(0.98f, 1.5f);
        }
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        // Only draw the teleport aura during charging states
        if ((int)NPC.ai[3] == STATE_CHARGING || (int)NPC.ai[3] == STATE_FAST_CHARGING)
        {
            // Get the aura texture
            var auraTexture = TextureAssets.Extra[51].Value;

            // Animation parameters (4 frames, each 32x20)
            var frameHeight = 20;
            var frameWidth = 32;
            var totalFrames = 4;

            // Calculate animation frame based on time
            var frameSpeed = (int)NPC.ai[3] == STATE_FAST_CHARGING ? 4 : 6; // Faster animation for fast charging
            var currentFrame = (int)(Main.GameUpdateCount / frameSpeed) % totalFrames;

            // Define the source rectangle for the current frame
            var sourceRectangle = new Rectangle(
                0,                       // X position always starts at 0
                currentFrame * frameHeight, // Y position depends on current frame
                frameWidth,              // Width of a single frame
                frameHeight              // Height of a single frame
            );

            // Calculate position (centered at the bottom of the NPC)
            var position = new Vector2(NPC.Bottom.X, NPC.Bottom.Y + 4f) - screenPos;
            position.Y -= 10; // Adjust vertically to place at feet

            // Calculate the center of the frame for proper positioning
            var origin = new Vector2(frameWidth / 2, frameHeight / 2);

            // Calculate the fade and pulsing effect
            float fadeProgress;
            float scale;
            Color auraColor;

            if ((int)NPC.ai[3] == STATE_CHARGING)
            {
                // Normal teleport charging - slower fade in
                fadeProgress = NPC.ai[2] / ChargeDuration;
                fadeProgress = Math.Min(fadeProgress * 1.5f, 1f); // Accelerate fade-in

                // Pulsing scale effect
                var pulseRate = 0.1f; // Speed of pulse
                scale = 1f + (float)Math.Sin(NPC.ai[2] * pulseRate) * 0.1f;

                // Purple color with fading opacity
                auraColor = new Color(165, 81, 255) * Math.Min(fadeProgress * 0.7f, 0.7f);
            }
            else // Fast charging
            {
                // Fast teleport charging - rapid fade in with more intensity
                fadeProgress = NPC.ai[2] / FastTeleportChargeDuration;
                fadeProgress = Math.Min(fadeProgress * 2.5f, 1f); // Faster fade-in

                // More aggressive pulsing for fast teleport
                var pulseRate = 0.2f; // Faster pulse
                scale = 1f + (float)Math.Sin(NPC.ai[2] * pulseRate) * 0.2f;

                // Brighter purple with higher opacity
                auraColor = new Color(185, 81, 255) * Math.Min(fadeProgress * 0.9f, 0.9f);
            }

            // Draw the aura texture with proper frame selection and additive blending
            for (var i = 0; i < 4; i++)
            {
                spriteBatch.Draw(
                    auraTexture,
                    position,
                    sourceRectangle,
                    auraColor,
                    0f,
                    origin,
                    scale,
                    SpriteEffects.None,
                    0f
                );
            }
        }
    }
}