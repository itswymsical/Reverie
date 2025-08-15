using Reverie.Content.Items.Mycology;
using Reverie.Core.Missions;
using Reverie.Utilities;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;

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

    private const float TRIGGER_DIST = 120f;
    private const float ATTACK_DIST = 100f;
    private const float SETTLE_DIST = 320f;
    private const float MAX_SPEED = 2.5f;
    private const float FLEE_SPEED = 6f;
    private const float ACCEL = 0.15f;
    private const float DECEL = 0.85f;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[NPC.type] = 16;
        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
        {
            Velocity = 0f,
            Direction = 1
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
    }

    public override void SetDefaults()
    {
        NPC.width = 42;
        NPC.height = 30;
        NPC.damage = 8;
        NPC.defense = 16;
        NPC.lifeMax = 80;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath6;
        NPC.knockBackResist = 1.25f;
        NPC.aiStyle = -1;
        NPC.noGravity = false;
        NPC.noTileCollide = false;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        if (spawnInfo.Player.ZoneDirtLayerHeight && !spawnInfo.Water)
        {
            var missionPlayer = spawnInfo.Player.GetModPlayer<MissionPlayer>();
            var mission = missionPlayer.GetMission(MissionID.PuffballHunt);

            if (mission?.Progress == MissionProgress.Ongoing)
                return 0.3f;

            else if (mission?.Progress == MissionProgress.Completed)
                return 0.15f;
        }
        return 0f;
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
            float targetVelX = fleeDirection.X * FLEE_SPEED;
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
        var missionPlayer = Main.LocalPlayer.GetModPlayer<MissionPlayer>();
        var mission = missionPlayer.GetMission(MissionID.PuffballHunt);

        if (mission?.Progress == MissionProgress.Ongoing)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PuffballItem>(), 1));
        else
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PuffballItem>(), 1, 1, 3));
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Underground,
            new FlavorTextBestiaryInfoElement("A crafty mushroom creature that disguises itself as an innocent puffball. When threatened, it releases a cloud of spores and flees.")
        });
    }

    public override void FindFrame(int frameHeight)
    {
        switch (State)
        {
            case AIState.Dormant:
                if (SubTimer > 0f)
                {
                    // Emerging animation
                    int emergeFrame = (int)(SubTimer / 8f);
                    emergeFrame = Math.Min(emergeFrame, 6);
                    NPC.frame.Y = frameHeight * emergeFrame;
                }
                else
                {
                    // Fully dormant
                    NPC.frame.Y = 0;
                }
                break;

            case AIState.Active:
                // Active motion frames
                NPC.frameCounter++;
                if (NPC.frameCounter >= 6)
                {
                    NPC.frameCounter = 0;
                    int currentFrame = (NPC.frame.Y / frameHeight) - 7;
                    currentFrame = (currentFrame + 1) % 9;
                    NPC.frame.Y = frameHeight * (7 + currentFrame);
                }

                // Ensure we stay in active frame range
                if (NPC.frame.Y < frameHeight * 7 || NPC.frame.Y > frameHeight * 15)
                {
                    NPC.frame.Y = frameHeight * 7;
                }
                break;

            case AIState.Settling:
                // Reverse emerging animation
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