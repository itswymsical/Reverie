using Reverie.Core.Cinematics;
using Reverie.Core.Cinematics.Camera;
using Reverie.Utilities;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Content.NPCs.Bosses.KingSlime;

public class KingSlime : ModNPC
{
    public override string Texture => $"{TEXTURE_DIRECTORY}NPCs/Bosses/KingSlime/KingSlime";

    private AIState State
    {
        get => (AIState)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    #region Pattern Management

    private enum PatternType
    {
        Phase1,
        Phase2,
        Phase3,
    }

    private struct AttackPattern
    {
        public AIState[] States;
        public int[] Durations;
        public int[] Repetitions;

        public AttackPattern(AIState[] states, int[] durations, int[] repetitions)
        {
            States = states;
            Durations = durations;
            Repetitions = repetitions;
        }
    }

    private static readonly Dictionary<PatternType, AttackPattern> AttackPatterns = new()
    {
        [PatternType.Phase1] = new AttackPattern(
            new[] { AIState.Strolling, AIState.Jumping, AIState.Strolling, AIState.GroundPound, AIState.Strolling, AIState.Jumping },
            new[] { 180, -1, 120, -1, 240, -1 },
            new[] { 1, 3, 1, 1, 1, 2 }
        ),

        [PatternType.Phase2] = new AttackPattern(
            new[] { AIState.Strolling, AIState.Jumping, AIState.ConsumingSlimes, AIState.GroundPound, AIState.Strolling, AIState.BounceHouse, AIState.ConsumingSlimes },
            new[] { 150, -1, -1, -1, 180, -1, 120 },
            new[] { 1, 4, 1, 1, 1, 1, 1 }
        ),

        [PatternType.Phase3] = new AttackPattern(
            new[] { AIState.Jumping, AIState.BounceHouse, AIState.ConsumingSlimes, AIState.GroundPound, AIState.BounceHouse },
            new[] { -1, -1, -1, -1, -1 },
            new[] { 5, 1, 1, 4, 1 }
        )
    };

    private PatternType currentPattern = PatternType.Phase1;
    private int patternStep = 0;
    private int currentRepetition = 0;
    private int patternTimer = 0;
    private bool patternOverride = false;

    #endregion

    private void UpdateScale()
    {
        float healthPercentage = (float)NPC.life / NPC.lifeMax;
        float newScale = MathHelper.Lerp(0.65f, 1.55f, healthPercentage);

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
        float baseHeight = -12f;
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
        ConsumingSlimes,
        BounceHouse,
        DeathAnimation
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

    private const float CONSUME_COOLDOWN = 600f;

    private const float TELEPORT_COOLDOWN = 240f;
    private const float LINE_OF_SIGHT_TIMEOUT = 240f;

    private float lastConsumeTime = -CONSUME_COOLDOWN;

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

    private int bounceHouseSlams = 0;
    private int targetBounceSlams = 3;
    private const float BOUNCE_HOUSE_JUMP_HEIGHT = -16f;
    private const float BOUNCE_HOUSE_SLAM_SPEED = 28f;

    private bool deathAnimationStarted = false;
    private float deathScale = 1f;
    private int deathAnimationPhase = 0;

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

        // Don't update scale during death animation
        if (State != AIState.DeathAnimation)
        {
            UpdateScale();
        }
        else
        {
            // Use death scale instead
            NPC.scale = deathScale;
            UpdateHitbox();
        }

        HandleSquishScale();
        HandleRotation();
        HandleDustTrail();
        HandleSlimeTrail();

        if (State != AIState.DeathAnimation)
        {
            UpdateLOS(target);
            HandlePatternTeleport(target);
            HandlePatternConsume(target);
            UpdatePatternFlow(target);
        }

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
            case AIState.BounceHouse:
                DoBounceHouse(target);
                break;
            case AIState.DeathAnimation:
                DoDeathAnimation(target);
                break;
        }

        NPCUtils.SlopedCollision(NPC);
        NPCUtils.CheckPlatform(NPC, target);
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        base.ModifyNPCLoot(npcLoot);
    }

    #region Pattern System Implementation

    private PatternType GetCurrentPhase()
    {
        float healthPercent = (float)NPC.life / NPC.lifeMax;

        PatternType phase;
        if (healthPercent <= 0.25f)
            phase = PatternType.Phase3;
        else if (healthPercent <= 0.60f)
            phase = PatternType.Phase2;
        else
            phase = PatternType.Phase1;

        return phase;
    }

