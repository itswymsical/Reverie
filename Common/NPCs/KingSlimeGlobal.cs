using Reverie.Content.Items.KingSlime;
using Reverie.Content.NPCs.Bosses.KingSlime;
using Reverie.Core.Cinematics;
using Reverie.Core.Cinematics.Camera;
using Reverie.Utilities;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace Reverie.Common.NPCs;

public class KingSlimeGlobalNPC : GlobalNPC
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation)
    {
        return entity.type == NPCID.KingSlime;
    }

    private AIState GetState(NPC npc)
    {
        return (AIState)npc.ai[0];
    }

    private void SetState(NPC npc, AIState value)
    {
        npc.ai[0] = (float)value;
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

    // Store pattern data per NPC
    private static Dictionary<int, PatternData> npcPatternData = new();

    private struct PatternData
    {
        public PatternType currentPattern;
        public int patternStep;
        public int currentRepetition;
        public int patternTimer;
        public bool patternOverride;
    }

    private PatternData GetPatternData(NPC npc)
    {
        if (!npcPatternData.ContainsKey(npc.whoAmI))
        {
            npcPatternData[npc.whoAmI] = new PatternData
            {
                currentPattern = PatternType.Phase1,
                patternStep = 0,
                currentRepetition = 0,
                patternTimer = 0,
                patternOverride = false
            };
        }
        return npcPatternData[npc.whoAmI];
    }

    private void SetPatternData(NPC npc, PatternData data)
    {
        npcPatternData[npc.whoAmI] = data;
    }

    #endregion

    private static Dictionary<int, NPCExtraData> npcExtraData = new();

    private struct NPCExtraData
    {
        public Vector2 squishScale;
        public bool wasGliding;
        public int targetStrolls;
        public float rotationSway;
        public float wobbleTimer;
        public bool isWobbling;
        public int groundPoundCount;
        public int targetGroundPounds;
        public int baseDefense;
        public float lastConsumeTime;
        public float lineOfSightTimer;
        public float teleportTimer;
        public TeleportPhase teleportPhase;
        public Vector2 teleportDestination;
        public float teleportScaleMultiplier;
        public bool isInvulnerable;
        public Vector2 lastPosition;
        public Vector2 teleportStartPos;
        public float stuckTimer;
        public int bounceHouseSlams;
        public int targetBounceSlams;
        public bool deathAnimationStarted;
        public float deathScale;
        public int deathAnimationPhase;
    }

    private NPCExtraData GetExtraData(NPC npc)
    {
        if (!npcExtraData.ContainsKey(npc.whoAmI))
        {
            npcExtraData[npc.whoAmI] = new NPCExtraData
            {
                squishScale = Vector2.One,
                wasGliding = false,
                targetStrolls = 3,
                rotationSway = 0f,
                wobbleTimer = 0f,
                isWobbling = false,
                groundPoundCount = 0,
                targetGroundPounds = 1,
                baseDefense = 18,
                lastConsumeTime = -CONSUME_COOLDOWN,
                lineOfSightTimer = 0f,
                teleportTimer = 0f,
                teleportPhase = TeleportPhase.Prepare,
                teleportDestination = Vector2.Zero,
                teleportScaleMultiplier = 1f,
                isInvulnerable = false,
                lastPosition = Vector2.Zero,
                teleportStartPos = Vector2.Zero,
                stuckTimer = 0f,
                bounceHouseSlams = 0,
                targetBounceSlams = 3,
                deathAnimationStarted = false,
                deathScale = 1f,
                deathAnimationPhase = 0
            };
        }
        return npcExtraData[npc.whoAmI];
    }

    private void SetExtraData(NPC npc, NPCExtraData data)
    {
        npcExtraData[npc.whoAmI] = data;
    }

    private void UpdateScale(NPC npc)
    {
        var healthPercentage = (float)npc.life / npc.lifeMax;
        var newScale = MathHelper.Lerp(0.65f, 1.55f, healthPercentage);

        if (newScale < 0.45f)
            newScale = 0.45f;

        var extraData = GetExtraData(npc);
        if (GetState(npc) == AIState.Teleporting)
            newScale *= extraData.teleportScaleMultiplier;

        npc.scale = newScale;
        UpdateDefenseBasedOnScale(npc);
        UpdateHitbox(npc);
    }

    private void UpdateDefenseBasedOnScale(NPC npc)
    {
        const int MIN_DEFENSE = 12;
        const int MAX_DEFENSE = 25;

        var extraData = GetExtraData(npc);
        var normalizedScale = (npc.scale - 0.45f) / (1.35f - 0.45f);
        normalizedScale = MathHelper.Clamp(normalizedScale, 0f, 1f);

        var defenseFactor = normalizedScale * normalizedScale * (3 - 2 * normalizedScale);
        var calculatedDefense = (int)(MIN_DEFENSE + (extraData.baseDefense - MIN_DEFENSE) * defenseFactor);

        npc.defense = (int)MathHelper.Clamp(calculatedDefense, MIN_DEFENSE, MAX_DEFENSE);
    }

    private void UpdateHitbox(NPC npc)
    {
        var center = npc.Center;

        npc.width = (int)(BASE_WIDTH * npc.scale);
        npc.height = (int)(BASE_HEIGHT * npc.scale);

        npc.position = center - new Vector2(npc.width / 2, npc.height / 2);
    }

    private float GetScaledJumpHeight(NPC npc)
    {
        var baseHeight = -16f;
        var scaleMultiplier = MathHelper.Lerp(1.4f, 0.8f, npc.scale / 1.35f);
        return baseHeight * scaleMultiplier;
    }

    private ref float GetTimer(NPC npc) => ref npc.ai[1];

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

    private ref float GetJumpPhase(NPC npc) => ref npc.ai[2];
    private ref float GetStrollCount(NPC npc) => ref npc.ai[3];

    private const int BASE_WIDTH = 158;
    private const int BASE_HEIGHT = 106;

    private const float CONSUME_DETECTION_RANGE = 200f;
    private const float SCALE_PER_SLIME = 0.05f;
    private const float MAX_CONSUME_SCALE = 2.0f;
    private const float HP_PER_SLIME = 0.08f;

    private const float CONSUME_COOLDOWN = 600f;

    private const float TELEPORT_COOLDOWN = 240f;
    private const float LINE_OF_SIGHT_TIMEOUT = 240f;

    private const float STUCK_TIMEOUT = 180f;
    private const float MIN_MOVEMENT_THRESHOLD = 16f;

    private const float BOUNCE_HOUSE_JUMP_HEIGHT = -16f;
    private const float BOUNCE_HOUSE_SLAM_SPEED = 28f;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPCID.KingSlime] = 6;

        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
        {
            Position = new Vector2(0f, -16f),
        };
    }

    public override void SetDefaults(NPC npc)
    {
        npc.damage = 12;
        npc.defense = 18;
        npc.lifeMax = 1400;
        npc.width = 158;
        npc.height = 106;
        npc.alpha = 30;
        npc.aiStyle = -1;
        npc.value = Item.buyPrice(gold: 4);
        npc.boss = true;
        npc.lavaImmune = true;
        npc.knockBackResist = 0f;

        npc.HitSound = new SoundStyle($"{SFX_DIRECTORY}KingSlimeHit") with { PitchVariance = 0.3f };
        npc.DeathSound = new SoundStyle($"{SFX_DIRECTORY}KingSlimeDeath") with { PitchVariance = 0.2f };
    }

    public override void AI(NPC npc)
    {        
        if (!Main.dedServ)
            MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}GelatinousJoust");

        var target = Main.player[npc.target];

        if (!target.active || target.dead)
        {
            npc.TargetClosest();
            target = Main.player[npc.target];
            if (!target.active || target.dead)
            {
                SetState(npc, AIState.Despawning);
                return;
            }
        }

        ref float timer = ref GetTimer(npc);
        timer++;

        var extraData = GetExtraData(npc);

        if (GetState(npc) != AIState.DeathAnimation)
        {
            UpdateScale(npc);
        }
        else
        {
            npc.scale = extraData.deathScale;
            UpdateHitbox(npc);
        }

        HandleSquishScale(npc);
        HandleRotation(npc);
        HandleDustTrail(npc);
        HandleSlimeTrail(npc);

        if (GetState(npc) != AIState.DeathAnimation)
        {
            UpdateLOS(npc, target);
            HandlePatternTeleport(npc, target);
            HandlePatternConsume(npc, target);
            UpdatePatternFlow(npc, target);
        }

        switch (GetState(npc))
        {
            case AIState.Strolling:
                DoStrolling(npc, target);
                break;
            case AIState.Jumping:
                DoJumping(npc, target);
                break;
            case AIState.GroundPound:
                DoGroundPound(npc, target);
                break;
            case AIState.Teleporting:
                DoTeleporting(npc, target);
                break;
            case AIState.ConsumingSlimes:
                DoConsumingSlimes(npc, target);
                break;
            case AIState.BounceHouse:
                DoBounceHouse(npc, target);
                break;
            case AIState.DeathAnimation:
                DoDeathAnimation(npc, target);
                break;
        }

        NPCUtils.SlopedCollision(npc);
        NPCUtils.CheckPlatform(npc, target);
    }

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type == ItemID.KingSlimeBossBag)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<GelatinousBlasterItem>(), 2));
        }
    }

    #region Pattern System Implementation

    private PatternType GetCurrentPhase(NPC npc)
    {
        var healthPercent = (float)npc.life / npc.lifeMax;

        PatternType phase;
        if (healthPercent <= 0.25f)
            phase = PatternType.Phase3;
        else if (healthPercent <= 0.60f)
            phase = PatternType.Phase2;
        else
            phase = PatternType.Phase1;

        return phase;
    }

    private void UpdatePatternFlow(NPC npc, Player target)
    {
        var patternData = GetPatternData(npc);

        if (patternData.patternOverride)
        {
            patternData.patternTimer++;
            SetPatternData(npc, patternData);
            return;
        }

        var targetPhase = GetCurrentPhase(npc);
        if (patternData.currentPattern != targetPhase)
        {
            TransitionToPhase(npc, targetPhase);
            return;
        }

        var pattern = AttackPatterns[patternData.currentPattern];
        var currentPatternState = pattern.States[patternData.patternStep];

        var shouldAdvance = false;

        if (pattern.Durations[patternData.patternStep] > 0)
        {
            if (patternData.patternTimer >= pattern.Durations[patternData.patternStep])
                shouldAdvance = true;
        }
        else
        {
            if (IsCurrentAttackComplete(npc))
                shouldAdvance = true;
        }

        if (shouldAdvance)
        {
            patternData.currentRepetition++;

            if (patternData.currentRepetition >= pattern.Repetitions[patternData.patternStep])
            {
                AdvancePatternStep(npc);
                return; // AdvancePatternStep handles saving data
            }
            else
            {
                StartPatternAttack(npc, currentPatternState, target);
            }

            patternData.patternTimer = 0;
        }
        else
        {
            patternData.patternTimer++;
        }

        SetPatternData(npc, patternData);
    }

    private void TransitionToPhase(NPC npc, PatternType newPhase)
    {
        var patternData = GetPatternData(npc);
        patternData.currentPattern = newPhase;
        patternData.patternStep = 0;
        patternData.currentRepetition = 0;
        patternData.patternTimer = 0;
        SetPatternData(npc, patternData);

        var pattern = AttackPatterns[patternData.currentPattern];
        StartPatternAttack(npc, pattern.States[0], Main.player[npc.target]);
    }

    private void AdvancePatternStep(NPC npc)
    {
        var patternData = GetPatternData(npc);
        var pattern = AttackPatterns[patternData.currentPattern];

        patternData.patternStep++;
        patternData.currentRepetition = 0;

        if (patternData.patternStep >= pattern.States.Length)
        {
            patternData.patternStep = 0;
        }

        SetPatternData(npc, patternData);
        StartPatternAttack(npc, pattern.States[patternData.patternStep], Main.player[npc.target]);
    }

    private void StartPatternAttack(NPC npc, AIState newState, Player target)
    {
        SetState(npc, newState);
        GetTimer(npc) = 0;

        var patternData = GetPatternData(npc);
        var extraData = GetExtraData(npc);
        var pattern = AttackPatterns[patternData.currentPattern];

        switch (newState)
        {
            case AIState.Jumping:
                GetJumpPhase(npc) = 0;
                break;

            case AIState.GroundPound:
                GetJumpPhase(npc) = 0;
                extraData.groundPoundCount = 0;

                var maxReps = pattern.Repetitions[patternData.patternStep];

                if (maxReps >= 3)
                    extraData.targetGroundPounds = 3;
                else if (maxReps >= 2)
                    extraData.targetGroundPounds = 2;
                else
                    extraData.targetGroundPounds = 1;

                SetExtraData(npc, extraData);
                break;

            case AIState.BounceHouse:
                GetJumpPhase(npc) = 0;
                extraData.bounceHouseSlams = 0;
                extraData.targetBounceSlams = Main.rand.Next(3, 6);
                SetExtraData(npc, extraData);
                break;

            case AIState.ConsumingSlimes:
                if (!HasNearbySlimes(npc))
                {
                    AdvancePatternStep(npc);
                    return;
                }
                break;

            case AIState.Strolling:
                GetStrollCount(npc) = 0;
                extraData.targetStrolls = pattern.Durations[patternData.patternStep] > 0 ? 1 : 3;
                SetExtraData(npc, extraData);
                break;
        }

        npc.netUpdate = true;
    }

    private bool IsCurrentAttackComplete(NPC npc)
    {
        var extraData = GetExtraData(npc);
        ref float timer = ref GetTimer(npc);

        switch (GetState(npc))
        {
            case AIState.Jumping:
                return GetJumpPhase(npc) == 1 && npc.velocity.Y == 0f && timer > 30;

            case AIState.GroundPound:
                return GetJumpPhase(npc) == 3 && timer >= 90f && extraData.groundPoundCount >= extraData.targetGroundPounds;

            case AIState.BounceHouse:
                return extraData.bounceHouseSlams >= extraData.targetBounceSlams && npc.velocity.Y == 0f && timer > 60;

            case AIState.ConsumingSlimes:
                return timer >= 450f || !HasNearbySlimes(npc);

            case AIState.Strolling:
                return GetStrollCount(npc) >= extraData.targetStrolls;

            case AIState.Teleporting:
                return extraData.teleportPhase == TeleportPhase.Prepare && timer >= 25f;

            default:
                return true;
        }
    }

    private void HandlePatternTeleport(NPC npc, Player target)
    {
        var patternData = GetPatternData(npc);
        var extraData = GetExtraData(npc);

        if (patternData.patternOverride || GetState(npc) == AIState.Teleporting || GetState(npc) == AIState.ConsumingSlimes ||
            GetState(npc) == AIState.BounceHouse ||
            GetState(npc) == AIState.GroundPound && GetJumpPhase(npc) != 0 ||
            GetState(npc) == AIState.Jumping && npc.velocity.Y != 0f)
            return;

        var distToPlayer = Vector2.Distance(npc.Center, target.Center);
        var shouldTeleport = false;

        if (distToPlayer > 1000f && extraData.teleportTimer >= TELEPORT_COOLDOWN * 0.8f)
            shouldTeleport = true;

        if (extraData.stuckTimer >= STUCK_TIMEOUT)
            shouldTeleport = true;

        if (extraData.lineOfSightTimer >= LINE_OF_SIGHT_TIMEOUT * 1.5f)
            shouldTeleport = true;

        if (shouldTeleport && Main.netMode != NetmodeID.MultiplayerClient)
        {
            patternData.patternOverride = true;
            SetPatternData(npc, patternData);
            BeginTeleport(npc, target);
        }
    }

    private void HandlePatternConsume(NPC npc, Player target)
    {
        var patternData = GetPatternData(npc);
        var extraData = GetExtraData(npc);

        if (patternData.patternOverride || GetState(npc) == AIState.Teleporting || GetState(npc) == AIState.ConsumingSlimes ||
            GetState(npc) == AIState.GroundPound && GetJumpPhase(npc) != 0 ||
            GetState(npc) == AIState.Jumping && npc.velocity.Y != 0f ||
            GetState(npc) == AIState.BounceHouse && GetJumpPhase(npc) != 0)
            return;

        // Only override if we're NOT in phase 1
        var currentPhase = GetCurrentPhase(npc);
        if (currentPhase == PatternType.Phase1)
            return;

        // Check cooldown
        if (Main.GameUpdateCount - extraData.lastConsumeTime < CONSUME_COOLDOWN)
            return;

        var nearbySlimes = CountNearbySlimes(npc);
        var shouldOverride = false;

        if (nearbySlimes >= 5)
        {
            shouldOverride = true;
        }
        else if (nearbySlimes >= 3)
        {
            var healthPercentage = (float)npc.life / npc.lifeMax;
            if (healthPercentage <= 0.4f)
                shouldOverride = true;
            else if (healthPercentage <= 0.7f && Main.rand.NextBool(3))
                shouldOverride = true;
        }

        if (shouldOverride && Main.netMode != NetmodeID.MultiplayerClient)
        {
            patternData.patternOverride = true;
            SetPatternData(npc, patternData);
            SetState(npc, AIState.ConsumingSlimes);
            GetTimer(npc) = 0;
            npc.netUpdate = true;
        }
    }

    private void ResumeBattlePattern(NPC npc)
    {
        var patternData = GetPatternData(npc);
        patternData.patternOverride = false;
        SetPatternData(npc, patternData);

        var pattern = AttackPatterns[patternData.currentPattern];
        StartPatternAttack(npc, pattern.States[patternData.patternStep], Main.player[npc.target]);
    }

    #endregion

    #region State Methods
    public override bool CheckDead(NPC npc)
    {
        var extraData = GetExtraData(npc);

        if (!extraData.deathAnimationStarted)
        {
            extraData.deathAnimationStarted = true;
            extraData.deathAnimationPhase = 0;
            extraData.deathScale = npc.scale;
            SetExtraData(npc, extraData);

            SetState(npc, AIState.DeathAnimation);
            GetTimer(npc) = 0;

            npc.life = 1;
            npc.dontTakeDamage = true;

            Main.StopSlimeRain();

            SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}KingSlimeDeath") with
            {
                Volume = 0.8f,
                PitchVariance = 0.1f
            }, npc.Center);

            return false;
        }

        return true;
    }

    private void DoDeathAnimation(NPC npc, Player target)
    {
        var extraData = GetExtraData(npc);
        ref float timer = ref GetTimer(npc);

        npc.velocity *= 0.92f;

        switch (extraData.deathAnimationPhase)
        {
            case 0:
                if (timer <= 120f)
                {
                    var shrinkProgress = timer / 120f;
                    extraData.deathScale = MathHelper.Lerp(npc.scale, 0.45f, EaseFunction.EaseQuadIn.Ease(shrinkProgress));

                    if (timer % 8 == 0)
                    {
                        for (var i = 0; i < 5; i++)
                        {
                            var dustPos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.4f, npc.height * 0.4f);
                            var dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.t_Slime,
                                Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f),
                                150, new Color(86, 162, 255, 150), 1.2f);
                            dust.noGravity = true;
                            dust.velocity *= 0.8f;
                        }
                    }

                    if (timer % 15 == 0)
                        CameraSystem.shake = (int)MathHelper.Lerp(2, 8, shrinkProgress);
                }
                else
                {
                    extraData.deathAnimationPhase = 1;
                    timer = 0;
                }
                break;

            case 1:
                extraData.deathScale = 0.35f;

                if (timer <= 60f)
                {
                    if (timer % 3 == 0)
                    {
                        for (var i = 0; i < 8; i++)
                        {
                            var dustVel = Vector2.One.RotatedBy(MathHelper.ToRadians(i * 45)) * 4f;
                            var dust = Dust.NewDustDirect(npc.Center, 0, 0, DustID.t_Slime,
                                dustVel.X, dustVel.Y, 150, new Color(86, 162, 255, 200), 1.5f);
                            dust.noGravity = true;
                        }
                    }

                    if (timer % 5 == 0)
                        CameraSystem.shake = (int)MathHelper.Lerp(3, 12, timer / 60f);
                }
                else
                {
                    extraData.deathAnimationPhase = 2;
                    timer = 0;
                }
                break;

            case 2: // big finish
                for (var i = 0; i < 150; i++)
                {
                    var dustVel = Main.rand.NextVector2CircularEdge(12f, 12f);
                    var dust = Dust.NewDustDirect(npc.Center, 0, 0, DustID.t_Slime,
                        dustVel.X, dustVel.Y, 150, new Color(86, 162, 255, 150), 2f);
                    dust.noGravity = true;
                    dust.velocity = dustVel;
                }

                for (var i = 0; i < 30; i++)
                {
                    var angle = i / 30f * MathHelper.TwoPi;
                    var velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) *
                                      Main.rand.NextFloat(8f, 15f);

                    var spawnPos = npc.Center + Main.rand.NextVector2Circular(20f, 20f);
                    var projType = ModContent.ProjectileType<GelBallProjectile>();
                    var proj = Main.projectile[Projectile.NewProjectile(npc.GetSource_FromAI(), spawnPos, velocity, projType, 0, 0f)];
                    proj.friendly = true;
                }

                NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.TownSlimeBlue);

                CameraSystem.shake = 20;

                SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeSlam") with
                {
                    Volume = 1f,
                    Pitch = -0.3f
                }, npc.Center);

                SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}KingSlimeRoar") with
                {
                    Volume = 1f,
                    Pitch = -0.25f
                }, npc.Center);

                timer = 0;

                npc.life = 0;
                npc.dontTakeDamage = false;
                NPC.downedSlimeKing = true;
                npc.checkDead();
                break;
        }

        SetExtraData(npc, extraData);
    }

    private void DoStrolling(NPC npc, Player target)
    {
        ref float timer = ref GetTimer(npc);
        var distToPlayer = Vector2.Distance(npc.Center, target.Center);
        float dirToPlayer = Math.Sign(target.Center.X - npc.Center.X);
        var strollDuration = 54f;
        var progress = timer / strollDuration;

        var easedProgress = EaseFunction.EaseCubicInOut.Ease(progress);
        var velocityCurve = (float)Math.Sin(easedProgress * Math.PI);
        var targetSpeed = velocityCurve * MathHelper.PiOver2 * 1.25f;

        var glideStrength = 0.92f;
        var accelStrength = 0.112f;

        npc.velocity.X = npc.velocity.X * glideStrength + targetSpeed * dirToPlayer * accelStrength;

        var maxVel = 5f;
        if (Math.Abs(npc.velocity.X) > maxVel)
            npc.velocity.X = maxVel * Math.Sign(npc.velocity.X);

        if (Math.Abs(npc.velocity.X) > 0.1f)
            npc.spriteDirection = -Math.Sign(npc.velocity.X);

        if (timer >= strollDuration)
        {
            GetStrollCount(npc)++;
            timer = 0;
        }
    }

    private void DoJumping(NPC npc, Player target)
    {
        ref float timer = ref GetTimer(npc);
        var extraData = GetExtraData(npc);

        float dirToPlayer = Math.Sign(target.Center.X - npc.Center.X);
        npc.spriteDirection = -Math.Sign(dirToPlayer);

        if (npc.velocity.Y == 0f)
        {
            npc.velocity.X *= 0.96f;
            if (Math.Abs(npc.velocity.X) < 0.1f)
                npc.velocity.X = 0f;

            if (timer >= 40f)
            {
                npc.netUpdate = true;

                var baseJumpHeight = GetScaledJumpHeight(npc);
                var baseXVelocity = 2.08f;
                var overshoot = Main.rand.NextFloat() < 0.33f ? 1.3f : 1f;
                npc.velocity.Y = baseJumpHeight;
                npc.velocity.X = baseXVelocity * dirToPlayer * overshoot;
                extraData.squishScale = new Vector2(0.8f, 1.2f);
                SetExtraData(npc, extraData);
                GetJumpPhase(npc) = 1;
                timer = 0;
            }
        }
        else
        {
            var maxSpeed = 4f;
            if (dirToPlayer == 1 && npc.velocity.X < maxSpeed ||
                dirToPlayer == -1 && npc.velocity.X > -maxSpeed)
            {
                npc.velocity.X += 0.15f * dirToPlayer;
            }

            if (timer > 30 && npc.velocity.Y > 0)
            {
                // Let pattern system handle transitions
                extraData.isWobbling = true;
                extraData.wobbleTimer = 0f;
                SetExtraData(npc, extraData);
            }
        }
    }

    private void DoGroundPound(NPC npc, Player target)
    {
        ref float timer = ref GetTimer(npc);
        var extraData = GetExtraData(npc);
        float dirToPlayer = Math.Sign(target.Center.X - npc.Center.X);

        switch (GetJumpPhase(npc))
        {
            case 0:
                npc.velocity.X *= 0.5f;
                if (Math.Abs(npc.velocity.X) < 0.1f)
                    npc.velocity.X = 0f;

                float chargeTime;
                if (extraData.targetGroundPounds > 1 && extraData.groundPoundCount > 0)
                    chargeTime = 25f;
                else if (timer < 0)
                    chargeTime = 45f;
                else
                    chargeTime = 60f;

                if (timer >= chargeTime)
                {
                    npc.netUpdate = true;
                    npc.spriteDirection = -Math.Sign(dirToPlayer);
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeSlamCharge") with { PitchVariance = 0.2f }, npc.Center);

                    npc.velocity.Y = -14f;
                    npc.velocity.X = 2f * dirToPlayer;

                    for (var i = 0; i < 10; i++)
                    {
                        var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height,
                            DustID.t_Slime, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f,
                            150, new Color(86, 162, 255, 100), 1.5f);
                        dust.noGravity = true;
                        dust.velocity *= 0.4f;
                    }

                    GetJumpPhase(npc) = 1;
                    timer = 0;
                }
                break;

            case 1:
                npc.damage = 30;
                var maxSpeed = 3f;
                if (dirToPlayer == 1 && npc.velocity.X < maxSpeed ||
                    dirToPlayer == -1 && npc.velocity.X > -maxSpeed)
                {
                    npc.velocity.X += 0.1f * dirToPlayer;
                }

                if (Math.Abs(npc.velocity.X) > 0.1f)
                    npc.spriteDirection = -Math.Sign(npc.velocity.X);

                if (npc.velocity.Y > 0f)
                {
                    npc.velocity = new Vector2(
                        target.Center.X > npc.Center.X ? 2f : -2f,
                        12f
                    );
                    npc.spriteDirection = target.Center.X > npc.Center.X ? -1 : 1;
                    GetJumpPhase(npc) = 2;
                    timer = 0;
                }
                break;

            case 2:
                dirToPlayer = Math.Sign(target.Center.X - npc.Center.X);
                if (timer % 10 == 0)
                    npc.spriteDirection = -Math.Sign(dirToPlayer);

                var hitGround = false;
                var tileX = (int)(npc.position.X / 16);
                var tileEndX = (int)((npc.position.X + npc.width) / 16);
                var tileY = (int)((npc.position.Y + npc.height) / 16);

                for (var i = tileX; i <= tileEndX; i++)
                {
                    var tile = Framing.GetTileSafely(i, tileY);
                    if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    {
                        hitGround = true;
                        break;
                    }
                }

                if (hitGround || npc.velocity.Y == 0f)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeSlam") with { PitchVariance = 0.2f }, npc.Center);
                    CameraSystem.shake = 6;

                    for (var i = 0; i < 30; i++)
                    {
                        var dustVel = Vector2.One.RotatedBy(MathHelper.ToRadians(i * 12)) * 8f;
                        var dust = Dust.NewDustDirect(npc.Center, 0, 0, DustID.t_Slime, dustVel.X, dustVel.Y,
                            newColor: new Color(86, 162, 255, 100));
                        dust.noGravity = true;
                        dust.scale = 2f;
                    }

                    var distToPlayer = Vector2.Distance(npc.Center, target.Center) / 16f;

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

                    for (var i = 0; i < gelBallCount; i++)
                    {
                        var angle = MathHelper.Lerp(-MathHelper.PiOver2 - 0.8f, -MathHelper.PiOver2 + 0.8f, (float)i / (gelBallCount - 1));
                        var velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) *
                                          (baseVelocity + Main.rand.NextFloat(-velocityVariance, velocityVariance));

                        var spawnPos = npc.Center + new Vector2(Main.rand.NextFloat(-20f, 20f), 0f);
                        var projType = ModContent.ProjectileType<GelBallProjectile>();
                        Projectile.NewProjectile(npc.GetSource_FromAI(), spawnPos, velocity, projType, 3, 0.5f);
                    }

                    if (extraData.targetGroundPounds > 1 && Main.rand.NextBool(2))
                    {
                        var spawnPos = npc.Center + new Vector2(0f, npc.height / 2f);
                        var leftSlime = NPC.NewNPC(npc.GetSource_FromAI(), (int)spawnPos.X - 20, (int)spawnPos.Y, NPCID.BlueSlime);
                        if (leftSlime < Main.maxNPCs)
                        {
                            Main.npc[leftSlime].velocity = new Vector2(-4f + Main.rand.NextFloat(-1f, 0f),
                                                                       -3f + Main.rand.NextFloat(-1f, 1f));
                            Main.npc[leftSlime].scale = 1.2f;
                        }
                        var rightSlime = NPC.NewNPC(npc.GetSource_FromAI(), (int)spawnPos.X + 20, (int)spawnPos.Y, NPCID.GreenSlime);
                        if (rightSlime < Main.maxNPCs)
                        {
                            Main.npc[rightSlime].scale = 1.2f;
                        }
                    }
                    GetJumpPhase(npc) = 3;
                    timer = 0;
                    extraData.isWobbling = true;
                    extraData.wobbleTimer = 0f;
                    SetExtraData(npc, extraData);
                }
                else
                {
                    var timeInPhase = timer;
                    var accelCurve = MathHelper.Clamp(timeInPhase / 20f, 0.1f, 1f);
                    var baseAccel = 0.25f;
                    var currentAccel = baseAccel + accelCurve * 0.4f;

                    npc.velocity.Y += currentAccel;

                    if (npc.velocity.Y > 14f)
                        npc.velocity.Y = 14f;

                    if (Main.rand.NextBool(3))
                    {
                        var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height,
                            DustID.t_Slime, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f,
                            150, new Color(86, 162, 255, 100), 1.5f);
                        dust.noGravity = true;
                        dust.velocity *= 0.4f;
                    }
                }
                break;

            case 3:
                npc.damage = 12;
                npc.velocity *= 0.8f;

                var recoveryTime = extraData.targetGroundPounds > 1 ? 45f : 90f;

                if (timer >= recoveryTime)
                {
                    extraData.groundPoundCount++;

                    if (extraData.groundPoundCount < extraData.targetGroundPounds)
                    {
                        GetJumpPhase(npc) = 0;
                        timer = -15f;
                        extraData.isWobbling = false;
                    }
                    else
                    {
                        extraData.isWobbling = false;
                    }
                    SetExtraData(npc, extraData);
                }
                break;
        }
    }

    private void DoBounceHouse(NPC npc, Player target)
    {
        ref float timer = ref GetTimer(npc);
        var extraData = GetExtraData(npc);

        switch (GetJumpPhase(npc))
        {
            case 0: // Setup and launch
                npc.velocity.X *= 0.5f;
                if (Math.Abs(npc.velocity.X) < 0.1f)
                    npc.velocity.X = 0f;

                var chargeTime = extraData.bounceHouseSlams > 0 ? 5f : 45f;

                if (timer >= chargeTime)
                {
                    npc.netUpdate = true;

                    // Sound effect
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeSlamCharge") with { PitchVariance = 0.2f }, npc.Center);

                    npc.velocity.Y = BOUNCE_HOUSE_JUMP_HEIGHT;
                    npc.velocity.X = 0f;

                    for (var i = 0; i < 12; i++)
                    {
                        var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height,
                            DustID.t_Slime, 0f, npc.velocity.Y * 0.4f,
                            150, new Color(86, 162, 255, 100), 1.8f);
                        dust.noGravity = true;
                        dust.velocity *= 0.5f;
                    }

                    GetJumpPhase(npc) = 1;
                    timer = 0;
                }
                break;

            case 1: // Airborne
                npc.damage = 40;
                npc.velocity.X *= 0.95f;

                if (npc.velocity.Y > 0f)
                {
                    npc.velocity = new Vector2(0f, BOUNCE_HOUSE_SLAM_SPEED);
                    GetJumpPhase(npc) = 2;
                    timer = 0;
                }
                break;

            case 2: // Slamming down
                var hitGround = false;
                var tileX = (int)(npc.position.X / 16);
                var tileEndX = (int)((npc.position.X + npc.width) / 16);
                var tileY = (int)((npc.position.Y + npc.height) / 16);

                for (var i = tileX; i <= tileEndX; i++)
                {
                    var tile = Framing.GetTileSafely(i, tileY);
                    if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                    {
                        hitGround = true;
                        break;
                    }
                }

                if (hitGround || npc.velocity.Y == 0f)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeSlam") with { PitchVariance = 0.2f }, npc.Center);
                    CameraSystem.shake = 8;

                    for (var i = 0; i < 25; i++)
                    {
                        var dustVel = Vector2.One.RotatedBy(MathHelper.ToRadians(i * 14.4f)) * 9f;
                        var dust = Dust.NewDustDirect(npc.Center, 0, 0, DustID.t_Slime, dustVel.X, dustVel.Y,
                            newColor: new Color(86, 162, 255, 100));
                        dust.noGravity = true;
                        dust.scale = 2.2f;
                    }

                    BounceHouseProjectiles(npc);

                    if (Main.rand.NextBool(3))
                    {
                        var spawnPos = npc.Center + new Vector2(0f, npc.height / 2f);
                        var leftSlime = NPC.NewNPC(npc.GetSource_FromAI(), (int)spawnPos.X - 25, (int)spawnPos.Y, NPCID.BlueSlime);
                        if (leftSlime < Main.maxNPCs)
                        {
                            Main.npc[leftSlime].velocity = new Vector2(-5f + Main.rand.NextFloat(-1.5f, 0f),
                                                                       -4f + Main.rand.NextFloat(-1f, 1f));
                            Main.npc[leftSlime].scale = 1.3f;
                        }
                        var rightSlime = NPC.NewNPC(npc.GetSource_FromAI(), (int)spawnPos.X + 25, (int)spawnPos.Y, NPCID.GreenSlime);
                        if (rightSlime < Main.maxNPCs)
                        {
                            Main.npc[rightSlime].velocity = new Vector2(5f + Main.rand.NextFloat(0f, 1.5f),
                                                                        -4f + Main.rand.NextFloat(-1f, 1f));
                            Main.npc[rightSlime].scale = 1.3f;
                        }
                    }

                    GetJumpPhase(npc) = 3;
                    timer = 0;
                    extraData.isWobbling = true;
                    extraData.wobbleTimer = 0f;
                    extraData.bounceHouseSlams++;
                    SetExtraData(npc, extraData);
                }
                else
                {
                    var timeInPhase = timer;
                    var accelCurve = MathHelper.Clamp(timeInPhase / 15f, 0.1f, 1f);
                    var baseAccel = 0.4f;
                    var currentAccel = baseAccel + accelCurve * 0.6f;

                    npc.velocity.Y += currentAccel;

                    if (npc.velocity.Y > BOUNCE_HOUSE_SLAM_SPEED)
                        npc.velocity.Y = BOUNCE_HOUSE_SLAM_SPEED;

                    if (Main.rand.NextBool(2))
                    {
                        var dust = Dust.NewDustDirect(npc.position, npc.width, npc.height,
                            DustID.t_Slime, 0f, npc.velocity.Y * 0.3f,
                            150, new Color(86, 162, 255, 100), 1.6f);
                        dust.noGravity = true;
                        dust.velocity *= 0.5f;
                    }
                }
                break;

            case 3: // Brief recovery
                npc.damage = 12;
                npc.velocity *= 0.85f;

                var recoveryTime = 10f;

                if (timer >= recoveryTime)
                {
                    if (extraData.bounceHouseSlams < extraData.targetBounceSlams)
                    {
                        GetJumpPhase(npc) = 0;
                        timer = 0;
                        extraData.isWobbling = false;
                        SetExtraData(npc, extraData);
                    }
                    else
                    {
                        extraData.isWobbling = false;
                        SetExtraData(npc, extraData);
                    }
                }
                break;
        }
    }

    private void BounceHouseProjectiles(NPC npc)
    {
        var gelBallCount = 24;
        var baseVelocity = 8f;
        var velocityVariance = 3f;

        for (var i = 0; i < gelBallCount; i++)
        {
            var angle = (float)i / gelBallCount * MathHelper.TwoPi;
            var velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) *
                              (baseVelocity + Main.rand.NextFloat(-velocityVariance, velocityVariance));

            var spawnPos = npc.Center + new Vector2(Main.rand.NextFloat(-25f, 25f), -5f);
            var projType = ModContent.ProjectileType<GelBallProjectile>();
            Projectile.NewProjectile(npc.GetSource_FromAI(), spawnPos, velocity, projType, 4, 0.7f);
        }

        for (var i = 0; i < 12; i++)
        {
            var angle = MathHelper.Lerp(-MathHelper.PiOver2 - 1f, -MathHelper.PiOver2 + 1f, i / 11f);
            var velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) *
                              (10f + Main.rand.NextFloat(-2f, 3f));

            var spawnPos = npc.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), -10f);
            var projType = ModContent.ProjectileType<GelBallProjectile>();
            Projectile.NewProjectile(npc.GetSource_FromAI(), spawnPos, velocity, projType, 5, 0.8f);
        }
    }

    private void DoTeleporting(NPC npc, Player target)
    {
        ref float timer = ref GetTimer(npc);
        var extraData = GetExtraData(npc);

        switch (extraData.teleportPhase)
        {
            case TeleportPhase.Prepare:
                npc.aiAction = 1;

                if (timer <= 20f)
                {
                    var fadeProgress = MathHelper.Clamp((20f - timer) / 20f, 0f, 1f);
                    extraData.teleportScaleMultiplier = 0.5f + fadeProgress * 0.5f;

                    if (timer >= 20f)
                    {
                        extraData.isInvulnerable = true;
                        npc.dontTakeDamage = true;
                        npc.hide = true;
                    }

                    if (timer == 20f)
                    {
                        Gore.NewGore(npc.GetSource_FromAI(),
                                    npc.Center + new Vector2(-40f, -npc.height / 2),
                                    npc.velocity, 734);
                    }
                }
                else if (timer <= 35f)
                {
                    npc.hide = true;
                    extraData.isInvulnerable = true;
                    npc.dontTakeDamage = true;
                    extraData.teleportScaleMultiplier = 0.5f;
                }
                else if (timer >= 35f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Execute teleport - use vanilla approach
                    npc.Bottom = new Vector2(npc.localAI[1], npc.localAI[2]);
                    extraData.teleportPhase = TeleportPhase.Execute;
                    timer = 0;
                    npc.netUpdate = true;
                }

                TeleportDust(npc, 1f);
                break;

            case TeleportPhase.Execute:
                npc.aiAction = 0;

                if (timer <= 10f)
                {
                    npc.hide = true;
                    extraData.isInvulnerable = true;
                    npc.dontTakeDamage = true;
                    extraData.teleportScaleMultiplier = 0.5f;

                    var windUpProgress = timer / 10f;
                    var dustIntensity = 1f + windUpProgress * 2f;
                    TeleportDust(npc, dustIntensity);

                    if (timer == 5f)
                    {
                        SoundEngine.PlaySound(SoundID.QueenSlime with { PitchVariance = 0.2f }, npc.Center);
                    }

                    if (timer > 5f && timer % 2 == 0)
                    {
                        CameraSystem.shake = (int)(3 + windUpProgress * 4);
                    }
                }
                else if (timer <= 25f)
                {
                    var fadeInProgress = MathHelper.Clamp((timer - 10f) / 15f, 0f, 1f);
                    extraData.teleportScaleMultiplier = 0.5f + fadeInProgress * 0.5f;

                    if (fadeInProgress >= 0.1f)
                    {
                        npc.hide = false;
                        extraData.isInvulnerable = false;
                        npc.dontTakeDamage = false;
                    }
                    else
                    {
                        npc.hide = true;
                        extraData.isInvulnerable = true;
                        npc.dontTakeDamage = true;
                    }

                    TeleportDust(npc, 3f - fadeInProgress * 2f);
                }
                else if (timer >= 25f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    CompleteTeleport(npc);
                }
                break;
        }

        SetExtraData(npc, extraData);
    }


    private void CompleteTeleport(NPC npc)
    {
        var extraData = GetExtraData(npc);

        npc.TargetClosest();
        npc.hide = false;
        extraData.isInvulnerable = false;
        npc.dontTakeDamage = false;
        extraData.teleportScaleMultiplier = 1f;
        extraData.teleportPhase = TeleportPhase.Prepare;
        extraData.lastPosition = npc.Center;

        SetExtraData(npc, extraData);
        ResumeBattlePattern(npc);
    }

    private void DoConsumingSlimes(NPC npc, Player target)
    {
        ref float timer = ref GetTimer(npc);
        var extraData = GetExtraData(npc);

        if (timer == 1)
        {
            CameraSystem.shake = 15;
            SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}KingSlimeRoar") with { Volume = 0.5f, }, npc.Center);

            for (var i = 0; i < 20; i++)
            {
                var dustVel = Vector2.One.RotatedBy(MathHelper.ToRadians(i * 18)) * 5f;
                var dust = Dust.NewDustDirect(npc.Center, 0, 0, DustID.t_Slime, dustVel.X, dustVel.Y,
                    newColor: new Color(78, 136, 255, 150));
                dust.noGravity = true;
                dust.scale = 1.8f;
            }
        }

        if (timer <= 90f && timer % 30 == 0 && timer > 1)
            CameraSystem.shake = (int)MathHelper.Lerp(8, 3, timer / 90f);

        npc.velocity *= 0.85f;

        if (timer % 15 == 0)
        {
            for (var i = 0; i < Main.maxNPCs; i++)
            {
                var slime = Main.npc[i];

                if (slime.active && slime.aiStyle == NPCAIStyleID.Slime
                    && slime.type != NPCID.KingSlime && !slime.boss)
                {
                    var distance = Vector2.Distance(npc.Center, slime.Center);

                    if (distance <= CONSUME_DETECTION_RANGE)
                    {
                        if (npc.Hitbox.Intersects(slime.Hitbox))
                        {
                            ConsumeSlime(npc, slime);
                        }
                    }
                }
            }

            if (timer % 30 == 0)
            {
                var healAmount = (int)(npc.lifeMax * 0.02f);
                npc.life += healAmount;
                npc.HealEffect(healAmount);

                if (npc.life > npc.lifeMax)
                    npc.life = npc.lifeMax;
            }
        }

        var pulse = (float)Math.Sin(timer * 0.05f) * 0.1f;
        extraData.squishScale = new Vector2(1f + pulse, 1f - pulse);
        SetExtraData(npc, extraData);

        if (Main.rand.NextBool(10))
        {
            var offset = new Vector2(Main.rand.NextFloat(-npc.width * 0.5f, npc.width * 0.5f),
                Main.rand.NextFloat(-npc.height * 0.5f, npc.height * 0.5f));
            var position = npc.Center + offset;

            var dust = Dust.NewDustDirect(position, 0, 0, DustID.t_Slime, 0f, 0f,
                150, new Color(78, 136, 255, 150), 1.3f);
            dust.noGravity = true;
            dust.velocity = Vector2.Zero;
            dust.fadeIn = 1.2f;
        }

        if (timer >= 450f || !HasNearbySlimes(npc))
        {
            extraData.lastConsumeTime = Main.GameUpdateCount;
            SetExtraData(npc, extraData);

            var patternData = GetPatternData(npc);
            if (patternData.patternOverride)
            {
                ResumeBattlePattern(npc);
            }
            else
            {
                extraData.isWobbling = true;
                extraData.wobbleTimer = 0f;
                SetExtraData(npc, extraData);
            }
        }
    }
    #endregion

    #region Helper Methods
    public override void Load()
    {
        base.Load();
        On_NPC.VanillaFindFrame += FindFrame;
    }

    public override void Unload()
    {
        base.Unload();
        On_NPC.VanillaFindFrame -= FindFrame;
    }

    private void FindFrame(On_NPC.orig_VanillaFindFrame orig, NPC self, int num, bool isLikeATownNPC, int type)
    {
        if (self.type != NPCID.KingSlime)
        {
            orig(self, num, isLikeATownNPC, type);
            return;
        }

        HandleKingSlimeFrames(self, num);
    }

    private void HandleKingSlimeFrames(NPC npc, int frameHeight)
    {
        var extraData = GetExtraData(npc);
        ref float timer = ref GetTimer(npc);

        if (GetState(npc) == AIState.Jumping)
        {
            if (npc.velocity.Y == 0f)
            {
                var animSpeed = 4f;
                npc.frameCounter++;
                if (npc.frameCounter >= animSpeed)
                {
                    npc.frameCounter = 0;
                    npc.frame.Y += frameHeight;

                    if (npc.frame.Y >= frameHeight * 4)
                    {
                        SoundEngine.PlaySound(SoundID.QueenSlime with { Volume = 0.25f }, npc.Center);
                        npc.frame.Y = frameHeight;
                    }
                }
            }
            else
            {
                npc.frame.Y = frameHeight * 5;
            }
        }
        else if (GetState(npc) == AIState.DeathAnimation)
        {
            switch (extraData.deathAnimationPhase)
            {
                case 0:
                    var animSpeed = 6f + timer / 120f * 10f;
                    npc.frameCounter++;
                    if (npc.frameCounter >= animSpeed)
                    {
                        npc.frameCounter = 0;
                        npc.frame.Y += frameHeight;

                        if (npc.frame.Y >= frameHeight * 4)
                            npc.frame.Y = frameHeight;
                    }
                    break;

                case 1:
                    var buildupSpeed = 3f;
                    npc.frameCounter++;
                    if (npc.frameCounter >= buildupSpeed)
                    {
                        npc.frameCounter = 0;
                        npc.frame.Y += frameHeight;

                        if (npc.frame.Y >= frameHeight * 4)
                            npc.frame.Y = frameHeight;
                    }
                    break;

                case 2:
                case 3:
                    npc.frame.Y = frameHeight * 5;
                    break;
            }
            return;
        }
        else if (GetState(npc) == AIState.GroundPound)
        {
            if (GetJumpPhase(npc) == 0)
            {
                var animSpeed = 2.5f;
                npc.frameCounter++;
                if (npc.frameCounter >= animSpeed)
                {
                    npc.frameCounter = 0;
                    npc.frame.Y += frameHeight;

                    if (npc.frame.Y >= frameHeight * 4)
                    {
                        SoundEngine.PlaySound(SoundID.QueenSlime with { Volume = 0.25f }, npc.Center);
                        npc.frame.Y = frameHeight;
                    }
                }
            }
            else if (GetJumpPhase(npc) == 3)
            {
                var animSpeed = 3f;
                npc.frameCounter++;
                if (npc.frameCounter >= animSpeed)
                {
                    npc.frameCounter = 0;
                    npc.frame.Y += frameHeight;

                    if (npc.frame.Y > frameHeight * 2)
                        npc.frame.Y = 0;
                }
            }
            else
            {
                npc.frame.Y = frameHeight * 5;
            }
        }
        else if (GetState(npc) == AIState.BounceHouse)
        {
            if (GetJumpPhase(npc) == 0)
            {
                var animSpeed = 4f;
                npc.frameCounter++;
                if (npc.frameCounter >= animSpeed)
                {
                    npc.frameCounter = 0;
                    npc.frame.Y += frameHeight;

                    if (npc.frame.Y >= frameHeight * 3)
                    {
                        SoundEngine.PlaySound(SoundID.QueenSlime with { Volume = 0.35f }, npc.Center);
                        npc.frame.Y = frameHeight;
                    }
                }
            }
            else if (GetJumpPhase(npc) == 1)
            {
                npc.frame.Y = frameHeight * 5;
            }
            else if (GetJumpPhase(npc) == 2)
            {
                npc.frame.Y = frameHeight * 4;
            }
            else
            {
                var animSpeed = 8f;
                npc.frameCounter++;
                if (npc.frameCounter >= animSpeed)
                {
                    npc.frameCounter = 0;
                    npc.frame.Y += frameHeight;

                    if (npc.frame.Y >= frameHeight * 4)
                    {
                        SoundEngine.PlaySound(SoundID.QueenSlime with { Volume = 0.35f }, npc.Center);
                        npc.frame.Y = frameHeight;
                    }
                }
            }
        }
        else if (GetState(npc) == AIState.Teleporting)
        {
            if (extraData.teleportPhase == TeleportPhase.Prepare)
            {
                var animSpeed = 4f;
                npc.frameCounter++;
                if (npc.frameCounter >= animSpeed)
                {
                    npc.frameCounter = 0;
                    npc.frame.Y += frameHeight;

                    if (npc.frame.Y >= frameHeight * 4)
                        npc.frame.Y = frameHeight;
                }
            }
            else
            {
                npc.frame.Y = frameHeight * 4;
            }
        }
        else
        {
            var speed = Math.Abs(npc.velocity.X);
            float animSpeed;
            var isGliding = speed < 0.3f;

            if (isGliding)
            {
                if (!extraData.wasGliding)
                {
                    SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}KingSlimeGlide") with { Pitch = -0.1f, Volume = 0.35f }, npc.Center);
                }

                animSpeed = 60f;
                npc.frame.Y = frameHeight * 4;
            }
            else if (speed < 1f)
            {
                if (npc.frame.Y == frameHeight)
                    npc.frame.Y = frameHeight;

                animSpeed = MathHelper.Lerp(12f, 6f, speed);

                npc.frameCounter++;
                if (npc.frameCounter >= animSpeed)
                {
                    npc.frameCounter = 0;
                    npc.frame.Y += frameHeight;

                    if (npc.frame.Y >= frameHeight * 4)
                        npc.frame.Y = frameHeight;
                }
            }
            else
            {
                animSpeed = MathHelper.Lerp(6f, 3f, Math.Min(speed / 3f, 1f));

                npc.frameCounter++;
                if (npc.frameCounter >= animSpeed)
                {
                    npc.frameCounter = 0;
                    npc.frame.Y += frameHeight;

                    if (npc.frame.Y >= frameHeight * 4)
                        npc.frame.Y = 0;
                }
            }

            extraData.wasGliding = isGliding;
            SetExtraData(npc, extraData);
        }
    }

    private void UpdateLOS(NPC npc, Player target)
    {
        var extraData = GetExtraData(npc);

        var hasLineOfSight = Collision.CanHitLine(npc.Center, 0, 0, target.Center, 0, 0);
        var heightDifferenceOk = Math.Abs(npc.Top.Y - target.Bottom.Y) <= 160f;

        var isAirborne = npc.velocity.Y != 0f;
        var shouldIgnoreLineOfSight = isAirborne && (GetState(npc) == AIState.Jumping || GetState(npc) == AIState.GroundPound || GetState(npc) == AIState.BounceHouse);

        if ((!hasLineOfSight || !heightDifferenceOk) && !shouldIgnoreLineOfSight)
        {
            extraData.teleportTimer += 2f;
            if (Main.netMode != NetmodeID.MultiplayerClient)
                extraData.lineOfSightTimer += 1.5f;
        }
        else if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            extraData.lineOfSightTimer -= 2f;
            if (extraData.lineOfSightTimer < 0f)
                extraData.lineOfSightTimer = 0f;
        }

        if (!isAirborne)
        {
            var distanceMoved = Vector2.Distance(npc.Center, extraData.lastPosition);
            if (distanceMoved < MIN_MOVEMENT_THRESHOLD && npc.velocity.Y == 0f)
            {
                extraData.stuckTimer++;
            }
            else
            {
                extraData.stuckTimer = 0f;
                extraData.lastPosition = npc.Center;
            }
        }
        else
        {
            extraData.lastPosition = npc.Center;
        }

        SetExtraData(npc, extraData);
    }

    private void BeginTeleport(NPC npc, Player target)
    {
        var extraData = GetExtraData(npc);

        SetState(npc, AIState.Teleporting);
        extraData.teleportPhase = TeleportPhase.Prepare;
        extraData.teleportTimer = 0f;
        extraData.stuckTimer = 0f;
        GetTimer(npc) = 0;

        // Use vanilla-style teleport destination finding
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            FindTeleportSpot(npc, target);
        }

        SetExtraData(npc, extraData);
        npc.netUpdate = true;
    }

    private void FindTeleportSpot(NPC npc, Player target)
    {
        var npcTile = npc.Center.ToTileCoordinates();
        var targetTile = target.Center.ToTileCoordinates();

        var searchRange = 10; // num240 in vanilla
        var attempts = 0;
        var foundSpot = false;

        while (!foundSpot && attempts < 100)
        {
            attempts++;

            // Pick random position within range of player
            var testX = Main.rand.Next(targetTile.X - searchRange, targetTile.X + searchRange + 1);
            var testY = Main.rand.Next(targetTile.Y - searchRange, targetTile.Y + 1);

            // Skip if too close to player (7 tile exclusion zone) or if tile is occupied
            if (testY >= targetTile.Y - 7 && testY <= targetTile.Y + 7 && testX >= targetTile.X - 7 && testX <= targetTile.X + 7 ||
                !WorldGen.InWorld(testX, testY) ||
                Main.tile[testX, testY].HasTile)
                continue;

            // Find ground level starting from testY
            var groundY = testY;
            var dropDistance = 0;

            if (Main.tile[testX, groundY].HasTile && Main.tileSolid[Main.tile[testX, groundY].TileType] && !Main.tileSolidTop[Main.tile[testX, groundY].TileType])
            {
                dropDistance = 1;
            }
            else
            {
                // Search down for solid ground
                for (; dropDistance < 150 && groundY + dropDistance < Main.maxTilesY; dropDistance++)
                {
                    var checkY = groundY + dropDistance;
                    if (Main.tile[testX, checkY].HasTile && Main.tileSolid[Main.tile[testX, checkY].TileType] && !Main.tileSolidTop[Main.tile[testX, checkY].TileType])
                    {
                        dropDistance--;
                        break;
                    }
                }
            }

            groundY += dropDistance;

            // Validate the spot
            var validSpot = true;

            // Check for lava
            if (validSpot && Main.tile[testX, groundY].LiquidType == LiquidID.Lava)
                validSpot = false;

            // Check line of sight (optional - vanilla does this but might be causing issues)
            // if (validSpot && !Collision.CanHitLine(npc.Center, 0, 0, target.Center, 0, 0))
            //     validSpot = false;

            if (validSpot)
            {
                // Store teleport destination in world coordinates
                npc.localAI[1] = testX * 16 + 8;    // X position (tile center)
                npc.localAI[2] = groundY * 16 + 16; // Y position (bottom of tile)
                foundSpot = true;
                break;
            }
        }

        // Fallback if no spot found - use player's position
        if (!foundSpot)
        {
            var closestPlayer = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)];
            npc.localAI[1] = closestPlayer.Bottom.X;
            npc.localAI[2] = closestPlayer.Bottom.Y;
        }
    }

    private void TeleportDust(NPC npc, float velocityMultiplier)
    {
        var extraData = GetExtraData(npc);
        if (extraData.isInvulnerable) return;

        for (var i = 0; i < 10; i++)
        {
            var dustIndex = Dust.NewDust(
                npc.position + Vector2.UnitX * -20f,
                npc.width + 40,
                npc.height,
                DustID.t_Slime,
                npc.velocity.X,
                npc.velocity.Y,
                150,
                new Color(86, 162, 255, 100),
                2f
            );

            Main.dust[dustIndex].noGravity = true;
            Main.dust[dustIndex].velocity *= velocityMultiplier;
        }
    }

    private void HandleRotation(NPC npc)
    {
        var extraData = GetExtraData(npc);
        ref float timer = ref GetTimer(npc);

        var isAirborne = npc.velocity.Y != 0f;
        if (isAirborne)
        {
            var fallSpeed = Math.Max(npc.velocity.Y, 0f);
            var swayIntensity = fallSpeed * 0.005f;

            if (GetState(npc) == AIState.GroundPound && (GetJumpPhase(npc) == 1 || GetJumpPhase(npc) == 2))
            {
                var groundPoundIntensity = Math.Abs(npc.velocity.Y) * 0.006f;
                swayIntensity = Math.Max(swayIntensity, groundPoundIntensity * 1.2f);
            }

            var swayFreq = 0.12f + Math.Abs(npc.velocity.Y) * 0.009f;
            var sway = (float)Math.Sin(timer * swayFreq) * swayIntensity * 2.25f;
            extraData.rotationSway = MathHelper.Lerp(extraData.rotationSway, sway, 0.08f);
        }
        else
        {
            extraData.rotationSway = MathHelper.Lerp(extraData.rotationSway, 0f, 0.15f);
        }
        npc.rotation = extraData.rotationSway;

        SetExtraData(npc, extraData);
    }

    private void HandleSquishScale(NPC npc)
    {
        var extraData = GetExtraData(npc);
        ref float timer = ref GetTimer(npc);

        var isAirborne = npc.velocity.Y != 0f;
        if (isAirborne)
        {
            var yVel = npc.velocity.Y;
            var stretchIntensity = Math.Abs(yVel) * 0.06f;

            if (GetState(npc) == AIState.GroundPound && GetJumpPhase(npc) == 2)
                stretchIntensity *= 0.75f;

            if (GetState(npc) == AIState.Jumping)
                stretchIntensity *= 0.55f;

            stretchIntensity = MathHelper.Clamp(stretchIntensity, 0f, 0.33f);
            if (yVel < 0)
            {
                extraData.squishScale.Y = 1f + stretchIntensity;
                extraData.squishScale.X = 1f - stretchIntensity * 0.18f;
            }
            else
            {
                extraData.squishScale.Y = 1f + stretchIntensity;
                extraData.squishScale.X = 1f - stretchIntensity * 0.25f;
            }
        }
        else if (extraData.isWobbling)
        {
            extraData.wobbleTimer += 0.15f;
            var wobbleIntensity = GetState(npc) == AIState.GroundPound && GetJumpPhase(npc) == 3 ? 0.06f : 0.085f;

            var dampening = MathHelper.Clamp(1f - extraData.wobbleTimer * 0.05f, 0.1f, 1f);
            wobbleIntensity *= dampening;
            var wobbleY = (float)Math.Sin(extraData.wobbleTimer * 1.2f) * wobbleIntensity;
            var wobbleX = (float)Math.Sin(extraData.wobbleTimer * 0.8f) * wobbleIntensity;
            extraData.squishScale.Y = 1f + wobbleY;
            extraData.squishScale.X = 1f + wobbleX;

            if (dampening <= 0.3f)
                extraData.isWobbling = false;
        }
        else
        {
            extraData.squishScale = Vector2.Lerp(extraData.squishScale, Vector2.One, 0.15f);
        }
        if (Math.Abs(npc.velocity.X) > 0.5f && !isAirborne)
        {
            var horizontalSquish = 0.995f;
            extraData.squishScale.Y *= horizontalSquish;
            extraData.squishScale.X *= 1.001f;
        }

        SetExtraData(npc, extraData);
    }

    private void HandleDustTrail(NPC npc)
    {
        if (Math.Abs(npc.velocity.X) > 0.3f && Math.Abs(npc.velocity.Y) < 2f)
        {
            var tileX = (int)(npc.Center.X / 16f);
            var tileY = (int)((npc.position.Y + npc.height + 4) / 16f);

            if (WorldGen.InWorld(tileX, tileY))
            {
                var tile = Framing.GetTileSafely(tileX, tileY);

                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    var dustChance = Math.Abs(npc.velocity.X) * 0.4f;

                    if (Main.rand.NextFloat() < dustChance)
                    {
                        var dustPos = new Vector2(
                            npc.position.X + Main.rand.Next(npc.width),
                            npc.position.Y + npc.height - 8
                        );

                        var dust = Dust.NewDustDirect(dustPos, 8, 8, DustID.t_Slime);
                        dust.velocity.X = npc.velocity.X * 0.3f + Main.rand.NextFloat(-1f, 1f);
                        dust.velocity.Y = Main.rand.NextFloat(-1f, 0.5f);
                        dust.color = new Color(86, 162, 255, 100);
                        dust.scale = Main.rand.NextFloat(0.8f, 1.3f);
                        dust.alpha = Main.rand.Next(50, 150);
                    }
                }
            }
        }
    }

    private void HandleSlimeTrail(NPC npc)
    {
        if (Math.Abs(npc.velocity.X) > 0.3f && Math.Abs(npc.velocity.Y) < 2f)
        {
            var tileX = (int)(npc.Center.X / 16f);
            var tileY = (int)((npc.position.Y + npc.height + 2) / 16f);

            if (WorldGen.InWorld(tileX, tileY))
            {
                var tile = Framing.GetTileSafely(tileX, tileY);

                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    var slimeChance = Math.Abs(npc.velocity.X) * 0.6f;

                    if (Main.rand.NextFloat() < slimeChance)
                    {
                        SlimedTileSystem.AddSlimedTile(tileX, tileY);

                        if (Main.rand.NextBool(3))
                        {
                            var offsetX = Main.rand.NextBool() ? -3 : 3;
                            if (WorldGen.InWorld(tileX + offsetX, tileY))
                            {
                                var adjacentTile = Framing.GetTileSafely(tileX + offsetX, tileY);
                                if (adjacentTile.HasTile && Main.tileSolid[adjacentTile.TileType])
                                    SlimedTileSystem.AddSlimedTile(tileX + offsetX, tileY);
                            }
                        }
                    }
                }
            }
        }
    }

    private void ConsumeSlime(NPC npc, NPC slime)
    {
        for (var d = 0; d < 15; d++)
        {
            var velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
            var dust = Dust.NewDustDirect(slime.Center, 0, 0, DustID.t_Slime, velocity.X, velocity.Y,
                150, new Color(78, 136, 255, 200), 1.5f);
            dust.noGravity = true;
        }

        var healthGain = slime.life * HP_PER_SLIME;
        var newScale = npc.scale + SCALE_PER_SLIME;
        npc.scale = MathHelper.Min(newScale, MAX_CONSUME_SCALE);

        npc.life += (int)healthGain;
        npc.HealEffect((int)healthGain);

        if (npc.life > npc.lifeMax)
            npc.life = npc.lifeMax;

        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}SlimeConsume")
        {
            PitchVariance = 0.3f,
            MaxInstances = 3
        }, slime.Center);

        slime.life = 0;
        slime.HitEffect();
        slime.active = false;

        UpdateHitbox(npc);
    }

    private bool HasNearbySlimes(NPC npc)
    {
        for (var i = 0; i < Main.maxNPCs; i++)
        {
            var slime = Main.npc[i];
            if (slime.active && slime.aiStyle == NPCAIStyleID.Slime
                && slime.type != NPCID.KingSlime && !slime.boss)
            {
                if (Vector2.Distance(npc.Center, slime.Center) <= CONSUME_DETECTION_RANGE)
                    return true;
            }
        }
        return false;
    }

    private int CountNearbySlimes(NPC npc)
    {
        var count = 0;
        for (var i = 0; i < Main.maxNPCs; i++)
        {
            var slime = Main.npc[i];
            if (slime.active && slime.aiStyle == NPCAIStyleID.Slime
                && slime.type != NPCID.KingSlime && !slime.boss)
            {
                if (Vector2.Distance(npc.Center, slime.Center) <= CONSUME_DETECTION_RANGE)
                    count++;
            }
        }
        return count;
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var extraData = GetExtraData(npc);

        var texture = TextureAssets.Npc[npc.type].Value;
        var capeTexture = ModContent.Request<Texture2D>($"{TEXTURE_DIRECTORY}NPCs/Bosses/KingSlime/KingSlime_Cape").Value;
        var origin = npc.frame.Size() / 2f;

        var groundOffset = npc.frame.Height * (1f - extraData.squishScale.Y) / 2f;
        var drawPos = npc.Center - screenPos + new Vector2(0f, groundOffset + npc.gfxOffY);

        var effects = npc.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        spriteBatch.Draw(texture, drawPos, npc.frame, npc.GetAlpha(drawColor * 0.75f), npc.rotation, origin, npc.scale * extraData.squishScale, effects, 0f);
        spriteBatch.Draw(capeTexture, drawPos, npc.frame, npc.GetAlpha(drawColor), npc.rotation, origin, npc.scale * extraData.squishScale, effects, 0f);

        return false;
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        base.OnSpawn(npc, source);

        Main.StartSlimeRain();

        Main.slimeRain = true;
    }
    public override void OnKill(NPC npc)
    {
        if (npcPatternData.ContainsKey(npc.whoAmI))
            npcPatternData.Remove(npc.whoAmI);

        if (npcExtraData.ContainsKey(npc.whoAmI))
            npcExtraData.Remove(npc.whoAmI);
    }

    public override bool CheckActive(NPC npc)
    {
        // Clean up data if NPC is becoming inactive
        if (!npc.active)
        {
            if (npcPatternData.ContainsKey(npc.whoAmI))
                npcPatternData.Remove(npc.whoAmI);

            if (npcExtraData.ContainsKey(npc.whoAmI))
                npcExtraData.Remove(npc.whoAmI);
        }

        return true; // Allow normal despawn logic
    }
    #endregion
}