using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Reverie.Common.NPCs;

public class SlimeGlobal : GlobalNPC
{
    public override bool InstancePerEntity => true;
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.aiStyle == NPCAIStyleID.Slime;

    private Vector2 squishScale = Vector2.One;
    private bool isBeingConsumed = false;
    private int consumingKingSlimeIndex = -1;

    public override void SetDefaults(NPC npc)
    {
        if (npc.aiStyle != NPCAIStyleID.Slime) return;

        npc.HitSound = new SoundStyle($"{SFX_DIRECTORY}SlimeHit") with { Volume = 0.46f, PitchVariance = 0.3f, MaxInstances = 8 };
        npc.DeathSound = new SoundStyle($"{SFX_DIRECTORY}SlimeKilled") with { Volume = 0.76f, PitchVariance = 0.2f, MaxInstances = 8 };
    }

    public override bool PreAI(NPC npc)
    {
        if (npc.aiStyle != NPCAIStyleID.Slime) return true;

        NPC consumingKingSlime = FindConsumingKingSlime(npc);

        if (consumingKingSlime != null)
        {
            isBeingConsumed = true;
            consumingKingSlimeIndex = consumingKingSlime.whoAmI;

            HandleConsumptionAttraction(npc, consumingKingSlime);
            return false; // Skip normal AI
        }
        else
        {
            isBeingConsumed = false;
            consumingKingSlimeIndex = -1;
        }

        return true; // Continue with normal AI
    }

    public override void AI(NPC npc)
    {
        if (npc.aiStyle == NPCAIStyleID.Slime && !isBeingConsumed)
        {
            HandleSquishScale(npc);
        }
    }

    private NPC FindConsumingKingSlime(NPC slime)
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC kingSlime = Main.npc[i];

            if (kingSlime.active && kingSlime.type == ModContent.NPCType<Content.NPCs.Bosses.KingSlime.KinguSlime>())
            {
                // Check if king slime is in consuming state and slime is in range
                if (kingSlime.ai[0] == 5f) // ConsumingSlimes state
                {
                    float distance = Vector2.Distance(slime.Center, kingSlime.Center);
                    if (distance <= 200f) // CONSUME_DETECTION_RANGE
                    {
                        return kingSlime;
                    }
                }
            }
        }
        return null;
    }

    private void HandleConsumptionAttraction(NPC slime, NPC kingSlime)
    {
        Vector2 toKingSlime = kingSlime.Center - slime.Center;
        float distance = toKingSlime.Length();

        if (distance > 10f) // Avoid division by zero
        {
            toKingSlime.Normalize();

            float attractionForce = MathHelper.Lerp(6f, 2f, distance / 200f);

            slime.velocity = Vector2.Lerp(slime.velocity, toKingSlime * attractionForce, 0.15f);

            slime.spriteDirection = Math.Sign(toKingSlime.X) == 1 ? -1 : 1;

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(slime.position, slime.width, slime.height,
                    DustID.t_Slime, toKingSlime.X, toKingSlime.Y,
                    150, new Color(78, 136, 255, 150), 1.2f);
                dust.velocity *= 0.3f;
                dust.noGravity = true;
            }
        }

        float speed = slime.velocity.Length();
        float stretchFactor = MathHelper.Clamp(speed / 8f, 0f, 0.4f);

        Vector2 direction = slime.velocity.SafeNormalize(Vector2.UnitX);
        squishScale.X = 1f + stretchFactor * Math.Abs(direction.X) * 0.3f;
        squishScale.Y = 1f - stretchFactor * Math.Abs(direction.Y) * 0.3f;

        slime.velocity.Y += 0.5f;
        if (slime.velocity.Y > 10f)
            slime.velocity.Y = 10f;

        if (slime.collideX)
            slime.velocity.X *= -0.8f;
        if (slime.collideY && slime.velocity.Y > 0f)
            slime.velocity.Y *= -0.6f;
    }

    private void HandleSquishScale(NPC npc)
    {
        const float MAX_STRETCH = 1.5f;
        const float MIN_SQUISH = 0.7f;

        float yVelocityFactor = MathHelper.Clamp(npc.velocity.Y / 16f, -1f, 1f);

        if (npc.velocity.Y == 0f && npc.ai[0] >= -30f)
        {
            float preparationProgress = Math.Min(1f, (npc.ai[0] + 30f) / 30f);
            squishScale.X = MathHelper.Lerp(1f, 1.3f, preparationProgress);
            squishScale.Y = MathHelper.Lerp(1f, 0.92f, preparationProgress);
            return;
        }

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
        if (npc.aiStyle != NPCAIStyleID.Slime) return base.PreDraw(npc, spriteBatch, screenPos, drawColor);

        Texture2D texture = TextureAssets.Npc[npc.type].Value;

        int frameCount = 2;
        int frameHeight = texture.Height / frameCount;
        int frameWidth = texture.Width;

        if (npc.ai[0] == -999f)
        {
            npc.frame.Y = 0;
            npc.frameCounter = 0.0;
        }

        Rectangle sourceRectangle = new(0, npc.frame.Y, frameWidth, frameHeight);

        Color finalColor = npc.color;
        finalColor.R = (byte)((finalColor.R * drawColor.R) / 255);
        finalColor.G = (byte)((finalColor.G * drawColor.G) / 255);
        finalColor.B = (byte)((finalColor.B * drawColor.B) / 255);
        finalColor.A = npc.color.A;

        // consumption glow
        if (isBeingConsumed)
        {
            float glowIntensity = 0.3f + 0.2f * (float)Math.Sin(Main.timeForVisualEffects * 0.1f);
            finalColor = Color.Lerp(finalColor, new Color(78, 136, 255), glowIntensity);
        }

        Vector2 drawPos = npc.Center - Main.screenPosition;
        SpriteEffects spriteEffects = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        float spriteRotation = 0f;
        if (npc.velocity.Y > 0f)
        {
            spriteRotation = MathHelper.Clamp(npc.velocity.X * 0.04f, -MathHelper.PiOver2, MathHelper.PiOver2);
        }

        Main.EntitySpriteDraw(
            texture,
            drawPos,
            sourceRectangle,
            finalColor,
            spriteRotation,
            new Vector2(frameWidth / 2, frameHeight / 2),
            squishScale * npc.scale,
            spriteEffects
        );

        return false;
    }
}