    private void UpdatePatternFlow(Player target)
    {
        if (patternOverride)
        {
            patternTimer++;
            return;
        }

        PatternType targetPhase = GetCurrentPhase();
        if (currentPattern != targetPhase)
        {
            TransitionToPhase(targetPhase);
            return;
        }

        var pattern = AttackPatterns[currentPattern];
        AIState currentPatternState = pattern.States[patternStep];

        bool shouldAdvance = false;

        if (pattern.Durations[patternStep] > 0)
        {
            if (patternTimer >= pattern.Durations[patternStep])
                shouldAdvance = true;
        }
        else
        {
            if (IsCurrentAttackComplete())
                shouldAdvance = true;
        }

        if (shouldAdvance)
        {
            currentRepetition++;

            if (currentRepetition >= pattern.Repetitions[patternStep])
            {
                AdvancePatternStep();
            }
            else
            {
                StartPatternAttack(currentPatternState, target);
            }

            patternTimer = 0;
        }

        patternTimer++;
    }

    private void TransitionToPhase(PatternType newPhase)
    {
        currentPattern = newPhase;
        patternStep = 0;
        currentRepetition = 0;
        patternTimer = 0;

        var pattern = AttackPatterns[currentPattern];
        StartPatternAttack(pattern.States[0], Main.player[NPC.target]);
    }

    private void AdvancePatternStep()
    {
        var pattern = AttackPatterns[currentPattern];

        patternStep++;
        currentRepetition = 0;

        if (patternStep >= pattern.States.Length)
        {
            patternStep = 0;
        }

        StartPatternAttack(pattern.States[patternStep], Main.player[NPC.target]);
    }

    private void StartPatternAttack(AIState newState, Player target)
    {
        State = newState;
        Timer = 0;

        var pattern = AttackPatterns[currentPattern];

        switch (newState)
        {
            case AIState.Jumping:
                JumpPhase = 0;
                break;

            case AIState.GroundPound:
                JumpPhase = 0;
                groundPoundCount = 0;

                int maxReps = pattern.Repetitions[patternStep];

                if (maxReps >= 3)
                    targetGroundPounds = 3;
                else if (maxReps >= 2)
                    targetGroundPounds = 2;
                else
                    targetGroundPounds = 1;

                break;

            case AIState.BounceHouse:
                JumpPhase = 0;
                bounceHouseSlams = 0;
                targetBounceSlams = Main.rand.Next(3, 6);

                break;

            case AIState.ConsumingSlimes:
                if (!HasNearbySlimes())
                {
                    AdvancePatternStep();
                    return;
                }
                break;

            case AIState.Strolling:
                StrollCount = 0;
                targetStrolls = pattern.Durations[patternStep] > 0 ? 1 : 3;
                break;
        }

        NPC.netUpdate = true;
    }

    private bool IsCurrentAttackComplete()
    {
        switch (State)
        {
            case AIState.Jumping:
                return JumpPhase == 1 && NPC.velocity.Y == 0f && Timer > 30;

            case AIState.GroundPound:
                return JumpPhase == 3 && Timer >= 90f && groundPoundCount >= targetGroundPounds;

            case AIState.BounceHouse:
                return bounceHouseSlams >= targetBounceSlams && NPC.velocity.Y == 0f && Timer > 60;

            case AIState.ConsumingSlimes:
                return Timer >= 450f || !HasNearbySlimes();

            case AIState.Strolling:
                return StrollCount >= targetStrolls;

            case AIState.Teleporting:
                return teleportPhase == TeleportPhase.Prepare && Timer >= 25f;

            default:
                return true;
        }
    }

    private void HandlePatternTeleport(Player target)
    {
        // Don't trigger teleport during BounceHouse or other critical attacks
        if (patternOverride || State == AIState.Teleporting || State == AIState.ConsumingSlimes ||
            State == AIState.BounceHouse ||
            (State == AIState.GroundPound && JumpPhase != 0) ||
            (State == AIState.Jumping && NPC.velocity.Y != 0f))
            return;

        float distToPlayer = Vector2.Distance(NPC.Center, target.Center);
        bool shouldTeleport = false;

        if (distToPlayer > 1000f && teleportTimer >= TELEPORT_COOLDOWN * 0.8f)
            shouldTeleport = true;

        if (stuckTimer >= STUCK_TIMEOUT)
            shouldTeleport = true;

        if (lineOfSightTimer >= LINE_OF_SIGHT_TIMEOUT * 1.5f)
            shouldTeleport = true;

        if (shouldTeleport && Main.netMode != NetmodeID.MultiplayerClient)
        {
            patternOverride = true;
            BeginTeleport(target);
        }
    }

