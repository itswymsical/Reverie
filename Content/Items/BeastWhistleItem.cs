using Reverie.Core.Dialogue;
using System.IO;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader.IO;

namespace Reverie.Content.Items;

public class BeastWhistleItem : ModItem
{
    public override string Texture => PLACEHOLDER;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.width = Item.height = 24;
        Item.rare = ItemRarityID.Blue;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.consumable = false;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.myPlayer == player.whoAmI)
        {
            SoundEngine.PlaySound(SoundID.ForceRoar, player.Center);
            float whistleRange = 400f;
            int critttersAffected = 0;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (!npc.active || npc.aiStyle != 7)
                    continue;

                if (npc.townNPC || npc.isLikeATownNPC || npc.boss)
                    continue;

                float distance = Vector2.Distance(player.Center, npc.Center);
                if (distance <= whistleRange)
                {
                    int poo = ItemID.PoopBlock;
                    Item.NewItem(npc.GetSource_Loot(), npc.getRect(), poo, 1);

                    npc.aiStyle = -1;
                    npc.ai[0] = 0f;
                    npc.ai[1] = player.whoAmI;
                    npc.netUpdate = true;

                    var globalNPC = npc.GetGlobalNPC<BeastWhistleGlobal>();
                    globalNPC.isFleeingCritter = true;

                    for (int j = 0; j < 8; j++)
                    {
                        Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height,
                            DustID.Dirt, 0f, -2f, 100, Color.SaddleBrown, 0.8f);
                        dust.velocity *= 0.5f;
                    }

                    critttersAffected++;
                }
            }

            if (critttersAffected > 0)
            {
                SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}CritterJump").WithPitchOffset(-0.3f), player.Center);
            }
        }
        return true;
    }
}

public class BeastWhistleGlobal : GlobalNPC
{
    public bool isFleeingCritter = false;

