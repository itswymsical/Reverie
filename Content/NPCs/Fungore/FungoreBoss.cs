using Reverie.Utilities;
using Terraria;

namespace Reverie.Content.NPCs.Fungore;

public class FungoreBoss : ModNPC
{
    internal enum AttackState
    {
        IntroScene,
        Walking,
        Jumping,
        Leaping,
        Punching,
        Slam,
        DeathScene,
        Despawning
    }

    private AttackState State
    {
        get => (AttackState)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    private float AttackCooldown
    {
        get => NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    private float TimeGrounded
    {
        get => NPC.ai[2];
        set => NPC.ai[2] = value;
    }

    private int frameY;
    private int frameX;

    private Player player;

    private int punchDirection;

    private float alpha;
    private float alphaTimer;
    private int AITimer;
    private bool flag = false;
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 16;
    }

    public override void SetDefaults()
    {
        NPC.boss = true;
        NPC.lavaImmune = true;

        NPC.width = NPC.height = 88;

        DrawOffsetY = 22;

        NPC.lifeMax = 1150;
        NPC.defense = 18;
        NPC.damage = 22;

        NPC.knockBackResist = 0f;

        NPC.aiStyle = -1;

        NPC.HitSound = SoundID.DD2_OgreHurt;
        NPC.DeathSound = SoundID.DD2_SkeletonDeath;
        NPC.value = Item.buyPrice(gold: 3);

    }

    //public override void AI()
    //{
    //    base.AI();
    //}

    private void HandleAll()
    {
        HandleStates();
        HandleAttacks();
        HandleDespawn();
        HandleCollision(60, 6f);
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.spriteDirection = State == AttackState.Punching ? punchDirection : NPC.direction;

        NPC.frame.Width = 138;
        NPC.frame.Height = 134;
        NPC.frameCounter++;

        int frameRate = 4;

        if (State == AttackState.Punching && (frameY == 5 || frameY == 6))
            frameRate = 15;
        

        if (State == AttackState.Jumping && frameY == 6)
            frameRate = 35;
        
        if (State == AttackState.Leaping && frameY == 2)
            frameRate = 35;
        
        if (State == AttackState.Leaping && (frameY == 6 || frameY == 7 || frameY == 8))
            frameRate = 25;
        
        if (State == AttackState.Leaping && frameY == 9)
            frameRate = 30;
        
        if (NPC.frameCounter > frameRate)
        {
            frameY++;
            NPC.frameCounter = 0;
        }

        frameX = State == AttackState.Walking ? 0 : State == AttackState.Punching ? 1 : State == AttackState.Jumping ? 2 : 3;

        if (State == AttackState.Walking && frameY > 7)
            frameY = 0;
        
        if (State == AttackState.Jumping && frameY > 11)
            frameY = 0;
        
        if (State == AttackState.Leaping && frameY > 15)
            frameY = 0;
        
        NPC.frame.Y = frameY * frameHeight;
        NPC.frame.X = frameX * NPC.frame.Width;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) 
        => NPC.DrawNPCCenteredWithTexture(ModContent.Request<Texture2D>($"{Texture}").Value, spriteBatch, drawColor);

    public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
    {
        scale = 1.5f;
        return null;
    }
    private void HandleStates()
    {
        NPC.TargetClosest();

        player = Main.player[NPC.target];

        switch (State)
        {
            case AttackState.Walking:
                Walk();
                break;

            case AttackState.Punching:
                break;

            case AttackState.Jumping:
                break;

            case AttackState.Leaping:
                break;
        }
    }

    private void CheckPlatform(Player player)
    {
        bool onplatform = true;
        for (int i = (int)NPC.position.X; i < NPC.position.X + NPC.width; i += NPC.height / 2)
        {
            Tile tile = Framing.GetTileSafely(new Point((int)NPC.position.X / 16, (int)(NPC.position.Y + NPC.height + 8) / 16));
            if (!TileID.Sets.Platforms[tile.TileType])
                onplatform = false;
        }
        if (onplatform && (NPC.Center.Y < player.position.Y - 20))
            NPC.noTileCollide = true;
        else
            NPC.noTileCollide = false;
    }

    private void HandleAttacks()
    {
        const int MIN_ATTACK_COOLDOWN = 180;
        CheckPlatform(player);
        AttackCooldown++;

        if (AttackCooldown > MIN_ATTACK_COOLDOWN)
        {
            const float MIN_PUNCH_DISTANCE = 120;
            if (player.Distance(NPC.Center) < MIN_PUNCH_DISTANCE && (Main.rand.NextBool(20)) || (Main.rand.NextBool(52)))
            {
                punchDirection = Math.Sign(player.position.X - NPC.position.X);

                frameY = 0;

                State = AttackState.Punching;
                AttackCooldown = 0;
            }
            if (player.Distance(NPC.Center) > 220f && (Main.rand.NextBool(40)) || (Main.rand.NextBool(70)))
            {
                frameY = 0;

                State = AttackState.Leaping;
                AttackCooldown = 0;
            }

            if (NPC.velocity.X == 0 && AttackCooldown > 60)
            {
                State = AttackState.Leaping;
                AttackCooldown = 0;
            }
            if (Main.rand.NextBool(48))
            {
                frameY = 0;

                State = AttackState.Jumping;
                AttackCooldown = 0;
            }
        }
    }

    private void Walk()
    {
        const float MAX_SPEED = 2.7f;

        if (NPC.velocity.X < -MAX_SPEED || NPC.velocity.X > MAX_SPEED)
        {
            if (NPC.velocity.Y == 0f)
            {
                NPC.velocity *= 0.8f;
            }
        }
        else
        {
            if (NPC.velocity.X < MAX_SPEED && NPC.direction == 1)
            {
                NPC.velocity.X += 0.056f;
            }

            if (NPC.velocity.X > -MAX_SPEED && NPC.direction == -1)
            {
                NPC.velocity.X -= 0.056f;
            }

            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -MAX_SPEED, MAX_SPEED);
        }
        Dust.NewDust(NPC.oldPosition, NPC.width, NPC.height, DustID.MushroomSpray, NPC.oldVelocity.X, NPC.oldVelocity.Y, 0, Color.Coral, 1f);
    }

    private void HandleCollision(int maxTime, float jumpHeight)
    {
        if (NPC.velocity.Y == 0f)
        {
            TimeGrounded++;
        }
        else
        {
            TimeGrounded = 0;
        }

        if (TimeGrounded > maxTime && NPC.collideX && NPC.position.X == NPC.oldPosition.X)
        {
            NPC.velocity.Y = -jumpHeight;
        }

        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
    }

    private void HandleDespawn()
    {
        if (!player.active || player.dead)
        {
            flag = true;
            State = AttackState.Leaping;
            if (++AITimer == 220)
            {
                NPC.active = false;
                NPC.TargetClosest(false);
            }
        }
    }
}
