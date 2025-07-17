namespace Reverie.Content.NPCs.Bosses.Shelledrake;

public class Shelledrake : ModNPC
{
    internal enum ShelledrakeAI
    {
        Idle,
        Prance,
        Charge,
        Attack,
        Jump,
        Land
    }

    private ShelledrakeAI State
    {
        get => (ShelledrakeAI)NPC.ai[0];
        set => NPC.ai[0] = (int)value;
    }

    private float StateTimer
    {
        get => NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    private ShelledrakeAI currentState = ShelledrakeAI.Idle;
    private ShelledrakeAI previousState;

    private float TimeGrounded
    {
        get => NPC.ai[2];
        set => NPC.ai[2] = value;
    }

    private const float PRANCE_SPEED = 7.6f;
    private const float ACCELERATION = 0.02f;

    private Player player;
    private const float CHARGE_SPEED = 12f;
    private const float CHARGE_ACCELERATION = 0.15f;
    private const float CHARGE_DURATION = 6f * 60f;
    private const float BACKSTEP_DURATION = 45f;

    private const float PROJECTILE_SPEED = 8f;
    private const float PROJECTILE_INTERVAL = 20f;
    private const float ARC_ANGLE_MIN = -45f;
    private const float ARC_ANGLE_MAX = 45f;
    private const int PROJECTILES_PER_VOLLEY = 5;

    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
        Main.npcFrameCount[NPC.type] = 1;
    }

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.lifeMax = 3200;
        NPC.defense = 22;
        NPC.damage = 28;
        NPC.knockBackResist = 0f;

        NPC.boss = true;
        NPC.lavaImmune = true;

        NPC.width = 194;
        NPC.height = 116;
        NPC.value = Item.buyPrice(gold: 8, silver: 50);

