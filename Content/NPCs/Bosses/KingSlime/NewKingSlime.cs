using Reverie.Utilities;
using Reverie.Core.Cinematics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Reverie.Core.Cinematics.Camera;
using Reverie.Core.Loaders;

namespace Reverie.Content.NPCs.Bosses.KingSlime;

public class KinguSlime : ModNPC
{
    public override string Texture => $"{TEXTURE_DIRECTORY}NPCs/Bosses/KingSlime/KingSlime";

    private AIState State
    {
        get => (AIState)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    private void UpdateScale()
    {
        float healthPercentage = (float)NPC.life / NPC.lifeMax;
        float newScale = MathHelper.Lerp(0.65f, 1.35f, healthPercentage);

        if (newScale < 0.45f)
            newScale = 0.45f;

        if (State == AIState.Teleporting)
            newScale *= teleportScaleMultiplier;

        NPC.scale = newScale;
        UpdateDefenseBasedOnScale();
        UpdateHitbox();
    }

    private void UpdateDefenseBasedOnScale()
    {
        const int MIN_DEFENSE = 12;
        const int MAX_DEFENSE = 25;

        float normalizedScale = (NPC.scale - 0.45f) / (1.35f - 0.45f);
        normalizedScale = MathHelper.Clamp(normalizedScale, 0f, 1f);

        float defenseFactor = normalizedScale * normalizedScale * (3 - 2 * normalizedScale);
        int calculatedDefense = (int)(MIN_DEFENSE + (baseDefense - MIN_DEFENSE) * defenseFactor);

        NPC.defense = (int)MathHelper.Clamp(calculatedDefense, MIN_DEFENSE, MAX_DEFENSE);
    }

    private void UpdateHitbox()
    {
        Vector2 center = NPC.Center;

        NPC.width = (int)(BASE_WIDTH * NPC.scale);
        NPC.height = (int)(BASE_HEIGHT * NPC.scale);

        NPC.position = center - new Vector2(NPC.width / 2, NPC.height / 2);
    }

    private float GetScaledJumpHeight()
    {
        float baseHeight = -8.5f;
        float scaleMultiplier = MathHelper.Lerp(1.4f, 0.8f, NPC.scale / 1.35f);
        return baseHeight * scaleMultiplier;
    }

    private ref float Timer => ref NPC.ai[1];

    internal enum AIState
    {
        Strolling,
        Jumping,
        GroundPound,
        Teleporting,
        Despawning,
        ConsumingSlimes
    }

    internal enum TeleportPhase
    {
        Prepare,
        Execute
    }

    private Vector2 squishScale = Vector2.One;
    private bool wasGliding = false;
    private ref float JumpPhase => ref NPC.ai[2];
    private ref float StrollCount => ref NPC.ai[3];
    private int targetStrolls = 3;

    private float rotationSway = 0f;
    private float wobbleTimer = 0f;
    private bool isWobbling = false;

    private int groundPoundCount = 0;
    private int targetGroundPounds = 1;

    private int baseDefense = 18;
    private const int BASE_WIDTH = 158;
    private const int BASE_HEIGHT = 106;

    private const float CONSUME_DETECTION_RANGE = 200f;
    private const float SCALE_PER_SLIME = 0.05f;
    private const float MAX_CONSUME_SCALE = 2.0f;
    private const float HP_PER_SLIME = 0.08f;

    private const int SLIME_THRESHOLD = 7;
    private const float CONSUME_COOLDOWN = 600f;

    // Teleportation constants
    private const int DESPAWN_DISTANCE = 3000;
    private const float TELEPORT_COOLDOWN = 180f;
    private const int TELEPORT_SEARCH_ATTEMPTS = 100;
    private const int TELEPORT_RADIUS = 20;
    private const int TELEPORT_AVOID_RADIUS = 7;
    private const float LINE_OF_SIGHT_TIMEOUT = 180f;

    private float lastConsumeTime = -CONSUME_COOLDOWN;

    // Teleportation tracking
    private float lineOfSightTimer = 0f;
    private float teleportTimer = 0f;
    private TeleportPhase teleportPhase = TeleportPhase.Prepare;
    private Vector2 teleportDestination;
    private float teleportScaleMultiplier = 1f;
    private bool isInvulnerable = false;

    private Vector2 lastPosition;
    private Vector2 teleportStartPos;
    private float stuckTimer = 0f;
    private const float STUCK_TIMEOUT = 180f;
    private const float MIN_MOVEMENT_THRESHOLD = 16f;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.npcFrameCount[Type] = 6;
    }

