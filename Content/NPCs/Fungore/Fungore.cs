using Reverie.Utilities;
using Terraria.Audio;
using Terraria.GameContent;

namespace Reverie.Content.NPCs.Fungore;

[AutoloadBossHead]
public class Fungore : ModNPC
{
    // States enum (unchanged)
    private enum States
    {
        Walking,
        Punching,
        Jumping,
        Leaping
    }

    // Attack-related constants
    private const float PUNCH_DISTANCE = 90f;
    private const float LEAP_DISTANCE = 180f;
    private const int MIN_ATTACK_COOLDOWN = 180;
    private const int STUCK_TIMER = 60;
    private const float OBSTACLE_JUMP_HEIGHT = 6f;

    // Movement constants
    private const float MAX_WALKING_SPEED = 2.05f;
    private const float WALKING_ACCELERATION = 0.056f;
    private const float MAX_LEAP_VELOCITY = 5.5f;
    private const float PUNCH_VELOCITY = 1.5265f;
    private const float JUMP_VELOCITY_MIN = -8.5f;
    private const float JUMP_VELOCITY_MAX = -7.5f;
    private const float LEAP_VELOCITY_MIN = -8.5f;
    private const float LEAP_VELOCITY_MAX = -7.5f;
    private const float DESPAIR_LEAP_VELOCITY = -20f;

    // Animation constants
    private const int DEFAULT_FRAME_RATE = 5;
    private const int PUNCH_FRAME_RATE = 5;
    private const int LEAP_FRAME_RATE = 10;
    private const int LEAP_END_FRAME_RATE = 24;
    private const int JUMP_FRAME_RATE = 28;
    private const int COLLISION_FRAME_RATE = 4;