    public override bool InstancePerEntity => true;

    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        base.ModifyNPCLoot(npc, npcLoot);
        if (npc.type == NPCID.Deerclops)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BeastWhistleItem>(), 12, 1, 1)); 
        }
    }
    public override bool PreAI(NPC npc)
    {
        if (isFleeingCritter && npc.aiStyle == -1)
        {
            FleeingCritterAI(npc);
            return false;
        }

        return true;
    }

    private void FleeingCritterAI(NPC npc)
    {
        npc.ai[0]++;

        const float SPEED = 3.5f;

        Player targetPlayer = null;
        int storedPlayerIndex = (int)npc.ai[1];

        if (storedPlayerIndex >= 0 && storedPlayerIndex < Main.maxPlayers && Main.player[storedPlayerIndex].active)
        {
            targetPlayer = Main.player[storedPlayerIndex];
        }
        else
        {
            // Find closest player if stored player is invalid
            float closestDistance = float.MaxValue;
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active)
                {
                    float distance = Vector2.Distance(npc.Center, player.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        targetPlayer = player;
                    }
                }
            }
        }

        if (npc.ai[0] > 180 && targetPlayer != null)
        {
            float distanceToPlayer = Vector2.Distance(npc.Center, targetPlayer.Center);
            Vector2 lastPos = new Vector2(npc.ai[2], npc.ai[3]);

            bool isStuck = false;
            if (npc.ai[0] > 240)
            {
                float distanceMoved = Vector2.Distance(npc.Center, lastPos);
                if (distanceMoved < 16f)
                {
                    isStuck = true;
                }
            }

            bool playerTooClose = distanceToPlayer < 48f;

            if (isStuck || playerTooClose)
            {
                SoundEngine.PlaySound(new SoundStyle($"{SFX_DIRECTORY}CritterJump").WithPitchOffset(0.5f), npc.Center);

                for (int i = 0; i < 10; i++)
                {
                    Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height,
                        DustID.Smoke, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f),
                        100, Color.Red, 0.8f);
                }

                npc.StrikeInstantKill();
                return;
            }
        }

        if (npc.ai[0] % 60 == 0)
        {
            npc.ai[2] = npc.Center.X;
            npc.ai[3] = npc.Center.Y;
        }

        if (targetPlayer != null)
        {
            Vector2 runDirection = Vector2.Normalize(npc.Center - targetPlayer.Center);

            if (float.IsNaN(runDirection.X) || float.IsNaN(runDirection.Y))
                runDirection = new Vector2(npc.direction, 0);

            int targetDirection = runDirection.X > 0 ? 1 : -1;
            npc.direction = targetDirection;
            npc.spriteDirection = npc.direction;

            if (npc.velocity.X < -SPEED || npc.velocity.X > SPEED)
            {
                if (npc.velocity.Y == 0f)
                {
                    npc.velocity *= 0.8f; // Apply friction when too fast
                }
            }
            else
            {
                // Accelerate in flee direction
                float acceleration = 0.15f; // Faster acceleration than normal critters

                if (npc.velocity.X < SPEED && npc.direction == 1)
                {
                    npc.velocity.X += acceleration;
                }
                if (npc.velocity.X > -SPEED && npc.direction == -1)
                {
                    npc.velocity.X -= acceleration;
                }

                // Clamp velocity to max speed
                npc.velocity.X = MathHelper.Clamp(npc.velocity.X, -SPEED, SPEED);
            }

            // Add some panic variation occasionally
            if (npc.ai[0] % 45 == 0) // Every 0.75 seconds
            {
                npc.velocity.X += Main.rand.NextFloat(-0.5f, 0.5f);
            }
        }
        else
        {
            // No target player, slow down
            npc.velocity.X *= 0.8f;
        }

        // Apply gravity (from vanilla)
        npc.velocity.Y += 0.4f;
        if (npc.velocity.Y > 10f)
            npc.velocity.Y = 10f;

        // CRITICAL: Use proper collision logic from CritterNPCActor
        Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);

        // Check if should despawn
        bool shouldDespawn = false;

        // Timer-based despawn (10 seconds)
        if (npc.ai[0] > 600)
            shouldDespawn = true;

        // Offscreen despawn check
        bool offscreen = true;
        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player player = Main.player[i];
            if (player.active)
            {
                Rectangle screenRect = new Rectangle(
                    (int)player.position.X - Main.screenWidth / 2 - 300,
                    (int)player.position.Y - Main.screenHeight / 2 - 300,
                    Main.screenWidth + 600,
                    Main.screenHeight + 600
                );

                if (npc.Hitbox.Intersects(screenRect))
                {
                    offscreen = false;
                    break;
                }
            }
        }

        if (offscreen && npc.ai[0] > 120) // Give 2 seconds before allowing offscreen despawn
            shouldDespawn = true;

        // Despawn
        if (shouldDespawn)
        {
            // Small puff of dust when despawning
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height,
                    DustID.Smoke, 0f, -1f, 100, Color.Gray, 0.6f);
            }

            npc.active = false;
            npc.netUpdate = true;
        }
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(isFleeingCritter);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        isFleeingCritter = bitReader.ReadBit();
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (!isFleeingCritter || npc.aiStyle != -1)
            return;

        if (npc.ai[0] > 50f)
            return;

        Texture2D exclamationTexture = ModContent.Request<Texture2D>($"{UI_ASSET_DIRECTORY}CritterScared").Value;

        float animationProgress = npc.ai[0] / 50f;
        float bounceTime = Math.Min(npc.ai[0] / 60f, 1f);

        float bounceHeight = (float)Math.Sin(bounceTime * Math.PI) * 20f;

        float shakeIntensity = (1f - animationProgress) * 2f;
        float shakeX = (float)Math.Sin(npc.ai[0] * 0.8f) * shakeIntensity;
        float shakeY = (float)Math.Cos(npc.ai[0] * 1.2f) * shakeIntensity * 0.5f;

        float alpha = 1f - (animationProgress * animationProgress);

        float scale = 1f;
        if (animationProgress < 0.2f)
        {
            scale = MathHelper.Lerp(1.5f, 1f, animationProgress / 0.2f);
        }
        else if (animationProgress > 0.7f)
        {
            scale = MathHelper.Lerp(1f, 0.3f, (animationProgress - 0.7f) / 0.3f);
        }

        Vector2 exclamationPos = new Vector2(
            npc.Center.X - screenPos.X + shakeX,
            npc.position.Y - screenPos.Y - npc.height - 10f - bounceHeight + shakeY
        );

        Vector2 origin = exclamationTexture.Size() * 0.5f;

        Color exclamationColor = Color.White * alpha;

        spriteBatch.Draw(
            exclamationTexture,
            exclamationPos,
            null,
            exclamationColor,
            0f,
            origin,
            scale,
            SpriteEffects.None,
            0f
        );

        if (alpha > 0.5f)
        {
            Color glowColor = Color.Yellow * (alpha * 0.3f);
            spriteBatch.Draw(
                exclamationTexture,
                exclamationPos,
                null,
                glowColor,
                0f,
                origin,
                scale * 1.2f,
                SpriteEffects.None,
                0f
            );
        }
    }
}