    public override void SetDefaults()
    {
        NPC.damage = 12;
        NPC.defense = baseDefense;
        NPC.lifeMax = 1100;
        NPC.width = 158;
        NPC.height = 106;
        NPC.aiStyle = -1;
        NPC.value = Item.buyPrice(gold: 4);
        NPC.boss = true;
        NPC.lavaImmune = true;
        NPC.knockBackResist = 0f;
        if (!Main.dedServ)
            Music = MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}GelatinousJoust");
        NPC.HitSound = new SoundStyle($"{SFX_DIRECTORY}KingSlimeHit") with { PitchVariance = 0.3f };
        NPC.DeathSound = new SoundStyle($"{SFX_DIRECTORY}KingSlimeDeath") with { PitchVariance = 0.2f };
    }

    public override void AI()
    {
        Main.StartSlimeRain();
        Player target = Main.player[NPC.target];

        if (!target.active || target.dead)
        {
            NPC.TargetClosest();
            target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                State = AIState.Despawning;
                return;
            }
        }
        Timer++;
        UpdateScale();
        HandleSquishScale();
        HandleRotation();
        HandleDustTrail();
        HandleSlimeTrail();

        UpdateLineOfSightTracking(target);
        CheckTeleportConditions(target);

        switch (State)
        {
            case AIState.Strolling:
                DoStrolling(target);
                break;
            case AIState.Jumping:
                DoJumping(target);
                break;
            case AIState.GroundPound:
                DoGroundPound(target);
                break;
            case AIState.Teleporting:
                DoTeleporting(target);
                break;
            case AIState.ConsumingSlimes:
                DoConsumingSlimes(target);
                break;
        }
        NPCUtils.SlopedCollision(NPC);
        NPCUtils.CheckPlatform(NPC, target);
    }

    private void DoStrolling(Player target)
    {
        if (Timer == 1)
        {
            targetStrolls = Main.rand.NextBool() ? 3 : 6;
        }

        float distToPlayer = Vector2.Distance(NPC.Center, target.Center);
        float dirToPlayer = Math.Sign(target.Center.X - NPC.Center.X);
        float strollDuration = 54f;
        float progress = Timer / strollDuration;

        float easedProgress = EaseFunction.EaseCubicInOut.Ease(progress);
        float velocityCurve = (float)Math.Sin(easedProgress * Math.PI);
        float targetSpeed = velocityCurve * (float)MathHelper.PiOver2 * 1.25f;

        float glideStrength = 0.92f;
        float accelStrength = 0.112f;

        NPC.velocity.X = NPC.velocity.X * glideStrength + targetSpeed * dirToPlayer * accelStrength;

        float maxVel = 5f;
        if (Math.Abs(NPC.velocity.X) > maxVel)
            NPC.velocity.X = maxVel * Math.Sign(NPC.velocity.X);

        if (Math.Abs(NPC.velocity.X) > 0.1f)
            NPC.spriteDirection = -Math.Sign(NPC.velocity.X);

        if (Timer >= strollDuration)
        {
            StrollCount++;

            if (StrollCount >= targetStrolls)
            {
                // Check for consumption opportunity first
                if (ShouldConsumeSlime())
                {
                    State = AIState.ConsumingSlimes;
                    Timer = 0;
                    StrollCount = 0;
                    return;
                }

                // Original attack selection logic
                float attackRoll = Main.rand.NextFloat();

                if (attackRoll < 0.6f)
                {
                    State = AIState.Jumping;
                    JumpPhase = 0;
                }
                else if (attackRoll < 0.55f)
                {
                    State = AIState.GroundPound;
                    JumpPhase = 0;
                    targetGroundPounds = 1;
                    groundPoundCount = 0;
                }
                else
                {
                    State = AIState.GroundPound;
                    JumpPhase = 0;
                    targetGroundPounds = 3;
                    groundPoundCount = 0;
                }
                StrollCount = 0;
            }
            Timer = 0;
        }
    }

    private void DoJumping(Player target)
    {
        if (NPC.velocity.Y == 0f)
        {
            NPC.velocity.X *= 0.5f;
            if (Math.Abs(NPC.velocity.X) < 0.1f)
                NPC.velocity.X = 0f;
            if (Timer >= 40f)
            {
                NPC.netUpdate = true;
                float dirToPlayer = Math.Sign(target.Center.X - NPC.Center.X);
                NPC.spriteDirection = -Math.Sign(dirToPlayer);
                float baseJumpHeight = GetScaledJumpHeight();
                float baseXVelocity = 2.15f;
                float overshootMod = Main.rand.NextFloat() < 0.33f ? 1.3f : 1f;
                NPC.velocity.Y = baseJumpHeight;
                NPC.velocity.X = baseXVelocity * dirToPlayer * overshootMod;
                squishScale = new Vector2(0.8f, 1.2f);
                JumpPhase = 1;
                Timer = 0;
            }
        }
        else
        {
            float dirToPlayer = Math.Sign(target.Center.X - NPC.Center.X);
            float maxSpeed = 4f;
            if ((dirToPlayer == 1 && NPC.velocity.X < maxSpeed) ||
                (dirToPlayer == -1 && NPC.velocity.X > -maxSpeed))
            {
                NPC.velocity.X += 0.15f * dirToPlayer;
            }

            if (Timer > 30 && NPC.velocity.Y > 0)
            {
                State = AIState.Strolling;
                JumpPhase = 0;
                Timer = 0;
                // Start wobble after landing
                isWobbling = true;
                wobbleTimer = 0f;
            }
        }
    }

    private void DoGroundPound(Player target)
    {
        float dirToPlayer = Math.Sign(target.Center.X - NPC.Center.X);

        switch (JumpPhase)
        {
            case 0: // Charging phase
                NPC.velocity.X *= 0.5f;
                if (Math.Abs(NPC.velocity.X) < 0.1f)
                    NPC.velocity.X = 0f;

                // Handle combo delay timing - rapid combo attacks
                float chargeTime;
                if (targetGroundPounds > 1 && groundPoundCount > 0)
                    chargeTime = 25f; // Very fast for combo follow-ups
                else if (Timer < 0)
                    chargeTime = 45f; // Normal follow-up
                else
                    chargeTime = 60f; // Initial attack

                if (Timer >= chargeTime)
                {
                    NPC.netUpdate = true;
                    NPC.spriteDirection = -Math.Sign(dirToPlayer);
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeSlamCharge") with { PitchVariance = 0.2f }, NPC.Center);

                    NPC.velocity.Y = -14f;
                    NPC.velocity.X = 2f * dirToPlayer;

                    for (int i = 0; i < 10; i++)
                    {
                        Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                            DustID.t_Slime, NPC.velocity.X * 0.4f, NPC.velocity.Y * 0.4f,
                            150, new Color(86, 162, 255, 100), 1.5f);
                        dust.noGravity = true;
                        dust.velocity *= 0.4f;
                    }

                    JumpPhase = 1;
                    Timer = 0;
                }
                break;

            case 1: // Airborne
                NPC.damage = 30;
                float maxSpeed = 3f;
                if ((dirToPlayer == 1 && NPC.velocity.X < maxSpeed) ||
                    (dirToPlayer == -1 && NPC.velocity.X > -maxSpeed))
                {
                    NPC.velocity.X += 0.1f * dirToPlayer;
                }

                // Update sprite direction to face movement direction
                if (Math.Abs(NPC.velocity.X) > 0.1f)
                    NPC.spriteDirection = -Math.Sign(NPC.velocity.X);

                if (NPC.velocity.Y > 0f)
                {
                    NPC.velocity = new Vector2(
                        target.Center.X > NPC.Center.X ? 2f : -2f,
                        12f
                    );
                    // Update sprite direction for slam direction
                    NPC.spriteDirection = target.Center.X > NPC.Center.X ? -1 : 1;
                    JumpPhase = 2;
                    Timer = 0;
                }
                break;

            case 2: // Slamming down
                dirToPlayer = Math.Sign(target.Center.X - NPC.Center.X);
                if (Timer % 10 == 0)
                    NPC.spriteDirection = -Math.Sign(dirToPlayer);

                bool hitGround = false;
                int tileX = (int)(NPC.position.X / 16);
                int tileEndX = (int)((NPC.position.X + NPC.width) / 16);
                int tileY = (int)((NPC.position.Y + NPC.height) / 16);

                for (int i = tileX; i <= tileEndX; i++)
                {
                    Tile tile = Framing.GetTileSafely(i, tileY);
                    if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    {
                        hitGround = true;
                        break;
                    }
                }

                if (hitGround || NPC.velocity.Y == 0f)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeSlam") with { PitchVariance = 0.2f }, NPC.Center);
                    CameraSystem.shake = 6;
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 dustVel = Vector2.One.RotatedBy(MathHelper.ToRadians(i * 12)) * 8f;
                        Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.t_Slime, dustVel.X, dustVel.Y,
                            newColor: new Color(86, 162, 255, 100));
                        dust.noGravity = true;
                        dust.scale = 2f;
                    }

                    // Calculate distance to player in tiles
                    float distToPlayer = Vector2.Distance(NPC.Center, target.Center) / 16f;

                    // Scale projectile count and velocity based on distance
                    int gelBallCount;
                    float baseVelocity;
                    float velocityVariance;

                    if (distToPlayer <= 20f) // Close range
                    {
                        gelBallCount = 8;
                        baseVelocity = 5.5f;
                        velocityVariance = 1.6f;
                    }
                    else if (distToPlayer <= 42f)
                    {
                        gelBallCount = 12;
                        baseVelocity = 7f;
                        velocityVariance = 2.1f;
                    }
                    else
                    {
                        gelBallCount = 16;
                        baseVelocity = 9f;
                        velocityVariance = 3f;
                    }

                    for (int i = 0; i < gelBallCount; i++)
                    {
                        float angle = MathHelper.Lerp(-MathHelper.PiOver2 - 0.8f, -MathHelper.PiOver2 + 0.8f, (float)i / (gelBallCount - 1));
                        Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) *
                                          (baseVelocity + Main.rand.NextFloat(-velocityVariance, velocityVariance));

                        Vector2 spawnPos = NPC.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), 0f);
                        int projType = ModContent.ProjectileType<GelBallProjectile>();
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity, projType, 3, 0.5f);
                    }

                    if (targetGroundPounds > 1 && Main.rand.NextBool(2))
                    {
                        Vector2 spawnPos = NPC.Center + new Vector2(0f, NPC.height / 2f);
                        int leftSlime = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X - 20, (int)spawnPos.Y, NPCID.BlueSlime);
                        if (leftSlime < Main.maxNPCs)
                        {
                            Main.npc[leftSlime].velocity = new Vector2(-4f + Main.rand.NextFloat(-1f, 0f),
                                                                       -3f + Main.rand.NextFloat(-1f, 1f));
                            Main.npc[leftSlime].scale = 1.2f;
                        }
                        int rightSlime = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X + 20, (int)spawnPos.Y, NPCID.GreenSlime);
                        if (rightSlime < Main.maxNPCs)
                        {
                            Main.npc[rightSlime].scale = 1.2f;
                        }
                    }
                    JumpPhase = 3;
                    Timer = 0;
                    isWobbling = true;
                    wobbleTimer = 0f;
                }
                else
                {
                    // Gliding acceleration
                    float timeInPhase = Timer;
                    float accelCurve = MathHelper.Clamp(timeInPhase / 20f, 0.1f, 1f);
                    float baseAccel = 0.25f;
                    float currentAccel = baseAccel + (accelCurve * 0.4f);

                    NPC.velocity.Y += currentAccel;

                    if (NPC.velocity.Y > 14f)
                        NPC.velocity.Y = 14f;

                    if (Main.rand.NextBool(3))
                    {
                        Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                            DustID.t_Slime, NPC.velocity.X * 0.4f, NPC.velocity.Y * 0.4f,
                            150, new Color(86, 162, 255, 100), 1.5f);
                        dust.noGravity = true;
                        dust.velocity *= 0.4f;
                    }
                }
                break;

            case 3: // Recovery with wobble
                NPC.damage = 12;
                NPC.velocity *= 0.8f;

                // Shorter recovery time for combo attacks
                float recoveryTime = targetGroundPounds > 1 ? 45f : 90f;

                if (Timer >= recoveryTime)
                {
                    groundPoundCount++;

                    if (groundPoundCount >= targetGroundPounds)
                    {
                        State = AIState.Strolling;
                        JumpPhase = 0;
                        Timer = 0;
                        isWobbling = false;
                        groundPoundCount = 0;
                    }
                    else
                    {
                        JumpPhase = 0;
                        Timer = 0;
                        isWobbling = false;

                        // Brief pause between combo attacks
                        Timer = -15f; // Negative timer for short delay
                    }
                }
                break;
        }
    }

    private void DoTeleporting(Player target)
    {
        switch (teleportPhase)
        {
            case TeleportPhase.Prepare:
                NPC.aiAction = 1;

                // Fade out effect (first 20 frames)
                if (Timer <= 20f)
                {
                    float fadeProgress = MathHelper.Clamp((20f - Timer) / 20f, 0f, 1f);
                    teleportScaleMultiplier = 0.5f + fadeProgress * 0.5f;

                    if (Timer >= 20f)
                    {
                        isInvulnerable = true;
                        NPC.dontTakeDamage = true;
                        NPC.hide = true;
                    }

                    // Gore effect when becoming invisible
                    if (Timer == 20f)
                    {
                        Gore.NewGore(NPC.GetSource_FromAI(),
                                    NPC.Center + new Vector2(-40f, -NPC.height / 2),
                                    NPC.velocity, 734);
                    }
                }
                // Transition phase (frames 20-35) - snap to destination quickly
                else if (Timer <= 35f)
                {
                    float transitionProgress = (Timer - 20f) / 15f; // 0 to 1 over 15 frames

                    if (Timer == 21f)
                    {
                        teleportStartPos = NPC.Bottom;
                    }

                    NPC.Bottom = Vector2.Lerp(teleportStartPos, teleportDestination,
                        EaseFunction.EaseQuadOut.Ease(transitionProgress)); // Faster easing

                    NPC.hide = true;
                    isInvulnerable = true;
                    NPC.dontTakeDamage = true;
                    teleportScaleMultiplier = 0.5f;
                }
                // Transition complete, start fade-in
                else if (Timer >= 35f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    teleportPhase = TeleportPhase.Execute;
                    Timer = 0;
                    NPC.netUpdate = true;
                }

                CreateTeleportDust(1f); // More intense dust
                break;

            case TeleportPhase.Execute:
                NPC.aiAction = 0;

                // Wind-up phase (frames 0-10) - very brief anticipation
                if (Timer <= 10f)
                {
                    NPC.hide = true;
                    isInvulnerable = true;
                    NPC.dontTakeDamage = true;
                    teleportScaleMultiplier = 0.5f;

                    float windUpProgress = Timer / 10f;
                    float dustIntensity = 1f + windUpProgress * 2f; // More intense buildup
                    CreateTeleportDust(dustIntensity);

                    if (Timer == 5f) // Quick sound effect
                    {
                        SoundEngine.PlaySound(SoundID.QueenSlime with { PitchVariance = 0.2f }, NPC.Center);
                    }

                    if (Timer > 5f && Timer % 2 == 0) // Rapid shake
                    {
                        CameraSystem.shake = (int)(3 + windUpProgress * 4);
                    }
                }
                // Fade-in phase (frames 10-25) - rapid emergence
                else if (Timer <= 25f)
                {
                    float fadeInProgress = MathHelper.Clamp((Timer - 10f) / 15f, 0f, 1f);
                    teleportScaleMultiplier = 0.5f + fadeInProgress * 0.5f;

                    if (fadeInProgress >= 0.1f) // Show slime almost immediately
                    {
                        NPC.hide = false;
                        isInvulnerable = false;
                        NPC.dontTakeDamage = false;
                    }
                    else
                    {
                        NPC.hide = true;
                        isInvulnerable = true;
                        NPC.dontTakeDamage = true;
                    }

                    CreateTeleportDust(3f - fadeInProgress * 2f); // Intense dust that fades
                }
                // Teleport complete - back to action!
                else if (Timer >= 25f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    State = AIState.Strolling;
                    Timer = 0;
                    NPC.netUpdate = true;
                    NPC.TargetClosest();
                    NPC.hide = false;
                    isInvulnerable = false;
                    NPC.dontTakeDamage = false;
                    teleportScaleMultiplier = 1f;
                    teleportPhase = TeleportPhase.Prepare;
                    lastPosition = NPC.Center;
                }
                break;
        }
    }

    private void DoConsumingSlimes(Player target)
    {
        if (Timer == 1)
        {
            CameraSystem.shake = 15;
            SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}KingSlimeRoar") with { Volume = 0.5f, }, NPC.Center);

            for (int i = 0; i < 20; i++)
            {
                Vector2 dustVel = Vector2.One.RotatedBy(MathHelper.ToRadians(i * 18)) * 5f;
                Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.t_Slime, dustVel.X, dustVel.Y,
                    newColor: new Color(78, 136, 255, 150));
                dust.noGravity = true;
                dust.scale = 1.8f;
            }
        }

        if (Timer <= 90f && Timer % 30 == 0 && Timer > 1)
            CameraSystem.shake = (int)MathHelper.Lerp(8, 3, Timer / 90f);

        NPC.velocity *= 0.85f;

        if (Timer % 15 == 0)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC slime = Main.npc[i];

                if (slime.active && slime.aiStyle == NPCAIStyleID.Slime
                    && slime.type != NPCID.KingSlime && !slime.boss)
                {
                    float distance = Vector2.Distance(NPC.Center, slime.Center);

                    if (distance <= CONSUME_DETECTION_RANGE)
                    {
                        if (NPC.Hitbox.Intersects(slime.Hitbox))
                        {
                            ConsumeSlime(slime);
                        }
                    }
                }
            }

            if (Timer % 30 == 0)
            {
                int healAmount = (int)(NPC.lifeMax * 0.02f);
                NPC.life += healAmount;
                NPC.HealEffect(healAmount);

                if (NPC.life > NPC.lifeMax)
                    NPC.life = NPC.lifeMax;
            }
        }

        float pulse = (float)Math.Sin(Timer * 0.05f) * 0.1f;
        squishScale = new Vector2(1f + pulse, 1f - pulse);

        if (Main.rand.NextBool(10))
        {
            Vector2 offset = new Vector2(Main.rand.NextFloat(-NPC.width * 0.5f, NPC.width * 0.5f),
                Main.rand.NextFloat(-NPC.height * 0.5f, NPC.height * 0.5f));
            Vector2 position = NPC.Center + offset;

            Dust dust = Dust.NewDustDirect(position, 0, 0, DustID.t_Slime, 0f, 0f,
                150, new Color(78, 136, 255, 150), 1.3f);
            dust.noGravity = true;
            dust.velocity = Vector2.Zero;
            dust.fadeIn = 1.2f;
        }

        if (Timer >= 450f || !HasNearbySlimes())
        {
            lastConsumeTime = Main.GameUpdateCount;
            State = AIState.Strolling;
            Timer = 0;
            isWobbling = true;
            wobbleTimer = 0f;
        }
    }

    public override void FindFrame(int frameHeight)
    {
        if (State == AIState.Jumping)
        {
            if (NPC.velocity.Y == 0f) // Charging jump
            {
                float animSpeed = 4f;
                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y >= frameHeight * 4)
                    {
                        SoundEngine.PlaySound(SoundID.QueenSlime with { Volume = 0.25f }, NPC.Center);
                        NPC.frame.Y = frameHeight;
                    }
                }
            }
            else // Airborne
            {
                NPC.frame.Y = frameHeight * 5;
            }
        }
        else if (State == AIState.GroundPound)
        {
            if (JumpPhase == 0) // Charging ground pound
            {
                float animSpeed = 2.5f;
                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y >= frameHeight * 4)
                    {
                        SoundEngine.PlaySound(SoundID.QueenSlime with { Volume = 0.25f }, NPC.Center);
                        NPC.frame.Y = frameHeight;
                    }
                }
            }
            else if (JumpPhase == 3) // Recovery wobble phase
            {
                float animSpeed = 3f; // Wobble animation speed
                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    // Loop through frames 0-2 for wobble
                    if (NPC.frame.Y > frameHeight * 2)
                        NPC.frame.Y = 0;
                }
            }
            else // Airborne/slamming
            {
                NPC.frame.Y = frameHeight * 5;
            }
        }
        else if (State == AIState.Teleporting)
        {
            if (teleportPhase == TeleportPhase.Prepare)
            {
                // Charging animation during prepare phase
                float animSpeed = 4f;
                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y >= frameHeight * 4)
                        NPC.frame.Y = frameHeight;
                }
            }
            else // Execute phase
            {
                // Static frame during teleport execution
                NPC.frame.Y = frameHeight * 4;
            }
        }
        else // Strolling
        {
            float speed = Math.Abs(NPC.velocity.X);
            float animSpeed;
            bool isGliding = speed < 0.3f;

            if (isGliding)
            {
                if (!wasGliding)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}KingSlimeGlide") with { Pitch = -0.1f, Volume = 0.35f }, NPC.Center);
                }

                animSpeed = 60f;
                NPC.frame.Y = frameHeight * 4;
            }
            else if (speed < 1f)
            {
                if (NPC.frame.Y == frameHeight)
                    NPC.frame.Y = frameHeight;

                animSpeed = MathHelper.Lerp(12f, 6f, speed);

                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y >= frameHeight * 4)
                        NPC.frame.Y = frameHeight;
                }
            }
            else
            {
                animSpeed = MathHelper.Lerp(6f, 3f, Math.Min(speed / 3f, 1f));

                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y >= frameHeight * 4)
                        NPC.frame.Y = 0;
                }
            }

            wasGliding = isGliding;
        }
    }

    #region Helper Methods

    private void UpdateLineOfSightTracking(Player target)
    {
        bool hasLineOfSight = Collision.CanHitLine(NPC.Center, 0, 0, target.Center, 0, 0);
        bool heightDifferenceOk = Math.Abs(NPC.Top.Y - target.Bottom.Y) <= 160f;

        if (!hasLineOfSight || !heightDifferenceOk)
        {
            teleportTimer += 3.5f; // Accumulate faster when no line of sight
            if (Main.netMode != NetmodeID.MultiplayerClient)
                lineOfSightTimer += 2.5f; // Accumulate faster
        }
        else if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            lineOfSightTimer--;
            if (lineOfSightTimer < 0f)
                lineOfSightTimer = 0f;
        }

        // Track if slime is stuck
        float distanceMoved = Vector2.Distance(NPC.Center, lastPosition);
        if (distanceMoved < MIN_MOVEMENT_THRESHOLD && NPC.velocity.Y == 0f)
        {
            stuckTimer++;
        }
        else
        {
            stuckTimer = 0f;
            lastPosition = NPC.Center;
        }
    }

    private void CheckTeleportConditions(Player target)
    {
        // Don't interrupt existing important states
        if (State == AIState.Teleporting || State == AIState.ConsumingSlimes || State == AIState.Despawning)
            return;

        float distToPlayer = Vector2.Distance(NPC.Center, target.Center);
        bool targetTooFar = target.dead || distToPlayer > DESPAWN_DISTANCE;

        if (targetTooFar)
        {
            NPC.TargetClosest();
            target = Main.player[NPC.target];

            if (target.dead || Vector2.Distance(NPC.Center, target.Center) > DESPAWN_DISTANCE)
            {
                BeginTeleport(target);
                return;
            }
        }

        bool shouldTeleport = false;

        if (teleportTimer >= TELEPORT_COOLDOWN && NPC.velocity.Y == 0f && State != AIState.GroundPound)
        {
            shouldTeleport = true;
        }

        if (stuckTimer >= STUCK_TIMEOUT && NPC.velocity.Y == 0f)
        {
            shouldTeleport = true;
        }

        if (distToPlayer > 600f && teleportTimer >= TELEPORT_COOLDOWN * 0.5f && NPC.velocity.Y == 0f)
        {
            shouldTeleport = true;
        }

        if (!target.dead && NPC.timeLeft > 10 && shouldTeleport)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                BeginTeleport(target);
            }
        }
    }

    private void BeginTeleport(Player target)
    {
        FindTeleportLocation(target);
        State = AIState.Teleporting;
        teleportPhase = TeleportPhase.Prepare;
        Timer = 0;
        teleportTimer = 0f;
        stuckTimer = 0f;
        NPC.netUpdate = true;
    }

    private void FindTeleportLocation(Player target)
    {
        Point npcTile = NPC.Center.ToTileCoordinates();
        Point targetTile = target.Center.ToTileCoordinates();
        Vector2 toTarget = target.Center - NPC.Center;

        bool foundSpot = false;
        int attempts = 0;
        bool forceRandomSpot = lineOfSightTimer >= LINE_OF_SIGHT_TIMEOUT || toTarget.Length() > 2000f;

        if (forceRandomSpot && lineOfSightTimer >= LINE_OF_SIGHT_TIMEOUT)
            lineOfSightTimer = LINE_OF_SIGHT_TIMEOUT;

        Vector2 playerVel = target.velocity;
        bool playerIsMoving = Math.Abs(playerVel.X) > 0.5f;

        int teleportSide = 0; // 0 = random, -1 = left of player, 1 = right of player
        if (playerIsMoving)
        {
            // If player moving left, teleport to their left (behind them)
            // If player moving right, teleport to their right (behind them)
            teleportSide = Math.Sign(playerVel.X);
        }

        while (!foundSpot && attempts < TELEPORT_SEARCH_ATTEMPTS)
        {
            attempts++;
            int testX, testY;

            if (teleportSide != 0 && attempts < TELEPORT_SEARCH_ATTEMPTS * 0.8f)
            {
                float behindDistance = Main.rand.NextFloat(10f, 20f);
                testX = targetTile.X + (int)(teleportSide * behindDistance);
                testY = targetTile.Y;
            }
            else
            {
                testX = Main.rand.Next(targetTile.X - TELEPORT_RADIUS, targetTile.X + TELEPORT_RADIUS + 1);
                testY = Main.rand.Next(targetTile.Y - TELEPORT_RADIUS, targetTile.Y + 1);
            }

            bool tooCloseToTarget = testY >= targetTile.Y - TELEPORT_AVOID_RADIUS &&
                                   testY <= targetTile.Y + TELEPORT_AVOID_RADIUS &&
                                   testX >= targetTile.X - TELEPORT_AVOID_RADIUS &&
                                   testX <= targetTile.X + TELEPORT_AVOID_RADIUS;

            bool tooCloseToNPC = testY >= npcTile.Y && testY <= npcTile.Y &&
                                 testX >= npcTile.X && testX <= npcTile.X;

            if (tooCloseToTarget || tooCloseToNPC || Main.tile[testX, testY].HasTile)
                continue;

            int groundY = testY;
            int dropDistance = 0;

            if (Main.tile[testX, groundY].HasTile &&
                Main.tileSolid[Main.tile[testX, groundY].TileType] &&
                !Main.tileSolidTop[Main.tile[testX, groundY].TileType])
            {
                dropDistance = 1;
            }
            else
            {
                for (; dropDistance < 150 && groundY + dropDistance < Main.maxTilesY; dropDistance++)
                {
                    int checkY = groundY + dropDistance;
                    if (Main.tile[testX, checkY].HasTile &&
                        Main.tileSolid[Main.tile[testX, checkY].TileType] &&
                        !Main.tileSolidTop[Main.tile[testX, checkY].TileType])
                    {
                        dropDistance--;
                        break;
                    }
                }
            }

            testY += dropDistance;
            Vector2 teleportPos = new Vector2(testX * 16 + 8, testY * 16 + 16);

            bool validSpot = true;

            if (validSpot && Main.tile[testX, testY].LiquidType == LiquidID.Lava)
                validSpot = false;

            if (validSpot && !Collision.CanHitLine(teleportPos, 0, 0, target.Center, 0, 0))
                validSpot = false;

            if (validSpot)
            {
                teleportDestination = teleportPos;
                foundSpot = true;
                break;
            }
        }

        if (attempts >= TELEPORT_SEARCH_ATTEMPTS)
        {
            teleportDestination = target.Bottom;
        }
    }

    private void CreateTeleportDust(float velocityMultiplier)
    {
        if (isInvulnerable) return;

        for (int i = 0; i < 10; i++)
        {
            int dustIndex = Dust.NewDust(
                NPC.position + Vector2.UnitX * -20f,
                NPC.width + 40,
                NPC.height,
                DustID.t_Slime,
                NPC.velocity.X,
                NPC.velocity.Y,
                150,
                new Color(86, 162, 255, 100),
                2f
            );

            Main.dust[dustIndex].noGravity = true;
            Main.dust[dustIndex].velocity *= velocityMultiplier;
        }
    }

    private void HandleRotation()
    {
        bool isAirborne = NPC.velocity.Y != 0f;
        if (isAirborne)
        {
            float fallSpeed = Math.Max(NPC.velocity.Y, 0f);
            float swayIntensity = fallSpeed * 0.005f;

            if (State == AIState.GroundPound && (JumpPhase == 1 || JumpPhase == 2))
            {
                float groundPoundIntensity = Math.Abs(NPC.velocity.Y) * 0.006f;
                swayIntensity = Math.Max(swayIntensity, groundPoundIntensity * 1.2f);
            }

            // Sinusoidal sway
            float swayFreq = 0.12f + Math.Abs(NPC.velocity.Y) * 0.009f;
            float sway = (float)Math.Sin(Timer * swayFreq) * swayIntensity * 2.25f;
            rotationSway = MathHelper.Lerp(rotationSway, sway, 0.08f);
        }
        else
        {
            rotationSway = MathHelper.Lerp(rotationSway, 0f, 0.15f);
        }
        NPC.rotation = rotationSway;
    }

    private void HandleSquishScale()
    {
        bool isAirborne = NPC.velocity.Y != 0f;
        if (isAirborne)
        {
            float yVel = NPC.velocity.Y;
            float stretchIntensity = Math.Abs(yVel) * 0.06f;

            if (State == AIState.GroundPound && JumpPhase == 2)
                stretchIntensity *= 0.75f;

            if (State == AIState.Jumping)
                stretchIntensity *= 0.55f;

            stretchIntensity = MathHelper.Clamp(stretchIntensity, 0f, 0.33f);
            if (yVel < 0) // Rising - stretch top
            {
                squishScale.Y = 1f + stretchIntensity;
                squishScale.X = 1f - stretchIntensity * 0.18f;
            }
            else // Falling - stretch bottom
            {
                squishScale.Y = 1f + stretchIntensity;
                squishScale.X = 1f - stretchIntensity * 0.25f;
            }
        }
        else if (isWobbling)
        {
            // Gelatin wobble effect after landing
            wobbleTimer += 0.15f;
            float wobbleIntensity = (State == AIState.GroundPound && JumpPhase == 3) ? 0.06f : 0.085f;

            float dampening = MathHelper.Clamp(1f - (wobbleTimer * 0.05f), 0.1f, 1f);
            wobbleIntensity *= dampening;
            float wobbleY = (float)Math.Sin(wobbleTimer * 1.2f) * wobbleIntensity;
            float wobbleX = (float)Math.Sin(wobbleTimer * 0.8f) * wobbleIntensity;
            squishScale.Y = 1f + wobbleY;
            squishScale.X = 1f + wobbleX;

            // Stop wobbling when intensity is low
            if (dampening <= 0.3f)
                isWobbling = false;
        }
        else
        {
            squishScale = Vector2.Lerp(squishScale, Vector2.One, 0.15f);
        }
        if (Math.Abs(NPC.velocity.X) > 0.5f && !isAirborne)
        {
            float horizontalSquish = 0.995f;
            squishScale.Y *= horizontalSquish;
            squishScale.X *= 1.001f;
        }
    }

    private void HandleDustTrail()
    {
        if (Math.Abs(NPC.velocity.X) > 0.3f && Math.Abs(NPC.velocity.Y) < 2f)
        {
            int tileX = (int)(NPC.Center.X / 16f);
            int tileY = (int)((NPC.position.Y + NPC.height + 4) / 16f);

            if (WorldGen.InWorld(tileX, tileY))
            {
                Tile tile = Framing.GetTileSafely(tileX, tileY);

                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    float dustChance = Math.Abs(NPC.velocity.X) * 0.4f;

                    if (Main.rand.NextFloat() < dustChance)
                    {
                        Vector2 dustPos = new Vector2(
                            NPC.position.X + Main.rand.Next(NPC.width),
                            NPC.position.Y + NPC.height - 8
                        );

                        Dust dust = Dust.NewDustDirect(dustPos, 8, 8, DustID.t_Slime);
                        dust.velocity.X = NPC.velocity.X * 0.3f + Main.rand.NextFloat(-1f, 1f);
                        dust.velocity.Y = Main.rand.NextFloat(-1f, 0.5f);
                        dust.color = new Color(86, 162, 255, 100);
                        dust.scale = Main.rand.NextFloat(0.8f, 1.3f);
                        dust.alpha = Main.rand.Next(50, 150);
                    }
                }
            }
        }
    }

    private void HandleSlimeTrail()
    {
        if (Math.Abs(NPC.velocity.X) > 0.3f && Math.Abs(NPC.velocity.Y) < 2f)
        {
            int tileX = (int)(NPC.Center.X / 16f);
            int tileY = (int)((NPC.position.Y + NPC.height + 2) / 16f);

            if (WorldGen.InWorld(tileX, tileY))
            {
                Tile tile = Framing.GetTileSafely(tileX, tileY);

                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    float slimeChance = Math.Abs(NPC.velocity.X) * 0.6f;

                    if (Main.rand.NextFloat() < slimeChance)
                    {
                        SlimedTileSystem.AddSlimedTile(tileX, tileY);

                        // wider trail
                        if (Main.rand.NextBool(3))
                        {
                            int offsetX = Main.rand.NextBool() ? -3 : 3;
                            if (WorldGen.InWorld(tileX + offsetX, tileY))
                            {
                                Tile adjacentTile = Framing.GetTileSafely(tileX + offsetX, tileY);
                                if (adjacentTile.HasTile && Main.tileSolid[adjacentTile.TileType])
                                    SlimedTileSystem.AddSlimedTile(tileX + offsetX, tileY);
                            }
                        }
                    }
                }
            }
        }
    }

    private void ConsumeSlime(NPC slime)
    {
        // Visual effects
        for (int d = 0; d < 15; d++)
        {
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
            Dust dust = Dust.NewDustDirect(slime.Center, 0, 0, DustID.t_Slime, velocity.X, velocity.Y,
                150, new Color(78, 136, 255, 200), 1.5f);
            dust.noGravity = true;
        }

        // Increase scale and HP
        float healthGain = slime.life * HP_PER_SLIME;
        float newScale = NPC.scale + SCALE_PER_SLIME;
        NPC.scale = MathHelper.Min(newScale, MAX_CONSUME_SCALE);

        NPC.life += (int)healthGain;
        NPC.HealEffect((int)healthGain);

        if (NPC.life > NPC.lifeMax)
            NPC.life = NPC.lifeMax;

        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeConsume")
        {
            PitchVariance = 0.3f,
            MaxInstances = 3
        }, slime.Center);

        // Kill the slime
        slime.life = 0;
        slime.HitEffect();
        slime.active = false;

        UpdateHitbox();
    }

    private bool HasNearbySlimes()
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC slime = Main.npc[i];
            if (slime.active && slime.aiStyle == NPCAIStyleID.Slime
                && slime.type != NPCID.KingSlime && !slime.boss)
            {
                if (Vector2.Distance(NPC.Center, slime.Center) <= CONSUME_DETECTION_RANGE)
                    return true;
            }
        }
        return false;
    }

    private int CountNearbySlimes()
    {
        int count = 0;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC slime = Main.npc[i];
            if (slime.active && slime.aiStyle == NPCAIStyleID.Slime
                && slime.type != NPCID.KingSlime && !slime.boss)
            {
                if (Vector2.Distance(NPC.Center, slime.Center) <= CONSUME_DETECTION_RANGE)
                    count++;
            }
        }
        return count;
    }

    private bool ShouldConsumeSlime()
    {
        if (Main.GameUpdateCount - lastConsumeTime < CONSUME_COOLDOWN)
            return false;

        int nearbySlimes = CountNearbySlimes();

        if (nearbySlimes == 0)
            return false;

        float healthPercentage = (float)NPC.life / NPC.lifeMax;

        if (healthPercentage >= 0.5f)
            return false;

        if (nearbySlimes >= SLIME_THRESHOLD && healthPercentage <= 0.5f)
            return true;

        float consumeChance = 0f;

        if (healthPercentage <= 0.3f)
            consumeChance = 0.33f;
        else if (healthPercentage <= 0.45f)
            consumeChance = 0.20f;
        else if (healthPercentage <= 0.65f)
            consumeChance = 0.15f;

        return Main.rand.NextFloat() < consumeChance;
    }

    #endregion

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D texture = TextureAssets.Npc[Type].Value;
        Texture2D capeTexture = ModContent.Request<Texture2D>($"{TEXTURE_DIRECTORY}NPCs/Bosses/KingSlime/KingSlime_Cape").Value;
        Vector2 origin = NPC.frame.Size() / 2f;

        float groundOffset = NPC.frame.Height * (1f - squishScale.Y) / 2f;
        Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, groundOffset + NPC.gfxOffY);

        SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        spriteBatch.Draw(texture, drawPos, NPC.frame, NPC.GetAlpha(drawColor * 0.75f), NPC.rotation, origin, NPC.scale * squishScale, effects, 0f);
        spriteBatch.Draw(capeTexture, drawPos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, origin, NPC.scale * squishScale, effects, 0f);

        return false;
    }
}