    // State property (unchanged)
    private States State
    {
        get => (States)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    // AttackCooldown property (unchanged)
    private float AttackCooldown
    {
        get => NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    // TimeGrounded property (unchanged)
    private float TimeGrounded
    {
        get => NPC.ai[2];
        set => NPC.ai[2] = value;
    }

    private Player player;

    private int frameY;
    private int frameX;
    private int frameRate;
    private int punchDirection;

    private Vector2 scale = Vector2.One;

    private int AITimer;
    private bool isDespairing = false;
    private bool hasPlayedAttackSound = false;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 16;
    }

    public override void SetDefaults()
    {
        NPC.damage = 21;
        NPC.defense = 15;
        NPC.lifeMax = 1112;

        NPC.width = NPC.height = 88;
        DrawOffsetY = 22;

        NPC.boss = true;
        NPC.lavaImmune = true;
        NPC.knockBackResist = 0f;

        NPC.aiStyle = -1;

        NPC.value = Item.buyPrice(gold: 1);

        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.DD2_OgreDeath;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.spriteDirection = State == States.Punching ? punchDirection : NPC.direction;

        NPC.frame.Width = 138;
        NPC.frame.Height = 134;
        NPC.frameCounter++;

        frameRate = DEFAULT_FRAME_RATE;

        // Set frame rates based on state and animation progress
        if (State == States.Punching && frameY > 4 && frameY < 7)
            frameRate = PUNCH_FRAME_RATE;

        if (State == States.Leaping && frameY > 4 && frameY < 8)
            frameRate = LEAP_FRAME_RATE;

        if (State == States.Leaping && frameY == 9)
            frameRate = LEAP_END_FRAME_RATE;

        if (State == States.Jumping && frameY >= 4 && frameY <= 8)
            frameRate = JUMP_FRAME_RATE;

        if (NPC.frameCounter > frameRate)
        {
            frameY++;
            NPC.frameCounter = 0;
        }

        // Set the appropriate frame column based on state
        frameX = State == States.Walking ? 0 :
                 State == States.Punching ? 1 :
                 State == States.Jumping ? 2 : 3;

        // Handle animation looping
        if (State == States.Walking && frameY > 7)
        {
            frameY = 0;
        }
        if (State == States.Jumping && frameY > 11)
        {
            frameY = 0;
        }
        if (State == States.Leaping && frameY > 15)
        {
            frameY = 0;
        }

        // Set the frame position
        NPC.frame.Y = frameY * frameHeight;
        NPC.frame.X = frameX * NPC.frame.Width;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        float damage = NPC.damage;

        if (Main.expertMode)
        {
            NPC.damage += (int)(damage * .2f);
            bossAdjustment = NPC.life;
            NPC.life += (int)(bossAdjustment * .2f);
        }
        if (Main.masterMode)
        {
            NPC.damage += (int)(damage * .35f);

            NPC.life += (int)(bossAdjustment * .35f);
            NPC.defense = 17;
        }
    }

    public override void AI() => HandleAll();

    private void HandleAll()
    {
        HandleStates();
        HandleAttacks();
        HandleDespawn();
        HandleCollision();
        HandleScale();
    }

    private void HandleStates()
    {
        NPC.TargetClosest(true);
        player = Main.player[NPC.target];

        switch (State)
        {
            case States.Walking:
                Walk();
                break;

            case States.Punching:
                Punch();
                break;

            case States.Jumping:
                Jump();
                break;

            case States.Leaping:
                Leap();
                break;
        }
    }

    private void CheckPlatform(Player player)
    {
        var onplatform = true;
        for (var i = (int)NPC.position.X; i < NPC.position.X + NPC.width; i += NPC.height / 2)
        {
            var tile = Framing.GetTileSafely(new Point((int)NPC.position.X / 16, (int)(NPC.position.Y + NPC.height + 8) / 16));
            if (!TileID.Sets.Platforms[tile.TileType])
                onplatform = false;
        }

        NPC.noTileCollide = onplatform && NPC.Center.Y < player.position.Y - 20;
    }

    private void HandleAttacks()
    {
        CheckPlatform(player);
        AttackCooldown++;

        // Don't check for new attacks if we're still on cooldown
        if (AttackCooldown <= MIN_ATTACK_COOLDOWN)
            return;

        // Prioritize attacks with clear conditions    
        if (ShouldPunch())
        {
            TransitionToPunching();
        }
        else if (ShouldLeap())
        {
            TransitionToLeaping();
        }
        else if (ShouldJump())
        {
            TransitionToJumping();
        }
    }

    private bool ShouldPunch()
    {
        return player.Distance(NPC.Center) < PUNCH_DISTANCE && (Main.rand.NextBool(20) || Main.rand.NextBool(30));
    }

    private bool ShouldLeap()
    {
        return player.Distance(NPC.Center) > LEAP_DISTANCE && (Main.rand.NextBool(30) || Main.rand.NextBool(60))
            || NPC.velocity.X == 0 && AttackCooldown > 60;
    }

    private bool ShouldJump()
    {
        return Main.rand.NextBool(32);
    }

    private void TransitionToPunching()
    {
        punchDirection = Math.Sign(player.position.X - NPC.position.X);
        frameY = 0;
        State = States.Punching;
        AttackCooldown = 0;
    }

    private void TransitionToLeaping()
    {
        frameY = 0;
        State = States.Leaping;
        AttackCooldown = 0;
    }

    private void TransitionToJumping()
    {
        frameY = 0;
        State = States.Jumping;
        AttackCooldown = 0;
    }

    private void HandleDespawn()
    {
        if (!player.active || player.dead)
        {
            isDespairing = true;
            State = States.Leaping;
            if (++AITimer == 220)
            {
                NPC.active = false;
                NPC.TargetClosest(false);
            }
        }
    }

    private void HandleCollision()
    {
        // Track time spent on ground
        if (NPC.velocity.Y == 0f)
        {
            TimeGrounded++;
        }
        else
        {
            TimeGrounded = 0;
        }

        // Auto-jump if stuck against a wall
        if (TimeGrounded > STUCK_TIMER && NPC.collideX && NPC.position.X == NPC.oldPosition.X)
        {
            NPC.velocity.Y = -OBSTACLE_JUMP_HEIGHT;
        }

        // Handle step-up for smoother movement
        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
    }

    private void Walk()
    {
        if (NPC.velocity.X < -MAX_WALKING_SPEED || NPC.velocity.X > MAX_WALKING_SPEED)
        {
            if (NPC.velocity.Y == 0f)
            {
                NPC.velocity *= 0.65f;
            }
        }
        else
        {
            if (NPC.velocity.X < MAX_WALKING_SPEED && NPC.direction == 1)
            {
                NPC.velocity.X += WALKING_ACCELERATION;
            }

            if (NPC.velocity.X > -MAX_WALKING_SPEED && NPC.direction == -1)
            {
                NPC.velocity.X -= WALKING_ACCELERATION;
            }

            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -MAX_WALKING_SPEED, MAX_WALKING_SPEED);
        }
    }