        NPC.HitSound = SoundID.DD2_WitherBeastHurt;
        NPC.DeathSound = SoundID.DD2_WitherBeastDeath;
        if (!Main.dedServ)
            Music = MusicLoader.GetMusicSlot($"{MUSIC_DIRECTORY}AurelianEscapade");

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
        }
    }

    public override void AI()
    {
        UpdateState();
        HandleState();

        SlopedCollision();
        CheckPlatform(player);
    }

    #region State Management
    private void ChangeState(ShelledrakeAI newState)
    {
        previousState = currentState;
        currentState = newState;
        StateTimer = 0f;
    }

    private void UpdateState()
    {
        StateTimer++;
        NPC.TargetClosest();
        player = Main.player[NPC.target];
       
        switch (State)
        {
            case ShelledrakeAI.Idle:
                Prance();
                if (StateTimer >= 7.5f * 60f)
                {
                    ChangeState(ShelledrakeAI.Charge);
                }
                break;
            //case ShelledrakeAI.Prance:
            //    ChangeState(ShelledrakeAI.Prance);
            //    break;
            case ShelledrakeAI.Charge:
                if (StateTimer >= 3.5f * 60f)
                {
                    ChangeState(ShelledrakeAI.Attack);
                }
                break;
            case ShelledrakeAI.Attack:
                if (StateTimer >= 1.5f * 60f)
                {
                    ChangeState(ShelledrakeAI.Idle);
                }
                break;
            //case ShelledrakeAI.Land:
            //    ChangeState(ShelledrakeAI.Land);
            //    break;
        }
    }

    private void HandleState()
    {
        switch (currentState)
        {
            case ShelledrakeAI.Idle:
                Prance();
                break;
            case ShelledrakeAI.Charge:
                ChargeAttack();
                break;
            case ShelledrakeAI.Attack:
                ProjectileAttack();
                break;
        }
    }
    #endregion

    #region AI Functions
    private void Prance()
    {
        if (NPC.velocity.X < -PRANCE_SPEED || NPC.velocity.X > PRANCE_SPEED)
        {
            if (NPC.velocity.Y == 0f)
            {
                NPC.velocity *= 0.65f;
            }
        }
        else
        {
            if (NPC.velocity.X < PRANCE_SPEED && NPC.direction == 1)
            {
                NPC.velocity.X += ACCELERATION;
            }

            if (NPC.velocity.X > -PRANCE_SPEED && NPC.direction == -1)
            {
                NPC.velocity.X -= ACCELERATION;
            }

            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -PRANCE_SPEED, PRANCE_SPEED);
        }
    }

    private void ChargeAttack()
    {
        if (StateTimer < BACKSTEP_DURATION)
        {
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -4f * NPC.direction, 0.1f);
            return;
        }

        var chargeProgress = (StateTimer - BACKSTEP_DURATION) / (CHARGE_DURATION - BACKSTEP_DURATION);
        var targetSpeed = CHARGE_SPEED * NPC.direction;

        NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, targetSpeed, CHARGE_ACCELERATION);

        if (NPC.Distance(player.Center) < 116f)
        {
            ChangeState(ShelledrakeAI.Attack);
        }
    }

    private void ProjectileAttack()
    {
        if (StateTimer % PROJECTILE_INTERVAL == 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Vector2 projectileOrigin = new(
                NPC.position.X + (NPC.direction == 1 ? 0 : NPC.width),
                NPC.position.Y + NPC.height * 0.25f
            );

            var angleStep = (ARC_ANGLE_MAX - ARC_ANGLE_MIN) / (PROJECTILES_PER_VOLLEY - 1);

            for (var i = 0; i < PROJECTILES_PER_VOLLEY; i++)
            {
                var angle = MathHelper.ToRadians(ARC_ANGLE_MIN + angleStep * i);
                if (NPC.direction == -1)
                    angle += MathHelper.Pi;

                var velocity = angle.ToRotationVector2() * PROJECTILE_SPEED;

                var projectile = Projectile.NewProjectile(
                    NPC.GetSource_FromAI(),
                    projectileOrigin,
                    velocity,
                    ProjectileID.LifeCrystalBoulder,
                    10,
                    3f,
                    Main.myPlayer
                );
            }
        }
    }
    #endregion

    #region Collision Detection
    public void CheckPlatform(Player player)
    {
        var onplatform = true;
        for (var i = (int)NPC.position.X; i < NPC.position.X + NPC.width; i += NPC.height / 2)
        {
            var tile = Framing.GetTileSafely(new Point((int)NPC.position.X / 16, (int)(NPC.position.Y + NPC.height + 8) / 16));
            if (!TileID.Sets.Platforms[tile.TileType])
                onplatform = false;
        }
        if (onplatform && NPC.Center.Y < player.position.Y - 20)
            NPC.noTileCollide = true;
        else
            NPC.noTileCollide = false;
    }

    public void SlopedCollision()
    {
        var velocityDirection = Math.Sign(NPC.velocity.X);
        var targetPosition = NPC.position + new Vector2(NPC.velocity.X, 0);

        var tileX = (int)((targetPosition.X + NPC.width / 2 + (NPC.width / 2 + 1) * velocityDirection) / 16f);
        var tileY = (int)((targetPosition.Y + NPC.height - 1f) / 16f);

        var tile1 = Framing.GetTileSafely(tileX, tileY);
        var tile2 = Framing.GetTileSafely(tileX, tileY - 1);
        var tile3 = Framing.GetTileSafely(tileX, tileY - 2);
        var tile4 = Framing.GetTileSafely(tileX, tileY - 3);
        var tile5 = Framing.GetTileSafely(tileX, tileY - 4);
        var tile6 = Framing.GetTileSafely(tileX - velocityDirection, tileY - 3);

        if (tileX * 16 < targetPosition.X + NPC.width && tileX * 16 + 16 > targetPosition.X &&
            (tile1.HasUnactuatedTile && !tile1.TopSlope && !tile2.TopSlope && Main.tileSolid[tile1.TileType] && !Main.tileSolidTop[tile1.TileType] ||
            tile2.IsHalfBlock && tile2.HasUnactuatedTile) && (!tile2.HasUnactuatedTile || !Main.tileSolid[tile2.TileType] || Main.tileSolidTop[tile2.TileType] ||
            tile2.IsHalfBlock &&
            (!tile5.HasUnactuatedTile || !Main.tileSolid[tile5.TileType] || Main.tileSolidTop[tile5.TileType])) &&
            (!tile3.HasUnactuatedTile || !Main.tileSolid[tile3.TileType] || Main.tileSolidTop[tile3.TileType]) &&
            (!tile4.HasUnactuatedTile || !Main.tileSolid[tile4.TileType] || Main.tileSolidTop[tile4.TileType]) &&
            (!tile6.HasUnactuatedTile || !Main.tileSolid[tile6.TileType]))
        {
            float tileYPosition = tileY * 16;
            if (Main.tile[tileX, tileY].IsHalfBlock)
            {
                tileYPosition += 8f;
            }
            if (Main.tile[tileX, tileY - 1].IsHalfBlock)
            {
                tileYPosition -= 8f;
            }

            if (tileYPosition < targetPosition.Y + NPC.height)
            {
                var targetYPosition = targetPosition.Y + NPC.height - tileYPosition;
                if (targetYPosition <= 16.1f)
                {
                    NPC.gfxOffY += NPC.position.Y + NPC.height - tileYPosition;
                    NPC.position.Y = tileYPosition - NPC.height;

                    if (targetYPosition < 9f)
                    {
                        NPC.stepSpeed = 1f;
                    }
                    else
                    {
                        NPC.stepSpeed = 2f;
                    }
                }
            }
        }
    }

    #endregion
}
