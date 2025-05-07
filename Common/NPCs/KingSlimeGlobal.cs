using Reverie.Core.Cinematics.Camera;
using Reverie.Utilities;
using Terraria.Audio;

namespace Reverie.Common.NPCs;

public class KingSlimeGlobal : GlobalNPC
{
    public override bool InstancePerEntity => true;
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.KingSlime;

    #region Constants & Fields
    private Vector2 squishScale = Vector2.One;

    private KingSlimeAI currentState = KingSlimeAI.Idle;
    private KingSlimeAI previousState;
    private float stateTimer;
    private int consecutiveSlams;

    private readonly float PHASE_2_THRESHOLD = 0.55f;
    private readonly int MAX_CONSECUTIVE_SLAMS = 3;

    private float JUMP_DURATION = 1.8f * 60f;
    private float SHOOT_DURATION = 4f * 60f;
    private float STROLL_SPEED = .65f;
    private float IDLE_DURATION = 2.5f * 60f;
    private float SLAM_SPEED = 11.6f;
    private float CONSUME_SLIMES_DURATION = 6.5f * 60f;

    private readonly float CONSUME_HEALTH_THRESHOLD = 0.8f;
    private readonly float CONSUME_DETECTION_RANGE = 1100f;
    private float BASE_SCALE = 1.38f;
    private float MAX_CONSUME_SCALE = 1.38f;
    private float SCALE_PER_SLIME = 0.095f;

    private int baseDefense = 9;
    private int projectileDamage = 8;
    private int currentJumps = 0;

    private float teleportCooldown = 0f;
    private readonly float TELEPORT_COOLDOWN = 120f;
    #endregion