    private void PerformLeap()
    {
        NPC.knockBackResist = 0f;
        NPC.noTileCollide = true;

        if (NPC.velocity.X < -MAX_LEAP_VELOCITY || NPC.velocity.X > MAX_LEAP_VELOCITY)
        {
            if (NPC.velocity.Y == 0f)
            {
                NPC.velocity *= 3f;
            }
        }
        else
        {
            if (!(frameY >= 6 && State == States.Leaping))
            {
                if (NPC.velocity.X < MAX_LEAP_VELOCITY && NPC.direction == 1)
                {
                    NPC.velocity.X += 1f;
                }

                if (NPC.velocity.X > -MAX_LEAP_VELOCITY && NPC.direction == -1)
                {
                    NPC.velocity.X -= 1f;
                }
            }
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -MAX_LEAP_VELOCITY, MAX_LEAP_VELOCITY);
        }
    }

    private void Punch()
    {
        NPC.knockBackResist = 0f;

        // Play attack sound at the appropriate frame
        if (frameY == 3 && !hasPlayedAttackSound)
        {
            SoundEngine.PlaySound(SoundID.DD2_OgreAttack, NPC.position);
            hasPlayedAttackSound = true;
        }

        // Apply forward momentum during punch frames
        if (frameY == 4 || frameY == 5)
        {
            NPC.velocity.X += punchDirection * PUNCH_VELOCITY;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -5f, 5f);
        }
        else
        {
            NPC.velocity.X *= 0.95f;
        }

        // Handle visual stretching during punch
        if (NPC.velocity.X != 0f)
        {
            var scaleMult = Math.Abs(NPC.velocity.X) * 0.05f;
            scale.X += scaleMult;

            if (scale.X > 1.75f)
            {
                scale.X -= scaleMult;
            }
        }

        // Go back to walking after finishing punching or if colliding with a wall
        if (frameY > 10 || NPC.collideX)
        {
            hasPlayedAttackSound = false;
            frameY = 0;
            State = States.Walking;
        }
    }

    private void Jump()
    {
        Walk();

        // Play attack sound at the appropriate frame
        if (frameY == 4)
            SoundEngine.PlaySound(SoundID.DD2_OgreAttack, NPC.position);

        NPC.knockBackResist = 0f;

        // Apply jump velocity at the correct point in the animation
        if (frameY > 2 && frameY < 4)
        {
            NPC.velocity.Y = Main.rand.NextFloat(JUMP_VELOCITY_MAX, JUMP_VELOCITY_MIN);
            NPC.TargetClosest();
            NPC.netUpdate = true;

            if (NPC.velocity.Y >= 0f)
            {
                Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY, 1, false, 1);
            }
        }

        // Adjust animation speed when colliding
        if (NPC.collideY || NPC.collideX)
        {
            frameRate = COLLISION_FRAME_RATE;
        }

        // Handle landing impact effects
        if (frameY == 7 && (NPC.collideY || NPC.collideX))
        {
            if (!hasPlayedAttackSound)
            {
                SoundEngine.PlaySound(SoundID.DD2_OgreGroundPound, NPC.position);
                // Spawn ground pound projectile (commented out)
                hasPlayedAttackSound = true;
            }

            // Spawn mushroom projectiles (commented out)
            for (var i = 0; i < 6; i++)
            {
                // Projectile code commented out
            }
        }

        // Return to walking state at end of animation
        if (frameY == 11 && (NPC.collideY || NPC.collideX))
        {
            hasPlayedAttackSound = false;
            frameY = 0;
            State = States.Walking;
        }
        if (frameY > 15 && (NPC.collideY || NPC.collideX))
        {
            hasPlayedAttackSound = false;
            SoundEngine.PlaySound(SoundID.DD2_OgreGroundPound, NPC.position);
            frameY = 0;
            State = States.Walking;
        }
    }

    private void Leap()
    {
        NPC.knockBackResist = 0f;

        // Play attack sound at the appropriate frame
        if (frameY == 4)
            SoundEngine.PlaySound(SoundID.DD2_OgreAttack, NPC.position);

        // Start with no horizontal movement
        if (frameY < 4)
        {
            NPC.velocity.X = 0;
        }
        else
        {
            PerformLeap();
        }

        // Apply leap velocity at the correct animation frame
        if (frameY > 2 && frameY < 4)
        {
            NPC.noTileCollide = false;
            NPC.velocity.Y = Main.rand.NextFloat(LEAP_VELOCITY_MAX, LEAP_VELOCITY_MIN);
            NPC.TargetClosest();
            NPC.netUpdate = true;

            if (NPC.velocity.Y >= 0f)
                Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY, 1, false, 1);

            // Special case for despawning
            if (isDespairing)
            {
                frameRate = DEFAULT_FRAME_RATE;
                if (frameY == 4)
                {
                    NPC.velocity.Y = DESPAIR_LEAP_VELOCITY;
                }
            }
        }

        // Adjust animation speed when colliding
        if (NPC.collideY || NPC.collideX)
        {
            frameRate = COLLISION_FRAME_RATE;
        }

        // Handle landing impact effects
        if (frameY == 9 && (NPC.collideY || NPC.collideX))
        {
            NPC.velocity.X = 0;
            frameRate = COLLISION_FRAME_RATE;

            if (!hasPlayedAttackSound)
            {
                SoundEngine.PlaySound(SoundID.DD2_OgreGroundPound, NPC.position);
                Dust.NewDust(NPC.oldPosition, NPC.width, NPC.height, DustID.OrangeTorch, NPC.oldVelocity.X, NPC.oldVelocity.Y, 0, default, 1f);
                // Spawn slam projectile (commented out)
                hasPlayedAttackSound = true;
            }

            // Spawn mushroom projectiles (commented out)
            for (var i = 0; i < 12; ++i)
            {
                // Projectile code commented out
            }
        }

        // Return to walking state at end of animation
        if (frameY == 15 && (NPC.collideY || NPC.collideX))
        {
            hasPlayedAttackSound = false;
            frameY = 0;
            State = States.Walking;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        => NPC.DrawNPCCenteredWithTexture(TextureAssets.Npc[NPC.type].Value, spriteBatch, drawColor);

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        scale = 1.5f;
        return null;
    }

    private void HandleScale()
    {
        var targetScale = Vector2.One;

        if (scale.Y != targetScale.Y)
        {
            scale.Y = MathHelper.Lerp(scale.Y, targetScale.Y, 0.33f);
        }

        if (scale.X != targetScale.X)
        {
            scale.X = MathHelper.Lerp(scale.X, targetScale.X, 0.33f);
        }
    }
}