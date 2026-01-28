using Reverie.Core.Cinematics.Camera;
using Reverie.Core.NPCs.Actors;
using Reverie.Utilities;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;

namespace Reverie.Content.NPCs.Enemies.Assailants;

public class NullHerald : FighterNPCActor
{
    public override float MaxSpeed => 0.2f;

    #region Fields & Properties
    private int HammerSlamCooldown = 120;
    private int SpearThrowCooldown = 180;
    private int HammerWindupDuration = 45;
    private int HammerSlamDuration = 30;
    private int SpearWindupDuration = 30;
    private int SpearThrowDuration = 40;
    private float HammerAttackRange = 100f;
    private float SpearAttackRange = 250f;
    private int HammerSlamDamageMultiplier = 2;
    private int HammerHitboxWidth = 80;
    private int HammerHitboxHeight = 40;

    private const int STATE_CHASE = 0;
    private const int STATE_HAMMER_WINDUP = 1;
    private const int STATE_HAMMER_SLAM = 2;
    private const int STATE_SPEAR_WINDUP = 3;
    private const int STATE_SPEAR_THROW = 4;

    private Rectangle originalHitbox;
    private bool hitboxExtended = false;
    private int attackDamage = 0;

    private int hammerCooldownTimer = 0;
    private int spearCooldownTimer = 0;
    #endregion

