using Reverie.Content.Items.Mycology;
using Reverie.Core.Missions;
using Reverie.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader.Utilities;

namespace Reverie.Content.NPCs.Enemies.Underground;
public class Scrunglepuff : ModNPC
{
    private enum AIState
    {
        Dormant,
        Active,
        Settling
    }

    private AIState State
    {
        get => (AIState)NPC.ai[0];
        set => NPC.ai[0] = (float)value;
    }

    private float Timer
    {
        get => NPC.ai[1];
        set => NPC.ai[1] = value;
    }

    private float SubTimer
    {
        get => NPC.ai[2];
        set => NPC.ai[2] = value;
    }

    private const float TRIGGER_DIST = 180f;
    private const float ATTACK_DIST = 120f;
    private const float SETTLE_DIST = 300f;
    private const float SPEED = 6f;
    private const float ACCEL = 0.09f;
    private const float DECEL = 0.85f;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 16;
        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
        {
            Velocity = 0f,
            Direction = 1,
            Frame = 6,
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
    }
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange([BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
            new FlavorTextBestiaryInfoElement("Mods.Reverie.NPCs.Scrunglepuff.Bestiary")
        ]);
    }

    public override void SetDefaults()
    {
        NPC.width = 42;
        NPC.height = 30;
        NPC.damage = 8;
        NPC.defense = 5;
        NPC.lifeMax = 80;
        NPC.HitSound = new SoundStyle($"{SFX_DIRECTORY}ScrunglepuffHit");
        NPC.DeathSound = SoundID.NPCDeath53 with { Pitch = 0.6f};
        NPC.knockBackResist = 1.5f;
        NPC.value = Item.buyPrice(copper: 80);
        NPC.aiStyle = -1;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (spawnInfo.PlayerSafe || spawnInfo.Water)
            return 0f;

        bool validZone = spawnInfo.Player.ZoneDirtLayerHeight || spawnInfo.Player.ZoneRockLayerHeight;
        if (!validZone)
            return 0f;

        var missionPlayer = spawnInfo.Player.GetModPlayer<MissionPlayer>();
        var mission = missionPlayer.GetMission(MissionID.PuffballHunt);

        if (mission?.Status == MissionStatus.Locked || mission?.Progress == MissionProgress.Inactive)
            return 0f;

        return mission.Progress == MissionProgress.Ongoing ? 0.85f : 0.25f;
    }

    public override void OnSpawn(IEntitySource source)
    {
        State = AIState.Dormant;
        Timer = 0f;
        SubTimer = 0f;
    }

    public override void AI()
    {
        Player nearestPlayer = Main.player[NPC.target];

        NPC.TargetClosest(false);
        float distanceToPlayer = Vector2.Distance(NPC.Center, nearestPlayer.Center);

        NPCUtils.SlopedCollision(NPC);
        NPCUtils.CheckPlatform(NPC, nearestPlayer);

        Timer++;

        if (NPC.justHit)
        {
            State = AIState.Active;
            ActiveBehavior(nearestPlayer, distanceToPlayer);
        }
        switch (State)
        {
            case AIState.Dormant:
                DormantBehavior(nearestPlayer, distanceToPlayer);
                break;

            case AIState.Active:
                ActiveBehavior(nearestPlayer, distanceToPlayer);
                break;

            case AIState.Settling:
                SettlingBehavior(nearestPlayer, distanceToPlayer);
                break;
        }

        if (!NPC.noGravity)
        {
            NPC.velocity.Y += 0.1f;
            if (NPC.velocity.Y > 8f)
                NPC.velocity.Y = 8f;
        }

        NPC.velocity.X *= DECEL;
    }

    private void DormantBehavior(Player player, float distance)
    {
        if (distance < TRIGGER_DIST)
        {
            SubTimer++;
            if (SubTimer >= 32f)
            {
                State = AIState.Active;
                Timer = 0f;
                SubTimer = 0f;
            }
        }
        else
        {
            SubTimer = 0f;
        }
    }

    private void ActiveBehavior(Player player, float distance)
    {
        Vector2 fleeDirection = NPC.Center - player.Center;
        if (fleeDirection != Vector2.Zero)
        {
            fleeDirection.Normalize();
            float targetVelX = fleeDirection.X * SPEED;
            NPC.velocity.X += (targetVelX - NPC.velocity.X) * ACCEL;
        }

        NPC.direction = NPC.velocity.X > 0 ? 1 : -1;
        NPC.spriteDirection = NPC.direction;

        if (distance < ATTACK_DIST && SubTimer <= 0f)
        {
            SpraySpores(player);
            SubTimer = 40f;
        }

        if (SubTimer > 0f)
            SubTimer--;

        if (distance > SETTLE_DIST)
        {
            State = AIState.Settling;
            Timer = 0f;
            SubTimer = 0f;
        }
    }

    private void SettlingBehavior(Player player, float distance)
    {
        SubTimer++;

        if (distance < TRIGGER_DIST)
        {
            State = AIState.Active;
            Timer = 0f;
            SubTimer = 0f;
            return;
        }

        if (SubTimer >= 48f)
        {
            State = AIState.Dormant;
            Timer = 0f;
            SubTimer = 0f;
        }
    }

    private void SpraySpores(Player target)
    {
        int sporeCount = 5;
        float baseAngle = (target.Center - NPC.Center).ToRotation();

        for (int i = 0; i < sporeCount; i++)
        {
            float angle = baseAngle + MathHelper.ToRadians(-30 + (60f / (sporeCount - 1)) * i);
            Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 6f;

            // Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, 
            //     ModContent.ProjectileType<SporeProjectile>(), 10, 1f);
        }

        SoundEngine.PlaySound(SoundID.Item17, NPC.position);

        for (int i = 0; i < 16; i++)
        {
            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                DustID.Smoke, 0f, -1f, 100, default, 1.2f);
            dust.velocity *= 0.5f;
        }
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PuffballItem>(), 1));
    }

    public override void FindFrame(int frameHeight)
    {
        switch (State)
        {
            case AIState.Dormant:
                if (SubTimer > 0f)
                {
                    if (SubTimer == 1f)
                    {
                        NPC.velocity.Y = -4f;
                        SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}ScrunglepuffEmerge"), NPC.position);
                    }

                    int emergeFrame = (int)(SubTimer / 4f);
                    emergeFrame = Math.Min(emergeFrame, 6);
                    NPC.frame.Y = frameHeight * emergeFrame;

                    float swayAmount = 0.2f;
                    NPC.rotation = MathF.Sin(SubTimer * 0.2f) * swayAmount * (emergeFrame / 6f);
                }
                else
                {
                    // hiding
                    NPC.frame.Y = 0;
                }
                break;

            case AIState.Active:
                NPC.frameCounter++;
                if (NPC.frameCounter >= 6)
                {
                    NPC.frameCounter = 0;
                    int currentFrame = (NPC.frame.Y / frameHeight) - 7;
                    currentFrame = (currentFrame + 1) % 9;
                    NPC.frame.Y = frameHeight * (7 + currentFrame);
                }

                if (NPC.frame.Y < frameHeight * 7 || NPC.frame.Y > frameHeight * 15)
                {
                    NPC.frame.Y = frameHeight * 7;
                }
                break;

            case AIState.Settling:
                int settleFrame = 6 - (int)(SubTimer / 8f);
                settleFrame = Math.Max(settleFrame, 0);
                NPC.frame.Y = frameHeight * settleFrame;
                break;
        }
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        for (int i = 0; i < 16; i++)
        {
            Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height,
                DustID.Smoke, hit.HitDirection * 2f, -1f, newColor: new(200, 200, 200, 50));
            dust.velocity.X += Main.rand.NextFloat(-1f, 1f);
        }
    }
}