    private void HandlePatternConsume(Player target)
    {
        // Don't override during critical moments or if already overriding
        if (patternOverride || State == AIState.Teleporting || State == AIState.ConsumingSlimes ||
            (State == AIState.GroundPound && JumpPhase != 0) ||
            (State == AIState.Jumping && NPC.velocity.Y != 0f) ||
            (State == AIState.BounceHouse && JumpPhase != 0))
            return;

        // Only override if we're NOT in phase 1
        PatternType currentPhase = GetCurrentPhase();
        if (currentPhase == PatternType.Phase1)
            return;

        // Check cooldown
        if (Main.GameUpdateCount - lastConsumeTime < CONSUME_COOLDOWN)
            return;

        int nearbySlimes = CountNearbySlimes();
        bool shouldOverride = false;

        if (nearbySlimes >= 5)
        {
            shouldOverride = true;
        }
        else if (nearbySlimes >= 3)
        {
            float healthPercentage = (float)NPC.life / NPC.lifeMax;
            if (healthPercentage <= 0.4f)
                shouldOverride = true;
            else if (healthPercentage <= 0.7f && Main.rand.NextBool(3))
                shouldOverride = true;
        }

        if (shouldOverride && Main.netMode != NetmodeID.MultiplayerClient)
        {
            patternOverride = true;
            State = AIState.ConsumingSlimes;
            Timer = 0;
            NPC.netUpdate = true;
        }
    }

    private void ResumeBattlePattern()
    {
        patternOverride = false;

        var pattern = AttackPatterns[currentPattern];
        StartPatternAttack(pattern.States[patternStep], Main.player[NPC.target]);
    }

    #endregion

    #region State Methods
    public override bool CheckDead()
    {
        if (!deathAnimationStarted)
        {
            deathAnimationStarted = true;
            State = AIState.DeathAnimation;
            Timer = 0;
            deathAnimationPhase = 0;
            deathScale = NPC.scale;

            NPC.life = 1;
            NPC.dontTakeDamage = true;

            Main.StopSlimeRain();

            SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}KingSlimeDeath") with
            {
                Volume = 0.8f,
                PitchVariance = 0.1f
            }, NPC.Center);