    public override void SetStaticDefaults()
    {
        NPCID.Sets.TrailingMode[NPC.type] = 0;
        NPCID.Sets.TrailCacheLength[NPC.type] = 7;
        Main.npcFrameCount[NPC.type] = 14;

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
            new FlavorTextBestiaryInfoElement("Mods.Reverie.NPCs.NullHerald.Bestiary"),
        ]);
    }

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.damage = 9;
        NPC.defense = 4;
        NPC.lifeMax = 70;
        NPC.width = 30;
        NPC.height = 48;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.12f;
        NPC.value = Item.sellPrice(copper: Main.rand.Next(102, 210));
    }

    public override void AI()
    {
        base.AI();
        var target = Main.player[NPC.target];
        NPCUtils.CheckPlatform(NPC, target);
        NPCUtils.HoleBelow(NPC);
        if (target.active && !target.dead)
        {
            NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
        }

        // Decrement cooldown timers
        if (hammerCooldownTimer > 0)
            hammerCooldownTimer--;

        if (spearCooldownTimer > 0)
            spearCooldownTimer--;

        // State machine
        switch ((int)NPC.ai[0])
        {
            case STATE_CHASE:
                HandleChaseState();
                break;

            case STATE_HAMMER_WINDUP:
                HandleHammerWindup();
                break;

            case STATE_HAMMER_SLAM:
                HandleHammerSlam();
                break;

            case STATE_SPEAR_WINDUP:
                HandleSpearWindup();
                break;

            case STATE_SPEAR_THROW:
                HandleSpearThrow();
                break;
        }
    }

    #region AI Functions
    private void HandleChaseState()
    {
        // Run base AI for movement
        base.AI();

        // Find target
        var target = Main.player[NPC.target];
        if (!target.active || target.dead)
        {
            NPC.TargetClosest();
            target = Main.player[NPC.target];
        }

        var distanceToTarget = Vector2.Distance(NPC.Center, target.Center);

        // Check if we should start an attack
        if (hammerCooldownTimer <= 0 && distanceToTarget < HammerAttackRange)
        {
            // Start hammer attack
            NPC.ai[0] = STATE_HAMMER_WINDUP;
            NPC.ai[1] = 0; // Timer reset
            NPC.velocity = Vector2.Zero;
            NPC.netUpdate = true;

            // Store original hitbox and damage
            originalHitbox = NPC.Hitbox;
            attackDamage = NPC.damage;

            // Play windup sound
            SoundEngine.PlaySound(SoundID.Item71, NPC.position);
            return;
        }

        if (spearCooldownTimer <= 0 && distanceToTarget < SpearAttackRange && distanceToTarget > HammerAttackRange * 0.7f)
        {
            // Start spear attack
            NPC.ai[0] = STATE_SPEAR_WINDUP;
            NPC.ai[1] = 0; // Timer reset
            NPC.velocity *= 0.3f; // Slow down but don't stop completely
            NPC.netUpdate = true;

            // Play windup sound
            SoundEngine.PlaySound(SoundID.Item103, NPC.position);
            return;
        }
    }

    private void HandleHammerWindup()
    {
        // Increment timer
        NPC.ai[1]++;

        // Stop movement during windup
        NPC.velocity = Vector2.Zero;

        // Create anticipation dust effect
        if (NPC.ai[1] % 5 == 0)
        {
            for (var i = 0; i < 3; i++)
            {
                var dustPos = NPC.Center + new Vector2(Main.rand.NextFloat(-20, 20), -30 + Main.rand.NextFloat(-10, 10));
                var dustIndex = Dust.NewDust(dustPos, 4, 4, DustID.Smoke, 0f, -2f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].scale = Main.rand.NextFloat(1.0f, 1.5f);
            }
        }

        // When windup is complete, transition to slam
        if (NPC.ai[1] >= HammerWindupDuration)
        {
            NPC.ai[0] = STATE_HAMMER_SLAM;
            NPC.ai[1] = 0; // Reset timer

            // Play slam sound
            SoundEngine.PlaySound(SoundID.Item14, NPC.position);
            NPC.netUpdate = true;
        }
    }

    private void HandleHammerSlam()
    {
        // Increment timer
        NPC.ai[1]++;

        // Keep NPC stationary during slam
        NPC.velocity = Vector2.Zero;

        // If not extended, apply extended hitbox at the start of slam
        if (!hitboxExtended && NPC.ai[1] == 1)
        {
            // Increase damage for slam
            NPC.damage = 20;

            // Extend hitbox forward in facing direction
            var slamHitbox = NPC.Hitbox;
            slamHitbox.Width = HammerHitboxWidth;
            slamHitbox.X = NPC.direction == 1 ? NPC.Hitbox.X : NPC.Hitbox.X - (HammerHitboxWidth - NPC.width);
            slamHitbox.Height = HammerHitboxHeight;
            slamHitbox.Y = NPC.Hitbox.Y + NPC.Hitbox.Height - HammerHitboxHeight;
            NPC.Hitbox = slamHitbox;

            hitboxExtended = true;

            // Create ground slam dust effect
            for (var i = 0; i < 30; i++)
            {
                var dustVel = new Vector2(Main.rand.NextFloat(-5, 5), Main.rand.NextFloat(-7, -2));
                int dustType = Main.rand.NextBool(2)? DustID.Smoke : DustID.Stone;
                var dustPos = NPC.Bottom + new Vector2(Main.rand.NextFloat(-60, 60), -5);

                var dustIndex = Dust.NewDust(dustPos, 4, 4, dustType, dustVel.X, dustVel.Y);
                Main.dust[dustIndex].scale = Main.rand.NextFloat(1.0f, 2.0f);
                Main.dust[dustIndex].noGravity = dustType == DustID.Smoke;
            }
            
            CameraSystem.shake = 5;
        }

        // Halfway through, reset the hitbox
        if (NPC.ai[1] >= HammerSlamDuration / 2 && hitboxExtended)
        {
            NPC.damage = attackDamage;
            NPC.Hitbox = originalHitbox;
            hitboxExtended = false;
        }

        // Attack complete, return to chase state
        if (NPC.ai[1] >= HammerSlamDuration)
        {
            NPC.ai[0] = STATE_CHASE;
            NPC.ai[1] = 0;
            hammerCooldownTimer = HammerSlamCooldown;
            NPC.netUpdate = true;
        }
    }

    private void HandleSpearWindup()
    {
        // Increment timer
        NPC.ai[1]++;

        // Slow movement during windup
        NPC.velocity *= 0.9f;

        // Face the player
        var target = Main.player[NPC.target];
        if (target.active && !target.dead)
        {
            NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
            NPC.spriteDirection = NPC.direction;
        }

        // Create anticipation dust effect
        if (NPC.ai[1] % 5 == 0)
        {
            var dustOffset = new Vector2(NPC.direction * 20, 0);
            for (var i = 0; i < 2; i++)
            {
                var dustPos = NPC.Center + dustOffset + new Vector2(0, Main.rand.NextFloat(-10, 10));
                var dustIndex = Dust.NewDust(dustPos, 4, 4, DustID.Electric, NPC.direction * 2, 0f);
                Main.dust[dustIndex].noGravity = true;
                Main.dust[dustIndex].scale = Main.rand.NextFloat(0.7f, 1.2f);
            }
        }

        // When windup is complete, transition to throw
        if (NPC.ai[1] >= SpearWindupDuration)
        {
            NPC.ai[0] = STATE_SPEAR_THROW;
            NPC.ai[1] = 0; // Reset timer

            // Play throw sound
            SoundEngine.PlaySound(SoundID.Item17, NPC.position);

            // Create projectile (spear)
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var velocity = target.Center - NPC.Center;
                velocity.Normalize();
                velocity *= 11.5f; // Spear speed

                // Shoot the spear
                int spearType = ProjectileID.JavelinHostile; // Substitute with your custom projectile if available
                var damage = NPC.damage / 2;
                var knockback = 3f;

                Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    NPC.Center,
                    velocity,
                    spearType,
                    damage,
                    knockback,
                    Main.myPlayer
                );
            }

            NPC.netUpdate = true;
        }
    }

    private void HandleSpearThrow()
    {
        // Increment timer
        NPC.ai[1]++;

        // Can start moving again slowly
        if (NPC.ai[1] > SpearThrowDuration / 2)
        {
            base.AI();
            NPC.velocity *= 0.5f; // Still slowed movement
        }

        // Attack complete, return to chase state
        if (NPC.ai[1] >= SpearThrowDuration)
        {
            NPC.ai[0] = STATE_CHASE;
            NPC.ai[1] = 0;
            spearCooldownTimer = SpearThrowCooldown;
            NPC.netUpdate = true;
        }
    }
    #endregion

    public override void FindFrame(int frameHeight)
    {
        base.FindFrame(frameHeight);
        NPC.frame.Width = 34;
        NPC.frame.Height = 50;
        frameHeight = NPC.frame.Height;

        NPC.frameCounter++;

        if (NPC.frameCounter >= 8)
        {
            NPC.frameCounter = 0;
            int currentFrame = (NPC.frame.Y / frameHeight);
            currentFrame = (currentFrame + 1) % 14;
            NPC.frame.Y = frameHeight * (currentFrame);
        }

        if (NPC.frame.Y < 0 || NPC.frame.Y > frameHeight * 14)
        {
            NPC.frame.Y = 0;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if ((int)NPC.ai[0] == STATE_HAMMER_SLAM)
        {
            // Draw a trail effect for hammer slam
            var drawOrigin = new Vector2(NPC.width * 0.5f, NPC.height * 0.5f);
            for (var i = 0; i < NPC.oldPos.Length; i++)
            {
                var drawPos = NPC.oldPos[i] - screenPos + drawOrigin + new Vector2(0f, NPC.gfxOffY);
                var trailColor = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - i) / (float)NPC.oldPos.Length);
                trailColor.A = 100;
                spriteBatch.Draw(
                    TextureAssets.Npc[NPC.type].Value,
                    drawPos,
                    null,
                    trailColor,
                    NPC.rotation,
                    drawOrigin,
                    NPC.scale,
                    NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                    0f
                );
            }
        }

        // Draw the aura effect during hammer windup
        if ((int)NPC.ai[0] == STATE_HAMMER_WINDUP && NPC.ai[1] >= HammerWindupDuration / 2)
        {
            // End current batch
            spriteBatch.End();

            // Begin new batch with additive blending
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                SamplerState.AnisotropicClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );

            // Draw the aura glow
            var auraTexture = TextureAssets.Extra[58].Value;
            var progress = (NPC.ai[1] - HammerWindupDuration / 2) / (HammerWindupDuration / 2);
            var pulseScale = 0.8f + (float)Math.Sin(NPC.ai[1] * 0.4f) * 0.1f;
            var auraColor = new Color(255, 50, 20) * Math.Min(progress, 0.7f);

            spriteBatch.Draw(
                auraTexture,
                NPC.Center - screenPos,
                null,
                auraColor,
                0f,
                new Vector2(auraTexture.Width / 2, auraTexture.Height / 2),
                pulseScale,
                SpriteEffects.None,
                0f
            );

            // End additive batch
            spriteBatch.End();

            // Restart batch with original settings
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );
        }

        return true;
    }
}