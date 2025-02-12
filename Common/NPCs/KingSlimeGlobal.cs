using Reverie.Common.Systems;
using Reverie.Utilities;
using Terraria.Audio;

namespace Reverie.Common.NPCs;

public enum KingSlimeState
{
    Idle,
    Strolling,
    Jumping,
    Shooting,
    PrepareSlamAttack,
    SlamAttack,
    PrepareTeleport,
    Teleporting,
    Reappearing,
    Despawning
}
public class KingSlimeGlobal : GlobalNPC
{
    public override bool InstancePerEntity => true;
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.KingSlime;

    private Vector2 squishScale = Vector2.One;
    private readonly float rotation;

    // State management
    private KingSlimeState currentState = KingSlimeState.Idle;
    private KingSlimeState previousState;
    private float stateTimer;
    private float jumpTimer;
    private int consecutiveSlams;

    // Constants
    private const float PHASE_2_THRESHOLD = 0.65f;
    private const int MAX_CONSECUTIVE_SLAMS = 3;
    private const float JUMP_DURATION = 5 * 60f;
    private const float SHOOT_DURATION = 5 * 60f;
    private const float STROLL_SPEED = .65f;
    private const float IDLE_DURATION = 3.5f * 60f;
    private const float SLAM_SPEED = 13f;


    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Main.npc[NPCID.KingSlime].type] = 5;
    }
    public override void SetDefaults(NPC npc)
    {
        if (npc.type != NPCID.KingSlime) return;

        npc.aiStyle = -1;
        npc.width = 158;
        npc.height = 100;
        npc.damage = 16;
        npc.defense = 8;
        npc.lifeMax = 950;
        npc.value = Item.buyPrice(gold: 2);
        npc.knockBackResist = 0f;
        npc.boss = true;
        npc.noGravity = false;
        npc.noTileCollide = false;
        npc.scale = 1.35f;
        npc.alpha = 35;
        npc.HitSound = new SoundStyle($"{SFX_DIRECTORY}KingSlimeHit") with { PitchVariance = 0.3f };
        npc.DeathSound = new SoundStyle($"{SFX_DIRECTORY}KingSlimeDeath") with { PitchVariance = 0.2f };
    }
    

    public override void AI(NPC npc)
    {
        if (npc.type != NPCID.KingSlime) return;

        Main.musicBox2 = MusicLoader.GetMusicSlot(Reverie.Instance, "Assets/Music/GelatinousJoust");
        UpdateHealthBasedScale(npc);

        UpdateState(npc);
        HandleState(npc);
        UpdateHitbox(npc);

        NPCUtils.SlopedCollision(npc);
        NPCUtils.CheckPlatform(npc, Main.player[npc.target]);
    }

    private void UpdateHealthBasedScale(NPC npc)
    {
        float healthPercentage = (float)npc.life / npc.lifeMax;
        npc.scale = MathHelper.Lerp(0.65f, 1.35f, healthPercentage);

        if (npc.scale < 0.45f)
        {
            npc.scale = 0.45f;
        }
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

    private const int MAX_JUMPS = 2;
    private int currentJumps = 0;
    private void UpdateState(NPC npc)
    {
        stateTimer++;
        HandleStrolling(npc);
        float healthPercentage = (float)npc.life / npc.lifeMax;

        switch (currentState)
        {
            case KingSlimeState.Idle:
                if (stateTimer >= IDLE_DURATION)
                {
                    if (healthPercentage <= PHASE_2_THRESHOLD)
                    {
                        consecutiveSlams = 0;
                        ChangeState(KingSlimeState.PrepareSlamAttack);
                    }
                    else
                        ChangeState(KingSlimeState.Shooting);       
                }
                break;

            case KingSlimeState.Shooting:
                if (stateTimer >= SHOOT_DURATION)
                {
                    if (healthPercentage <= PHASE_2_THRESHOLD)
                    {
                        consecutiveSlams = 0;
                        ChangeState(KingSlimeState.PrepareSlamAttack);
                    }
                    else
                        ChangeState(KingSlimeState.Jumping);                
                }
                break;

            case KingSlimeState.Jumping:
                if (stateTimer >= JUMP_DURATION || npc.velocity.Y == 0f)
                {
                    ChangeState(KingSlimeState.PrepareSlamAttack);
                    stateTimer = 0f;
                    currentJumps++;
                }
                break;

            case KingSlimeState.PrepareSlamAttack:
                if (HandleSlamSetup(npc))
                {
                    ChangeState(KingSlimeState.SlamAttack);
                }
                break;

            case KingSlimeState.SlamAttack:
                if (HandleSlam(npc))
                {
                    consecutiveSlams++;
                    if (consecutiveSlams >= MAX_CONSECUTIVE_SLAMS)
                    {
                        ChangeState(KingSlimeState.Idle);
                    }
                    else
                        ChangeState(KingSlimeState.PrepareSlamAttack);       
                }
                break;
        }
    }

    private void ChangeState(KingSlimeState newState)
    {
        previousState = currentState;
        currentState = newState;
        stateTimer = 0f;
        OnStateEnter(newState);
    }

    private void OnStateEnter(KingSlimeState state)
    {
        switch (state)
        {
            case KingSlimeState.Jumping:
                jumpTimer = 0f;
                break;
        }
    }

    private void HandleState(NPC npc)
    {
        switch (currentState)
        {
            case KingSlimeState.Strolling:
                HandleStrolling(npc);
                break;
            case KingSlimeState.Jumping:
                HandleJumping(npc);
                break;
            case KingSlimeState.Shooting:
                HandleShooting(npc);
                break;
        }
        UpdateVisualEffects(npc);
    }

    private static void HandleStrolling(NPC npc)
    {
        if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead)
            npc.TargetClosest();

        Player target = Main.player[npc.target];

        npc.direction = npc.spriteDirection = npc.Center.X < target.Center.X ? 1 : -1;

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

    private void HandleSlimes(NPC npc)
    {
        for (int slimes = 0; slimes < 3; slimes++)
        {
            NPC slime = Main.npc[NPC.NewNPC(default, (int)npc.Center.X + ((slimes * 33) * npc.direction), (int)npc.Center.Y, NPCID.BlueSlime)];
            slime.active = true;
            slime.life = 30;
            slime.lifeMax = 30;
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

    private static void ShootProjectiles(NPC npc)
    {
        Player player = Main.player[npc.target];
        float projectileSpeed = 6.8f;
        int projectileType = ProjectileID.SpikedSlimeSpike;
        int damage = 8;

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

                float baseJumpHeight = -8f - (currentJumps * 2f);
                float baseXVelocity = 2f + (currentJumps * 0.5f);

                if (currentState == KingSlimeState.Jumping && stateTimer >= JUMP_DURATION * 0.8f)
                {
                    npc.velocity.Y = baseJumpHeight * 1.6f;
                    npc.velocity.X += baseXVelocity * 0.875f * npc.direction;
                    stateTimer = -320f;
                }
                else if (currentState == KingSlimeState.Jumping && stateTimer >= JUMP_DURATION * 0.5f)
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

    public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
    {
        if (npc.type != NPCID.KingSlime) return;

        for (int i = 0; i < 10; i++)
        {
            int dust = Dust.NewDust(npc.position, npc.width, npc.height,
                DustID.t_Slime, npc.velocity.X * 0.4f, npc.velocity.Y * 0.4f,
                150, new Color(78, 136, 255, 150), 1.5f);
            Main.dust[dust].noGravity = true;
            Main.dust[dust].velocity *= 0.4f;
        }
    }

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
            rotation,
            sourceRectangle.Size() * 0.5f,
            squishScale * npc.scale,
            spriteEffects
        );

        return false;
    }
}