            return false;
        }

        return true;
    }

    private void DoDeathAnimation(Player target)
    {
        NPC.velocity *= 0.92f;

        switch (deathAnimationPhase)
        {
            case 0:
                if (Timer <= 120f)
                {
                    float shrinkProgress = Timer / 120f;
                    deathScale = MathHelper.Lerp(NPC.scale, 0.45f, EaseFunction.EaseQuadIn.Ease(shrinkProgress));

                    if (Timer % 8 == 0)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            Vector2 dustPos = NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.4f, NPC.height * 0.4f);
                            Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.t_Slime,
                                Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f),
                                150, new Color(86, 162, 255, 150), 1.2f);
                            dust.noGravity = true;
                            dust.velocity *= 0.8f;
                        }
                    }

                    if (Timer % 15 == 0)
                        CameraSystem.shake = (int)MathHelper.Lerp(2, 8, shrinkProgress);
                }
                else
                {
                    deathAnimationPhase = 1;
                    Timer = 0;
                }
                break;

            case 1:
                deathScale = 0.35f;

                if (Timer <= 60f)
                {
                    if (Timer % 3 == 0)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Vector2 dustVel = Vector2.One.RotatedBy(MathHelper.ToRadians(i * 45)) * 4f;
                            Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.t_Slime,
                                dustVel.X, dustVel.Y, 150, new Color(86, 162, 255, 200), 1.5f);
                            dust.noGravity = true;
                        }
                    }

                    if (Timer % 5 == 0)
                        CameraSystem.shake = (int)MathHelper.Lerp(3, 12, Timer / 60f);
                }
                else
                {
                    deathAnimationPhase = 2;
                    Timer = 0;
                }
                break;

            case 2: // big finish
                for (int i = 0; i < 150; i++)
                {
                    Vector2 dustVel = Main.rand.NextVector2CircularEdge(12f, 12f);
                    Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.t_Slime,
                        dustVel.X, dustVel.Y, 150, new Color(86, 162, 255, 150), 2f);
                    dust.noGravity = true;
                    dust.velocity = dustVel;
                }

                for (int i = 0; i < 30; i++)
                {
                    float angle = (float)i / 30f * MathHelper.TwoPi;
                    Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) *
                                      Main.rand.NextFloat(8f, 15f);

                    Vector2 spawnPos = NPC.Center + Main.rand.NextVector2Circular(20f, 20f);
                    int projType = ModContent.ProjectileType<GelBallProjectile>();
                    var proj = Main.projectile[Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity, projType, 0, 0f)];
                    proj.friendly = true;
                }

                NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, NPCID.TownSlimeBlue);

                CameraSystem.shake = 20;

                SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeSlam") with
                {
                    Volume = 1f,
                    Pitch = -0.3f
                }, NPC.Center);

                SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}KingSlimeRoar") with
                {
                    Volume = 1f,
                    Pitch = -0.25f
                }, NPC.Center);

                Timer = 0;

                NPC.life = 0;
                NPC.dontTakeDamage = false;
                NPC.downedSlimeKing = true;
                NPC.checkDead();
                break;
        }
    }

    private void DoStrolling(Player target)
    {
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
            Timer = 0;
        }
    }

    private void DoJumping(Player target)
    {
        float dirToPlayer = Math.Sign(target.Center.X - NPC.Center.X);
        NPC.spriteDirection = -Math.Sign(dirToPlayer);

        if (NPC.velocity.Y == 0f)
        {
            NPC.velocity.X *= 0.96f;
            if (Math.Abs(NPC.velocity.X) < 0.1f)
                NPC.velocity.X = 0f;

            if (Timer >= 40f)
            {
                NPC.netUpdate = true;

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
            float maxSpeed = 4f;
            if ((dirToPlayer == 1 && NPC.velocity.X < maxSpeed) ||
                (dirToPlayer == -1 && NPC.velocity.X > -maxSpeed))
            {
                NPC.velocity.X += 0.15f * dirToPlayer;
            }

            if (Timer > 30 && NPC.velocity.Y > 0)
            {
                // Let pattern system handle transitions
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
            case 0:
                NPC.velocity.X *= 0.5f;
                if (Math.Abs(NPC.velocity.X) < 0.1f)
                    NPC.velocity.X = 0f;

                float chargeTime;
                if (targetGroundPounds > 1 && groundPoundCount > 0)
                    chargeTime = 25f;
                else if (Timer < 0)
                    chargeTime = 45f;
                else
                    chargeTime = 60f;

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

            case 1:
                NPC.damage = 30;
                float maxSpeed = 3f;
                if ((dirToPlayer == 1 && NPC.velocity.X < maxSpeed) ||
                    (dirToPlayer == -1 && NPC.velocity.X > -maxSpeed))
                {
                    NPC.velocity.X += 0.1f * dirToPlayer;
                }

                if (Math.Abs(NPC.velocity.X) > 0.1f)
                    NPC.spriteDirection = -Math.Sign(NPC.velocity.X);

                if (NPC.velocity.Y > 0f)
                {
                    NPC.velocity = new Vector2(
                        target.Center.X > NPC.Center.X ? 2f : -2f,
                        12f
                    );
                    NPC.spriteDirection = target.Center.X > NPC.Center.X ? -1 : 1;
                    JumpPhase = 2;
                    Timer = 0;
                }
                break;

            case 2:
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

                    float distToPlayer = Vector2.Distance(NPC.Center, target.Center) / 16f;

                    int gelBallCount;
                    float baseVelocity;
                    float velocityVariance;

                    if (distToPlayer <= 20f)
                    {
                        gelBallCount = 10;
                        baseVelocity = 7.5f;
                        velocityVariance = 1.6f;
                    }
                    else if (distToPlayer <= 42f)
                    {
                        gelBallCount = 14;
                        baseVelocity = 9f;
                        velocityVariance = 2.1f;
                    }
                    else
                    {
                        gelBallCount = 18;
                        baseVelocity = 11f;
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

            case 3:
                NPC.damage = 12;
                NPC.velocity *= 0.8f;

                float recoveryTime = targetGroundPounds > 1 ? 45f : 90f;

                if (Timer >= recoveryTime)
                {
                    groundPoundCount++;

                    if (groundPoundCount < targetGroundPounds)
                    {
                        JumpPhase = 0;
                        Timer = -15f;
                        isWobbling = false;
                    }
                    else
                    {
                        isWobbling = false;
                    }
                }
                break;
        }
    }

    private void DoBounceHouse(Player target)
    {
        switch (JumpPhase)
        {
            case 0: // Setup and launch
                NPC.velocity.X *= 0.5f;
                if (Math.Abs(NPC.velocity.X) < 0.1f)
                    NPC.velocity.X = 0f;

                float chargeTime = bounceHouseSlams > 0 ? 5f : 45f;

                if (Timer >= chargeTime)
                {
                    NPC.netUpdate = true;

                    // Sound effect
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeSlamCharge") with { PitchVariance = 0.2f }, NPC.Center);

                    NPC.velocity.Y = BOUNCE_HOUSE_JUMP_HEIGHT;
                    NPC.velocity.X = 0f;

                    for (int i = 0; i < 12; i++)
                    {
                        Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                            DustID.t_Slime, 0f, NPC.velocity.Y * 0.4f,
                            150, new Color(86, 162, 255, 100), 1.8f);
                        dust.noGravity = true;
                        dust.velocity *= 0.5f;
                    }

                    JumpPhase = 1;
                    Timer = 0;
                }
                break;

            case 1: // Airborne
                NPC.damage = 40;
                NPC.velocity.X *= 0.95f;

                if (NPC.velocity.Y > 0f)
                {
                    NPC.velocity = new Vector2(0f, BOUNCE_HOUSE_SLAM_SPEED);
                    JumpPhase = 2;
                    Timer = 0;
                }
                break;

            case 2: // Slamming down
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
                    CameraSystem.shake = 8;

                    for (int i = 0; i < 25; i++)
                    {
                        Vector2 dustVel = Vector2.One.RotatedBy(MathHelper.ToRadians(i * 14.4f)) * 9f;
                        Dust dust = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.t_Slime, dustVel.X, dustVel.Y,
                            newColor: new Color(86, 162, 255, 100));
                        dust.noGravity = true;
                        dust.scale = 2.2f;
                    }

                    BounceHouseProjectiles();

                    if (Main.rand.NextBool(3))
                    {
                        Vector2 spawnPos = NPC.Center + new Vector2(0f, NPC.height / 2f);
                        int leftSlime = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X - 25, (int)spawnPos.Y, NPCID.BlueSlime);
                        if (leftSlime < Main.maxNPCs)
                        {
                            Main.npc[leftSlime].velocity = new Vector2(-5f + Main.rand.NextFloat(-1.5f, 0f),
                                                                       -4f + Main.rand.NextFloat(-1f, 1f));
                            Main.npc[leftSlime].scale = 1.3f;
                        }
                        int rightSlime = NPC.NewNPC(NPC.GetSource_FromAI(), (int)spawnPos.X + 25, (int)spawnPos.Y, NPCID.GreenSlime);
                        if (rightSlime < Main.maxNPCs)
                        {
                            Main.npc[rightSlime].velocity = new Vector2(5f + Main.rand.NextFloat(0f, 1.5f),
                                                                        -4f + Main.rand.NextFloat(-1f, 1f));
                            Main.npc[rightSlime].scale = 1.3f;
                        }
                    }

                    JumpPhase = 3;
                    Timer = 0;
                    isWobbling = true;
                    wobbleTimer = 0f;
                    bounceHouseSlams++;
                }
                else
                {
                    float timeInPhase = Timer;
                    float accelCurve = MathHelper.Clamp(timeInPhase / 15f, 0.1f, 1f);
                    float baseAccel = 0.4f;
                    float currentAccel = baseAccel + (accelCurve * 0.6f);

                    NPC.velocity.Y += currentAccel;

                    if (NPC.velocity.Y > BOUNCE_HOUSE_SLAM_SPEED)
                        NPC.velocity.Y = BOUNCE_HOUSE_SLAM_SPEED;

                    if (Main.rand.NextBool(2))
                    {
                        Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                            DustID.t_Slime, 0f, NPC.velocity.Y * 0.3f,
                            150, new Color(86, 162, 255, 100), 1.6f);
                        dust.noGravity = true;
                        dust.velocity *= 0.5f;
                    }
                }
                break;

            case 3: // Brief recovery
                NPC.damage = 12;
                NPC.velocity *= 0.85f;

                float recoveryTime = 10f;

                if (Timer >= recoveryTime)
                {
                    if (bounceHouseSlams < targetBounceSlams)
                    {
                        JumpPhase = 0;
                        Timer = 0;
                        isWobbling = false;
                    }
                    else
                    {
                        isWobbling = false;
                    }
                }
                break;
        }
    }

    private void BounceHouseProjectiles()
    {
        int gelBallCount = 24;
        float baseVelocity = 8f;
        float velocityVariance = 3f;

        for (int i = 0; i < gelBallCount; i++)
        {
            float angle = (float)i / gelBallCount * MathHelper.TwoPi;
            Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) *
                              (baseVelocity + Main.rand.NextFloat(-velocityVariance, velocityVariance));

            Vector2 spawnPos = NPC.Center + new Vector2(Main.rand.NextFloat(-25f, 25f), -5f);
            int projType = ModContent.ProjectileType<GelBallProjectile>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity, projType, 4, 0.7f);
        }

        for (int i = 0; i < 12; i++)
        {
            float angle = MathHelper.Lerp(-MathHelper.PiOver2 - 1f, -MathHelper.PiOver2 + 1f, (float)i / 11f);
            Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) *
                              (10f + Main.rand.NextFloat(-2f, 3f));

            Vector2 spawnPos = NPC.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), -10f);
            int projType = ModContent.ProjectileType<GelBallProjectile>();
            Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity, projType, 5, 0.8f);
        }
    }

    private void DoTeleporting(Player target)
    {
        switch (teleportPhase)
        {
            case TeleportPhase.Prepare:
                NPC.aiAction = 1;

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

                    if (Timer == 20f)
                    {
                        Gore.NewGore(NPC.GetSource_FromAI(),
                                    NPC.Center + new Vector2(-40f, -NPC.height / 2),
                                    NPC.velocity, 734);
                    }
                }
                else if (Timer <= 35f)
                {
                    float transitionProgress = (Timer - 20f) / 15f;

                    if (Timer == 21f)
                    {
                        teleportStartPos = NPC.Bottom;
                    }

                    NPC.Bottom = Vector2.Lerp(teleportStartPos, teleportDestination,
                        EaseFunction.EaseQuadOut.Ease(transitionProgress));

                    NPC.hide = true;
                    isInvulnerable = true;
                    NPC.dontTakeDamage = true;
                    teleportScaleMultiplier = 0.5f;
                }
                else if (Timer >= 35f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    teleportPhase = TeleportPhase.Execute;
                    Timer = 0;
                    NPC.netUpdate = true;
                }

                TeleportDust(1f);
                break;

            case TeleportPhase.Execute:
                NPC.aiAction = 0;

                if (Timer <= 10f)
                {
                    NPC.hide = true;
                    isInvulnerable = true;
                    NPC.dontTakeDamage = true;
                    teleportScaleMultiplier = 0.5f;

                    float windUpProgress = Timer / 10f;
                    float dustIntensity = 1f + windUpProgress * 2f;
                    TeleportDust(dustIntensity);

                    if (Timer == 5f)
                    {
                        SoundEngine.PlaySound(SoundID.QueenSlime with { PitchVariance = 0.2f }, NPC.Center);
                    }

                    if (Timer > 5f && Timer % 2 == 0)
                    {
                        CameraSystem.shake = (int)(3 + windUpProgress * 4);
                    }
                }
                else if (Timer <= 25f)
                {
                    float fadeInProgress = MathHelper.Clamp((Timer - 10f) / 15f, 0f, 1f);
                    teleportScaleMultiplier = 0.5f + fadeInProgress * 0.5f;

                    if (fadeInProgress >= 0.1f)
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

                    TeleportDust(3f - fadeInProgress * 2f);
                }
                else if (Timer >= 25f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    CompleteTeleport();
                }
                break;
        }
    }

    private void CompleteTeleport()
    {
        NPC.TargetClosest();
        NPC.hide = false;
        isInvulnerable = false;
        NPC.dontTakeDamage = false;
        teleportScaleMultiplier = 1f;
        teleportPhase = TeleportPhase.Prepare;
        lastPosition = NPC.Center;

        ResumeBattlePattern();
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

            if (patternOverride)
            {
                ResumeBattlePattern();
            }
            else
            {
                isWobbling = true;
                wobbleTimer = 0f;
            }
        }
    }
    #endregion

    #region Helper Methods

    public override void FindFrame(int frameHeight)
    {
        if (State == AIState.Jumping)
        {
            if (NPC.velocity.Y == 0f)
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
            else
            {
                NPC.frame.Y = frameHeight * 5;
            }
        }
        else if (State == AIState.DeathAnimation)
        {
            // Death animation frames
            switch (deathAnimationPhase)
            {
                case 0: // Shrinking - use wobbling frames
                    float animSpeed = 6f + (Timer / 120f) * 10f; // Speed up as we shrink
                    NPC.frameCounter++;
                    if (NPC.frameCounter >= animSpeed)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;

                        if (NPC.frame.Y >= frameHeight * 4)
                            NPC.frame.Y = frameHeight;
                    }
                    break;

                case 1: // Buildup - fast wobbling
                    float buildupSpeed = 3f;
                    NPC.frameCounter++;
                    if (NPC.frameCounter >= buildupSpeed)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;

                        if (NPC.frame.Y >= frameHeight * 4)
                            NPC.frame.Y = frameHeight;
                    }
                    break;

                case 2: // Explosion - hold final frame
                case 3:
                    NPC.frame.Y = frameHeight * 5; // Jump frame for explosion effect
                    break;
            }
            return;
        }
        else if (State == AIState.GroundPound)
        {
            if (JumpPhase == 0)
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
            else if (JumpPhase == 3)
            {
                float animSpeed = 3f;
                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y > frameHeight * 2)
                        NPC.frame.Y = 0;
                }
            }
            else
            {
                NPC.frame.Y = frameHeight * 5;
            }
        }
        else if (State == AIState.BounceHouse)
        {
            if (JumpPhase == 0)
            {
                float animSpeed = 4f;
                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y >= frameHeight * 3)
                    {
                        SoundEngine.PlaySound(SoundID.QueenSlime with { Volume = 0.35f }, NPC.Center);
                        NPC.frame.Y = frameHeight;
                    }
                }
            }
            else if (JumpPhase == 1)
            {
                NPC.frame.Y = frameHeight * 5;
            }
            else if (JumpPhase == 2)
            {
                NPC.frame.Y = frameHeight * 4;
            }
            else
            {
                float animSpeed = 8f;
                NPC.frameCounter++;
                if (NPC.frameCounter >= animSpeed)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;

                    if (NPC.frame.Y >= frameHeight * 4)
                    {
                        SoundEngine.PlaySound(SoundID.QueenSlime with { Volume = 0.35f }, NPC.Center);
                        NPC.frame.Y = frameHeight;
                    }
                }
            }
        }
        else if (State == AIState.Teleporting)
        {
            if (teleportPhase == TeleportPhase.Prepare)
            {
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
            else
            {
                NPC.frame.Y = frameHeight * 4;
            }
        }
        else
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

    private void UpdateLOS(Player target)
    {
        bool hasLineOfSight = Collision.CanHitLine(NPC.Center, 0, 0, target.Center, 0, 0);
        bool heightDifferenceOk = Math.Abs(NPC.Top.Y - target.Bottom.Y) <= 160f;

        bool isAirborne = NPC.velocity.Y != 0f;
        bool shouldIgnoreLineOfSight = isAirborne && (State == AIState.Jumping || State == AIState.GroundPound || State == AIState.BounceHouse);

        if ((!hasLineOfSight || !heightDifferenceOk) && !shouldIgnoreLineOfSight)
        {
            teleportTimer += 2f;
            if (Main.netMode != NetmodeID.MultiplayerClient)
                lineOfSightTimer += 1.5f;
        }
        else if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            lineOfSightTimer -= 2f;
            if (lineOfSightTimer < 0f)
                lineOfSightTimer = 0f;
        }

        if (!isAirborne)
        {
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
        else
        {
            lastPosition = NPC.Center;
        }
    }

    private void BeginTeleport(Player target)
    {
        FindTeleportSpot(target);
        State = AIState.Teleporting;
        teleportPhase = TeleportPhase.Prepare;
        Timer = 0;
        teleportTimer = 0f;
        stuckTimer = 0f;
        NPC.netUpdate = true;
    }

    private void FindTeleportSpot(Player target)
    {
        Point npcTile = NPC.Center.ToTileCoordinates();
        Point targetTile = target.Center.ToTileCoordinates();

        Vector2 bestLocation = Vector2.Zero;
        bool foundSpot = false;
        int attempts = 0;

        for (int i = 0; i < 20 && !foundSpot; i++)
        {
            attempts++;

            Vector2 playerVel = target.velocity;
            int direction = Math.Abs(playerVel.X) > 0.5f ? -Math.Sign(playerVel.X) : target.direction;

            int offsetX = Main.rand.Next(8, 16) * direction;
            int offsetY = Main.rand.Next(-3, 2);

            int testX = targetTile.X + offsetX;
            int testY = targetTile.Y + offsetY;

            Vector2 testPos = FindGroundPos(testX, testY, target);
            if (testPos != Vector2.Zero)
            {
                bestLocation = testPos;
                foundSpot = true;
                break;
            }
        }

        if (!foundSpot)
        {
            for (int i = 0; i < 30 && !foundSpot; i++)
            {
                attempts++;

                int side = Main.rand.NextBool() ? -1 : 1;
                int offsetX = Main.rand.Next(10, 20) * side;
                int offsetY = Main.rand.Next(-4, 3);

                int testX = targetTile.X + offsetX;
                int testY = targetTile.Y + offsetY;

                Vector2 testPos = FindGroundPos(testX, testY, target);
                if (testPos != Vector2.Zero)
                {
                    bestLocation = testPos;
                    foundSpot = true;
                    break;
                }
            }
        }

        if (!foundSpot)
        {
            for (int i = 0; i < 50 && !foundSpot; i++)
            {
                attempts++;

                int offsetX = Main.rand.Next(-25, 26);
                int offsetY = Main.rand.Next(-8, 5);

                int testX = targetTile.X + offsetX;
                int testY = targetTile.Y + offsetY;

                Vector2 testPos = FindGroundPosLenient(testX, testY, target);
                if (testPos != Vector2.Zero)
                {
                    bestLocation = testPos;
                    foundSpot = true;
                    break;
                }
            }
        }

        if (!foundSpot)
        {
            int safeDirection = target.direction == 1 ? -1 : 1;
            Vector2 safeOffset = new Vector2(safeDirection * 400f, -100f);
            bestLocation = target.Center + safeOffset;

            Point safeTile = bestLocation.ToTileCoordinates();
            Vector2 groundPos = FindGroundPosLenient(safeTile.X, safeTile.Y, target);
            if (groundPos != Vector2.Zero)
                bestLocation = groundPos;
        }

        teleportDestination = bestLocation;
    }

    private Vector2 FindGroundPos(int startX, int startY, Player target)
    {
        // Don't teleport too close to player
        float distToPlayer = Vector2.Distance(new Vector2(startX * 16, startY * 16), target.Center) / 16f;
        if (distToPlayer < 6f) // Minimum 6 tile distance
            return Vector2.Zero;

        // Find ground level
        int groundY = startY;
        bool foundGround = false;

        // Look down for ground (up to 20 tiles)
        for (int checkY = startY; checkY < startY + 20 && checkY < Main.maxTilesY; checkY++)
        {
            if (WorldGen.InWorld(startX, checkY))
            {
                Tile tile = Framing.GetTileSafely(startX, checkY);
                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                {
                    groundY = checkY;
                    foundGround = true;
                    break;
                }
            }
        }

        if (!foundGround)
            return Vector2.Zero;

        Vector2 testPos = new Vector2(startX * 16 + 8, groundY * 16);

        // Basic checks
        if (Main.tile[startX, groundY].LiquidType == LiquidID.Lava)
            return Vector2.Zero;

        // Make sure there's space above for the boss (3 tiles high)
        for (int y = groundY - 1; y >= groundY - 3; y--)
        {
            if (WorldGen.InWorld(startX, y))
            {
                Tile tile = Framing.GetTileSafely(startX, y);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                    return Vector2.Zero; // Not enough space
            }
        }

        return testPos;
    }

    private Vector2 FindGroundPosLenient(int startX, int startY, Player target)
    {
        float distToPlayer = Vector2.Distance(new Vector2(startX * 16, startY * 16), target.Center) / 16f;
        if (distToPlayer < 4f)
            return Vector2.Zero;

        // Find ground level
        int groundY = startY;
        bool foundGround = false;

        // Look both up and down for ground
        for (int offset = 0; offset <= 25; offset++)
        {
            // Check down first
            int checkDownY = startY + offset;
            if (WorldGen.InWorld(startX, checkDownY))
            {
                Tile tile = Framing.GetTileSafely(startX, checkDownY);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    groundY = checkDownY;
                    foundGround = true;
                    break;
                }
            }

            // Then check up
            if (offset > 0)
            {
                int checkUpY = startY - offset;
                if (WorldGen.InWorld(startX, checkUpY))
                {
                    Tile tile = Framing.GetTileSafely(startX, checkUpY);
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        groundY = checkUpY;
                        foundGround = true;
                        break;
                    }
                }
            }
        }

        if (!foundGround)
            return Vector2.Zero;

        Vector2 testPos = new Vector2(startX * 16 + 8, groundY * 16);

        if (Main.tile[startX, groundY].LiquidType == LiquidID.Lava)
            return Vector2.Zero;

        return testPos;
    }

    private void TeleportDust(float velocityMultiplier)
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
            if (yVel < 0)
            {
                squishScale.Y = 1f + stretchIntensity;
                squishScale.X = 1f - stretchIntensity * 0.18f;
            }
            else
            {
                squishScale.Y = 1f + stretchIntensity;
                squishScale.X = 1f - stretchIntensity * 0.25f;
            }
        }
        else if (isWobbling)
        {
            wobbleTimer += 0.15f;
            float wobbleIntensity = (State == AIState.GroundPound && JumpPhase == 3) ? 0.06f : 0.085f;

            float dampening = MathHelper.Clamp(1f - (wobbleTimer * 0.05f), 0.1f, 1f);
            wobbleIntensity *= dampening;
            float wobbleY = (float)Math.Sin(wobbleTimer * 1.2f) * wobbleIntensity;
            float wobbleX = (float)Math.Sin(wobbleTimer * 0.8f) * wobbleIntensity;
            squishScale.Y = 1f + wobbleY;
            squishScale.X = 1f + wobbleX;

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
        for (int d = 0; d < 15; d++)
        {
            Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
            Dust dust = Dust.NewDustDirect(slime.Center, 0, 0, DustID.t_Slime, velocity.X, velocity.Y,
                150, new Color(78, 136, 255, 200), 1.5f);
            dust.noGravity = true;
        }

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

    #endregion
}