    internal enum KingSlimeAI
    {
        Idle,
        Strolling,
        Jumping,
        Shooting,
        PrepareSlamAttack,
        SlamAttack,
        Teleporting,
        Despawning,
        ConsumingSlimes
    }

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.npcFrameCount[Main.npc[NPCID.KingSlime].type] = 5;
    }

    public override void SetDefaults(NPC npc)
    {
        if (npc.type != NPCID.KingSlime) return;

        npc.aiStyle = -1;
        npc.width = 158;
        npc.height = 100;
        npc.damage = 16;
        npc.defense = baseDefense;
        npc.lifeMax = 780;
        npc.value = Item.buyPrice(gold: 2);
        npc.knockBackResist = 0f;
        npc.boss = true;
        npc.noGravity = false;
        npc.noTileCollide = false;
        npc.scale = BASE_SCALE;
        npc.alpha = 35;
        npc.HitSound = new SoundStyle($"{SFX_DIRECTORY}KingSlimeHit") with { PitchVariance = 0.3f };
        npc.DeathSound = new SoundStyle($"{SFX_DIRECTORY}KingSlimeDeath") with { PitchVariance = 0.2f };
    }
    public override void ApplyDifficultyAndPlayerScaling(NPC npc, int numPlayers, float balance, float bossAdjustment)
    {
        base.ApplyDifficultyAndPlayerScaling(npc, numPlayers, balance, bossAdjustment);

        if (npc.type != NPCID.KingSlime) return;

        npc.lifeMax = (int)(npc.lifeMax * 1.1f * bossAdjustment);
        npc.damage = (int)(npc.damage * 1.15f * bossAdjustment);

        baseDefense = (int)(baseDefense * (1f + 0.15f * bossAdjustment));

        projectileDamage = (int)(projectileDamage * (1f + 0.25f * bossAdjustment));

        if (Main.expertMode)
        {
            JUMP_DURATION *= 0.85f;
            SHOOT_DURATION *= 0.75f;
            IDLE_DURATION *= 0.7f;
            SLAM_SPEED *= 1.2f;
            STROLL_SPEED *= 1.2f;
        }

        if (Main.masterMode)
        {
            SLAM_SPEED *= 1.1f;
            STROLL_SPEED *= 1.1f;
            CONSUME_SLIMES_DURATION *= 0.75f;
            BASE_SCALE = 1.47f;
            SCALE_PER_SLIME = 0.105f;
        }

        //if (numPlayers > 1)
        //{
        //    float multiplayerScaling = 1f + (numPlayers - 1) * 0.35f;
        //    npc.lifeMax = (int)(npc.lifeMax * multiplayerScaling);
        //    baseDefense = (int)(baseDefense * (1f + 0.1f * (numPlayers - 1)));
        //}
    }

    public override void AI(NPC npc)
    {
        if (npc.type != NPCID.KingSlime) return;

        if (teleportCooldown > 0)
            teleportCooldown--;

        if (currentState != KingSlimeAI.Teleporting && ShouldForceTelepot(npc))
        {
            ChangeState(KingSlimeAI.Teleporting);
        }
        Main.musicBox2 = MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}GelatinousJoust");

        MaintainSlimePopulation(npc);

        UpdateScale(npc);

        UpdateState(npc);
        HandleState(npc);
        UpdateHitbox(npc);

        NPCUtils.SlopedCollision(npc);
        NPCUtils.CheckPlatform(npc, Main.player[npc.target]);
    }
    private bool ShouldForceTelepot(NPC npc)
    {
        if (teleportCooldown > 0)
            return false;

        // Ensure target is valid
        if (npc.target < 0 || npc.target >= Main.maxPlayers || !Main.player[npc.target].active || Main.player[npc.target].dead)
        {
            npc.TargetClosest(true);
            return false; // Don't teleport if we can't find a valid target
        }

        Player target = Main.player[npc.target];
        float distanceToPlayer = Vector2.Distance(npc.Center, target.Center);

        // Force teleport if player is far
        if (distanceToPlayer > 800f)
            return true;

        // or if line of sight is broken for too long
        bool lineOfSightBroken = !Collision.CanHitLine(npc.Center, 1, 1, target.Center, 1, 1);
        if (lineOfSightBroken && currentState != KingSlimeAI.Jumping)
        {
            return Main.rand.NextBool(30); // 1/30 chance per frame when LOS broken (~2 second average)
        }

        return false;
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit)
    {
        base.HitEffect(npc, hit);

        if (npc.type != NPCID.KingSlime) return;

        for (int i = 0; i < 10; i++)
        {
            int dust = Dust.NewDust(npc.position, npc.width, npc.height,
                DustID.t_Slime, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f,
                150, new Color(78, 136, 255, 150), 1.5f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.4f;
        }
        if (Main.rand.NextBool(18) && currentState != KingSlimeAI.ConsumingSlimes)
        {
            if (Main.rand.NextBool(3))
            {
                NPC.NewNPC(npc.GetSource_FromThis(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.BlueSlime);
            }
            else if (Main.rand.NextBool(3))
            {
                NPC.NewNPC(npc.GetSource_FromThis(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.GreenSlime);
            }
            else
            {
                NPC.NewNPC(npc.GetSource_FromThis(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.PurpleSlime);
            }
        }
    }


    #region State Management
    private void UpdateScale(NPC npc)
    {
        float healthPercentage = (float)npc.life / npc.lifeMax;
        float scaleRatio = npc.scale = MathHelper.Lerp(0.65f, 1.35f, healthPercentage);

        if (npc.scale < 0.45f)
        {
            npc.scale = 0.45f;
        }

        UpdateDefenseBasedOnScale(npc);
    }

    private void UpdateDefenseBasedOnScale(NPC npc)
    {
        const int MIN_DEFENSE = 8; 
        const int MAX_DEFENSE = 22;

        float normalizedScale = (npc.scale - 0.45f) / (MAX_CONSUME_SCALE - 0.45f);
        normalizedScale = MathHelper.Clamp(normalizedScale, 0f, 1f);

        float defenseFactor = normalizedScale * normalizedScale * (3 - 2 * normalizedScale);

        int calculatedDefense = (int)(MIN_DEFENSE + (baseDefense - MIN_DEFENSE) * defenseFactor);

        int consumptionBonus = npc.defense - (int)(baseDefense * npc.scale);
        if (consumptionBonus > 0)
        {
            calculatedDefense += consumptionBonus;
        }

        npc.defense = (int)MathHelper.Clamp(calculatedDefense, MIN_DEFENSE, MAX_DEFENSE);
    }

    private void UpdateHitbox(NPC npc)
    {
        Vector2 center = npc.Center;

        const int BASE_WIDTH = 158;
        const int BASE_HEIGHT = 100;

        float healthPercentage = (float)npc.life / npc.lifeMax;
        float hitboxScale = MathHelper.Lerp(0.65f, 1.35f, healthPercentage);

        npc.width = (int)(BASE_WIDTH * hitboxScale);
        npc.height = (int)(BASE_HEIGHT * hitboxScale);

        npc.position = center - new Vector2(npc.width / 2, npc.height / 2);
    }

    private void UpdateState(NPC npc)
    {
        stateTimer++;
        HandleStrolling(npc);
        float healthPercentage = (float)npc.life / npc.lifeMax;
        switch (currentState)
        {
            case KingSlimeAI.Idle:
                if (stateTimer >= IDLE_DURATION)
                {
                    if (ShouldTeleport(npc))
                    {
                        ChangeState(KingSlimeAI.Teleporting);
                    }

                    if (healthPercentage <= CONSUME_HEALTH_THRESHOLD && NPC.CountNPCS(NPCAIStyleID.Slime) > 3
                        && Main.rand.NextBool(2))
                    {
                        ChangeState(KingSlimeAI.ConsumingSlimes);
                    }

                    else if (healthPercentage <= PHASE_2_THRESHOLD && NPC.CountNPCS(NPCAIStyleID.Slime) <= 9)
                    {
                        consecutiveSlams = 0;
                        ChangeState(KingSlimeAI.PrepareSlamAttack);
                    }
                    else if (Main.rand.NextBool(2))
                        ChangeState(KingSlimeAI.Shooting);
                    else
                        ChangeState(KingSlimeAI.Jumping);
                }
                break;

            case KingSlimeAI.Shooting:
                if (stateTimer >= SHOOT_DURATION)
                {
                    if (ShouldTeleport(npc))
                    {
                        ChangeState(KingSlimeAI.Teleporting);
                    }
                    if (healthPercentage <= PHASE_2_THRESHOLD && NPC.CountNPCS(NPCAIStyleID.Slime) <= 9)
                    {
                        consecutiveSlams = 0;
                        ChangeState(KingSlimeAI.PrepareSlamAttack);
                    }
                    else
                        ChangeState(KingSlimeAI.Jumping);                
                }
                break;

            case KingSlimeAI.Jumping:
                if (stateTimer >= JUMP_DURATION || npc.velocity.Y == 0f)
                {
                    stateTimer = 0f;

                    if (currentJumps >= 3)
                    {
                        currentJumps = 0;
                    }

                    if (previousState == KingSlimeAI.PrepareSlamAttack ||
                        (healthPercentage <= PHASE_2_THRESHOLD && NPC.CountNPCS(NPCAIStyleID.Slime) <= 9))
                    {
                        ChangeState(KingSlimeAI.PrepareSlamAttack);
                        currentJumps++;
                    }
                    else if (healthPercentage <= CONSUME_HEALTH_THRESHOLD && NPC.CountNPCS(NPCAIStyleID.Slime) > 3
                        && Main.rand.NextBool(3))
                    {
                        ChangeState(KingSlimeAI.ConsumingSlimes);
                    }
                    else if (Main.rand.NextBool(2))
                    {
                        ChangeState(KingSlimeAI.PrepareSlamAttack);
                        currentJumps++;
                    }
                    else
                    {
                        ChangeState(KingSlimeAI.Strolling);
                    }
                }
                break;

            case KingSlimeAI.PrepareSlamAttack:
                if (HandleSlamSetup(npc))
                {
                    ChangeState(KingSlimeAI.SlamAttack);
                }
                break;

            case KingSlimeAI.SlamAttack:
                if (HandleSlam(npc))
                {
                    consecutiveSlams++;
                    if (consecutiveSlams >= MAX_CONSECUTIVE_SLAMS)
                    {
                        ChangeState(KingSlimeAI.Idle);
                    }
                    else
                        ChangeState(KingSlimeAI.PrepareSlamAttack);       
                }
                break;

            case KingSlimeAI.ConsumingSlimes:
                if (stateTimer >= CONSUME_SLIMES_DURATION)
                {   
                    if (Main.rand.NextBool(2))
                        ChangeState(KingSlimeAI.Jumping);      
                    else
                        ChangeState(KingSlimeAI.Strolling);   
                }
                break;

            case KingSlimeAI.Strolling:         
                if (stateTimer >= 2f * 60f)
                {
                    if (ShouldTeleport(npc))
                    {
                        ChangeState(KingSlimeAI.Teleporting);
                    }
                    if (healthPercentage <= CONSUME_HEALTH_THRESHOLD && NPC.CountNPCS(NPCAIStyleID.Slime) > 3
                         && Main.rand.NextBool(3))
                    {
                        ChangeState(KingSlimeAI.ConsumingSlimes);
                    }
                    else if (healthPercentage <= PHASE_2_THRESHOLD && NPC.CountNPCS(NPCAIStyleID.Slime) <= 9)
                    {
                        consecutiveSlams = 0;
                        ChangeState(KingSlimeAI.PrepareSlamAttack);
                    }
                    else if (Main.rand.NextBool(2))
                        ChangeState(KingSlimeAI.Shooting);
                    else
                        ChangeState(KingSlimeAI.Jumping);
                }
                break;

            case KingSlimeAI.Teleporting:
                if (HandleTeleporting(npc))
                {
                    ChangeState(KingSlimeAI.Idle);
                }
                break;
        }
    }

    private void ChangeState(KingSlimeAI newState)
    {
        previousState = currentState;
        currentState = newState;
        stateTimer = 0f;
    }
    #endregion

    #region Attack Handlers
    private void HandleState(NPC npc)
    {
        switch (currentState)
        {
            case KingSlimeAI.Strolling:
                HandleStrolling(npc);
                break;
            case KingSlimeAI.Jumping:
                HandleJumping(npc);
                break;
            case KingSlimeAI.Shooting:
                HandleShooting(npc);
                break;
            case KingSlimeAI.ConsumingSlimes:
                HandleConsumingSlimes(npc);
                break;
        }
        UpdateVisualEffects(npc);
    }

    private void HandleStrolling(NPC npc)
    {
        if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead)
            npc.TargetClosest();

        Player target = Main.player[npc.target];

        npc.direction = npc.spriteDirection = npc.Center.X < target.Center.X ? 1 : -1;

        if (currentState == KingSlimeAI.ConsumingSlimes)
            STROLL_SPEED = 0.85f; 
        else
            STROLL_SPEED = 0.65f;
        

        if (npc.velocity.X < -STROLL_SPEED || npc.velocity.X > STROLL_SPEED)
        {
            if (npc.velocity.Y == 0f)
            {
                npc.velocity *= 0.76f;
            }
        }
        else
        {
            if (npc.velocity.X < STROLL_SPEED && npc.direction == 1)
            {
                npc.velocity.X += 0.03f;
            }
            if (npc.velocity.X > -STROLL_SPEED && npc.direction == -1)
            {
                npc.velocity.X -= 0.03f;
            }
            npc.velocity.X = MathHelper.Clamp(npc.velocity.X, -STROLL_SPEED, STROLL_SPEED);
        }
    }

    private void HandleConsumingSlimes(NPC npc)
    {
        if (stateTimer == 1)
        {
            CameraSystem.shake = 8;
            SoundEngine.PlaySound(SoundID.Roar, npc.Center);

            for (int i = 0; i < 20; i++)
            {
                Vector2 dustVel = Vector2.One.RotatedBy(MathHelper.ToRadians(i * 18)) * 5f;
                int dust = Dust.NewDust(npc.Center, 0, 0, DustID.t_Slime, dustVel.X, dustVel.Y,
                    newColor: new Color(78, 136, 255, 150));
                Main.dust[dust].noGravity = true;
                Main.dust[dust].scale = 1.8f;
            }
        }

        if (stateTimer % 15 == 0)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC target = Main.npc[i];

                if (target.active && target.aiStyle == NPCAIStyleID.Slime
                    && target.type != NPCID.KingSlime && !target.boss)
                {
                    if (Vector2.Distance(npc.Center, target.Center) <= CONSUME_DETECTION_RANGE)
                    {
                        if (!npc.Hitbox.Intersects(target.Hitbox))
                        {
                            Vector2 toKingSlime = npc.Center - target.Center;
                            toKingSlime.Normalize();
                            target.velocity = toKingSlime * 4f;

                            if (Main.rand.NextBool(3))
                            {
                                Dust.NewDust(target.position, target.width, target.height,
                                    DustID.t_Slime, toKingSlime.X, toKingSlime.Y,
                                    150, new Color(78, 136, 255, 150), 1.2f);
                            }
                        }
                        else if (npc.Hitbox.Intersects(target.Hitbox))
                        {
                            for (int d = 0; d < 15; d++)
                            {
                                Vector2 velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                                int dust = Dust.NewDust(target.Center, 0, 0,
                                    DustID.t_Slime, velocity.X, velocity.Y,
                                    150, new Color(78, 136, 255, 200), 1.5f);
                                Main.dust[dust].noGravity = true;
                            }

                            float newScale = npc.scale + SCALE_PER_SLIME;
                            npc.scale = MathHelper.Min(newScale, MAX_CONSUME_SCALE);
                            npc.life += target.life / 8;
                            npc.HealEffect(target.life / 8);
                            if (npc.life >= npc.lifeMax)
                            {
                                npc.life = npc.lifeMax;
                            }

                            SoundEngine.PlaySound(new SoundStyle(
                                $"{SFX_DIRECTORY}SlimeConsume")
                                {
                                    PitchVariance = 0.3f,
                                    MaxInstances = 3
                                }, target.Center);

                            target.life = 0;
                            target.HitEffect();
                            target.active = false;

                            UpdateHitbox(npc);
                        }
                    }
                }
            }
        }

        float pulse = (float)Math.Sin(stateTimer * 0.05f) * 0.1f;
        squishScale = new Vector2(1f + pulse, 1f - pulse);

        if (Main.rand.NextBool(10))
        {
            Vector2 offset = new Vector2(Main.rand.NextFloat(-npc.width * 0.5f, npc.width * 0.5f),
                Main.rand.NextFloat(-npc.height * 0.5f, npc.height * 0.5f));
            Vector2 position = npc.Center + offset;

            int dust = Dust.NewDust(position, 0, 0, DustID.t_Slime, 0f, 0f,
                150, new Color(78, 136, 255, 150), 1.3f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity = Vector2.Zero;
            Main.dust[dust].fadeIn = 1.2f;
        }
    }

    private void HandleSlimes(NPC npc)
    {
        if (NPC.CountNPCS(NPCAIStyleID.Slime) <= 9)
        {
            for (int slimes = 0; slimes < 3; slimes++)
            {
                NPC slime = Main.npc[NPC.NewNPC(default, (int)npc.Center.X + ((slimes * 33) * npc.direction), (int)npc.Center.Y, NPCID.BlueSlime)];
                slime.active = true;
                slime.life = 30;
                slime.lifeMax = 30;
                slime.timeLeft = 2400;
                slime.defense = 0;
                slime.velocity = npc.velocity;
                for (int i = 0; i < 30; i++)
                {
                    Vector2 dustVel = Vector2.One.RotatedBy(MathHelper.ToRadians(i * 12)) * 8f;
                    int dust = Dust.NewDust(slime.Center, 0, 0, DustID.t_Slime, dustVel.X, dustVel.Y, newColor: new Color(78, 136, 255, 150));
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].scale = 1.4f;
                }
            }
        }
    }

    private static void ShootProjectiles(NPC npc)
    {
        Player player = Main.player[npc.target];
        float projectileSpeed = 6.8f;
        int projectileType = ProjectileID.SpikedSlimeSpike;
        int damage = npc.damage / 4;

        int numProjectiles = 5;
        float arcSpread = 60f;
        float startAngle = -arcSpread / 2f;
        float angleStep = arcSpread / (numProjectiles - 1);

        for (int i = 0; i < numProjectiles; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 velocity = new Vector2(projectileSpeed, 0f).RotatedBy(MathHelper.ToRadians(currentAngle));

            Vector2 toPlayer = player.Center - npc.Center;
            velocity = velocity.RotatedBy(toPlayer.ToRotation());

            Projectile.NewProjectile(
                npc.GetSource_FromAI(),
                npc.Center,
                velocity,
                projectileType,
                damage,
                1f,
                Main.myPlayer
            );
        }
    }

    private void MaintainSlimePopulation(NPC npc)
    {
        if (npc.ai[3]++ % 180 == 0)
        {
            float healthPercentage = (float)npc.life / npc.lifeMax;
            if (healthPercentage <= CONSUME_HEALTH_THRESHOLD && NPC.CountNPCS(NPCAIStyleID.Slime) < 4)
            {
                int slimesToSpawn = Main.rand.Next(2, 4);
                for (int i = 0; i < slimesToSpawn; i++)
                {
                    int spawnX = (int)npc.Center.X + Main.rand.Next(-400, 400);
                    int spawnY = (int)npc.Center.Y;

                    bool foundSurface = false;
                    for (int y = 0; y < 200; y++)
                    {
                        int tileX = spawnX / 16;
                        int tileY = (spawnY + y) / 16;

                        Tile tile = Framing.GetTileSafely(tileX, tileY);
                        if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                        {
                            spawnY = (tileY * 16) - 24;
                            foundSurface = true;
                            break;
                        }
                    }

                    if (foundSurface)
                    {
                        int slimeType = Main.rand.NextBool(2) ? NPCID.BlueSlime :
                                      (Main.rand.NextBool() ? NPCID.GreenSlime : NPCID.PurpleSlime);

                        for (int d = 0; d < 15; d++)
                        {
                            Vector2 velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -0.5f));
                            int dust = Dust.NewDust(new Vector2(spawnX, spawnY), 16, 8,
                                DustID.t_Slime, velocity.X, velocity.Y,
                                150, new Color(78, 136, 255, 200), 1f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].fadeIn = 1.2f;
                        }

                        int newSlime = NPC.NewNPC(npc.GetSource_FromAI(), spawnX, spawnY, slimeType);
                        if (newSlime >= 0 && newSlime < Main.maxNPCs)
                        {
                            Main.npc[newSlime].scale = 0.2f;
                            Main.npc[newSlime].alpha = 125;
                        }
                    }
                }
            }
        }
    }

    private void HandleJumping(NPC npc)
    {
        if (npc.velocity.Y == 0f)
        {
            npc.velocity.X *= 0.5f;
            if (Math.Abs(npc.velocity.X) < 0.1f)
                npc.velocity.X = 0f;

            stateTimer += 2f;
            if (npc.life < npc.lifeMax * 0.8f) stateTimer += 1f;
            if (npc.life < npc.lifeMax * 0.6f) stateTimer += 1f;
            if (npc.life < npc.lifeMax * 0.4f) stateTimer += 2f;
            if (npc.life < npc.lifeMax * 0.2f) stateTimer += 3f;
            if (npc.life < npc.lifeMax * 0.1f) stateTimer += 4f;

            if (stateTimer >= 0f)
            {
                npc.netUpdate = true;
                npc.TargetClosest();
                npc.direction = npc.spriteDirection = npc.Center.X < Main.player[npc.target].Center.X ? 1 : -1;

                // Cap the jump count
                currentJumps = Math.Min(currentJumps, 3);

                // Limit base jump height to a maximum of -14f
                float baseJumpHeight = Math.Max(-14f, -8f - (currentJumps * 2f));
                float baseXVelocity = 2f + (currentJumps * 0.5f);

                if (currentState == KingSlimeAI.Jumping && stateTimer >= JUMP_DURATION * 0.8f)
                {
                    npc.velocity.Y = baseJumpHeight * 1.5f; // Reduced from 1.6f
                    npc.velocity.X += baseXVelocity * 0.875f * npc.direction;
                    stateTimer = -320f;
                }
                else if (currentState == KingSlimeAI.Jumping && stateTimer >= JUMP_DURATION * 0.5f)
                {
                    npc.velocity.Y = baseJumpHeight * 0.75f;
                    npc.velocity.X += baseXVelocity * 1.125f * npc.direction;
                    stateTimer = -320f;
                }
                else
                {
                    npc.velocity.Y = baseJumpHeight;
                    npc.velocity.X = baseXVelocity * npc.direction;
                    stateTimer = -320f;
                }

                squishScale = new Vector2(0.8f, 1.2f);
            }
        }

        else if (npc.target < 255)
        {
            float maxSpeed = Main.getGoodWorld ? 6f : 3f;

            if ((npc.direction == 1 && npc.velocity.X < maxSpeed) ||
                (npc.direction == -1 && npc.velocity.X > -maxSpeed))
            {
                if ((npc.direction == -1 && npc.velocity.X < 0.1f) ||
                    (npc.direction == 1 && npc.velocity.X > -0.1f))
                    npc.velocity.X += 0.2f * npc.direction;
                else
                    npc.velocity.X *= 0.93f;
            }

            if (npc.velocity.Y < 0)
                squishScale = new Vector2(0.8f, 1.2f);
            else if (npc.velocity.Y > 0)
                squishScale = new Vector2(1.2f, 0.8f);
        }

        int dust = Dust.NewDust(npc.position, npc.width, npc.height,
            DustID.t_Slime, npc.velocity.X, npc.velocity.Y, 255,
            new Color(78, 136, 255, 150), npc.scale * 1.2f);
        Main.dust[dust].noGravity = true;
        Main.dust[dust].velocity *= 0.5f;
    }

    private void HandleShooting(NPC npc)
    {
        if (stateTimer % 60f == 0)
        {
            ShootProjectiles(npc);
        }
    }

    private bool HandleSlamSetup(NPC npc)
    {

        if (npc.velocity.Y == 0f)
        {
            if (!npc.localAI[1].Equals(1f))
            {
                npc.localAI[1] = 1f;
                SoundEngine.PlaySound(new SoundStyle(
                $"{SFX_DIRECTORY}SlimeSlamCharge")
                {
                    PitchVariance = 0.2f
                }, npc.Center);
            }

            npc.direction = npc.spriteDirection = npc.Center.X < Main.player[npc.target].Center.X ? 1 : -1;

            npc.velocity.Y = -14f;
            npc.velocity.X = 2f * npc.direction;

            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(npc.position, npc.width, npc.height,
                    DustID.t_Slime, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f,
                    150, new Color(78, 136, 255, 150), 1.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.4f;
            }
        }
        else // In air
        {
            npc.damage = npc.GetAttackDamage_ScaledByStrength(30);
            npc.localAI[1] = 0f;
            float maxSpeed = 3f;
            if ((npc.direction == 1 && npc.velocity.X < maxSpeed) ||
                (npc.direction == -1 && npc.velocity.X > -maxSpeed))
            {
                npc.velocity.X += 0.1f * npc.direction;
            }
            if (npc.velocity.Y > 0f)
            {
                return true;
            }
        }

        return false;
    }

    private bool HandleSlam(NPC npc)
    {
        npc.knockBackResist = 0f;

        if (stateTimer == 0)
        {
            Player target = Main.player[npc.target];
            Vector2 toTarget = target.Center - npc.Center;
            float targetAngle = toTarget.ToRotation();
            npc.velocity = new Vector2(
                target.Center.X > npc.Center.X ? 2f : -2f,
                SLAM_SPEED
            );
        }

        bool hitGround = false;
        int tileX = (int)(npc.position.X / 16);
        int tileEndX = (int)((npc.position.X + npc.width) / 16);
        int tileY = (int)((npc.position.Y + npc.height) / 16);

        for (int i = tileX; i <= tileEndX; i++)
        {
            Tile tile = Framing.GetTileSafely(i, tileY);
            if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
            {
                hitGround = true;
                break;
            }
        }

        if (hitGround || npc.velocity.Y == 0f)
        {
            if (!npc.localAI[0].Equals(1f))
            {
                HandleSlimes(npc);
                CameraSystem.shake = 15;
                npc.localAI[0] = 1f;
                npc.damage = npc.GetAttackDamage_ScaledByStrength(16);       
                SoundEngine.PlaySound(new SoundStyle(
                $"{SFX_DIRECTORY}SlimeSlam")
                {
                    PitchVariance = 0.2f
                }, npc.Center);

                for (int i = 0; i < 30; i++)
                {
                    Vector2 dustVel = Vector2.One.RotatedBy(MathHelper.ToRadians(i * 12)) * 8f;
                    int dust = Dust.NewDust(npc.Center, 0, 0, DustID.t_Slime, dustVel.X, dustVel.Y, newColor: new Color(78, 136, 255, 150));
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].scale = 2f;
                }
            }

            stateTimer++;
            npc.velocity *= 0.8f;

            if (stateTimer > 10f)
            {
                npc.noTileCollide = false;
                npc.noGravity = false;
                npc.localAI[0] = 0f;
                return true;
            }
        }
        else
        {
            npc.velocity.Y += 0.5f;
            if (npc.velocity.Y > SLAM_SPEED)
                npc.velocity.Y = SLAM_SPEED;

            for (int i = 0; i < 3; i++)
            {
                int dust = Dust.NewDust(npc.position, npc.width, npc.height,
                    DustID.t_Slime, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f,
                    150, new Color(78, 136, 255, 150), 1.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.4f;
            }
        }

        return false;
    }

    private bool HandleTeleporting(NPC npc)
    {
        // Phase 1: Fade out and scale down (first 60 frames)
        if (stateTimer < 60)
        {
            // Calculate fade out progress (0 to 1)
            float fadeOutProgress = stateTimer / 60f;
            npc.alpha = (int)MathHelper.Lerp(35, 255, fadeOutProgress);

            // Get current health-based scale that would normally apply
            float healthPercentage = (float)npc.life / npc.lifeMax;
            float baseScale = MathHelper.Lerp(0.65f, 1.35f, healthPercentage);

            // Scale down as we fade out (minimum 0.3f)
            float scaleReduction = 1f - (fadeOutProgress * 0.7f); // At most reduce to 30% of original
            npc.scale = baseScale * scaleReduction;
            npc.scale = Math.Max(npc.scale, 0.3f); // Safety minimum

            // Update hitbox for new scale
            UpdateHitbox(npc);

            // Slow down movement while vanishing
            npc.velocity *= 0.95f;

            // Create dust effects while fading out
            if (stateTimer % 5 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = npc.position + new Vector2(Main.rand.Next(npc.width), Main.rand.Next(npc.height));
                    int dust = Dust.NewDust(dustPos, 4, 4, DustID.t_Slime, 0f, 0f,
                        150, new Color(78, 136, 255, 80), 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 0.5f;
                }
            }

            // Dust burst at the end of fade out
            if (stateTimer == 59)
            {
                SoundEngine.PlaySound(SoundID.Item8, npc.Center);

                for (int i = 0; i < 50; i++)
                {
                    Vector2 velocity = Vector2.One.RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(2f, 8f);
                    int dust = Dust.NewDust(npc.Center, 0, 0, DustID.t_Slime,
                        velocity.X, velocity.Y, 150, new Color(78, 136, 255, 200), 2f);
                    Main.dust[dust].noGravity = true;
                }
            }
        }
        // Phase 2: Teleport (frame 60)
        else if (stateTimer == 60)
        {
            // Stay completely invisible and small
            npc.alpha = 255;

            // Get current health-based scale that would normally apply
            float healthPercentage = (float)npc.life / npc.lifeMax;
            float baseScale = MathHelper.Lerp(0.65f, 1.35f, healthPercentage);

            // Maintain small scale during teleport
            npc.scale = baseScale * 0.3f; // 30% of normal scale
            UpdateHitbox(npc);

            // Ensure target is valid before teleporting
            if (npc.target < 0 || npc.target >= Main.maxPlayers || !Main.player[npc.target].active)
                npc.TargetClosest(true);

            // Get teleport position above the player
            Player target = Main.player[npc.target];
            Vector2 teleportPos = target.Center;
            teleportPos.Y -= 580; // More consistent height
            
            teleportPos.X += Main.rand.Next(-40, 41); // Smaller horizontal variance

            // Set position and reset velocity
            npc.Center = teleportPos;
            npc.velocity = Vector2.Zero;

            // Always flag for immediate slam in Master Mode
            if (Main.masterMode)
            {
                npc.localAI[3] = 1f; // Flag for slam attack after teleport
            }
            // 50% chance in Expert Mode
            else if (Main.expertMode && Main.rand.NextBool())
            {
                npc.localAI[3] = 1f;
            }
        }

        // Phase 3: Fade in and scale up (frames 61-90)
        else if (stateTimer < 90)
        {
            // Calculate fade in progress (0 to 1)
            float fadeInProgress = (stateTimer - 60) / 30f;
            npc.alpha = (int)MathHelper.Lerp(255, 35, fadeInProgress);

            // Get current health-based scale that would normally apply
            float healthPercentage = (float)npc.life / npc.lifeMax;
            float baseScale = MathHelper.Lerp(0.65f, 1.35f, healthPercentage);

            // Scale back up as we fade in (from 30% to 100% of base scale)
            float scaleIncrease = 0.3f + (fadeInProgress * 0.7f);
            npc.scale = baseScale * scaleIncrease;

            // Update hitbox for new scale
            UpdateHitbox(npc);

            // Create dust effects while fading in
            if (stateTimer % 5 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 dustPos = npc.position + new Vector2(Main.rand.Next(npc.width), Main.rand.Next(npc.height));
                    int dust = Dust.NewDust(dustPos, 4, 4, DustID.t_Slime, 0f, 0f,
                        150, new Color(78, 136, 255, 80), 2f);
                    Main.dust[dust].noGravity = true;
                    Main.dust[dust].velocity *= 2f;
                }
            }

            // Sound when reappearing
            if (stateTimer == 61)
            {
                SoundEngine.PlaySound(SoundID.Item8, npc.Center);
            }
        }
        // Phase 4: Teleport complete
        else
        {
            npc.TargetClosest();
            npc.direction = npc.spriteDirection = npc.Center.X < Main.player[npc.target].Center.X ? 1 : -1;

            // Restore normal scale based on health
            float healthPercentage = (float)npc.life / npc.lifeMax;
            npc.scale = MathHelper.Lerp(0.65f, 1.35f, healthPercentage);
            UpdateHitbox(npc);

            teleportCooldown = TELEPORT_COOLDOWN;

            if (npc.localAI[3] == 1f)
            {
                npc.localAI[3] = 0f;
                ChangeState(KingSlimeAI.SlamAttack);
                npc.velocity.Y = SLAM_SPEED * 1.2f;

                for (int i = 0; i < 20; i++)
                {
                    int dust = Dust.NewDust(npc.position, npc.width, npc.height,
                        DustID.t_Slime, 0f, npc.velocity.Y * 0.4f,
                        150, new Color(78, 136, 255, 150), 2f);
                    Main.dust[dust].noGravity = true;
                }
            }
            else
            {
                ChangeState(KingSlimeAI.PrepareSlamAttack);
            }

            return true;
        }

        if (stateTimer >= 60 && npc.Distance(Main.LocalPlayer.Center) > 10f)
        {
            CameraSystem.shake = 3;
           
        }
        return false;
    }

    private bool ShouldTeleport(NPC npc)
    {
        if (teleportCooldown > 0f || currentState == KingSlimeAI.Teleporting)
            return false;

        float healthPercentage = (float)npc.life / npc.lifeMax;
        Player target = Main.player[npc.target];

        // If player is dead or not active, don't teleport
        if (target.dead || !target.active)
            return false;

        // Random chance based on health - MUCH more frequent
        int teleportChance;
        if (healthPercentage <= 0.3f)
            teleportChance = 60; // 1.67% chance per frame
        else if (healthPercentage <= 0.6f)
            teleportChance = 90; // 1.11% chance per frame
        else
            teleportChance = 120; // 0.83% chance per frame

        // Check trigger conditions (but don't use the extreme ones handled by ShouldForceTelepot)
        bool playerFarAway = Vector2.Distance(npc.Center, target.Center) > 350f && Vector2.Distance(npc.Center, target.Center) <= 800f;
        bool playerAbove = target.Center.Y < npc.Center.Y - 100f;

        // If ANY of these are true, increase chance of teleport
        if (playerFarAway || playerAbove)
            teleportChance /= 2; // Double the chance

        // Return true if we pass the random check
        return Main.rand.NextBool(teleportChance);
    }
    #endregion

    #region Animation
    private int frame;
    private float frameCounter;
    private const int TOTAL_FRAMES = 5;
    private const int FRAME_HEIGHT = 110;
    private const int FRAME_WIDTH = 168;
    private float ANIMATION_SPEED = 0.13f;

    public override void FindFrame(NPC npc, int frameHeight)
    {
        if (npc.type == NPCID.KingSlime)
        {
            npc.frame.Width = FRAME_WIDTH;
            npc.frame.Height = FRAME_HEIGHT;

            frameCounter += ANIMATION_SPEED;
            if (frameCounter >= 1f)
            {
                frameCounter = 0;
                frame++;

                if (frame >= TOTAL_FRAMES)
                {
                    frame = 0;
                }
            }

            npc.frame.Y = frame * FRAME_HEIGHT;
        }
    }

    private void UpdateVisualEffects(NPC npc)
    {
        HandleSquishScale(npc);
    }

    private void HandleSquishScale(NPC npc)
    {
        const float MAX_STRETCH = 1.3f;
        const float MIN_SQUISH = 0.9f;

        float yVelocityFactor = MathHelper.Clamp(npc.velocity.Y / 16f, -1f, 1f);

        if (npc.velocity.Y != 0)
        {
            if (npc.velocity.Y < 0)
            {
                squishScale.Y = MathHelper.Lerp(1f, MAX_STRETCH, -yVelocityFactor);
                squishScale.X = MathHelper.Lerp(1f, MIN_SQUISH, -yVelocityFactor);
            }
            else
            {
                squishScale.Y = MathHelper.Lerp(1f, MIN_SQUISH, yVelocityFactor);
                squishScale.X = MathHelper.Lerp(1f, MAX_STRETCH, yVelocityFactor);
            }
        }
        else
        {
            squishScale = Vector2.Lerp(squishScale, Vector2.One, 0.15f);
        }
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.type != NPCID.KingSlime) return base.PreDraw(npc, spriteBatch, screenPos, drawColor);

        Texture2D texture = ModContent.Request<Texture2D>("Reverie/Assets/Textures/NPCs/KingSlime/KingSlime").Value;
        Rectangle sourceRectangle = new(0, frame * FRAME_HEIGHT, FRAME_WIDTH, FRAME_HEIGHT);
        Vector2 drawPos = npc.Center - Main.screenPosition;
        SpriteEffects spriteEffects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        float alpha = (255 - npc.alpha) / 255f;
        Color finalColor = Color.White * alpha;
        finalColor.A = (byte)(alpha * 255);

        Main.EntitySpriteDraw(
            texture,
            drawPos,
            sourceRectangle,
            finalColor,
            0,
            sourceRectangle.Size() * 0.5f,
            squishScale * npc.scale,
            spriteEffects
        );

        return false;
    }
    #endregion
}